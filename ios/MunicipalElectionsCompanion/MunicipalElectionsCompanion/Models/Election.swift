import Foundation

// Answer of GET /api/elections?city=...: the election date of the city and its elected offices.
struct Election: Codable, Hashable {
    let municipalityId: Int
    let city: String
    let slug: String
    let electionDate: Date
    let province: String
    let offices: [Position]

    var electionDateText: String { APIDate.displayText(electionDate) }
}
