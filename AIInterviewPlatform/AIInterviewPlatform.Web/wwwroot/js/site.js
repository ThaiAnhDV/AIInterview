function showToast(message, type = "success") {
    let stack = document.getElementById("notitree") || document.getElementById("notification-stack");
    if (!stack) {
        stack = document.createElement("div");
        stack.id = "notitree";
        stack.className = "notification-stack";
        document.body.appendChild(stack);
    }

    const noti = document.createElement("div");
    const variant = type === "error" ? "is-error" : "is-success";
    const translate = window.t || ((text) => text);
    const title = type === "error" ? translate("Error") : translate("Success");

    noti.className = `notification ${variant}`;
    noti.innerHTML = `
        <div class="notiborderglow"></div>
        <div class="notiglow"></div>
        <div class="notititle">${title}</div>
        <div class="notibody">${String(translate(message ?? ""))}</div>
    `;

    stack.appendChild(noti);

    // Trigger animation
    requestAnimationFrame(() => {
        noti.classList.add("is-visible");
    });

    const removeNoti = () => {
        noti.classList.remove("is-visible");
        setTimeout(() => {
            noti.remove();
            if (stack && stack.childElementCount === 0) stack.remove();
        }, 220);
    };

    // Auto-dismiss
    const timer = setTimeout(removeNoti, 2500);

    // Click to dismiss
    noti.addEventListener("click", () => {
        clearTimeout(timer);
        removeNoti();
    });
}
