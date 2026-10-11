import Foundation

struct Candidate: Codable, Identifiable, Hashable {
    let candidateId: Int
    let name: String
    let biography: String?
    let photoUrl: URL?
    let website: String?
    let email: String?
    let positionId: Int
    let positionName: String
    let municipalityId: Int
    let municipalityName: String

    var id: Int { candidateId }
}
