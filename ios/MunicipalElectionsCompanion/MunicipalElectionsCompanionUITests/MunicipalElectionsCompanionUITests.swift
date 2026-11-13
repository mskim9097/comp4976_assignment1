import XCTest

// End-to-end UI tests. The app runs with the launch argument -fakeFirebase (Firebase data kept in memory) against a
// Web API that must be running (API_BASE_URL, default: the local Aspire API https://localhost:7261).
// Every test creates a new account through the API and logs in with the debug launch argument -autoLogin.
final class MunicipalElectionsCompanionUITests: XCTestCase {
    private var app: XCUIApplication!
    private var isPad: Bool { UIDevice.current.userInterfaceIdiom == .pad }
    private static let password = "Passw0rd!"
    private let apiBase = ProcessInfo.processInfo.environment["API_BASE_URL"] ?? "https://localhost:7261"

    override func setUpWithError() throws {
        continueAfterFailure = false
        XCUIDevice.shared.orientation = .portrait
    }

    // MARK: - Tests

    func testLoginAndRegistrationScreens() throws {
        launch(autoLoginEmail: nil)
        XCTAssertTrue(app.buttons["Log In"].waitForExistence(timeout: 10))
        shot("01-login")
        app.buttons["Create an account"].tap()
        XCTAssertTrue(app.buttons["Create Account"].waitForExistence(timeout: 10))
        shot("02-registration")
        // Empty form: validation messages appear and nothing is sent.
        app.buttons["Create Account"].tap()
        XCTAssertTrue(app.staticTexts["Name is required."].waitForExistence(timeout: 10))
        shot("02b-registration-validation")
    }

    func testFirstLoginChooseCityAndManageSelections() throws {
        launch(autoLoginEmail: try createAccount())
        XCTAssertTrue(app.navigationBars["Where do you live?"].waitForExistence(timeout: 30))
        shot("03-choose-city")
        chooseCity("Burnaby")

        XCTAssertTrue(app.staticTexts["City: Burnaby"].firstMatch.waitForExistence(timeout: 30))
        shot("04-home")

        go(to: "Candidates")
        let mayor = candidateButton("Mayor", index: 0)
        XCTAssertTrue(mayor.waitForExistence(timeout: 30))
        shot("05-candidate-directory")
        mayor.tap()

        XCTAssertTrue(app.buttons["Add Selection"].waitForExistence(timeout: 10))
        shot("06-candidate-details")
        app.buttons["Add Selection"].tap()
        let notes = app.textViews.firstMatch
        XCTAssertTrue(notes.waitForExistence(timeout: 10))
        notes.tap()
        notes.typeText("Good plan for transit.")
        shot("07-create-selection")
        tapSave()

        XCTAssertTrue(app.buttons["Edit Selection"].waitForExistence(timeout: 20))
        XCTAssertTrue(app.buttons["Delete Selection"].exists)
        app.buttons["Edit Selection"].tap()
        let editNotes = app.textViews.firstMatch
        XCTAssertTrue(editNotes.waitForExistence(timeout: 10))
        editNotes.tap()
        editNotes.typeText(" Edited.")
        shot("08-edit-selection")
        tapSave()
        XCTAssertTrue(label(containing: "Edited.").waitForExistence(timeout: 10))

        go(to: "My Selections")
        XCTAssertTrue(label(containing: "Edited.").waitForExistence(timeout: 10))
        shot("09-selection-list")

        go(to: "Profile")
        XCTAssertTrue(app.staticTexts["Saved selections"].waitForExistence(timeout: 10))
        shot("10-profile")

        go(to: "My Selections")
        app.buttons["Delete"].firstMatch.tap()
        shot("11-delete-confirmation")
        app.buttons["Delete Selection"].tap()
        XCTAssertTrue(app.staticTexts["No selections yet"].waitForExistence(timeout: 10))
        shot("12-selection-deleted")
    }

    func testSelectionRulesAreEnforced() throws {
        try startInCity("Burnaby")
        go(to: "Candidates")

        // One mayor only.
        let firstMayor = candidateButton("Mayor", index: 0)
        XCTAssertTrue(firstMayor.waitForExistence(timeout: 30))
        firstMayor.tap()
        app.buttons["Add Selection"].tap()
        tapSave()
        XCTAssertTrue(app.buttons["Edit Selection"].waitForExistence(timeout: 20))
        goBack()

        let secondMayor = candidateButton("Mayor", index: 1)
        XCTAssertTrue(secondMayor.waitForExistence(timeout: 10))
        secondMayor.tap()
        app.buttons["Add Selection"].tap()
        tapSave()
        XCTAssertTrue(app.staticTexts["You can select only one Mayor."].waitForExistence(timeout: 10))
        shot("seat-limit-error")
        app.buttons["Cancel"].tap()

        // Notes stop at 500 characters.
        app.buttons["Add Selection"].tap()
        let notes = app.textViews.firstMatch
        XCTAssertTrue(notes.waitForExistence(timeout: 10))
        notes.tap()
        notes.typeText(String(repeating: "x", count: 505))
        XCTAssertTrue(app.staticTexts["0 characters left"].waitForExistence(timeout: 10))
        shot("notes-limit")
    }

