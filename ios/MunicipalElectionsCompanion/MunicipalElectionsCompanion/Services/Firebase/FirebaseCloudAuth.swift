import Foundation
import FirebaseAuth

// Firebase Authentication. The Web API stays the place where people register and log in (JWT); the app also
// signs in to Firebase with the same email and password so Firestore security rules know who is asking.
struct FirebaseCloudAuth: CloudAuthenticating {
    var currentEmail: String? { Auth.auth().currentUser?.email?.lowercased() }

    func signInOrCreate(email: String, password: String) async throws {
        try await FirebaseErrorMapper.run { try await signIn(email: email, password: password) }
    }

    private func signIn(email: String, password: String) async throws {
        do {
            try await Auth.auth().signIn(withEmail: email, password: password)
        } catch {
            let nsError = error as NSError
            guard nsError.domain == AuthErrorDomain,
                  let code = AuthErrorCode(rawValue: nsError.code),
                  code == .userNotFound || code == .invalidCredential else {
                throw error
            }
            // Unknown to Firebase (for example registered before Firebase was added): create the account.
            // If it already exists, the password simply differs and the sign-in error is reported.
            do {
                try await Auth.auth().createUser(withEmail: email, password: password)
            } catch let createError as NSError where createError.code == AuthErrorCode.emailAlreadyInUse.rawValue {
                throw error
            }
        }
    }

    func signOut() {
        try? Auth.auth().signOut()
    }
}
