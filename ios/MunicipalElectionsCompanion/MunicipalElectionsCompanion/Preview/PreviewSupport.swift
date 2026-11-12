#if DEBUG
import Foundation

// Sample data and an API that answers from it, so the SwiftUI previews work without the server.

enum PreviewData {
    private static let site = "https://municipalelections-api-mskim9097-edg2grfychegfwes.canadacentral-01.azurewebsites.net"

    static let municipalitiesJSON = """
    [
      {"municipalityId": 1, "name": "Pitt Meadows", "province": "British Columbia", "electionDate": "2026-10-17",
       "description": "A small city on the Fraser River.", "logoUrl": "\(site)/images/logos/pitt-meadows.png"},
      {"municipalityId": 2, "name": "Maple Ridge", "province": "British Columbia", "electionDate": "2026-10-17",
       "description": "A city in Metro Vancouver.", "logoUrl": "\(site)/images/logos/maple-ridge.png"}
    ]
    """

    static let positionsJSON = """
    [
      {"positionId": 1, "name": "Mayor", "seatsAvailable": 1, "municipalityId": 1, "municipalityName": "Pitt Meadows"},
      {"positionId": 2, "name": "Councillor", "seatsAvailable": 6, "municipalityId": 1, "municipalityName": "Pitt Meadows"},
      {"positionId": 3, "name": "School Trustee", "seatsAvailable": 2, "municipalityId": 1, "municipalityName": "Pitt Meadows"},
      {"positionId": 4, "name": "Mayor", "seatsAvailable": 1, "municipalityId": 2, "municipalityName": "Maple Ridge"}
    ]
    """

    static let candidatesJSON = """
    [
      \(candidateJSON(1, "Alana Whitfield", 1, "Mayor", 1, "Pitt Meadows", "c01")),
      \(candidateJSON(6, "Gordon Lyall", 2, "Councillor", 1, "Pitt Meadows", "c06")),
      \(candidateJSON(8, "Daniel Cho", 3, "School Trustee", 1, "Pitt Meadows", "c08")),
      \(candidateJSON(10, "Hamid Karimi", 4, "Mayor", 2, "Maple Ridge", "c10"))
    ]
    """

    static let savedCandidatesJSON = """
    [
      {"savedCandidateId": 1, "dateSaved": "2026-11-12T18:00:00Z",
       "candidate": \(candidateJSON(1, "Alana Whitfield", 1, "Mayor", 1, "Pitt Meadows", "c01"))},
      {"savedCandidateId": 2, "dateSaved": "2026-11-12T18:05:00Z",
       "candidate": \(candidateJSON(6, "Gordon Lyall", 2, "Councillor", 1, "Pitt Meadows", "c06"))}
    ]
    """

    static let userJSON = #"{"name": "Minsu Kim", "email": "minsu@example.com"}"#

    private static func candidateJSON(_ id: Int, _ name: String, _ positionId: Int, _ position: String,
                                      _ municipalityId: Int, _ municipality: String, _ photo: String) -> String {
        """
        {"candidateId": \(id), "name": "\(name)",
         "biography": "Long-time resident who wants to keep the city affordable, safe and green.",
         "photoUrl": "\(site)/images/candidates/\(photo).png", "website": "https://example.com", "email": "candidate@example.com",
         "positionId": \(positionId), "positionName": "\(position)",
         "municipalityId": \(municipalityId), "municipalityName": "\(municipality)"}
        """
    }

    private static func decode<T: Decodable>(_ json: String) -> T {
        try! APIDate.makeDecoder().decode(T.self, from: Data(json.utf8))
    }

    static let municipalities: [Municipality] = decode(municipalitiesJSON)
    static let candidates: [Candidate] = decode(candidatesJSON)
    static var municipality: Municipality { municipalities[0] }
    static var candidate: Candidate { candidates[0] }
}

// Answers the API requests of the previews with the sample data above.
final class PreviewURLProtocol: URLProtocol {
    override class func canInit(with request: URLRequest) -> Bool { true }
    override class func canonicalRequest(for request: URLRequest) -> URLRequest { request }

    override func startLoading() {
        let path = request.url?.path ?? ""
        let body: String? = switch path {
        case "/api/municipalities": PreviewData.municipalitiesJSON
        case "/api/positions": PreviewData.positionsJSON
        case "/api/candidates", "/api/candidates/search": PreviewData.candidatesJSON
        case "/api/savedcandidates": PreviewData.savedCandidatesJSON
        case "/api/auth/me": PreviewData.userJSON
        default: nil
        }
        let response = HTTPURLResponse(url: request.url!, statusCode: body == nil ? 404 : 200, httpVersion: nil,
                                       headerFields: ["Content-Type": "application/json"])!
        client?.urlProtocol(self, didReceive: response, cacheStoragePolicy: .notAllowed)
        client?.urlProtocol(self, didLoad: Data((body ?? "{}").utf8))
        client?.urlProtocolDidFinishLoading(self)
    }

    override func stopLoading() {}
}

extension APIClient {
    static var preview: APIClient {
        let configuration = URLSessionConfiguration.ephemeral
        configuration.protocolClasses = [PreviewURLProtocol.self]
        return APIClient(baseURL: URL(string: "https://preview.invalid")!, urlSession: URLSession(configuration: configuration))
    }
}

extension Session {
    // A logged-in user, or nobody when `loggedIn` is false.
    @MainActor
    static func preview(loggedIn: Bool = true) -> Session {
        Session(api: .preview,
                token: loggedIn ? "preview-token" : nil,
                user: loggedIn ? try? APIDate.makeDecoder().decode(UserResponse.self, from: Data(PreviewData.userJSON.utf8)) : nil)
    }
}
#endif
