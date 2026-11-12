import SwiftUI

// Small red validation message shown under a form field.
struct FieldError: View {
    let message: String?

    var body: some View {
        if let message {
            Text(message)
                .font(.footnote)
                .foregroundStyle(.red)
        }
    }
}

#Preview {
    VStack(alignment: .leading) {
        FieldError(message: "Email is required.")
        FieldError(message: nil)
    }
    .padding()
}
