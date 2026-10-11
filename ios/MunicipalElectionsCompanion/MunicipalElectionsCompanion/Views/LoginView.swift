import SwiftUI

struct LoginView: View {
    @Environment(Session.self) private var session
    @State private var viewModel = LoginViewModel()

    var body: some View {
        Form {
            Section {
                VStack(spacing: 8) {
                    Image(systemName: "checkmark.seal.fill")
                        .font(.system(size: 48))
                        .foregroundStyle(.tint)
                    Text("Municipal Elections Companion")
                        .font(.title3.bold())
                    Text("Log in to browse candidates and build your voting guide.")
                        .font(.subheadline)
                        .foregroundStyle(.secondary)
                        .multilineTextAlignment(.center)
                }
                .frame(maxWidth: .infinity)
                .padding(.vertical, 8)
            }

            if let notice = session.notice {
                Section {
                    Text(notice).foregroundStyle(.orange)
                }
            }

            Section {
                TextField("Email", text: $viewModel.email)
                    .textContentType(.emailAddress)
                    .keyboardType(.emailAddress)
                    .textInputAutocapitalization(.never)
                    .autocorrectionDisabled()
                FieldError(message: viewModel.emailError)

                SecureField("Password", text: $viewModel.password)
                    .textContentType(.password)
                FieldError(message: viewModel.passwordError)
            }

            if let error = viewModel.errorMessage {
                Section {
                    Text(error).foregroundStyle(.red)
                }
            }

            Section {
                Button {
                    Task { await viewModel.submit(session: session) }
                } label: {
                    HStack {
                        Spacer()
                        if viewModel.isLoading {
                            ProgressView()
                        } else {
                            Text("Log In").bold()
                        }
                        Spacer()
                    }
                }
                .disabled(viewModel.isLoading)

                NavigationLink("Create an account") {
                    RegisterView()
                }
            }
        }
        .navigationTitle("Log In")
        .navigationBarTitleDisplayMode(.inline)
    }
}

#Preview {
    NavigationStack { LoginView() }
        .environment(Session())
}
