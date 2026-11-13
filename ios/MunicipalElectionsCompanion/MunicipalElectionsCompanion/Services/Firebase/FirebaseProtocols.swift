import Foundation

// What the app needs from Firebase. The live implementations use the Firebase SDK; the previews use
// in-memory versions, so no view or view model depends on the SDK directly.

protocol CloudAuthenticating {
    // Email of the user signed in to Firebase (nil when nobody is).
    var currentEmail: String? { get }
    // Signs in with the same credentials as the Web API; creates the Firebase account the first time.
    func signInOrCreate(email: String, password: String) async throws
    func signOut()
}

protocol ProfileStoring {
    func profile(email: String) async throws -> UserProfile?
    func save(_ profile: UserProfile) async throws
    func markSynced(email: String, at date: Date) async
}

protocol SelectionStoring {
    // The selections of one user in one city.
    func selections(email: String, city: String) async throws -> [CandidateSelection]
    // Throws `SelectionError.duplicate` when the candidate is already saved.
    func create(_ selection: CandidateSelection) async throws
    func update(_ selection: CandidateSelection) async throws
    func delete(id: String) async throws
}

// An error of Firebase with a message that can be shown to the user as it is.
struct CloudError: LocalizedError {
    let message: String
    // True when the Firebase session is gone and the user has to log in again.
    var requiresLogin = false
    var errorDescription: String? { message }
}
