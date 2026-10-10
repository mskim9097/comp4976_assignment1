import SwiftUI

// Shows the login flow until the user has a token, then the main tabs.
struct RootView: View {
    @Environment(Session.self) private var session

    var body: some View {
        if session.isAuthenticated {
            MainTabView()
        } else {
            NavigationStack { LoginView() }
        }
    }
}
