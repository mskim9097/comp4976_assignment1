import Foundation

struct Position: Codable, Identifiable, Hashable {
    let positionId: Int
    let name: String
    let seatsAvailable: Int
    let municipalityId: Int
    let municipalityName: String

    var id: Int { positionId }
}
