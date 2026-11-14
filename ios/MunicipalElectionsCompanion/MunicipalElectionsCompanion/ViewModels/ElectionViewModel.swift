import Foundation
import Observation

// Election date (GET /api/elections?city=...) and offices (GET /api/offices?city=...) of the user's city.
@MainActor
@Observable
final class ElectionViewModel {
    private(set) var election: Election?
    private(set) var offices: [Position] = []
    private(set) var isLoading = false
    var errorMessage: String?

    func clear() {
        election = nil
        offices = []
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
            async let loadedElection = session.services.election.election(city: city)
            async let loadedOffices = session.services.election.offices(city: city)
            let (newElection, newOffices) = try await (loadedElection, loadedOffices)
            guard city == session.city else { return } // the user changed the city while this was loading
            election = newElection
            offices = newOffices
            errorMessage = nil
        } catch {
            errorMessage = session.message(for: error) ?? errorMessage
        }
    }
}
