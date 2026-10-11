import SwiftUI

struct MainTabView: View {
    var body: some View {
        TabView {
            MunicipalitiesView()
                .tabItem { Label("Municipalities", systemImage: "building.2") }

            NavigationStack { CandidatesView() }
                .tabItem { Label("Candidates", systemImage: "person.3") }

            VotingGuideView()
                .tabItem { Label("My Guide", systemImage: "checklist") }

            ProfileView()
                .tabItem { Label("Profile", systemImage: "person.crop.circle") }
        }
    }
}
