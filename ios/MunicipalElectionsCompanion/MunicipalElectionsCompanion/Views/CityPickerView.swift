import SwiftUI

// Choose the municipality: required at the first login, changeable later from the profile.
struct CityPickerView: View {
    enum Mode { case firstLogin, change }

    @Environment(\.dismiss) private var dismiss
    @Environment(Session.self) private var session
    @State private var viewModel = CityPickerViewModel()
    @State private var pending: Municipality?

    let mode: Mode

    var body: some View {
        NavigationStack {
            content
                .navigationTitle(mode == .firstLogin ? "Where do you live?" : "Change Municipality")
                .navigationBarTitleDisplayMode(.inline)
                .toolbar {
                    if mode == .change {
                        ToolbarItem(placement: .cancellationAction) {
                            Button("Cancel") { dismiss() }
                        }
                    } else {
                        ToolbarItem(placement: .topBarTrailing) {
                            Button("Log Out", role: .destructive) { session.logout() }
                        }
                    }
                }
                .task { await viewModel.load(session: session) }
                .confirmationDialog(
                    pending.map { "Change your municipality to \($0.name)?" } ?? "",
                    isPresented: Binding(get: { pending != nil }, set: { if !$0 { pending = nil } }),
                    titleVisibility: .visible,
                    presenting: pending
                ) { municipality in
                    Button("Change to \(municipality.name)") {
                        Task { await choose(municipality) }
                    }
                } message: { _ in
                    Text("Candidates, offices and selections of \(session.city ?? "your current city") will no longer be shown.")
                }
        }
        .interactiveDismissDisabled(viewModel.isSaving)
    }

    @ViewBuilder
    private var content: some View {
        if viewModel.municipalities.isEmpty && viewModel.isLoading {
            ProgressView("Loading municipalities...")
        } else if viewModel.municipalities.isEmpty, let message = viewModel.errorMessage {
            ContentUnavailableView {
                Label("Could not load municipalities", systemImage: "wifi.exclamationmark")
            } description: {
                Text(message)
            } actions: {
                Button("Try Again") {
                    Task { await viewModel.load(session: session) }
                }
            }
        } else {
            List {
                Section {
                    ForEach(viewModel.municipalities) { municipality in
                        Button {
                            select(municipality)
                        } label: {
                            HStack {
                                MunicipalityRow(municipality: municipality)
                                Spacer()
                                if municipality.name == session.city {
                                    Image(systemName: "checkmark").foregroundStyle(.tint)
                                }
                            }
                        }
                        .buttonStyle(.plain)
                        .disabled(viewModel.isSaving)
                    }
                } header: {
                    if mode == .firstLogin {
                        Text("Choose the municipality you live in. The app shows only its candidates and offices. You can change it later in your profile.")
                            .textCase(nil)
                    }
                } footer: {
                    if let message = viewModel.errorMessage {
                        Text(message).foregroundStyle(.red)
                    }
                }
            }
            .refreshable { await viewModel.load(session: session) }
        }
    }

    private func select(_ municipality: Municipality) {
        if mode == .change {
            if municipality.name != session.city {
                pending = municipality
            }
        } else {
            Task { await choose(municipality) }
        }
    }

    private func choose(_ municipality: Municipality) async {
        if await viewModel.choose(municipality, session: session), mode == .change {
            dismiss()
        }
    }
}

#Preview("First login") {
    CityPickerView(mode: .firstLogin)
        .environment(Session.preview(city: nil))
}

#Preview("Change") {
    CityPickerView(mode: .change)
        .environment(Session.preview())
}
