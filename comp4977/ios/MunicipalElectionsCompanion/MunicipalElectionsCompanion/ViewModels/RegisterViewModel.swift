import Foundation
import Observation

@MainActor
@Observable
final class RegisterViewModel {
    var name = ""
    var email = ""
    var password = ""
    var confirmPassword = ""
    var isLoading = false
    var errorMessage: String?
    private var showFieldErrors = false

    var nameError: String? { showFieldErrors ? Validation.nameError(name) : nil }
    var emailError: String? { showFieldErrors ? Validation.emailError(email) : nil }
    var passwordError: String? { showFieldErrors ? Validation.passwordError(password) : nil }
    var confirmError: String? { showFieldErrors ? Validation.confirmError(password, confirmPassword) : nil }

    func submit(session: Session) async {
        showFieldErrors = true
        errorMessage = nil
        guard Validation.nameError(name) == nil,
              Validation.emailError(email) == nil,
              Validation.passwordError(password) == nil,
              Validation.confirmError(password, confirmPassword) == nil else {
            return
        }

        isLoading = true
        defer { isLoading = false }
        do {
            try await session.register(name: name, email: email, password: password)
        } catch {
            errorMessage = session.message(for: error)
        }
    }
}
