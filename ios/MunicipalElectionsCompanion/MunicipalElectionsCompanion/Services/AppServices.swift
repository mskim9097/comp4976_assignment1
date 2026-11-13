import Foundation

// Everything the app talks to, in one place: the Web API (authentication and election data) and Firebase
// (cloud sign-in, user profile, candidate selections). The live set is used by the app, the previews use fakes.
struct AppServices {
    let auth: AuthAPI
    let election: ElectionAPI
    let cloud: CloudAuthenticating
    let profiles: ProfileStoring
    let selections: SelectionStoring

    // The services of the running app. Debug builds started with the launch argument -fakeFirebase keep
    // the Firebase data in memory (used by the UI tests and for trying the app without a Firebase project).
    static func current() -> AppServices {
        #if DEBUG
        if usesFakeFirebase {
            return AppServices(auth: AuthAPI(client: APIClient()), election: ElectionAPI(client: APIClient()),
                               cloud: InMemoryCloud(email: nil), profiles: InMemoryProfiles(stored: nil),
                               selections: InMemorySelections(items: []))
        }
        #endif
        return live()
    }

    static var usesFakeFirebase: Bool {
        #if DEBUG
        ProcessInfo.processInfo.arguments.contains("-fakeFirebase")
        #else
        false
        #endif
    }

    // Debug builds started with "-autoLogin email:password" log in at launch (used by the UI tests, because typing
    // into secure text fields is unreliable in the simulator).
    static var debugCredentials: (email: String, password: String)? {
        #if DEBUG
        let arguments = ProcessInfo.processInfo.arguments
        guard let index = arguments.firstIndex(of: "-autoLogin"), index + 1 < arguments.count else { return nil }
        let parts = arguments[index + 1].split(separator: ":", maxSplits: 1).map(String.init)
        return parts.count == 2 ? (parts[0], parts[1]) : nil
        #else
        nil
        #endif
    }

    static func live(client: APIClient = APIClient()) -> AppServices {
        AppServices(auth: AuthAPI(client: client),
                    election: ElectionAPI(client: client),
                    cloud: FirebaseCloudAuth(),
                    profiles: FirestoreProfileStore(),
                    selections: FirestoreSelectionStore())
    }
}
