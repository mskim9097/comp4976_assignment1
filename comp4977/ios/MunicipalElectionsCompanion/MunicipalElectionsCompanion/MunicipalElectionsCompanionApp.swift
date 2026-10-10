import SwiftUI

@main
struct MunicipalElectionsCompanionApp: App {
    @State private var session = Session()

    var body: some Scene {
        WindowGroup {
            RootView()
                .environment(session)
        }
    }
}
