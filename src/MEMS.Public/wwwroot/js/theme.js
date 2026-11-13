// Light / dark color theme switch (Bootstrap color modes). The saved choice is applied by a small script in <head>
// before the page is drawn; this file handles the button and keeps printouts light.
// The click handler listens on the document because Blazor draws the button again when the page changes.
(() => {
    const storageKey = "municipal-elections.theme";
    const root = document.documentElement;
    const current = () => (root.getAttribute("data-bs-theme") === "dark" ? "dark" : "light");

    document.addEventListener("click", (event) => {
        if (!event.target.closest("#theme-toggle")) {
            return;
        }
        const next = current() === "dark" ? "light" : "dark";
        root.setAttribute("data-bs-theme", next);
        try {
            window.localStorage.setItem(storageKey, next);
        } catch (error) {
            // The choice just is not remembered for the next visit.
        }
    });

    // The printed voting guide always uses the light theme (dark text on white paper).
    let themeBeforePrint = null;
    window.addEventListener("beforeprint", () => {
        themeBeforePrint = current();
        root.setAttribute("data-bs-theme", "light");
    });
    window.addEventListener("afterprint", () => {
        if (themeBeforePrint) {
            root.setAttribute("data-bs-theme", themeBeforePrint);
            themeBeforePrint = null;
        }
    });
})();
