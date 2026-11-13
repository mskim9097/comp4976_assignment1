import SwiftUI

// What the editor sheet does: save a new selection for a candidate, or change the notes of a saved one.
enum SelectionEditorMode: Identifiable {
    case add(Candidate)
    case edit(CandidateSelection)

    var id: String {
        switch self {
        case .add(let candidate): "add-\(candidate.candidateId)"
        case .edit(let selection): "edit-\(selection.id)"
        }
    }
}

// Create / edit a selection: candidate summary, notes (at most 500 characters).
struct SelectionEditorView: View {
    @Environment(\.dismiss) private var dismiss
    @Environment(Session.self) private var session
    @Environment(ElectionViewModel.self) private var election
    @Environment(SelectionsViewModel.self) private var selections

    let mode: SelectionEditorMode
    @State private var notes: String
    @State private var errorMessage: String?
    @State private var isSaving = false

    init(mode: SelectionEditorMode) {
        self.mode = mode
        if case .edit(let selection) = mode {
            _notes = State(initialValue: selection.notes)
        } else {
            _notes = State(initialValue: "")
        }
    }

    private var summary: (name: String, position: String, city: String, photo: URL?) {
        switch mode {
        case .add(let candidate):
            (candidate.name, candidate.positionName, candidate.municipalityName, candidate.photoUrl)
        case .edit(let selection):
            (selection.candidateName, selection.positionName, selection.municipalityName, selection.photoURL)
        }
    }

    private var isEditing: Bool {
        if case .edit = mode { true } else { false }
    }

    private var remaining: Int { SelectionRules.maxNotesLength - notes.count }

    var body: some View {
        NavigationStack {
            Form {
                Section {
                    HStack(spacing: 12) {
                        RemoteImage(url: summary.photo)
                            .frame(width: 56, height: 56)
                            .clipShape(Circle())
                        VStack(alignment: .leading) {
                            Text(summary.name).font(.headline)
                            Text(summary.position).font(.subheadline)
                            Text(summary.city).font(.caption).foregroundStyle(.secondary)
                        }
                    }
                }

                Section {
                    TextEditor(text: $notes)
                        .frame(minHeight: 120)
                        .onChange(of: notes) {
                            // Typing stops at the limit; pasted text is cut to it.
                            if notes.count > SelectionRules.maxNotesLength {
                                notes = String(notes.prefix(SelectionRules.maxNotesLength))
                            }
                        }
                } header: {
                    Text("Notes (optional)")
                } footer: {
                    Text("\(remaining) characters left")
                        .foregroundStyle(remaining < 20 ? .orange : .secondary)
                }

                if let errorMessage {
                    Section {
                        Text(errorMessage).foregroundStyle(.red)
                    }
                }
            }
            .navigationTitle(isEditing ? "Edit Selection" : "Add Selection")
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .cancellationAction) {
                    Button("Cancel") { dismiss() }
                }
                ToolbarItem(placement: .confirmationAction) {
                    if isSaving {
                        ProgressView()
                    } else {
                        Button("Save") { Task { await save() } }
                    }
                }
            }
            .interactiveDismissDisabled(isSaving)
        }
    }

    private func save() async {
        isSaving = true
        errorMessage = nil
        defer { isSaving = false }
        let failure: String?
        switch mode {
        case .add(let candidate):
            failure = await selections.add(candidate: candidate, notes: notes, offices: election.offices, session: session)
        case .edit(let selection):
            failure = await selections.update(selection, notes: notes, session: session)
        }
        if let failure, !failure.isEmpty {
            errorMessage = failure
        } else {
            dismiss()
        }
    }
}

#Preview("Add") {
    SelectionEditorView(mode: .add(PreviewData.candidate))
        .environment(Session.preview())
        .environment(ElectionViewModel())
        .environment(SelectionsViewModel())
}
