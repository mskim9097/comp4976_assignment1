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

struct MunicipalityRow: View {
    let municipality: Municipality

    var body: some View {
        HStack(spacing: 12) {
            RemoteImage(url: municipality.logoUrl, placeholderSymbol: "building.2.crop.circle")
                .frame(width: 56, height: 56)
                .clipShape(RoundedRectangle(cornerRadius: 12))

            VStack(alignment: .leading, spacing: 2) {
                Text(municipality.name)
                    .font(.headline)
                Text("Election: \(municipality.electionDateText)")
                    .font(.subheadline)
                Text(municipality.province)
                    .font(.caption)
                    .foregroundStyle(.secondary)
            }
        }
        .accessibilityElement(children: .combine)
    }
}
