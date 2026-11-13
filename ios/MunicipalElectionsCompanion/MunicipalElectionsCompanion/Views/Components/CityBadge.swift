import SwiftUI

// "City: Burnaby" - shows the user's municipality.
struct CityBadge: View {
    let city: String?

    var body: some View {
        Label("City: \(city ?? "not chosen")", systemImage: "mappin.and.ellipse")
            .font(.subheadline.weight(.medium))
    }
}

#Preview {
    CityBadge(city: "Burnaby")
}
