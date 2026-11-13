import Foundation

// Client-side form checks shown before anything is sent to the API.
enum Validation {
    static let minimumPasswordLength = 6

    static func nameError(_ value: String) -> String? {
        value.trimmingCharacters(in: .whitespaces).isEmpty ? "Name is required." : nil
    }

    static func emailError(_ value: String) -> String? {
        let email = value.trimmingCharacters(in: .whitespaces)
        if email.isEmpty {
            return "Email is required."
        }
        let pattern = #"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$"#
        return email.range(of: pattern, options: .regularExpression) == nil ? "Enter a valid email address." : nil
    }

    static func passwordError(_ value: String) -> String? {
        if value.isEmpty {
            return "Password is required."
        }
        return value.count < minimumPasswordLength ? "Password must be at least \(minimumPasswordLength) characters." : nil
    }

    static func confirmError(_ password: String, _ confirm: String) -> String? {
        confirm != password ? "Passwords do not match." : nil
    }
}
