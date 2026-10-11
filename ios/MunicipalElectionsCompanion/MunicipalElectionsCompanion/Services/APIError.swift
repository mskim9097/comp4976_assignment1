import Foundation

enum APIError: LocalizedError {
    case network(String)
    case unauthorized(String)
    case server(status: Int, message: String)
    case decoding
    case invalidResponse

    var errorDescription: String? {
        switch self {
        case .network(let message), .unauthorized(let message):
            return message
        case .server(_, let message):
            return message
        case .decoding:
            return "The server sent a response the app could not read."
        case .invalidResponse:
            return "The server sent an invalid response."
        }
    }
}
