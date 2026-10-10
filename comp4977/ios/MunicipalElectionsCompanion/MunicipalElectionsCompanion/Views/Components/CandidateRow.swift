import SwiftUI

struct CandidateRow: View {
    let candidate: Candidate

    var body: some View {
        HStack(spacing: 12) {
            RemoteImage(url: candidate.photoUrl)
                .frame(width: 56, height: 56)
                .clipShape(Circle())

            VStack(alignment: .leading, spacing: 2) {
                Text(candidate.name)
                    .font(.headline)
                Text(candidate.positionName)
                    .font(.subheadline)
                Text(candidate.municipalityName)
                    .font(.caption)
                    .foregroundStyle(.secondary)
            }
        }
        .accessibilityElement(children: .combine)
    }
}
