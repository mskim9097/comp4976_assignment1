import Foundation

// Registration, login and "who am I" of the Web API (JWT).
struct AuthAPI {
    let client: APIClient

    func login(email: String, password: String) async throws -> String {
        let response: TokenResponse = try await client.post(
            "api/auth/login", body: LoginRequest(email: email, password: password))
        return response.token
    }

    func register(name: String, email: String, password: String) async throws {
        let _: MessageResponse = try await client.post(
            "api/auth/register", body: RegisterRequest(name: name, email: email, password: password))
    }

    func me(token: String) async throws -> UserResponse {
        try await client.get("api/auth/me", token: token)
    }
}
