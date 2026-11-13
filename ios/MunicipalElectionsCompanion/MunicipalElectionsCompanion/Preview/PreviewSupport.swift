#if DEBUG
import Foundation

// Sample data and fake services, so the SwiftUI previews work without the server and without Firebase.

enum PreviewData {
    private static let site = "https://municipalelections-api-mskim9097-edg2grfychegfwes.canadacentral-01.azurewebsites.net"

    static let municipalitiesJSON = """
    [
      {"municipalityId": 1, "name": "Burnaby", "slug": "burnaby", "province": "British Columbia", "electionDate": "2026-10-17",
       "description": "A large city in Metro Vancouver.", "logoUrl": "\(site)/images/logos/burnaby.png"},
      {"municipalityId": 2, "name": "Surrey", "slug": "surrey", "province": "British Columbia", "electionDate": "2026-10-17",
       "description": "One of the fastest growing cities in British Columbia.", "logoUrl": "\(site)/images/logos/surrey.png"}
    ]
    """

    static let officesJSON = """
    [
      {"positionId": 4, "name": "Mayor", "seatsAvailable": 1, "municipalityId": 1, "municipalityName": "Burnaby"},
      {"positionId": 5, "name": "Councillor", "seatsAvailable": 8, "municipalityId": 1, "municipalityName": "Burnaby"},
      {"positionId": 6, "name": "School Trustee", "seatsAvailable": 2, "municipalityId": 1, "municipalityName": "Burnaby"}
    ]
    """

    static let electionJSON = """
    {"municipalityId": 1, "city": "Burnaby", "slug": "burnaby", "electionDate": "2026-10-17",
     "province": "British Columbia", "offices": \(officesJSON)}
    """

    static let candidatesJSON = """
    [
      \(candidateJSON(1, "Alana Whitfield", 4, "Mayor", "c01")),
      \(candidateJSON(6, "Gordon Lyall", 5, "Councillor", "c06")),
      \(candidateJSON(8, "Daniel Cho", 6, "School Trustee", "c08")),
      \(candidateJSON(10, "Mei-Ling Tran", 4, "Mayor", "c10"))
    ]
    """

    static let userJSON = #"{"name": "Minsu Kim", "email": "minsu@example.com"}"#

    private static func candidateJSON(_ id: Int, _ name: String, _ positionId: Int, _ position: String, _ photo: String) -> String {
        """
        {"candidateId": \(id), "name": "\(name)",
         "biography": "Long-time resident who wants to keep the city affordable, safe and green.",
         "photoUrl": "\(site)/images/candidates/\(photo).png", "website": "https://example.com", "email": "candidate@example.com",
         "positionId": \(positionId), "positionName": "\(position)",
         "municipalityId": 1, "municipalityName": "Burnaby"}
        """
    }

    private static func decode<T: Decodable>(_ json: String) -> T {
        try! APIDate.makeDecoder().decode(T.self, from: Data(json.utf8))
    }

    static let municipalities: [Municipality] = decode(municipalitiesJSON)
    static let candidates: [Candidate] = decode(candidatesJSON)
    static let election: Election = decode(electionJSON)
    static var candidate: Candidate { candidates[0] }

    static let selections: [CandidateSelection] = [
        {
            var selection = CandidateSelection(candidate: candidates[0], userEmail: "minsu@example.com",
                                               notes: "Strong plan for transit and housing.",
                                               now: Date(timeIntervalSince1970: 1_792_000_000))
            selection.notes = "Strong plan for transit and housing."
            return selection
        }(),
        CandidateSelection(candidate: candidates[1], userEmail: "minsu@example.com", notes: "",
                           now: Date(timeIntervalSince1970: 1_792_100_000))
    ]
}

// Answers the Web API requests of the previews with the sample data above.
final class PreviewURLProtocol: URLProtocol {
    override class func canInit(with request: URLRequest) -> Bool { true }
    override class func canonicalRequest(for request: URLRequest) -> URLRequest { request }

    override func startLoading() {
        let path = request.url?.path ?? ""
        let body: String? = switch path {
        case "/api/municipalities": PreviewData.municipalitiesJSON
        case "/api/offices": PreviewData.officesJSON
        case "/api/elections": PreviewData.electionJSON
        case "/api/candidates", "/api/candidates/search": PreviewData.candidatesJSON
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

// Firebase stand-ins that keep everything in memory.
final class InMemoryCloud: CloudAuthenticating {
    var currentEmail: String?
    init(email: String? = "minsu@example.com") { currentEmail = email }
    func signInOrCreate(email: String, password: String) async throws { currentEmail = email.lowercased() }
    func signOut() { currentEmail = nil }
}

final class InMemoryProfiles: ProfileStoring {
    var stored: UserProfile?
    init(stored: UserProfile?) { self.stored = stored }
    func profile(email: String) async throws -> UserProfile? { stored }
    func save(_ profile: UserProfile) async throws { stored = profile }
    func markSynced(email: String, at date: Date) async { stored?.lastSync = date }
}

final class InMemorySelections: SelectionStoring {
    var items: [CandidateSelection]
    init(items: [CandidateSelection]) { self.items = items }
    func selections(email: String, city: String) async throws -> [CandidateSelection] {
        items.filter { $0.userEmail == email && $0.city == city }
    }
    func create(_ selection: CandidateSelection) async throws {
        if items.contains(where: { $0.id == selection.id }) { throw SelectionError.duplicate(selection.candidateName) }
        items.append(selection)
    }
    func update(_ selection: CandidateSelection) async throws {
        if let index = items.firstIndex(where: { $0.id == selection.id }) { items[index] = selection }
    }
    func delete(id: String) async throws { items.removeAll { $0.id == id } }
}

extension Session {
    // A logged-in user (with the given city, or none to see the first-login screen), or nobody when `loggedIn` is false.
    @MainActor
    static func preview(loggedIn: Bool = true, city: String? = "Burnaby") -> Session {
        let profile = UserProfile(email: "minsu@example.com", name: "Minsu Kim", city: city, municipalityId: city == nil ? nil : 1,
                                  lastSync: Date(timeIntervalSince1970: 1_792_100_000), dateCreated: .now)
        let services = AppServices(
            auth: AuthAPI(client: .preview), election: ElectionAPI(client: .preview),
            cloud: InMemoryCloud(), profiles: InMemoryProfiles(stored: profile),
            selections: InMemorySelections(items: PreviewData.selections))
        return Session(services: services,
                       token: loggedIn ? "preview-token" : nil,
                       user: loggedIn ? try? APIDate.makeDecoder().decode(UserResponse.self, from: Data(PreviewData.userJSON.utf8)) : nil,
                       profile: loggedIn ? profile : nil)
    }
}
#endif
