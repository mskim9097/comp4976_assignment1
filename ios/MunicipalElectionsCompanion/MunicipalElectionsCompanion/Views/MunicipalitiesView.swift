import SwiftUI

struct MunicipalitiesView: View {
    @Environment(Session.self) private var session
    @State private var viewModel = MunicipalitiesViewModel()

    var body: some View {
        NavigationStack {
            Group {
                if viewModel.isLoading && viewModel.municipalities.isEmpty {
                    ProgressView("Loading municipalities...")
                } else if viewModel.municipalities.isEmpty, let message = viewModel.errorMessage {
                    ContentUnavailableView {
                        Label("Could not load municipalities", systemImage: "wifi.exclamationmark")
                    } description: {
                        Text(message)
                    } actions: {
                        Button("Try Again") {
                            Task { await viewModel.load(session: session) }
                        }
                    }
                } else {
                    List(viewModel.municipalities) { municipality in
                        NavigationLink(value: municipality) {
                            MunicipalityRow(municipality: municipality)
                        }
                    }
                }
            }
            .navigationTitle("Municipalities")
            .navigationDestination(for: Municipality.self) { municipality in
                CandidatesView(municipality: municipality)
            }
            .task { await viewModel.load(session: session) }
            .refreshable { await viewModel.load(session: session) }
        }
    }
}

#Preview {
    MunicipalitiesView()
        .environment(Session.preview())
}
