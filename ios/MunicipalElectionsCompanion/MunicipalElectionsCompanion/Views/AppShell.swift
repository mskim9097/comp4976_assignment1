import SwiftUI

enum AppSection: String, CaseIterable, Identifiable {
    case home, candidates, selections, profile

    var id: String { rawValue }

    var title: String {
        switch self {
        case .home: "Home"
        case .candidates: "Candidates"
        case .selections: "My Selections"
        case .profile: "Profile"
        }
    }

    var symbol: String {
        switch self {
        case .home: "house"
        case .candidates: "person.3"
        case .selections: "checklist"
        case .profile: "person.crop.circle"
        }
    }

    @ViewBuilder
    var screen: some View {
        switch self {
        case .home: HomeView()
        case .candidates: CandidatesView()
        case .selections: SelectionsView()
        case .profile: ProfileView()
        }
    }
}

// The main navigation: tabs on iPhone, a sidebar with a detail column (NavigationSplitView) on iPad.
// The container is chosen by device type, not by size, so rotating the device never rebuilds it
// (navigation state, loaded data and unsaved edits stay).
struct AppShell: View {
    @Environment(Session.self) private var session
    @Environment(ElectionViewModel.self) private var election
    @Environment(SelectionsViewModel.self) private var selections
    @State private var section: AppSection = .home

    private var isPad: Bool { UIDevice.current.userInterfaceIdiom == .pad }

    var body: some View {
        Group {
            if isPad {
                splitView
            } else {
                tabView
            }
        }
        // Runs at start and again whenever the user changes the municipality: the data of the old city is
        // dropped at once and the data of the new city is loaded from the API and Firebase.
        .task(id: session.city) {
            election.clear()
            selections.clear()
            async let loadedElection: Void = election.load(session: session)
            async let loadedSelections: Void = selections.load(session: session)
            _ = await (loadedElection, loadedSelections)
        }
    }

    private var tabView: some View {
        TabView(selection: $section) {
            ForEach(AppSection.allCases) { item in
                Tab(item.title, systemImage: item.symbol, value: item) {
                    NavigationStack { item.screen }
                }
            }
        }
    }

    private var splitView: some View {
        NavigationSplitView {
            List(AppSection.allCases, selection: Binding<AppSection?>(get: { section }, set: { section = $0 ?? section })) { item in
                Label(item.title, systemImage: item.symbol)
                    .tag(item)
            }
            .navigationSplitViewColumnWidth(min: 180, ideal: 210, max: 280)
            .navigationTitle("Elections")
            .navigationSubtitle(session.city.map { "City: \($0)" } ?? "")
        } detail: {
            NavigationStack { section.screen }
                .id(section)
        }
    }
}

#Preview("iPhone") {
    AppShell()
        .environment(Session.preview())
        .environment(ElectionViewModel())
        .environment(SelectionsViewModel())
}
