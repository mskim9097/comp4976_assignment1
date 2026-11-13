import SwiftUI

// Account, municipality (changeable), cloud sync information and log out.
struct ProfileView: View {
    @Environment(Session.self) private var session
    @Environment(SelectionsViewModel.self) private var selections
    @State private var showLogoutConfirmation = false
    @State private var showCityPicker = false

    var body: some View {
        Form {
            Section("Account") {
                LabeledContent("Name", value: session.user?.name ?? "-")
                LabeledContent("Email", value: session.email ?? "-")
            }

            Section("Municipality") {
                LabeledContent("City", value: session.city ?? "-")
                Button("Change Municipality") {
                    showCityPicker = true
                }
            }

            Section("Cloud sync") {
                LabeledContent("Saved selections", value: "\(selections.total)")
                LabeledContent("Last synchronization") {
                    if let date = session.profile?.lastSync {
                        Text(date.formatted(date: .abbreviated, time: .shortened))
                    } else {
                        Text("Not yet")
                    }
                }
            }

            Section {
                Button("Log Out", role: .destructive) {
                    showLogoutConfirmation = true
                }
            }
        }
        .navigationTitle("Profile")
        .navigationSubtitle(session.city.map { "City: \($0)" } ?? "")
        .sheet(isPresented: $showCityPicker) {
            CityPickerView(mode: .change)
        }
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

#Preview {
    NavigationStack { ProfileView() }
        .environment(Session.preview())
        .environment(SelectionsViewModel())
}
