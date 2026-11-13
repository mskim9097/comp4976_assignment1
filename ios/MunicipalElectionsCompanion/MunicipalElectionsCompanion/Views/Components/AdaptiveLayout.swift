import SwiftUI

// Shared layout helpers: grids that add columns when there is room, and one place for the width breakpoint.
enum AdaptiveLayout {
    // From this width on (iPad, or an iPhone in landscape) screens show more information and use several columns.
    static let wideWidth: CGFloat = 600

    // Smallest width of a card in a grid: one column on an iPhone in portrait, two on an iPad in portrait
    // (next to the sidebar) or an iPhone in landscape, three or more on an iPad in landscape.
    static let cardWidth: CGFloat = 270

    static func columns(minimum: CGFloat = cardWidth) -> [GridItem] {
        [GridItem(.adaptive(minimum: minimum), spacing: 12, alignment: .top)]
    }
}

// A rounded card background used by the grid cells.
struct CardBackground: ViewModifier {
    func body(content: Content) -> some View {
        content
            .padding(12)
            .frame(maxWidth: .infinity, alignment: .leading)
            .background(.background.secondary, in: RoundedRectangle(cornerRadius: 14))
    }
}

extension View {
    func cardBackground() -> some View { modifier(CardBackground()) }
}
