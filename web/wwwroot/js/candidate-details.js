// "Add to / Remove from preferred candidates" on the candidate details page.
// Uses the same browser storage key and candidate shape as preferred-candidates.js (the directory page).
(() => {
    const storageKey = "municipal-elections.preferred-candidates.v1";
    const details = document.getElementById("candidate-details");
    const button = document.getElementById("preferred-candidate-details-toggle");
    const status = document.getElementById("preferred-candidate-details-status");
    if (!details || !button || !status) {
        return;
    }

    const candidateId = Number(details.dataset.candidateId);
    let preferred;
    try {
        const raw = window.localStorage.getItem(storageKey);
        preferred = raw === null ? [] : JSON.parse(raw);
        if (!Array.isArray(preferred)) {
            throw new Error("invalid");
        }
    } catch (error) {
        status.textContent = "Your preferred-candidate list could not be read from local storage. The saved data was left unchanged.";
        status.classList.remove("d-none");
        button.disabled = true;
        return;
    }

    const isSelected = () => preferred.some((candidate) => candidate.id === candidateId);

    const render = () => {
        const selected = isSelected();
        button.textContent = selected ? "Remove from preferred candidates" : "Add to preferred candidates";
        button.classList.toggle("btn-outline-primary", !selected);
        button.classList.toggle("btn-outline-danger", selected);
        button.setAttribute("aria-pressed", String(selected));
    };

    button.addEventListener("click", () => {
        const next = isSelected()
            ? preferred.filter((candidate) => candidate.id !== candidateId)
            : [...preferred, {
                id: candidateId,
                name: details.dataset.candidateName,
                position: details.dataset.candidatePosition,
                municipality: details.dataset.candidateMunicipality,
                electionDate: details.dataset.candidateElectionDate || "",
                profile: details.dataset.candidateProfile || "",
                email: details.dataset.candidateEmail || "",
                website: details.dataset.candidateWebsite || "",
                photo: details.dataset.candidatePhoto || ""
            }];
        try {
            window.localStorage.setItem(storageKey, JSON.stringify(next));
            preferred = next;
            status.classList.add("d-none");
        } catch (error) {
            status.textContent = "Your selection could not be saved in this browser.";
            status.classList.remove("d-none");
        }
        render();
    });

    render();
})();