    func testChangingMunicipalityReloadsData() throws {
        try startInCity("Burnaby")
        go(to: "Candidates")
        XCTAssertTrue(candidateButton("Mayor", index: 0).waitForExistence(timeout: 30))
        candidateButton("Mayor", index: 0).tap()
        app.buttons["Add Selection"].tap()
        tapSave()
        XCTAssertTrue(app.buttons["Edit Selection"].waitForExistence(timeout: 20))
        goBack()

        go(to: "Profile")
        XCTAssertTrue(app.staticTexts["Saved selections"].waitForExistence(timeout: 10))
        app.buttons["Change Municipality"].tap()
        let surrey = app.buttons.matching(NSPredicate(format: "label BEGINSWITH 'Surrey'")).firstMatch
        XCTAssertTrue(surrey.waitForExistence(timeout: 20))
        surrey.tap()
        app.buttons["Change to Surrey"].tap()
        XCTAssertTrue(app.staticTexts["City: Surrey"].firstMatch.waitForExistence(timeout: 30))

        go(to: "My Selections")
        XCTAssertTrue(app.staticTexts["No selections yet"].waitForExistence(timeout: 15))
        go(to: "Candidates")
        XCTAssertTrue(app.buttons.matching(NSPredicate(format: "label CONTAINS 'Surrey'")).firstMatch.waitForExistence(timeout: 20))
        XCTAssertFalse(app.buttons.matching(NSPredicate(format: "label CONTAINS 'Burnaby'")).firstMatch.exists,
                       "no Burnaby candidate may be shown")
        shot("surrey-candidates")
    }

    func testAdaptiveLayoutInBothOrientations() throws {
        try startInCity("Burnaby")
        go(to: "Candidates")
        XCTAssertTrue(candidateButton("Mayor", index: 0).waitForExistence(timeout: 30))

        for (name, orientation) in [("portrait", UIDeviceOrientation.portrait), ("landscape", .landscapeLeft)] {
            XCUIDevice.shared.orientation = orientation
            sleep(1)
            go(to: "Home")
            XCTAssertTrue(app.staticTexts["Municipal Elections Companion"].waitForExistence(timeout: 10))
            shot("\(name)-home")
            go(to: "Candidates")
            XCTAssertTrue(candidateButton("Mayor", index: 0).waitForExistence(timeout: 10))
            shot("\(name)-candidate-directory")

            // Candidate details, and an edit in progress that must survive a rotation.
            candidateButton("Councillor", index: name == "portrait" ? 0 : 1).tap()
            XCTAssertTrue(app.buttons["Add Selection"].waitForExistence(timeout: 10))
            shot("\(name)-candidate-details")
            app.buttons["Add Selection"].tap()
            let notes = app.textViews.firstMatch
            XCTAssertTrue(notes.waitForExistence(timeout: 10))
            notes.tap()
            notes.typeText("Kept while rotating")
            XCUIDevice.shared.orientation = orientation == .portrait ? .landscapeLeft : .portrait
            sleep(1)
            XCTAssertEqual(app.textViews.firstMatch.value as? String, "Kept while rotating", "edits survive a rotation")
            shot("\(name)-create-selection-after-rotation")
            tapSave()
            XCTAssertTrue(app.buttons["Edit Selection"].waitForExistence(timeout: 20))
            XCUIDevice.shared.orientation = orientation
            sleep(1)
            goBack()

            go(to: "My Selections")
            XCTAssertTrue(app.buttons["Edit"].firstMatch.waitForExistence(timeout: 10))
            shot("\(name)-selection-list")
            go(to: "Profile")
            XCTAssertTrue(app.staticTexts["Saved selections"].waitForExistence(timeout: 10))
            shot("\(name)-profile")
        }
    }

    // MARK: - Documentation (only with DEMO=1, i.e. TEST_RUNNER_DEMO=1)

    private static let demoEmail = "demo.voter@example.com"
    private static var demoEnabled: Bool { ProcessInfo.processInfo.environment["DEMO"] == "1" }

