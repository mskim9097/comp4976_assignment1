import SwiftUI

// "My Candidate Selections": the user's saved candidates of the current city, with edit, delete and pull to refresh.
struct SelectionsView: View {
    @Environment(Session.self) private var session
    @Environment(SelectionsViewModel.self) private var selections
    @State private var editor: SelectionEditorMode?
    @State private var pendingDelete: CandidateSelection?
    @State private var deleteError: String?

    var body: some View {
        Group {
            if selections.items.isEmpty && selections.isLoading {
                ProgressView("Loading your selections...")
            } else if selections.items.isEmpty, let message = selections.errorMessage {
                ContentUnavailableView {
                    Label("Could not load your selections", systemImage: "icloud.slash")
                } description: {
                    Text(message)
                } actions: {
                    Button("Try Again") {
                        Task { await selections.load(session: session) }
                    }
                }
            } else if selections.items.isEmpty {
                ContentUnavailableView(
                    "No selections yet",
                    systemImage: "checklist",
                    description: Text("Open a candidate and tap \"Add Selection\"."))
            } else {
                ScrollView {
                    LazyVGrid(columns: AdaptiveLayout.columns(minimum: AdaptiveLayout.cardWidth), spacing: 12) {
                        ForEach(selections.items) { selection in
                            SelectionCard(selection: selection,
                                          onEdit: { editor = .edit(selection) },
                                          onDelete: { pendingDelete = selection })
                        }
                    }
                    .padding()

                    if let message = deleteError ?? selections.errorMessage {
                        Text(message)
                            .font(.footnote)
                            .foregroundStyle(.red)
                            .padding(.horizontal)
                    }
                }
            }
        }
        .navigationTitle("My Candidate Selections")
        .navigationSubtitle(session.city.map { "City: \($0)" } ?? "")
        .refreshable { await selections.load(session: session) }
        .sheet(item: $editor) { mode in
            SelectionEditorView(mode: mode)
        }
        .confirmationDialog(
            pendingDelete.map { "Remove \($0.candidateName) from your selections?" } ?? "",
            isPresented: Binding(get: { pendingDelete != nil }, set: { if !$0 { pendingDelete = nil } }),
            titleVisibility: .visible,
            presenting: pendingDelete
        ) { selection in
            Button("Delete Selection", role: .destructive) {
                Task { deleteError = await selections.delete(selection, session: session) }
            }
        }
    }
}

#Preview {
    NavigationStack { SelectionsView() }
        .environment(Session.preview())
        .environment(SelectionsViewModel())
        .environment(ElectionViewModel())
}
