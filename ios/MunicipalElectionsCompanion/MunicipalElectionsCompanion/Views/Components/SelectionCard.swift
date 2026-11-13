import SwiftUI

// A saved selection: photo, candidate, position, municipality, notes, date saved, and the edit / delete buttons.
struct SelectionCard: View {
    let selection: CandidateSelection
    let onEdit: () -> Void
    let onDelete: () -> Void

    var body: some View {
        VStack(alignment: .leading, spacing: 10) {
            HStack(spacing: 12) {
                RemoteImage(url: selection.photoURL)
                    .frame(width: 64, height: 64)
                    .clipShape(Circle())

                VStack(alignment: .leading, spacing: 2) {
                    Text(selection.candidateName).font(.headline)
                    Text(selection.positionName).font(.subheadline)
                    Text(selection.municipalityName).font(.caption).foregroundStyle(.secondary)
                }
                Spacer(minLength: 0)
            }

            Text(selection.notes.isEmpty ? "No notes" : selection.notes)
                .font(.footnote)
                .foregroundStyle(selection.notes.isEmpty ? .tertiary : .secondary)
                .frame(maxWidth: .infinity, alignment: .leading)

            Text("Saved \(selection.dateCreated.formatted(date: .abbreviated, time: .shortened))")
                .font(.caption2)
                .foregroundStyle(.secondary)

            HStack(spacing: 12) {
                Button("Edit", systemImage: "pencil", action: onEdit)
                    .buttonStyle(.bordered)
                Button("Delete", systemImage: "trash", role: .destructive, action: onDelete)
                    .buttonStyle(.bordered)
            }
            .controlSize(.small)
        }
        .cardBackground()
    }
}

#Preview {
    SelectionCard(selection: PreviewData.selections[0], onEdit: {}, onDelete: {})
        .padding()
}
