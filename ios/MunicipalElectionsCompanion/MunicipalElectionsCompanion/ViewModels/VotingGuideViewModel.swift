import Foundation
import Observation

@MainActor
@Observable
final class VotingGuideViewModel {
    private(set) var items: [SavedCandidate] = []
    private(set) var isLoading = false
    var errorMessage: String?

    func load(session: Session) async {
        isLoading = true
        defer { isLoading = false }
        do {
            items = try await session.api.get("api/savedcandidates", token: session.token)
            errorMessage = nil
        } catch {
            errorMessage = session.message(for: error) ?? errorMessage
        }
    }

    func remove(at offsets: IndexSet, session: Session) async {
        let removed = offsets.map { items[$0] }
        for item in removed {
            do {
                try await session.api.delete("api/savedcandidates/\(item.savedCandidateId)", token: session.token ?? "")
                items.removeAll { $0.savedCandidateId == item.savedCandidateId }
            } catch {
                errorMessage = session.message(for: error)
                return
            }
        }
    }
}
