import SwiftUI

struct CandidateDetailView: View {
    @Environment(Session.self) private var session
    @State private var viewModel: CandidateDetailViewModel

    init(candidate: Candidate) {
        _viewModel = State(initialValue: CandidateDetailViewModel(candidate: candidate))
    }

    private var candidate: Candidate { viewModel.candidate }

    var body: some View {
        ScrollView {
            VStack(spacing: 16) {
                RemoteImage(url: candidate.photoUrl)
                    .frame(width: 180, height: 180)
                    .clipShape(Circle())
                    .shadow(radius: 4)

                VStack(spacing: 4) {
                    Text(candidate.name)
                        .font(.title.bold())
                        .multilineTextAlignment(.center)
                    Text(candidate.positionName)
                        .font(.headline)
                    Text(candidate.municipalityName)
                        .foregroundStyle(.secondary)
                }

                GroupBox("Biography") {
                    Text(candidate.biography ?? "No biography provided.")
                        .frame(maxWidth: .infinity, alignment: .leading)
                }

                if candidate.website != nil || candidate.email != nil {
                    GroupBox("Contact") {
                        VStack(alignment: .leading, spacing: 8) {
                            if let website = candidate.website, let url = URL(string: website) {
                                Link(destination: url) {
                                    Label(website, systemImage: "globe")
                                }
                            }
                            if let email = candidate.email, let url = URL(string: "mailto:\(email)") {
                                Link(destination: url) {
                                    Label(email, systemImage: "envelope")
                                }
                            }
                        }
                        .frame(maxWidth: .infinity, alignment: .leading)
                    }
                }
            }
            .padding()
        }
        .navigationTitle("Candidate")
        .navigationBarTitleDisplayMode(.inline)
        .safeAreaInset(edge: .bottom) {
            VStack(spacing: 8) {
                if let error = viewModel.errorMessage {
                    Text(error)
                        .font(.footnote)
                        .foregroundStyle(.red)
                        .multilineTextAlignment(.center)
                }
                Button {
                    Task { await viewModel.toggleSaved(session: session) }
                } label: {
                    HStack {
                        if viewModel.isWorking {
                            ProgressView()
                        }
                        Label(viewModel.isSaved ? "Remove from Voting Guide" : "Add to Voting Guide",
                              systemImage: viewModel.isSaved ? "minus.circle" : "plus.circle")
                            .bold()
                    }
                    .frame(maxWidth: .infinity)
                }
                .buttonStyle(.borderedProminent)
                .tint(viewModel.isSaved ? .red : .accentColor)
                .controlSize(.large)
                .disabled(viewModel.isWorking)
            }
            .padding()
            .background(.bar)
        }
        .task { await viewModel.checkSaved(session: session) }
    }
}
