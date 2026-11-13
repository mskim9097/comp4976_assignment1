import Foundation
import FirebaseFirestore

// Collection "Users": one document per user (id = email) with the chosen municipality.
struct FirestoreProfileStore: ProfileStoring {
    private var collection: CollectionReference { Firestore.firestore().collection("Users") }

    func profile(email: String) async throws -> UserProfile? {
        try await FirebaseErrorMapper.run {
            let snapshot = try await collection.document(email).getDocument()
            guard snapshot.exists else { return nil }
            return try snapshot.data(as: UserProfile.self)
        }
    }

    func save(_ profile: UserProfile) async throws {
        try await FirebaseErrorMapper.run {
            try await collection.document(profile.email).setData(Firestore.Encoder().encode(profile))
        }
    }

    func markSynced(email: String, at date: Date) async {
        try? await collection.document(email).updateData(["lastSync": Timestamp(date: date)])
    }
}
