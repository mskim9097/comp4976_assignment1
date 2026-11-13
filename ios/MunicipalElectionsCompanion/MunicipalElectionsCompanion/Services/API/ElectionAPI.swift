import Foundation

// Election data of the Web API. Every call that returns election data carries the city (?city=Burnaby),
// so the API only answers with that municipality's records.
struct ElectionAPI {
    let client: APIClient

    // The list of municipalities is only used to choose the city; it contains no candidates.
    func municipalities() async throws -> [Municipality] {
        try await client.get("api/municipalities")
    }

    func election(city: String) async throws -> Election {
        try await client.get("api/elections", query: ["city": city])
    }

    func offices(city: String) async throws -> [Position] {
        try await client.get("api/offices", query: ["city": city])
    }

    func candidates(city: String, name: String?, positionId: Int?) async throws -> [Candidate] {
        let query: [String: String?] = ["city": city, "name": name, "positionId": positionId.map(String.init)]
        // Name searches use the search endpoint; plain browsing and filtering use the list endpoint.
        let path = (name ?? "").isEmpty ? "api/candidates" : "api/candidates/search"
        return try await client.get(path, query: query)
    }
}
