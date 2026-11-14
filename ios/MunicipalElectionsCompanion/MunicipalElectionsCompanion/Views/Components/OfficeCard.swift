import SwiftUI

// An elected office of the city: seats and how many candidates the user selected for it.
struct OfficeCard: View {
    let office: Position
    let selected: Int

    var body: some View {
        VStack(alignment: .leading, spacing: 4) {
            Text(office.name).font(.headline)
            Text(office.seatsAvailable == 1 ? "1 seat" : "\(office.seatsAvailable) seats")
                .font(.subheadline)
                .foregroundStyle(.secondary)
            Text("You selected \(selected) of \(office.seatsAvailable)")
                .font(.caption)
                .foregroundStyle(selected == office.seatsAvailable ? .green : .secondary)
        }
        .cardBackground()
        .accessibilityElement(children: .combine)
    }
}

#Preview {
    OfficeCard(office: PreviewData.election.offices[0], selected: 1)
        .padding()
}
