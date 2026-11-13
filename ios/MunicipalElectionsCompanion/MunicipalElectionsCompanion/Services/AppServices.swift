import Foundation

// Everything the app talks to, in one place: the Web API (authentication and election data) and Firebase
// (cloud sign-in, user profile, candidate selections). The app uses the live set; the SwiftUI previews use in-memory fakes.
struct AppServices {
    let auth: AuthAPI
    let election: ElectionAPI
    let cloud: CloudAuthenticating
    let profiles: ProfileStoring
    let selections: SelectionStoring

    static func live(client: APIClient = APIClient()) -> AppServices {
        AppServices(auth: AuthAPI(client: client),
                    election: ElectionAPI(client: client),
                    cloud: FirebaseCloudAuth(),
                    profiles: FirestoreProfileStore(),
                    selections: FirestoreSelectionStore())
    }
}
