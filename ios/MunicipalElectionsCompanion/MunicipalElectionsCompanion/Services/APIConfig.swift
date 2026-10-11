import Foundation

enum APIConfig {
    // Deployed API on Azure App Service. Set the API_BASE_URL environment variable in the Xcode scheme
    // (for example http://localhost:5214) to test against a local API instead.
    static let baseURL: URL = {
        if let override = ProcessInfo.processInfo.environment["API_BASE_URL"], let url = URL(string: override) {
            return url
        }
        return URL(string: "https://municipalelections-api-mskim9097-edg2grfychegfwes.canadacentral-01.azurewebsites.net")!
    }()
}
