import SwiftUI

// A candidate in the directory. With enough room (`showsDetails`) it also shows the biography and the contact details.
struct CandidateCard: View {
    let candidate: Candidate
    var showsDetails = false
    var isSelected = false

    var body: some View {
        VStack(alignment: .leading, spacing: 10) {
            HStack(spacing: 12) {
                RemoteImage(url: candidate.photoUrl)
                    .frame(width: 64, height: 64)
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
                Spacer(minLength: 0)
                if isSelected {
                    Image(systemName: "checkmark.circle.fill")
                        .foregroundStyle(.green)
                        .accessibilityLabel("Selected")
                }
            }

            if showsDetails {
                if let biography = candidate.biography {
                    Text(biography)
                        .font(.footnote)
                        .foregroundStyle(.secondary)
                        .lineLimit(3)
                }
                if let email = candidate.email {
                    Label(email, systemImage: "envelope")
                        .font(.caption)
                        .foregroundStyle(.secondary)
                        .lineLimit(1)
                }
                if let website = candidate.website {
                    Label(website, systemImage: "globe")
                        .font(.caption)
                        .foregroundStyle(.secondary)
                        .lineLimit(1)
                }
            }
        }
        .cardBackground()
        .accessibilityElement(children: .combine)
    }
}

#Preview {
    ScrollView {
        LazyVGrid(columns: AdaptiveLayout.columns(), spacing: 12) {
            ForEach(PreviewData.candidates) { CandidateCard(candidate: $0, showsDetails: true) }
        }
        .padding()
    }
}
