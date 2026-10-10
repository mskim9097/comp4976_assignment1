import Foundation

struct SavedCandidate: Codable, Identifiable, Hashable {
    let savedCandidateId: Int
    let dateSaved: Date
    let candidate: Candidate

    var id: Int { savedCandidateId }
}

struct SaveCandidateRequest: Encodable {
    let candidateId: Int
}
