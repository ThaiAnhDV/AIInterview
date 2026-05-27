function showToast(message, type = "success") {
    const toast = document.createElement("div");

    toast.className = `custom-toast ${type}`;
    toast.innerHTML = message;

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.classList.add("show");
    }, 100);

    setTimeout(() => {
        toast.classList.remove("show");

        setTimeout(() => {
            toast.remove();
        }, 400);
    }, 2500);
}

function toggleSidebar() {
    const sidebar = document.getElementById("sidebar");
    const mainContent = document.getElementById("mainContent");

    sidebar.classList.toggle("hide");
    mainContent.classList.toggle("sidebar-open");
}