    // Creates the demo user's selections in Firebase through the app (run once, with REAL_FIREBASE=1).
    func testSeedDemoData() throws {
        try XCTSkipUnless(Self.demoEnabled)
        try ensureDemoAccount()
        launch(autoLoginEmail: Self.demoEmail)
        if app.navigationBars["Where do you live?"].waitForExistence(timeout: 30) {
            chooseCity("Burnaby")
        }
        XCTAssertTrue(app.staticTexts["City: Burnaby"].firstMatch.waitForExistence(timeout: 30))
        go(to: "Candidates")
        let seeds = [("Mei-Ling Tran", "Strong on transit and safer streets."),
                     ("Arjun Bhatia", "Likes the tree canopy plan."),
                     ("Hannah Greenwood", ""),
                     ("Oliver Fraser", "Focus on student well-being.")]
        for (name, note) in seeds {
            let button = candidateButton(name, index: 0)
            XCTAssertTrue(button.waitForExistence(timeout: 20), name)
            button.tap()
            if app.buttons["Add Selection"].waitForExistence(timeout: 10) {
                app.buttons["Add Selection"].tap()
                if !note.isEmpty {
                    let notes = app.textViews.firstMatch
                    XCTAssertTrue(notes.waitForExistence(timeout: 10))
                    notes.tap()
                    notes.typeText(note)
                }
                tapSave()
                XCTAssertTrue(app.buttons["Edit Selection"].waitForExistence(timeout: 20))
            }
            goBack()
        }
    }

    // The screen captures for the project document, in portrait and landscape, with the demo user's data.
    func testDocumentationScreens() throws {
        try XCTSkipUnless(Self.demoEnabled)
        // City selection: only a new user sees it.
        launch(autoLoginEmail: try createAccount())
        XCTAssertTrue(app.navigationBars["Where do you live?"].waitForExistence(timeout: 30))
        shot("doc-03-city-selection")

        try ensureDemoAccount()
        launch(autoLoginEmail: Self.demoEmail)
        XCTAssertTrue(app.staticTexts["City: Burnaby"].firstMatch.waitForExistence(timeout: 30))

        for (name, orientation) in [("portrait", UIDeviceOrientation.portrait), ("landscape", .landscapeLeft)] {
            XCUIDevice.shared.orientation = orientation
            sleep(1)
            go(to: "Home")
            XCTAssertTrue(app.staticTexts["Municipal Elections Companion"].waitForExistence(timeout: 10))
            shot("doc-\(name)-04-home")

            go(to: "Candidates")
            XCTAssertTrue(candidateButton("Mayor", index: 0).waitForExistence(timeout: 20))
            shot("doc-\(name)-05-candidate-directory")

            // A candidate that is saved: Edit and Delete buttons.
            candidateButton("Mei-Ling Tran", index: 0).tap()
            XCTAssertTrue(app.buttons["Edit Selection"].waitForExistence(timeout: 20))
            shot("doc-\(name)-06-candidate-details-saved")
            app.buttons["Edit Selection"].tap()
            XCTAssertTrue(app.textViews.firstMatch.waitForExistence(timeout: 10))
            shot("doc-\(name)-08-edit-selection")
            app.buttons["Cancel"].tap()
            goBack()

            // A candidate that is not saved yet: Add Selection and the create sheet.
            candidateButton("Victor Salazar", index: 0).tap()
            XCTAssertTrue(app.buttons["Add Selection"].waitForExistence(timeout: 20))
            shot("doc-\(name)-06b-candidate-details")
            app.buttons["Add Selection"].tap()
            let notes = app.textViews.firstMatch
            XCTAssertTrue(notes.waitForExistence(timeout: 10))
            notes.tap()
            notes.typeText("Compare with the other mayor.")
            shot("doc-\(name)-07-create-selection")
            app.buttons["Cancel"].tap()
            goBack()

            go(to: "My Selections")
            XCTAssertTrue(app.buttons["Edit"].firstMatch.waitForExistence(timeout: 20))
            shot("doc-\(name)-09-selection-list")

            go(to: "Profile")
            XCTAssertTrue(app.staticTexts["Saved selections"].waitForExistence(timeout: 10))
            shot("doc-\(name)-10-profile")
        }
    }

    private func ensureDemoAccount() throws {
        var request = URLRequest(url: URL(string: "\(apiBase)/api/auth/register")!)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.httpBody = try JSONSerialization.data(withJSONObject: ["name": "Demo Voter", "email": Self.demoEmail, "password": Self.password])
        let done = expectation(description: "register demo")
        var status = 0
        URLSession(configuration: .ephemeral, delegate: TrustLocalhost(), delegateQueue: nil).dataTask(with: request) { _, response, _ in
            status = (response as? HTTPURLResponse)?.statusCode ?? 0
            done.fulfill()
        }.resume()
        wait(for: [done], timeout: 60)
        XCTAssertTrue(status == 200 || status == 400, "demo account could not be registered (\(status))") // 400: already exists
    }

    // MARK: - Helpers

