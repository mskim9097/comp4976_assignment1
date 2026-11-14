# Municipal Elections Companion (iOS app)

Part of COMP4977. API documentation: [../docs/api.md](../docs/api.md).

SwiftUI, MVVM, async/await, `URLSession`, `Codable`, `NavigationStack`, Firebase Auth and Firestore. Project:
`MunicipalElectionsCompanion/MunicipalElectionsCompanion.xcodeproj` (Xcode, iOS simulator).

Run: open the project in Xcode, choose an iPhone or iPad simulator, press Run. The Firebase package is
resolved by Xcode and `GoogleService-Info.plist` of the Firebase project `mems-companion` is part of the
project. The app calls the deployed Azure API. To use a local API instead, add the environment variable
`API_BASE_URL` (for example `https://localhost:7261`) under Product > Scheme > Edit Scheme > Run > Arguments.

Screens: Log In, Create Account, Where do you live? (choose the municipality on the first login), Home
(election and offices of the city), Candidates (photo, name, position; search by name and filter by
position), Candidate Details (biography, website, email, Add / Edit / Delete Selection), My Candidate
Selections (notes, date saved, edit, delete, pull to refresh) and Profile (email, municipality, total
selections, last sync, change municipality, log out). The layout adapts to iPhone and iPad, portrait and
landscape.

To try it, create an account on the Create Account screen. The login token is kept in the Keychain. The
selected municipality and the candidate selections are stored in Firestore (collections `Users` and
`CandidateSelections`, rules in [../firebase/firestore.rules](../firebase/firestore.rules)).

```
ios/MunicipalElectionsCompanion/MunicipalElectionsCompanion/
  Models/        Codable models (Municipality, Position, Candidate, CandidateSelection, UserProfile, auth)
  Services/      API/ (APIClient, URLSession + async/await), Auth/ (Keychain token, validation),
                 Firebase/ (Firestore stores, Firebase Auth), Session, SelectionRules
  ViewModels/    one @Observable view model per screen
  Views/         SwiftUI screens (one view per file, each with a #Preview) and small components
  Preview/       Sample data and fakes for the SwiftUI previews (debug builds only)
```

Form checks: required fields, valid email format, password of at least 6 characters, matching password
confirmation. Candidate selections: no duplicates, notes up to 500 characters, only candidates of the
selected municipality and no more candidates for a position than it has seats (one mayor, for example).
Server, Firestore and network errors are shown as readable messages; an expired token logs the user out.
