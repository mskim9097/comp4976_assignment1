import SwiftUI

struct VotingGuideView: View {
    @Environment(Session.self) private var session
    @State private var viewModel = VotingGuideViewModel()

    var body: some View {
        NavigationStack {
            Group {
                if viewModel.items.isEmpty && viewModel.isLoading {
                    ProgressView("Loading your voting guide...")
                } else if viewModel.items.isEmpty, let message = viewModel.errorMessage {
                    ContentUnavailableView {
                        Label("Could not load your voting guide", systemImage: "wifi.exclamationmark")
                    } description: {
                        Text(message)
                    } actions: {
                        Button("Try Again") {
                            Task { await viewModel.load(session: session) }
                        }
                    }
                } else if viewModel.items.isEmpty {
                    ContentUnavailableView(
                        "No saved candidates",
                        systemImage: "checklist",
                        description: Text("Open a candidate and tap \"Add to Voting Guide\"."))
                } else {
                    List {
                        ForEach(viewModel.items) { item in
                            NavigationLink(value: item.candidate) {
                                CandidateRow(candidate: item.candidate)
                            }
                        }
                        .onDelete { offsets in
                            Task { await viewModel.remove(at: offsets, session: session) }
                        }

                        if let error = viewModel.errorMessage {
                            Text(error).foregroundStyle(.red).font(.footnote)
                        }
                    }
                }
            }
            .navigationTitle("My Voting Guide")
            .navigationDestination(for: Candidate.self) { candidate in
                CandidateDetailView(candidate: candidate)
            }
            .toolbar {
                if !viewModel.items.isEmpty {
                    EditButton()
                }
            }
            .task { await viewModel.load(session: session) }
            .refreshable { await viewModel.load(session: session) }
        }
    }
}
