import Foundation
import Observation

@MainActor
@Observable
final class CandidatesViewModel {
    var searchText = ""
    var selectedMunicipalityId: Int?
    var selectedPositionId: Int?

    private(set) var candidates: [Candidate] = []
    private(set) var municipalities: [Municipality] = []
    private(set) var positions: [Position] = []
    private(set) var isLoading = false
    var errorMessage: String?

    init(municipalityId: Int? = nil) {
        selectedMunicipalityId = municipalityId
    }

    // Changes whenever the search text or a filter changes, so the view can reload.
    var queryKey: String {
        "\(searchText.trimmingCharacters(in: .whitespaces))|\(selectedMunicipalityId ?? -1)|\(selectedPositionId ?? -1)"
    }

    var hasActiveFilters: Bool {
        selectedMunicipalityId != nil || selectedPositionId != nil
    }

    // Positions offered by the filter: only the chosen municipality's, or all of them.
    var visiblePositions: [Position] {
        guard let selectedMunicipalityId else { return positions }
        return positions.filter { $0.municipalityId == selectedMunicipalityId }
    }

    func positionTitle(_ position: Position) -> String {
        selectedMunicipalityId == nil ? "\(position.name) - \(position.municipalityName)" : position.name
    }

    func municipalityChanged() {
        if let selectedPositionId, !visiblePositions.contains(where: { $0.positionId == selectedPositionId }) {
            self.selectedPositionId = nil
        }
    }

    func clearFilters() {
        selectedMunicipalityId = nil
        selectedPositionId = nil
    }

    func loadFilters(session: Session) async {
        do {
            async let loadedMunicipalities: [Municipality] = session.api.get("api/municipalities")
            async let loadedPositions: [Position] = session.api.get("api/positions")
            municipalities = try await loadedMunicipalities
            positions = try await loadedPositions
        } catch {
            errorMessage = session.message(for: error) ?? errorMessage
        }
    }

    func loadCandidates(session: Session) async {
        isLoading = true
        defer { isLoading = false }

        let name = searchText.trimmingCharacters(in: .whitespaces)
        let query: [String: String?] = [
            "name": name.isEmpty ? nil : name,
            "municipalityId": selectedMunicipalityId.map(String.init),
            "positionId": selectedPositionId.map(String.init)
        ]
        do {
            // Name searches use the search endpoint; plain browsing and filtering use the list endpoint.
            let path = name.isEmpty ? "api/candidates" : "api/candidates/search"
            candidates = try await session.api.get(path, query: query)
            errorMessage = nil
        } catch {
            if let message = session.message(for: error) {
                errorMessage = message
            }
        }
    }
}
