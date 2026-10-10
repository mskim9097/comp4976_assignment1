import Foundation
import Observation

@MainActor
@Observable
final class CandidateDetailViewModel {
    let candidate: Candidate
    private(set) var savedCandidateId: Int?
    private(set) var isWorking = false
    var errorMessage: String?

    var isSaved: Bool { savedCandidateId != nil }

    init(candidate: Candidate) {
        self.candidate = candidate
    }

    func checkSaved(session: Session) async {
        do {
            let saved: [SavedCandidate] = try await session.api.get("api/savedcandidates", token: session.token)
            savedCandidateId = saved.first { $0.candidate.candidateId == candidate.candidateId }?.savedCandidateId
        } catch {
            errorMessage = session.message(for: error) ?? errorMessage
        }
    }

    func toggleSaved(session: Session) async {
        isWorking = true
        errorMessage = nil
        defer { isWorking = false }
        do {
            if let savedCandidateId {
                try await session.api.delete("api/savedcandidates/\(savedCandidateId)", token: session.token ?? "")
                self.savedCandidateId = nil
            } else {
                let saved: SavedCandidate = try await session.api.post(
                    "api/savedcandidates", body: SaveCandidateRequest(candidateId: candidate.candidateId), token: session.token)
                savedCandidateId = saved.savedCandidateId
            }
        } catch APIError.server(let status, _) where status == 409 {
            await checkSaved(session: session) // already in the guide (for example saved on another device)
        } catch {
            errorMessage = session.message(for: error)
        }
    }
}
