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

    // The document id holds the candidate and the user, so a second selection of the same candidate would overwrite the
    // first. The security rules refuse that (the creation date would change); only then the document is read to tell the
    // user that the candidate is already saved. Reading a document that does not exist is not allowed by the rules.
    func create(_ selection: CandidateSelection) async throws {
        let document = collection.document(selection.id)
        do {
            try await FirebaseErrorMapper.run {
                try await document.setData(Firestore.Encoder().encode(selection))
            }
        } catch let error as CloudError {
            if let existing = try? await document.getDocument(), existing.exists {
                throw SelectionError.duplicate(selection.candidateName)
            }
            throw error
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
