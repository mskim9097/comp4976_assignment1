import SwiftUI

struct ProfileView: View {
    @Environment(Session.self) private var session
    @State private var showLogoutConfirmation = false

    var body: some View {
        NavigationStack {
            Form {
                Section("Logged in as") {
                    LabeledContent("Name", value: session.user?.name ?? "-")
                    LabeledContent("Email", value: session.user?.email ?? "-")
                }

                Section {
                    Button("Log Out", role: .destructive) {
                        showLogoutConfirmation = true
                    }
                }
            }
            .navigationTitle("Profile")
            .confirmationDialog("Log out of your account?", isPresented: $showLogoutConfirmation, titleVisibility: .visible) {
                Button("Log Out", role: .destructive) {
                    session.logout()
                }
            }
            .task {
                if session.user == nil {
                    await session.loadUser()
                }
            }
        }
    }
}
