document.addEventListener("DOMContentLoaded", () => {
    const overlay = document.getElementById("ai-loading-overlay");
    const overlayTitle = document.getElementById("ai-loading-title");
    const overlayMessage = document.getElementById("ai-loading-message");

    const showAiLoading = (title, message) => {
        if (overlay && overlayTitle && overlayMessage) {
            overlayTitle.textContent = title;
            overlayMessage.textContent = message;
            overlay.classList.remove("hidden");
            overlay.classList.add("flex");
        }
    };

    document.querySelectorAll("form[data-ai-loading='true']").forEach((form) => {
        form.addEventListener("submit", (event) => {
            if (!form.checkValidity()) {
                return;
            }

            const title = form.getAttribute("data-ai-loading-title") || "AI analysis in progress";
            const message = form.getAttribute("data-ai-loading-message") || "Please wait while the system processes the complaint.";

            showAiLoading(title, message);

            form.querySelectorAll("button[type='submit']").forEach((button) => {
                button.disabled = true;
                button.classList.add("opacity-70", "cursor-not-allowed");
            });
        });
    });
});
