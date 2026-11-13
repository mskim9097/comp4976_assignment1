import Foundation
import Observation

// The candidate directory of the user's city: name search and position filter.
@MainActor
@Observable
final class CandidatesViewModel {
    var searchText = ""
    var selectedPositionId: Int?

    private(set) var candidates: [Candidate] = []
    private(set) var isLoading = false
    var errorMessage: String?

    // Changes whenever the search text or the filter changes, so the view can reload.
    var queryKey: String {
        "\(searchText.trimmingCharacters(in: .whitespaces))|\(selectedPositionId ?? -1)"
    }

    var hasActiveFilters: Bool { selectedPositionId != nil }

    func clear() {
        candidates = []
        selectedPositionId = nil
        errorMessage = nil
    }

    func load(session: Session) async {
        guard let city = session.city else {
            candidates = []
            return
        }
        isLoading = true
        defer { isLoading = false }
        let name = searchText.trimmingCharacters(in: .whitespaces)
        do {
            let loaded = try await session.services.election.candidates(
                city: city, name: name.isEmpty ? nil : name, positionId: selectedPositionId)
            guard city == session.city else { return } // the user changed the city while this was loading
            candidates = loaded
            errorMessage = nil
        } catch {
            if let message = session.message(for: error) {
                errorMessage = message
            }
        }
    }
}
