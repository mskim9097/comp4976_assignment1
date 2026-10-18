import SwiftUI

struct RegisterView: View {
    @Environment(Session.self) private var session
    @State private var viewModel = RegisterViewModel()

    var body: some View {
        Form {
            Section("Your details") {
                TextField("Name", text: $viewModel.name)
                    .textContentType(.name)
                    .textInputAutocapitalization(.never)
                FieldError(message: viewModel.nameError)

                TextField("Email", text: $viewModel.email)
                    .textContentType(.emailAddress)
                    .keyboardType(.emailAddress)
                    .textInputAutocapitalization(.never)
                    .autocorrectionDisabled()
                FieldError(message: viewModel.emailError)
            }

            Section("Password (at least \(Validation.minimumPasswordLength) characters)") {
                SecureField("Password", text: $viewModel.password)
                    .textInputAutocapitalization(.never)
                FieldError(message: viewModel.passwordError)

                SecureField("Confirm password", text: $viewModel.confirmPassword)
                    .textInputAutocapitalization(.never)
                FieldError(message: viewModel.confirmError)
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
                            Text("Create Account").bold()
                        }
                        Spacer()
                    }
                }
                .disabled(viewModel.isLoading)
            }
        }
        .navigationTitle("Create Account")
        .navigationBarTitleDisplayMode(.inline)
    }
}

#Preview {
    NavigationStack { RegisterView() }
        .environment(Session())
}
