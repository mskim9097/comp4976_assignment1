import Foundation
import FirebaseAuth
import FirebaseFirestore

// Friendly text for errors of Firebase Auth and Firestore (no technical details for the user).
enum FirebaseErrorMapper {
    // Runs a Firebase call and turns every failure into a `CloudError` with a friendly message
    // (`SelectionError`s of our own rules pass through unchanged). A write waits for the server's answer, so that
    // permission errors are reported; when the server cannot be reached within `timeout` the call is given up.
    static func run<T>(timeout: Duration = .seconds(15), _ body: @escaping () async throws -> T) async throws -> T {
        do {
            return try await withThrowingTaskGroup(of: T.self) { group in
                group.addTask { try await body() }
                group.addTask {
                    try await Task.sleep(for: timeout)
                    throw CloudError(message: "Could not reach the cloud storage. Check your connection and try again.")
                }
                guard let result = try await group.next() else { throw CancellationError() }
                group.cancelAll()
                return result
            }
        } catch let error as SelectionError {
            throw error
        } catch let error as CloudError {
            throw error
        } catch is CancellationError {
            throw CancellationError()
        } catch {
            let text = message(for: error)
            throw CloudError(message: text, requiresLogin: text.contains("Please log in again"))
        }
    }

    static func message(for error: Error) -> String {
        let nsError = error as NSError

        if nsError.domain == AuthErrorDomain, let code = AuthErrorCode(rawValue: nsError.code) {
            switch code {
            case .networkError:
                return "No internet connection. Check your network and try again."
            case .wrongPassword, .invalidCredential, .userNotFound:
                return "The cloud account could not be signed in with this password."
            case .tooManyRequests:
                return "Too many attempts. Please wait a moment and try again."
            case .userTokenExpired, .requiresRecentLogin, .invalidUserToken:
                return "Your cloud session has expired. Please log in again."
            default:
                return "Signing in to the cloud storage failed. Please try again."
            }
        }

        if nsError.domain == FirestoreErrorDomain, let code = FirestoreErrorCode.Code(rawValue: nsError.code) {
            switch code {
            case .unavailable, .deadlineExceeded:
                return "Could not reach the cloud storage. Check your connection and try again."
            case .permissionDenied:
                return "You are not allowed to do that."
            case .unauthenticated:
                return "Your cloud session has expired. Please log in again."
            case .notFound:
                return "That selection no longer exists."
            case .alreadyExists:
                return "That candidate is already in your selections."
            case .resourceExhausted:
                return "The cloud storage is busy. Please try again later."
            default:
                return "The cloud storage returned an unexpected answer. Please try again."
            }
        }

        if error is DecodingError {
            return "The cloud storage returned data the app could not read."
        }
        if (error as? URLError) != nil {
            return "No internet connection. Check your network and try again."
        }
        return error.localizedDescription
    }
}
