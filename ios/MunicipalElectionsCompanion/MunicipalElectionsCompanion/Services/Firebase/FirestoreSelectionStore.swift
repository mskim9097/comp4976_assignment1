import Foundation
import FirebaseFirestore

// Collection "CandidateSelections": CRUD for the candidates a user saved. Security rules (firebase/firestore.rules)
// let a user read and change only the documents whose userEmail is their own.
struct FirestoreSelectionStore: SelectionStoring {
    private var collection: CollectionReference { Firestore.firestore().collection("CandidateSelections") }

    func selections(email: String, city: String) async throws -> [CandidateSelection] {
        try await FirebaseErrorMapper.run {
            let snapshot = try await collection
                .whereField("userEmail", isEqualTo: email)
                .whereField("city", isEqualTo: city)
                .getDocuments()
            return try snapshot.documents.map { try $0.data(as: CandidateSelection.self) }
        }
    }

    func create(_ selection: CandidateSelection) async throws {
        try await FirebaseErrorMapper.run {
            let document = collection.document(selection.id)
            if try await document.getDocument().exists {
                throw SelectionError.duplicate(selection.candidateName)
            }
            try await document.setData(Firestore.Encoder().encode(selection))
        }
    }

    func update(_ selection: CandidateSelection) async throws {
        try await FirebaseErrorMapper.run {
            try await collection.document(selection.id).setData(Firestore.Encoder().encode(selection))
        }
    }

    func delete(id: String) async throws {
        try await FirebaseErrorMapper.run { try await collection.document(id).delete() }
    }
}
