function requireLogin() {
    const token = localStorage.getItem("token");

    if (!token || token.trim() === "") {
        window.location.href = "/Auth/Login";
        return false;
    }

    return true;
}

function logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    window.location.href = "/Auth/Login";
}