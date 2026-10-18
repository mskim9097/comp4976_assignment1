(() => {
    const storageKey = "municipal-elections.preferred-candidates.v1";
    const cards = Array.from(document.querySelectorAll(".candidate-directory-card"));
    const list = document.getElementById("preferred-candidate-list");
    const emptyMessage = document.getElementById("preferred-candidate-empty");
    const count = document.getElementById("preferred-candidate-count");
    const status = document.getElementById("preferred-candidate-status");
    const printButton = document.getElementById("print-preferred-candidates");
    const includePhotos = document.getElementById("include-candidate-photos");
    const printGuide = document.getElementById("candidate-print-guide");
    const printGroups = document.getElementById("candidate-print-groups");
    const printDate = document.getElementById("candidate-print-date");

    if (!list || !emptyMessage || !count || !status || !printButton || !includePhotos ||
        !printGuide || !printGroups || !printDate) {
        return;
    }

    const showError = (message) => {
        status.textContent = message;
        status.classList.remove("d-none");
    };

    const parseCandidateList = (rawValue) => {
        if (rawValue === null) {
            return [];
        }

        const parsed = JSON.parse(rawValue);
        if (!Array.isArray(parsed) || parsed.some((candidate) =>
            !candidate ||
            !Number.isInteger(candidate.id) ||
            typeof candidate.name !== "string" ||
            typeof candidate.position !== "string" ||
            typeof candidate.municipality !== "string")) {
            throw new Error("The saved candidate list has an invalid format.");
        }

        return parsed;
    };

    let preferredCandidates;
    try {
        preferredCandidates = parseCandidateList(window.localStorage.getItem(storageKey));
    } catch (error) {
        showError("Your preferred-candidate list could not be read from local storage. The saved data was left unchanged.");
        cards.forEach((card) => {
            card.querySelector(".preferred-candidate-toggle").disabled = true;
        });
        return;
    }

    let municipalityElectionDates;
    try {
        municipalityElectionDates = JSON.parse(printGuide.dataset.municipalityElectionDates || "{}");
    } catch {
        municipalityElectionDates = {};
        showError("Municipality election dates could not be loaded for the print guide.");
    }

    const persist = () => {
        try {
            window.localStorage.setItem(storageKey, JSON.stringify(preferredCandidates));
            status.classList.add("d-none");
            return true;
        } catch (error) {
            showError("Your preferred-candidate list could not be saved in local storage. Check your browser storage settings and try again.");
            return false;
        }
    };

    const createText = (tagName, className, text) => {
        const element = document.createElement(tagName);
        if (className) {
            element.className = className;
        }
        element.textContent = text;
        return element;
    };

    const safeWebsite = (value) => {
        if (typeof value !== "string" || value.length === 0) {
            return null;
        }
        try {
            const url = new URL(value);
            return url.protocol === "https:" || url.protocol === "http:" ? url.href : null;
        } catch {
            return null;
        }
    };

    const formatElectionDate = (value) => {
        if (typeof value !== "string" || !/^\d{4}-\d{2}-\d{2}$/.test(value)) {
            return "Not available";
        }
        const [year, month, day] = value.split("-").map(Number);
        return new Date(year, month - 1, day).toLocaleDateString(undefined, {
            year: "numeric",
            month: "long",
            day: "numeric"
        });
    };

    const renderPrintGuide = () => {
        printGroups.replaceChildren();
        const groups = new Map();

        preferredCandidates.forEach((candidate) => {
            const electionDate = candidate.electionDate || municipalityElectionDates[candidate.municipality] || "";
            const key = `${candidate.municipality}\u0000${electionDate}`;
            if (!groups.has(key)) {
                groups.set(key, { municipality: candidate.municipality, electionDate, candidates: [] });
            }
            groups.get(key).candidates.push(candidate);
        });

        groups.forEach((group) => {
            const section = document.createElement("section");
            section.className = "candidate-print-municipality";
            section.append(createText("h2", "", group.municipality));
            section.append(createText("p", "candidate-print-election-date", `Election date: ${formatElectionDate(group.electionDate)}`));

            group.candidates.forEach((candidate) => {
                const entry = document.createElement("article");
                entry.className = "candidate-print-entry";
                if (typeof candidate.photo === "string" && candidate.photo.startsWith("/images/candidates/")) {
                    const image = document.createElement("img");
                    image.src = candidate.photo;
                    image.alt = `Photo of ${candidate.name}`;
                    image.className = "candidate-print-photo";
                    entry.append(image);
                }

                const details = document.createElement("div");
                details.append(createText("h3", "", candidate.name));
                details.append(createText("p", "", `Position: ${candidate.position}`));
                entry.append(details);
                section.append(entry);
            });

            printGroups.append(section);
        });
    };

    const render = () => {
        list.replaceChildren();
        emptyMessage.classList.toggle("d-none", preferredCandidates.length > 0);
        count.textContent = `${preferredCandidates.length} selected`;
        printButton.disabled = preferredCandidates.length === 0;
        renderPrintGuide();

        preferredCandidates.forEach((candidate) => {
            const column = document.createElement("li");
            column.className = "col";
            const card = document.createElement("article");
            card.className = "card h-100";
            const body = document.createElement("div");
            body.className = "card-body";

            if (typeof candidate.photo === "string" && candidate.photo.startsWith("/images/candidates/")) {
                const image = document.createElement("img");
                image.src = candidate.photo;
                image.alt = `Photo of ${candidate.name}`;
                image.className = "rounded mb-3";
                image.style.width = "5rem";
                image.style.height = "5rem";
                image.style.objectFit = "cover";
                body.append(image);
            }

            body.append(createText("h3", "h6", candidate.name));
            body.append(createText("p", "small text-muted", `${candidate.position} · ${candidate.municipality}`));
            if (typeof candidate.profile === "string" && candidate.profile.length > 0) {
                body.append(createText("p", "mb-2", candidate.profile));
            }
            if (typeof candidate.email === "string" && candidate.email.length > 0) {
                const email = document.createElement("a");
                email.href = `mailto:${encodeURIComponent(candidate.email)}`;
                email.textContent = candidate.email;
                body.append(email);
            }
            const websiteUrl = safeWebsite(candidate.website);
            if (websiteUrl) {
                const website = document.createElement("a");
                website.href = websiteUrl;
                website.target = "_blank";
                website.rel = "noopener noreferrer";
                website.textContent = "Website";
                website.className = "d-block";
                body.append(website);
            }

            const removeButton = createText("button", "btn btn-outline-danger btn-sm mt-3", "Remove from preferred candidates");
            removeButton.type = "button";
            removeButton.addEventListener("click", () => removeCandidate(candidate.id));
            body.append(removeButton);
            card.append(body);
            column.append(card);
            list.append(column);
        });

        cards.forEach((card) => {
            const candidateId = Number(card.dataset.candidateId);
            const isSelected = preferredCandidates.some((candidate) => candidate.id === candidateId);
            const button = card.querySelector(".preferred-candidate-toggle");
            button.textContent = isSelected ? "Remove from preferred candidates" : "Add to preferred candidates";
            button.classList.toggle("btn-outline-primary", !isSelected);
            button.classList.toggle("btn-outline-danger", isSelected);
            button.setAttribute("aria-pressed", String(isSelected));
        });
    };

    const removeCandidate = (candidateId) => {
        const previous = preferredCandidates;
        preferredCandidates = preferredCandidates.filter((candidate) => candidate.id !== candidateId);
        if (!persist()) {
            preferredCandidates = previous;
            return;
        }
        render();
    };

    cards.forEach((card) => {
        card.querySelector(".preferred-candidate-toggle").addEventListener("click", () => {
            const candidateId = Number(card.dataset.candidateId);
            if (preferredCandidates.some((candidate) => candidate.id === candidateId)) {
                removeCandidate(candidateId);
                return;
            }

            const candidate = {
                id: candidateId,
                name: card.dataset.candidateName,
                position: card.dataset.candidatePosition,
                municipality: card.dataset.candidateMunicipality,
                electionDate: card.dataset.candidateElectionDate || "",
                profile: card.dataset.candidateProfile || "",
                email: card.dataset.candidateEmail || "",
                website: card.dataset.candidateWebsite || "",
                photo: card.dataset.candidatePhoto || ""
            };
            preferredCandidates = [...preferredCandidates, candidate];
            if (!persist()) {
                preferredCandidates = preferredCandidates.filter((item) => item.id !== candidateId);
                return;
            }
            render();
        });
    });

    const updatePrintDate = () => {
        printDate.textContent = new Date().toLocaleString();
    };

    printButton.addEventListener("click", () => {
        updatePrintDate();
        document.body.classList.toggle("candidate-print-without-photos", !includePhotos.checked);
        window.print();
    });
    window.addEventListener("beforeprint", () => {
        updatePrintDate();
        document.body.classList.toggle("candidate-print-without-photos", !includePhotos.checked);
    });
    window.addEventListener("afterprint", () => {
        document.body.classList.remove("candidate-print-without-photos");
    });

    render();
})();