    private func launch(autoLoginEmail: String?) {
        app = XCUIApplication()
        // REAL_FIREBASE=1 (pass it as TEST_RUNNER_REAL_FIREBASE=1 to xcodebuild) runs against the real Firebase project
        // of the GoogleService-Info.plist instead of the in-memory fake.
        app.launchArguments = ["-resetSession"]
        if ProcessInfo.processInfo.environment["REAL_FIREBASE"] != "1" {
            app.launchArguments += ["-fakeFirebase"]
        }
        if let email = autoLoginEmail {
            app.launchArguments += ["-autoLogin", "\(email):\(Self.password)"]
        }
        app.launchEnvironment["API_BASE_URL"] = apiBase
        app.launch()
    }

    // New account, logged in, municipality chosen.
    private func startInCity(_ city: String) throws {
        launch(autoLoginEmail: try createAccount())
        chooseCity(city)
        XCTAssertTrue(app.staticTexts["City: \(city)"].firstMatch.waitForExistence(timeout: 30))
    }

    // Creates a new account through the Web API and returns its email.
    private func createAccount() throws -> String {
        let email = "ui\(UUID().uuidString.prefix(8).lowercased())@example.com"
        var request = URLRequest(url: URL(string: "\(apiBase)/api/auth/register")!)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.httpBody = try JSONSerialization.data(withJSONObject: ["name": "UI Tester", "email": email, "password": Self.password])
        let done = expectation(description: "register")
        var status = 0
        URLSession(configuration: .ephemeral, delegate: TrustLocalhost(), delegateQueue: nil).dataTask(with: request) { _, response, _ in
            status = (response as? HTTPURLResponse)?.statusCode ?? 0
            done.fulfill()
        }.resume()
        wait(for: [done], timeout: 60)
        XCTAssertEqual(status, 200, "the test account could not be registered")
        // Lets a clean-up script delete the Firebase users these tests create.
        if let path = ProcessInfo.processInfo.environment["CREATED_EMAILS_FILE"],
           let handle = FileHandle(forWritingAtPath: path) ?? { FileManager.default.createFile(atPath: path, contents: nil); return FileHandle(forWritingAtPath: path) }() {
            handle.seekToEndOfFile()
            handle.write(Data((email + "\n").utf8))
            try? handle.close()
        }
        return email
    }

    private func chooseCity(_ city: String) {
        let row = app.buttons.matching(NSPredicate(format: "label BEGINSWITH %@", city)).firstMatch
        XCTAssertTrue(row.waitForExistence(timeout: 30))
        row.tap()
    }

    // iPhone: tab bar. iPad: sidebar of the NavigationSplitView (collapsed in portrait: open it first).
    private func go(to section: String) {
        let tab = app.tabBars.buttons[section]
        if tab.exists {
            tab.tap()
            return
        }
        let row = app.cells.containing(.staticText, identifier: section).firstMatch
        if !row.waitForExistence(timeout: 1) {
            let toggle = app.navigationBars.buttons.matching(NSPredicate(format: "label CONTAINS[c] 'sidebar'")).firstMatch
            XCTAssertTrue(toggle.waitForExistence(timeout: 5), "sidebar toggle")
            toggle.tap()
        }
        XCTAssertTrue(row.waitForExistence(timeout: 5), "sidebar row \(section)")
        row.tap()
    }

    private func goBack() {
        let back = app.navigationBars.buttons["BackButton"]
        if back.waitForExistence(timeout: 3) {
            back.tap()
        } else {
            app.navigationBars.buttons.firstMatch.tap()
        }
    }

    private func tapSave() {
        app.buttons["Save"].firstMatch.tap()
    }

    private func candidateButton(_ position: String, index: Int) -> XCUIElement {
        app.buttons.matching(NSPredicate(format: "label CONTAINS %@", position)).element(boundBy: index)
    }

    private func label(containing text: String) -> XCUIElement {
        app.staticTexts.matching(NSPredicate(format: "label CONTAINS %@", text)).firstMatch
    }

    private func shot(_ name: String) {
        let attachment = XCTAttachment(screenshot: XCUIScreen.main.screenshot())
        attachment.name = name
        attachment.lifetime = .keepAlways
        add(attachment)
    }
}

// Accepts the self-signed development certificate of the local API.
private final class TrustLocalhost: NSObject, URLSessionDelegate {
    func urlSession(_ session: URLSession, didReceive challenge: URLAuthenticationChallenge,
                    completionHandler: @escaping (URLSession.AuthChallengeDisposition, URLCredential?) -> Void) {
        if challenge.protectionSpace.host == "localhost", let trust = challenge.protectionSpace.serverTrust {
            completionHandler(.useCredential, URLCredential(trust: trust))
        } else {
            completionHandler(.performDefaultHandling, nil)
        }
    }
}
