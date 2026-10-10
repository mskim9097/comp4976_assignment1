import Foundation

// Thin async/await wrapper around URLSession for the Municipal Elections API.
final class APIClient {
    private let baseURL: URL
    private let urlSession: URLSession
    private let decoder = APIDate.makeDecoder()

    init(baseURL: URL = APIConfig.baseURL, urlSession: URLSession = .shared) {
        self.baseURL = baseURL
        self.urlSession = urlSession
    }

    func get<T: Decodable>(_ path: String, query: [String: String?] = [:], token: String? = nil) async throws -> T {
        let data = try await send(method: "GET", path: path, query: query, body: nil, token: token)
        return try decode(data)
    }

    func post<T: Decodable>(_ path: String, body: any Encodable, token: String? = nil) async throws -> T {
        let data = try await send(method: "POST", path: path, query: [:], body: body, token: token)
        return try decode(data)
    }

    func delete(_ path: String, token: String) async throws {
        _ = try await send(method: "DELETE", path: path, query: [:], body: nil, token: token)
    }

    // MARK: - Private

    private func send(method: String, path: String, query: [String: String?], body: (any Encodable)?, token: String?) async throws -> Data {
        var components = URLComponents(url: baseURL.appendingPathComponent(path), resolvingAgainstBaseURL: false)
        let items = query
            .compactMap { key, value -> URLQueryItem? in
                guard let value, !value.isEmpty else { return nil }
                return URLQueryItem(name: key, value: value)
            }
            .sorted { $0.name < $1.name }
        if !items.isEmpty {
            components?.queryItems = items
        }
        guard let url = components?.url else {
            throw APIError.invalidResponse
        }

        var request = URLRequest(url: url)
        request.httpMethod = method
        request.timeoutInterval = 60 // the free Azure plan can take a while to wake up
        request.setValue("application/json", forHTTPHeaderField: "Accept")
        if let token {
            request.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
        }
        if let body {
            request.setValue("application/json", forHTTPHeaderField: "Content-Type")
            request.httpBody = try JSONEncoder().encode(body)
        }

        let data: Data
        let response: URLResponse
        do {
            (data, response) = try await urlSession.data(for: request)
        } catch let error as URLError {
            throw map(error)
        }

        guard let http = response as? HTTPURLResponse else {
            throw APIError.invalidResponse
        }
        switch http.statusCode {
        case 200..<300:
            return data
        case 401:
            throw APIError.unauthorized(message(from: data) ?? "Please log in again.")
        default:
            throw APIError.server(status: http.statusCode, message: message(from: data) ?? "Server error (\(http.statusCode)).")
        }
    }

    private func decode<T: Decodable>(_ data: Data) throws -> T {
        do {
            return try decoder.decode(T.self, from: data)
        } catch {
            throw APIError.decoding
        }
    }

    private func map(_ error: URLError) -> Error {
        switch error.code {
        case .cancelled:
            return CancellationError()
        case .notConnectedToInternet, .networkConnectionLost:
            return APIError.network("No internet connection. Check your network and try again.")
        case .timedOut:
            return APIError.network("The server took too long to respond. Please try again.")
        default:
            return APIError.network("Could not reach the server. Please try again.")
        }
    }

    // Turns an ASP.NET problem-details body into one readable message.
    private func message(from data: Data) -> String? {
        guard let problem = try? JSONDecoder().decode(ProblemDetails.self, from: data) else {
            return nil
        }
        if let errors = problem.errors {
            var seen = Set<String>()
            let lines = errors.keys.sorted().flatMap { errors[$0] ?? [] }.filter { seen.insert($0).inserted }
            if !lines.isEmpty {
                return lines.joined(separator: "\n")
            }
        }
        return problem.title
    }
}
