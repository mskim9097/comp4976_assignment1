import Foundation
import Observation

// Election date and offices of the user's city (GET /api/elections?city=...).
@MainActor
@Observable
final class ElectionViewModel {
    private(set) var election: Election?
    private(set) var isLoading = false
    var errorMessage: String?

    var offices: [Position] { election?.offices ?? [] }

    func clear() {
        election = nil
        errorMessage = nil
    }

    func load(session: Session) async {
        guard let city = session.city else {
            clear()
            return
        }
        isLoading = true
        defer { isLoading = false }
        do {
            let loaded = try await session.services.election.election(city: city)
            guard city == session.city else { return } // the user changed the city while this was loading
            election = loaded
            errorMessage = nil
        } catch {
            errorMessage = session.message(for: error) ?? errorMessage
        }
    }
}
