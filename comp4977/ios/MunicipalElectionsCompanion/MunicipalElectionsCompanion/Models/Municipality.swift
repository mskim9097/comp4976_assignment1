import Foundation

struct Municipality: Codable, Identifiable, Hashable {
    let municipalityId: Int
    let name: String
    let province: String
    let electionDate: Date
    let description: String?
    let logoUrl: URL?

    var id: Int { municipalityId }
    var electionDateText: String { APIDate.displayText(electionDate) }
}
