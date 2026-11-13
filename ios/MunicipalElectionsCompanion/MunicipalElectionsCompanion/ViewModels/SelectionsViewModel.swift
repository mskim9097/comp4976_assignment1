import Foundation
import Observation

// The user's candidate selections of the current city (Firestore "CandidateSelections"): read, create, update, delete.
// Shared by the candidate details, the My Candidate Selections screen and the profile.
@MainActor
@Observable
final class SelectionsViewModel {
    private(set) var items: [CandidateSelection] = []
    private(set) var isLoading = false
    var errorMessage: String?

    var total: Int { items.count }

    func selection(for candidate: Candidate) -> CandidateSelection? {
        items.first { $0.candidateId == candidate.candidateId }
    }

    func count(positionId: Int) -> Int {
        items.filter { $0.positionId == positionId }.count
    }

    // Forgets the selections of the previous city (and of the previous user).
    func clear() {
        items = []
        errorMessage = nil
    }

    func load(session: Session) async {
        guard let email = session.email, let city = session.city else {
            clear()
            return
        }
        isLoading = true
        defer { isLoading = false }
        do {
            let loaded = try await session.services.selections.selections(email: email, city: city)
            guard city == session.city else { return } // the user changed the city while this was loading
            items = loaded.sorted { $0.dateCreated > $1.dateCreated }
            errorMessage = nil
            await session.recordSync()
        } catch {
            errorMessage = session.message(for: error) ?? errorMessage
        }
    }

    // Each of these returns an error text for the editor to show, or nil when it worked.

    func add(candidate: Candidate, notes: String, offices: [Position], session: Session) async -> String? {
        guard let email = session.email, let city = session.city else {
            return "Choose your municipality first."
        }
        let cleaned = notes.trimmingCharacters(in: .whitespacesAndNewlines)
        do {
            try SelectionRules.validate(candidate: candidate, notes: cleaned, city: city, userEmail: email,
                                        existing: items, offices: offices, editing: false)
            let selection = CandidateSelection(candidate: candidate, userEmail: email, notes: cleaned)
            try await session.services.selections.create(selection)
            items.insert(selection, at: 0)
            return nil
        } catch {
            return session.message(for: error) ?? ""
        }
    }

    func update(_ selection: CandidateSelection, notes: String, session: Session) async -> String? {
        let cleaned = notes.trimmingCharacters(in: .whitespacesAndNewlines)
        do {
            if notes.count > SelectionRules.maxNotesLength {
                throw SelectionError.notesTooLong(SelectionRules.maxNotesLength)
            }
            var changed = selection
            changed.notes = cleaned
            changed.dateModified = .now
            try await session.services.selections.update(changed)
            if let index = items.firstIndex(where: { $0.id == selection.id }) {
                items[index] = changed
            }
            return nil
        } catch {
            return session.message(for: error) ?? ""
        }
    }

    func delete(_ selection: CandidateSelection, session: Session) async -> String? {
        do {
            try await session.services.selections.delete(id: selection.id)
            items.removeAll { $0.id == selection.id }
            return nil
        } catch {
            return session.message(for: error) ?? ""
        }
    }
}
