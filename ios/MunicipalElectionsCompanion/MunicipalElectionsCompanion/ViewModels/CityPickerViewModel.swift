import Foundation
import Observation

// Lets the user choose the municipality they live in (first login, and later from the profile).
@MainActor
@Observable
final class CityPickerViewModel {
    private(set) var municipalities: [Municipality] = []
    private(set) var isLoading = false
    private(set) var isSaving = false
    var errorMessage: String?

    func load(session: Session) async {
        isLoading = true
        defer { isLoading = false }
        do {
            municipalities = try await session.services.election.municipalities()
            errorMessage = nil
        } catch {
            errorMessage = session.message(for: error) ?? errorMessage
        }
    }

    // Saves the choice to Firebase. Returns true when it worked.
    func choose(_ municipality: Municipality, session: Session) async -> Bool {
        isSaving = true
        defer { isSaving = false }
        do {
            try await session.setCity(municipality)
            errorMessage = nil
            return true
        } catch {
            errorMessage = session.message(for: error)
            return false
        }
    }
}
