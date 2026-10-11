import SwiftUI

// Candidate directory: name search plus municipality and position filters.
// Used as a tab (all candidates) and when a municipality is picked (that municipality's candidates).
struct CandidatesView: View {
    @Environment(Session.self) private var session
    @State private var viewModel: CandidatesViewModel
    private let title: String

    init(municipality: Municipality? = nil) {
        _viewModel = State(initialValue: CandidatesViewModel(municipalityId: municipality?.municipalityId))
        title = municipality?.name ?? "Candidates"
    }

    var body: some View {
        Group {
            if viewModel.candidates.isEmpty && viewModel.isLoading {
                ProgressView("Loading candidates...")
            } else if viewModel.candidates.isEmpty, let message = viewModel.errorMessage {
                ContentUnavailableView {
                    Label("Could not load candidates", systemImage: "wifi.exclamationmark")
                } description: {
                    Text(message)
                } actions: {
                    Button("Try Again") {
                        Task { await viewModel.loadCandidates(session: session) }
                    }
                }
            } else if viewModel.candidates.isEmpty {
                ContentUnavailableView.search
            } else {
                List(viewModel.candidates) { candidate in
                    NavigationLink(value: candidate) {
                        CandidateRow(candidate: candidate)
                    }
                }
            }
        }
        .navigationTitle(title)
        .navigationDestination(for: Candidate.self) { candidate in
            CandidateDetailView(candidate: candidate)
        }
        .searchable(text: $viewModel.searchText, prompt: "Search candidates by name")
        .toolbar { filterMenu }
        .onChange(of: viewModel.selectedMunicipalityId) {
            viewModel.municipalityChanged()
        }
        .task { await viewModel.loadFilters(session: session) }
        // Runs again (after a short pause while typing) whenever the search text or a filter changes.
        .task(id: viewModel.queryKey) {
            try? await Task.sleep(for: .milliseconds(300))
            guard !Task.isCancelled else { return }
            await viewModel.loadCandidates(session: session)
        }
        .refreshable { await viewModel.loadCandidates(session: session) }
    }

    @ToolbarContentBuilder
    private var filterMenu: some ToolbarContent {
        ToolbarItem(placement: .topBarTrailing) {
            Menu {
                Picker("Municipality", selection: $viewModel.selectedMunicipalityId) {
                    Text("All Municipalities").tag(Int?.none)
                    ForEach(viewModel.municipalities) { municipality in
                        Text(municipality.name).tag(Int?.some(municipality.municipalityId))
                    }
                }
                Picker("Position", selection: $viewModel.selectedPositionId) {
                    Text("All Positions").tag(Int?.none)
                    ForEach(viewModel.visiblePositions) { position in
                        Text(viewModel.positionTitle(position)).tag(Int?.some(position.positionId))
                    }
                }
                if viewModel.hasActiveFilters {
                    Button("Clear Filters", role: .destructive) {
                        viewModel.clearFilters()
                    }
                }
            } label: {
                Label("Filter", systemImage: viewModel.hasActiveFilters
                      ? "line.3.horizontal.decrease.circle.fill"
                      : "line.3.horizontal.decrease.circle")
            }
        }
    }
}
