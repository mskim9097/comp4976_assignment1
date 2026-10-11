import Foundation

// The API sends election dates as "yyyy-MM-dd" and timestamps as ISO-8601 UTC ("2026-10-10T22:37:03Z").
nonisolated enum APIDate {
    static func parse(_ string: String) -> Date? {
        let day = DateFormatter()
        day.locale = Locale(identifier: "en_US_POSIX")
        day.timeZone = TimeZone(secondsFromGMT: 0)
        day.dateFormat = "yyyy-MM-dd"
        if let date = day.date(from: string) {
            return date
        }

        let iso = ISO8601DateFormatter()
        if let date = iso.date(from: string) {
            return date
        }
        iso.formatOptions = [.withInternetDateTime, .withFractionalSeconds]
        return iso.date(from: string)
    }

    static func makeDecoder() -> JSONDecoder {
        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .custom { decoder in
            let string = try decoder.singleValueContainer().decode(String.self)
            guard let date = parse(string) else {
                throw DecodingError.dataCorrupted(
                    .init(codingPath: decoder.codingPath, debugDescription: "Unrecognized date: \(string)"))
            }
            return date
        }
        return decoder
    }

    // Election dates have no time zone, so they are shown in UTC to avoid slipping a day.
    static func displayText(_ date: Date) -> String {
        let formatter = DateFormatter()
        formatter.locale = Locale(identifier: "en_CA")
        formatter.timeZone = TimeZone(secondsFromGMT: 0)
        formatter.dateStyle = .long
        formatter.timeStyle = .none
        return formatter.string(from: date)
    }
}
