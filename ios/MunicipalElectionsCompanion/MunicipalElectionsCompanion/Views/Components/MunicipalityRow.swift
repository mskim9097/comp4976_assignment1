import SwiftUI

struct MunicipalityRow: View {
    let municipality: Municipality

    var body: some View {
        HStack(spacing: 12) {
            RemoteImage(url: municipality.logoUrl, placeholderSymbol: "building.2.crop.circle")
                .frame(width: 56, height: 56)
                .clipShape(RoundedRectangle(cornerRadius: 12))

            VStack(alignment: .leading, spacing: 2) {
                Text(municipality.name)
                    .font(.headline)
                Text("Election: \(municipality.electionDateText)")
                    .font(.subheadline)
                Text(municipality.province)
                    .font(.caption)
                    .foregroundStyle(.secondary)
            }
        }
        .accessibilityElement(children: .combine)
    }
}

#Preview {
    List(PreviewData.municipalities) { MunicipalityRow(municipality: $0) }
}
