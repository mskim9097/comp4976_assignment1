import Foundation
import Observation

// Who is logged in. Owns the API client and the JWT (stored in the Keychain).
@MainActor
@Observable
final class Session {
    let api: APIClient
    private(set) var token: String?
    private(set) var user: UserResponse?
    var notice: String?

    var isAuthenticated: Bool { token != nil }

    init(api: APIClient? = nil) {
        self.api = api ?? APIClient()
        self.token = TokenStore.load()
    }

    func login(email: String, password: String) async throws {
        let response: TokenResponse = try await api.post(
            "api/auth/login", body: LoginRequest(email: email.trimmingCharacters(in: .whitespaces), password: password))
        TokenStore.save(response.token)
        notice = nil
        token = response.token
        await loadUser()
    }

    // Creates the account, then logs in with the same credentials.
    func register(name: String, email: String, password: String) async throws {
        let cleanEmail = email.trimmingCharacters(in: .whitespaces)
        let _: MessageResponse = try await api.post(
            "api/auth/register",
            body: RegisterRequest(name: name.trimmingCharacters(in: .whitespaces), email: cleanEmail, password: password))
        try await login(email: cleanEmail, password: password)
    }

    func loadUser() async {
        guard let token else { return }
        do {
            user = try await api.get("api/auth/me", token: token)
        } catch {
            _ = message(for: error)
        }
    }

    func logout() {
        TokenStore.delete()
        token = nil
        user = nil
    }

    // Text to show for an error, or nil when nothing should be shown (cancelled request or an expired session,
    // which sends the user back to the login screen).
    func message(for error: Error) -> String? {
        if error is CancellationError {
            return nil
        }
        if case APIError.unauthorized = error, isAuthenticated {
            logout()
            notice = "Your session has expired. Please log in again."
            return nil
        }
        return error.localizedDescription
    }
}
