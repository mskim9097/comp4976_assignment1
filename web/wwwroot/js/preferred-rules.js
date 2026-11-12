// Rules for the preferred-candidates list (used by the directory page and the candidate details page).
// A voter lives in one municipality and can only choose as many candidates for a position as it has seats.
window.PreferredRules = {
    // Returns a message when the candidate cannot be added to the list, or null when it is allowed.
    check(selected, candidate) {
        if (selected.length > 0 && selected[0].municipality !== candidate.municipality) {
            return `You can only choose candidates from one municipality (${selected[0].municipality}). ` +
                `Remove your current choices first to choose candidates from ${candidate.municipality}.`;
        }

        const seats = Number(candidate.seats);
        if (Number.isInteger(seats) && seats > 0) {
            const chosen = selected.filter((item) =>
                item.municipality === candidate.municipality && item.position === candidate.position).length;
            if (chosen >= seats) {
                return `${candidate.position} in ${candidate.municipality} has ${seats} ${seats === 1 ? "seat" : "seats"}, ` +
                    `so you can choose at most ${seats}. Remove a choice first to pick someone else.`;
            }
        }

        return null;
    }
};
