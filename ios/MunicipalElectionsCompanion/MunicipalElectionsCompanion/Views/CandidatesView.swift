import SwiftUI

// Candidate directory of the user's city: name search and position filter.
// Single column on iPhone portrait; more columns and more information per candidate when there is room.
struct CandidatesView: View {
    @Environment(Session.self) private var session
    @Environment(ElectionViewModel.self) private var election
    @Environment(SelectionsViewModel.self) private var selections
    @Environment(\.verticalSizeClass) private var verticalSizeClass
    @State private var viewModel = CandidatesViewModel()

    var body: some View {
        VStack(spacing: 0) {
            filterBar
            content
        }
            .navigationTitle("Candidates")
            .navigationSubtitle(session.city.map { "City: \($0)" } ?? "")
            .navigationDestination(for: Candidate.self) { candidate in
                CandidateDetailView(candidate: candidate)
            }
            .searchable(text: $viewModel.searchText, placement: .navigationBarDrawer(displayMode: .always), prompt: "Search candidates by name")
            .textInputAutocapitalization(.never)
            .onChange(of: session.city) {
                viewModel.clear()
            }
            // Runs again (after a short pause while typing) whenever the city, the search text or the filter changes.
            .task(id: "\(session.city ?? "")|\(viewModel.queryKey)") {
                try? await Task.sleep(for: .milliseconds(300))
                guard !Task.isCancelled else { return }
                await viewModel.load(session: session)
            }
            .refreshable { await viewModel.load(session: session) }
    }

    @ViewBuilder
    private var content: some View {
        if viewModel.candidates.isEmpty && viewModel.isLoading {
            ProgressView("Loading candidates...")
        } else if viewModel.candidates.isEmpty, let message = viewModel.errorMessage {
            ContentUnavailableView {
                Label("Could not load candidates", systemImage: "wifi.exclamationmark")
            } description: {
                Text(message)
            } actions: {
                Button("Try Again") {
                    Task { await viewModel.load(session: session) }
                }
            }
        } else if viewModel.candidates.isEmpty {
            ContentUnavailableView.search
        } else {
            GeometryReader { proxy in
                let showsDetails = proxy.size.width >= AdaptiveLayout.wideWidth || verticalSizeClass == .compact
                ScrollView {
                    LazyVGrid(columns: AdaptiveLayout.columns(minimum: AdaptiveLayout.cardWidth), spacing: 12) {
                        ForEach(viewModel.candidates) { candidate in
                            NavigationLink(value: candidate) {
                                CandidateCard(candidate: candidate,
                                              showsDetails: showsDetails,
                                              isSelected: selections.selection(for: candidate) != nil)
                            }
                            .buttonStyle(.plain)
                        }
                    }
                    .padding()
                }
            }
        }
    }

    // The position filter sits above the list, not in the toolbar: a toolbar item disappears while the search field is active.
    private var filterBar: some View {
        HStack {
            Menu {
                Picker("Position", selection: $viewModel.selectedPositionId) {
                    Text("All Positions").tag(Int?.none)
                    ForEach(election.offices) { office in
                        Text(office.name).tag(Int?.some(office.positionId))
                    }
                }
                if viewModel.hasActiveFilters {
                    Button("Clear Filter", role: .destructive) {
                        viewModel.selectedPositionId = nil
                    }
                }
            } label: {
                Label(positionName, systemImage: viewModel.hasActiveFilters
                      ? "line.3.horizontal.decrease.circle.fill"
                      : "line.3.horizontal.decrease.circle")
            }
            .buttonStyle(.bordered)
            Spacer()
        }
        .padding(.horizontal)
        .padding(.vertical, 8)
    }

    private var positionName: String {
        election.offices.first { $0.positionId == viewModel.selectedPositionId }?.name ?? "All Positions"
    }
}

#Preview {
    NavigationStack { CandidatesView() }
        .environment(Session.preview())
        .environment(ElectionViewModel())
        .environment(SelectionsViewModel())
}
