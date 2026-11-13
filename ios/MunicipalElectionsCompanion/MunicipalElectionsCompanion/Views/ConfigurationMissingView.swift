import SwiftUI

// Shown instead of the app when the Firebase configuration file was not added to the project.
struct ConfigurationMissingView: View {
    var body: some View {
        ContentUnavailableView {
            Label("Firebase is not configured", systemImage: "exclamationmark.icloud")
        } description: {
            Text("Add GoogleService-Info.plist of your Firebase project to the app target and run again.")
        }
    }
}

#Preview {
    ConfigurationMissingView()
}
