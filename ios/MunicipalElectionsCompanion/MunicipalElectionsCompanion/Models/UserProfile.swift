import Foundation

// One document of the Firestore collection "Users" (document id = the user's email).
// The municipality in the profile decides which election data the app loads.
struct UserProfile: Codable, Hashable {
    var email: String
    var name: String
    var city: String?
    var municipalityId: Int?
    var lastSync: Date?
    var dateCreated: Date
}
