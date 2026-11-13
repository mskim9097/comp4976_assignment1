import SwiftUI

// Candidate information from the Web API plus the user's selection (Firebase): add, edit, delete.
// Narrow screens stack the photo above the information; wide screens put them side by side.
struct CandidateDetailView: View {
    @Environment(Session.self) private var session
    @Environment(SelectionsViewModel.self) private var selections

    let candidate: Candidate
    @State private var editor: SelectionEditorMode?
    @State private var showDeleteConfirmation = false
    @State private var isWorking = false
    @State private var errorMessage: String?

    private var saved: CandidateSelection? { selections.selection(for: candidate) }

    var body: some View {
        GeometryReader { proxy in
            ScrollView {
                if proxy.size.width >= AdaptiveLayout.wideWidth {
                    HStack(alignment: .top, spacing: 32) {
                        identity
                            .frame(width: min(proxy.size.width * 0.35, 320))
                        VStack(alignment: .leading, spacing: 16) {
                            details
                        }
                    }
                    .padding()
                } else {
                    VStack(spacing: 16) {
                        identity
                        details
                    }
                    .padding()
                }
            }
        }
        .navigationTitle("Candidate")
        .navigationBarTitleDisplayMode(.inline)
        .navigationSubtitle(session.city.map { "City: \($0)" } ?? "")
        .safeAreaInset(edge: .bottom) { actionBar }
        .sheet(item: $editor) { mode in
            SelectionEditorView(mode: mode)
        }
        .confirmationDialog("Remove \(candidate.name) from your selections?", isPresented: $showDeleteConfirmation,
                            titleVisibility: .visible) {
            Button("Delete Selection", role: .destructive) {
                Task { await deleteSelection() }
            }
        }
    }

    private var identity: some View {
        VStack(spacing: 12) {
            RemoteImage(url: candidate.photoUrl)
                .frame(width: 180, height: 180)
                .clipShape(Circle())
                .shadow(radius: 4)

            VStack(spacing: 4) {
                Text(candidate.name)
                    .font(.title.bold())
                    .multilineTextAlignment(.center)
                Text(candidate.positionName)
                    .font(.headline)
                Text(candidate.municipalityName)
                    .foregroundStyle(.secondary)
            }
        }
        .frame(maxWidth: .infinity)
    }

    @ViewBuilder
    private var details: some View {
        GroupBox("Biography") {
            Text(candidate.biography ?? "No biography provided.")
                .frame(maxWidth: .infinity, alignment: .leading)
        }

        if candidate.website != nil || candidate.email != nil {
            GroupBox("Contact") {
                VStack(alignment: .leading, spacing: 8) {
                    if let website = candidate.website, let url = URL(string: website) {
                        Link(destination: url) {
                            Label(website, systemImage: "globe")
                        }
                    }
                    if let email = candidate.email, let url = URL(string: "mailto:\(email)") {
                        Link(destination: url) {
                            Label(email, systemImage: "envelope")
                        }
                    }
                }
                .frame(maxWidth: .infinity, alignment: .leading)
            }
        }

        if let saved {
            GroupBox("Your selection") {
                VStack(alignment: .leading, spacing: 6) {
                    Text(saved.notes.isEmpty ? "No notes." : saved.notes)
                    Text("Saved \(saved.dateCreated.formatted(date: .abbreviated, time: .shortened))")
                        .font(.caption)
                        .foregroundStyle(.secondary)
                }
                .frame(maxWidth: .infinity, alignment: .leading)
            }
        }
    }

    private var actionBar: some View {
        VStack(spacing: 8) {
            if let errorMessage {
                Text(errorMessage)
                    .font(.footnote)
                    .foregroundStyle(.red)
                    .multilineTextAlignment(.center)
            }
            if let saved {
                HStack(spacing: 12) {
                    Button {
                        editor = .edit(saved)
                    } label: {
                        Label("Edit Selection", systemImage: "pencil").frame(maxWidth: .infinity)
                    }
                    .buttonStyle(.borderedProminent)

                    Button(role: .destructive) {
                        showDeleteConfirmation = true
                    } label: {
                        Label("Delete Selection", systemImage: "trash").frame(maxWidth: .infinity)
                    }
                    .buttonStyle(.bordered)
                }
                .controlSize(.large)
                .disabled(isWorking)
            } else {
                Button {
                    errorMessage = nil
                    editor = .add(candidate)
                } label: {
                    Label("Add Selection", systemImage: "plus.circle").bold().frame(maxWidth: .infinity)
                }
                .buttonStyle(.borderedProminent)
                .controlSize(.large)
            }
        }
        .padding()
        .background(.bar)
    }

    private func deleteSelection() async {
        guard let saved else { return }
        isWorking = true
        defer { isWorking = false }
        errorMessage = await selections.delete(saved, session: session)
    }
}

#Preview {
    NavigationStack { CandidateDetailView(candidate: PreviewData.candidate) }
        .environment(Session.preview())
        .environment(SelectionsViewModel())
        .environment(ElectionViewModel())
}
