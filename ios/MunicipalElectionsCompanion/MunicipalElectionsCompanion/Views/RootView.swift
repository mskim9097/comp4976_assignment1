import SwiftUI

// Login flow until the user has a token, then (on the first login) the choice of the municipality, then the app.
struct RootView: View {
    @Environment(Session.self) private var session
    @Environment(ElectionViewModel.self) private var election
    @Environment(SelectionsViewModel.self) private var selections

    var body: some View {
        content
            .task { await session.restore() }
            .onChange(of: session.isAuthenticated) {
                if !session.isAuthenticated {
                    election.clear()
                    selections.clear()
                }
            }
    }

    @ViewBuilder
    private var content: some View {
        if !session.isAuthenticated {
            NavigationStack { LoginView() }
        } else if session.profile == nil, let message = session.profileError {
            ContentUnavailableView {
                Label("Could not load your profile", systemImage: "icloud.slash")
            } description: {
                Text(message)
            } actions: {
                Button("Try Again") {
                    Task { await session.loadProfile() }
                }
                Button("Log Out", role: .destructive) {
                    session.logout()
                }
            }
        } else if session.profile == nil {
            ProgressView("Loading your profile...")
        } else if session.needsCity {
            CityPickerView(mode: .firstLogin)
        } else {
            AppShell()
        }
    }
}

#Preview("Logged out") {
    RootView()
        .environment(Session.preview(loggedIn: false))
        .environment(ElectionViewModel())
        .environment(SelectionsViewModel())
}

#Preview("Logged in") {
    RootView()
        .environment(Session.preview())
        .environment(ElectionViewModel())
        .environment(SelectionsViewModel())
}

#Preview("First login: choose city") {
    RootView()
        .environment(Session.preview(city: nil))
        .environment(ElectionViewModel())
        .environment(SelectionsViewModel())
}
