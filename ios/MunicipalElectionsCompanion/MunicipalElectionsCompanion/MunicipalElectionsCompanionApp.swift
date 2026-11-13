import SwiftUI
import FirebaseCore

@main
struct MunicipalElectionsCompanionApp: App {
    // Firebase needs the GoogleService-Info.plist of the Firebase project; without it the app explains what is missing.
    private static let firebaseConfigured: Bool = {
        guard !AppServices.usesFakeFirebase else { return false }
        guard Bundle.main.path(forResource: "GoogleService-Info", ofType: "plist") != nil else { return false }
        FirebaseApp.configure()
        return true
    }()

    @State private var session = Session()
    @State private var election = ElectionViewModel()
    @State private var selections = SelectionsViewModel()

    var body: some Scene {
        WindowGroup {
            if Self.firebaseConfigured || AppServices.usesFakeFirebase {
                RootView()
                    .environment(session)
                    .environment(election)
                    .environment(selections)
            } else {
                ConfigurationMissingView()
            }
        }
    }
}
