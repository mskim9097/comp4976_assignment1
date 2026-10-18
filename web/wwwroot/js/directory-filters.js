// Candidate directory: when the Municipality choice changes, rebuild the Position list right away
// (without pressing Filter). Without JavaScript the server-rendered list is used.
(() => {
    const municipality = document.getElementById("MunicipalityId");
    const position = document.getElementById("PositionId");
    if (!municipality || !position) {
        return;
    }

    let allPositions;
    try {
        allPositions = JSON.parse(position.dataset.positions || "[]");
    } catch (error) {
        return; // keep the server-rendered list
    }

    const addOption = (value, text, selected) => {
        const option = document.createElement("option");
        option.value = value;
        option.textContent = text;
        option.selected = selected;
        position.append(option);
    };

    const rebuild = () => {
        const municipalityId = municipality.value;
        const selected = position.value;

        position.replaceChildren();
        addOption("", "All positions", false);

        let stillAvailable = false;
        allPositions
            .filter((item) => !municipalityId || String(item.municipalityId) === municipalityId)
            .forEach((item) => {
                const isSelected = String(item.positionId) === selected;
                stillAvailable = stillAvailable || isSelected;
                addOption(
                    String(item.positionId),
                    municipalityId ? item.name : `${item.municipalityName} - ${item.name}`,
                    isSelected);
            });

        // A position that belongs to another municipality can no longer be selected.
        if (!stillAvailable) {
            position.value = "";
        }
    };

    municipality.addEventListener("change", rebuild);
})();
