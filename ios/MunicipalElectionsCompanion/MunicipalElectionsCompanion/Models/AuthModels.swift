import Foundation

struct LoginRequest: Encodable {
    let email: String
    let password: String
}

struct RegisterRequest: Encodable {
    let name: String
    let email: String
    let password: String
}

struct TokenResponse: Decodable {
    let token: String
}

struct MessageResponse: Decodable {
    let message: String
}

struct UserResponse: Decodable {
    let name: String
    let email: String
}

// Standard ASP.NET Core error body: `title` for general errors, `errors` for validation failures.
struct ProblemDetails: Decodable {
    let title: String?
    let errors: [String: [String]]?
}
