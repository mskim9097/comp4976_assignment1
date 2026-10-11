import Foundation
import Observation

@MainActor
@Observable
final class LoginViewModel {
    var email = ""
    var password = ""
    var isLoading = false
    var errorMessage: String?
    private var showFieldErrors = false

    var emailError: String? { showFieldErrors ? Validation.emailError(email) : nil }
    var passwordError: String? { showFieldErrors ? Validation.passwordError(password) : nil }

    func submit(session: Session) async {
        showFieldErrors = true
        errorMessage = nil
        guard Validation.emailError(email) == nil, Validation.passwordError(password) == nil else {
            return
        }

        isLoading = true
        defer { isLoading = false }
        do {
            try await session.login(email: email, password: password)
        } catch {
            errorMessage = session.message(for: error)
        }
    }
}
