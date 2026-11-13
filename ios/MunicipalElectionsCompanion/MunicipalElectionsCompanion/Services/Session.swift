import Foundation
import Observation

// Who is logged in. The JWT of the Web API lives in the Keychain; the profile (with the chosen municipality)
// lives in Firestore. A user is "signed in" only when both services accepted them.
@MainActor
@Observable
final class Session {
    let services: AppServices
    private(set) var token: String?
    private(set) var user: UserResponse?
    private(set) var profile: UserProfile?
    private(set) var isRestoring: Bool
    private(set) var profileError: String?
    var notice: String?

    var isAuthenticated: Bool { token != nil }
    var email: String? { (user?.email ?? profile?.email)?.lowercased() }
    // The municipality the user lives in; every election request and every selection query uses it.
    var city: String? { profile?.city }
    var needsCity: Bool { isAuthenticated && profile != nil && profile?.city == nil }

    init(services: AppServices? = nil) {
        self.services = services ?? .current()
        let saved = TokenStore.load()
        self.token = saved
        self.isRestoring = saved != nil
    }

    // For the SwiftUI previews: a session in a given state without touching the Keychain.
    init(services: AppServices, token: String?, user: UserResponse?, profile: UserProfile?) {
        self.services = services
        self.token = token
        self.user = user
        self.profile = profile
        self.isRestoring = false
    }

    // Called at launch: checks the saved JWT and the Firebase sign-in, then loads the user and the profile.
    func restore() async {
        guard let token else {
            isRestoring = false
            return
        }
        if JWT.isExpired(token) || services.cloud.currentEmail == nil {
            endSession(notice: "Your session has expired. Please log in again.")
            return
        }
        await loadUser()
        await loadProfile()
        isRestoring = false
    }

    func login(email: String, password: String) async throws {
        let clean = email.trimmingCharacters(in: .whitespaces)
        let newToken = try await services.auth.login(email: clean, password: password)
        try await services.cloud.signInOrCreate(email: clean.lowercased(), password: password)
        TokenStore.save(newToken)
        notice = nil
        isRestoring = true
        token = newToken
        await loadUser()
        await loadProfile()
        isRestoring = false
    }

    // Creates the account in the Web API, then logs in (which also creates the Firebase account).
    func register(name: String, email: String, password: String) async throws {
        let cleanEmail = email.trimmingCharacters(in: .whitespaces)
        try await services.auth.register(name: name.trimmingCharacters(in: .whitespaces), email: cleanEmail, password: password)
        try await login(email: cleanEmail, password: password)
    }

    func loadUser() async {
        guard let token else { return }
        do {
            user = try await services.auth.me(token: token)
        } catch {
            _ = message(for: error)
        }
    }

    // Reads the profile from Firestore, or creates it (without a city yet) the first time.
    func loadProfile() async {
        guard let email else { return }
        do {
            if let stored = try await services.profiles.profile(email: email) {
                profile = stored
            } else {
                let created = UserProfile(email: email, name: user?.name ?? "", city: nil, municipalityId: nil,
                                          lastSync: nil, dateCreated: .now)
                try await services.profiles.save(created)
                profile = created
            }
            profileError = nil
        } catch {
            profileError = message(for: error)
        }
    }

    // Saves the chosen municipality to Firebase first; the app reloads its data when `city` changes.
    func setCity(_ municipality: Municipality) async throws {
        guard var updated = profile else { return }
        updated.city = municipality.name
        updated.municipalityId = municipality.municipalityId
        try await services.profiles.save(updated)
        profile = updated
    }

    func recordSync(at date: Date = .now) async {
        guard let email, profile != nil else { return }
        profile?.lastSync = date
        await services.profiles.markSynced(email: email, at: date)
    }

    func logout() {
        endSession(notice: nil)
    }

    private func endSession(notice: String?) {
        TokenStore.delete()
        services.cloud.signOut()
        token = nil
        user = nil
        profile = nil
        profileError = nil
        isRestoring = false
        self.notice = notice
    }

    // Text to show for an error, or nil when nothing should be shown (cancelled request, or an expired session,
    // which sends the user back to the login screen).
    func message(for error: Error) -> String? {
        if error is CancellationError {
            return nil
        }
        if case APIError.unauthorized = error, isAuthenticated {
            endSession(notice: "Your session has expired. Please log in again.")
            return nil
        }
        if let cloud = error as? CloudError, cloud.requiresLogin, isAuthenticated {
            endSession(notice: cloud.message)
            return nil
        }
        return error.localizedDescription
    }
}
