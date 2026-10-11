import Foundation
import Observation

@MainActor
@Observable
final class MunicipalitiesViewModel {
    private(set) var municipalities: [Municipality] = []
    private(set) var isLoading = false
    var errorMessage: String?

    func load(session: Session) async {
        isLoading = true
        defer { isLoading = false }
        do {
            municipalities = try await session.api.get("api/municipalities")
            errorMessage = nil
        } catch {
            errorMessage = session.message(for: error) ?? errorMessage
        }
    }
}
