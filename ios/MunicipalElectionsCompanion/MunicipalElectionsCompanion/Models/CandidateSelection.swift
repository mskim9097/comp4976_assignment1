import Foundation

// One document of the Firestore collection "CandidateSelections".
// `city` repeats `municipalityName`; the queries filter on it ("userEmail" + "city").
struct CandidateSelection: Codable, Identifiable, Hashable {
    var id: String
    var userEmail: String
    var candidateId: Int
    var candidateName: String
    var candidatePhotoUrl: String?
    var positionId: Int
    var positionName: String
    var municipalityId: Int
    var municipalityName: String
    var city: String
    var notes: String
    var dateCreated: Date
    var dateModified: Date

    var photoURL: URL? { candidatePhotoUrl.flatMap(URL.init(string:)) }

    // The id is built from the candidate and the user, so one candidate can be saved only once per user.
    // Firestore security rules check the same format.
    static func documentId(candidateId: Int, userEmail: String) -> String {
        "\(candidateId)_\(userEmail)"
    }

    init(candidate: Candidate, userEmail: String, notes: String, now: Date = .now) {
        self.id = Self.documentId(candidateId: candidate.candidateId, userEmail: userEmail)
        self.userEmail = userEmail
        self.candidateId = candidate.candidateId
        self.candidateName = candidate.name
        self.candidatePhotoUrl = candidate.photoUrl?.absoluteString
        self.positionId = candidate.positionId
        self.positionName = candidate.positionName
        self.municipalityId = candidate.municipalityId
        self.municipalityName = candidate.municipalityName
        self.city = candidate.municipalityName
        self.notes = notes
        self.dateCreated = now
        self.dateModified = now
    }
}
