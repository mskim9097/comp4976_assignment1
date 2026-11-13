import Foundation

// Reads the expiry of a JWT so an expired token is noticed at launch instead of by a failed request.
enum JWT {
    static func expiry(of token: String) -> Date? {
        let parts = token.split(separator: ".")
        guard parts.count == 3 else { return nil }
        var base64 = parts[1].replacingOccurrences(of: "-", with: "+").replacingOccurrences(of: "_", with: "/")
        while base64.count % 4 != 0 { base64 += "=" }
        guard let data = Data(base64Encoded: base64),
              let json = try? JSONSerialization.jsonObject(with: data) as? [String: Any],
              let exp = json["exp"] as? TimeInterval else {
            return nil
        }
        return Date(timeIntervalSince1970: exp)
    }

    static func isExpired(_ token: String, now: Date = .now) -> Bool {
        guard let expiry = expiry(of: token) else { return false }
        return expiry <= now
    }
}
