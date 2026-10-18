// Light / dark color theme switch (Bootstrap color modes). The saved choice is applied by a small script in <head>;
// this file wires up the button and keeps printouts light.
(() => {
    const storageKey = "municipal-elections.theme";
    const root = document.documentElement;
    const button = document.getElementById("theme-toggle");

    const current = () => (root.getAttribute("data-bs-theme") === "dark" ? "dark" : "light");

    const render = () => {
        if (!button) {
            return;
        }
        const dark = current() === "dark";
        button.textContent = dark ? "Light mode" : "Dark mode";
        button.setAttribute("aria-pressed", String(dark));
        button.setAttribute("aria-label", dark ? "Switch to light mode" : "Switch to dark mode");
    };

    if (button) {
        button.addEventListener("click", () => {
            const next = current() === "dark" ? "light" : "dark";
            root.setAttribute("data-bs-theme", next);
            try {
                window.localStorage.setItem(storageKey, next);
            } catch (error) {
                // The choice just is not remembered for the next visit.
            }
            render();
        });
    }

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

    render();
})();
