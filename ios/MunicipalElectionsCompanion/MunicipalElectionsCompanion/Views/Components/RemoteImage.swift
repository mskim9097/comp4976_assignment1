import SwiftUI

// AsyncImage with a progress indicator while loading and an icon if the image cannot be loaded.
struct RemoteImage: View {
    let url: URL?
    var placeholderSymbol = "person.crop.circle.fill"

    var body: some View {
        AsyncImage(url: url) { phase in
            switch phase {
            case .success(let image):
                image.resizable().aspectRatio(contentMode: .fill)
            case .failure:
                Image(systemName: placeholderSymbol)
                    .resizable()
                    .aspectRatio(contentMode: .fit)
                    .foregroundStyle(.secondary)
                    .padding(8)
            default:
                ProgressView()
            }
        }
    }
}

#Preview {
    HStack {
        RemoteImage(url: PreviewData.candidate.photoUrl)
            .frame(width: 80, height: 80)
            .clipShape(Circle())
        RemoteImage(url: nil, placeholderSymbol: "building.2.crop.circle")
            .frame(width: 80, height: 80)
    }
}
