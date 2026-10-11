# Municipal Elections Companion (iOS app)

Part of COMP4977. API documentation: [../docs/api.md](../docs/api.md).

SwiftUI, MVVM, async/await, `URLSession`, `Codable`, `NavigationStack`. Project:
`MunicipalElectionsCompanion/MunicipalElectionsCompanion.xcodeproj` (Xcode, iOS simulator).

Run: open the project in Xcode, choose an iPhone simulator, press Run. The app calls the deployed
Azure API. To use a local API instead, add the environment variable `API_BASE_URL`
(for example `http://localhost:5276`) under Product > Scheme > Edit Scheme > Run > Arguments.

Screens: Log In, Create Account, Municipalities (logo, name, election date), Candidates (photo, name,
position, municipality; search by name and filter by municipality/position), Candidate Details (biography,
website, email, Add to / Remove from Voting Guide), My Voting Guide (view, swipe to remove) and Profile
(logged-in user, log out).

To try it, create an account on the Create Account screen. The login token is kept in the Keychain.

```
ios/MunicipalElectionsCompanion/MunicipalElectionsCompanion/
  Models/        Codable models (Municipality, Position, Candidate, SavedCandidate, auth)
  Services/      APIClient (URLSession + async/await), Session, TokenStore (Keychain), Validation
  ViewModels/    one @Observable view model per screen
  Views/         SwiftUI screens and small components
```

Form checks: required fields, valid email format, password of at least 6 characters, matching password
confirmation. Server and network errors are shown as readable messages; an expired token logs the user out.
