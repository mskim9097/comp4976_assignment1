import SwiftUI

// Start screen: the application title, the user's city, the election date and the offices of that city.
struct HomeView: View {
    @Environment(Session.self) private var session
    @Environment(ElectionViewModel.self) private var election
    @Environment(SelectionsViewModel.self) private var selections

    var body: some View {
        ScrollView {
            VStack(alignment: .leading, spacing: 16) {
                header

                if let message = election.errorMessage, election.election == nil {
                    ContentUnavailableView {
                        Label("Could not load the election", systemImage: "wifi.exclamationmark")
                    } description: {
                        Text(message)
                    } actions: {
                        Button("Try Again") {
                            Task { await election.load(session: session) }
                        }
                    }
                } else if let current = election.election {
                    VStack(alignment: .leading, spacing: 4) {
                        Text("Election day").font(.caption).foregroundStyle(.secondary)
                        Text(current.electionDateText).font(.title3.bold())
                        Text(current.province).font(.subheadline).foregroundStyle(.secondary)
                    }
                    .cardBackground()

                    Text("Elected positions").font(.headline)
                    LazyVGrid(columns: AdaptiveLayout.columns(minimum: 260), spacing: 12) {
                        ForEach(election.offices) { office in
                            OfficeCard(office: office, selected: selections.count(positionId: office.positionId))
                        }
                    }
                } else {
                    ProgressView("Loading the election...")
                        .frame(maxWidth: .infinity)
                }
            }
            .padding()
        }
        .navigationTitle("Home")
        .navigationSubtitle(session.city.map { "City: \($0)" } ?? "")
        .refreshable {
            await election.load(session: session)
            await selections.load(session: session)
        }
    }

    private var header: some View {
        VStack(alignment: .leading, spacing: 6) {
            Text("Municipal Elections Companion")
                .font(.title2.bold())
            CityBadge(city: session.city)
                .foregroundStyle(.tint)
            if let name = session.user?.name {
                Text("Welcome, \(name).")
                    .foregroundStyle(.secondary)
            }
        }
    }
}

#Preview {
    NavigationStack { HomeView() }
        .environment(Session.preview())
        .environment(ElectionViewModel())
        .environment(SelectionsViewModel())
}
