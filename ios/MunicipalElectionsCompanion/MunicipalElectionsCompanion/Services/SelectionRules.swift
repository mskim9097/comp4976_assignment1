import Foundation

enum SelectionError: LocalizedError, Equatable {
    case duplicate(String)
    case notesTooLong(Int)
    case otherCity(String)
    case seatsFull(position: String, seats: Int)
    case missingInformation(String)

    var errorDescription: String? {
        switch self {
        case .duplicate(let name):
            return "\(name) is already in your selections."
        case .notesTooLong(let max):
            return "Notes cannot be longer than \(max) characters."
        case .otherCity(let city):
            return "You can only select candidates of \(city)."
        case .seatsFull(let position, let seats):
            return seats == 1
                ? "You can select only one \(position)."
                : "You can select at most \(seats) for \(position)."
        case .missingInformation(let what):
            return "\(what) is required."
        }
    }
}

// The rules for saving a candidate. They run before anything is sent to Firestore.
enum SelectionRules {
    static let maxNotesLength = 500

    // `existing` are the saved selections of the user in the current city, `offices` the offices of that city.
    // When `editing` is true the candidate is already saved and only the notes change.
    static func validate(candidate: Candidate, notes: String, city: String, userEmail: String,
                         existing: [CandidateSelection], offices: [Position], editing: Bool) throws {
        if userEmail.trimmingCharacters(in: .whitespaces).isEmpty {
            throw SelectionError.missingInformation("The logged-in user")
        }
        if candidate.name.trimmingCharacters(in: .whitespaces).isEmpty {
            throw SelectionError.missingInformation("The candidate")
        }
        if notes.count > maxNotesLength {
            throw SelectionError.notesTooLong(maxNotesLength)
        }
        if editing {
            return
        }
        if candidate.municipalityName.caseInsensitiveCompare(city) != .orderedSame {
            throw SelectionError.otherCity(city)
        }
        if existing.contains(where: { $0.candidateId == candidate.candidateId }) {
            throw SelectionError.duplicate(candidate.name)
        }
        if let office = offices.first(where: { $0.positionId == candidate.positionId }) {
            let used = existing.filter { $0.positionId == candidate.positionId }.count
            if used >= office.seatsAvailable {
                throw SelectionError.seatsFull(position: office.name, seats: office.seatsAvailable)
            }
        }
    }
}
