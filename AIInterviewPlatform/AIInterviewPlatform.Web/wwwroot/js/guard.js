function requireLogin() {
    const token = localStorage.getItem("token");

    if (!token) {
        window.location.href = "/Auth/Login";
    }
}

function getCurrentUser() {
    const userJson = localStorage.getItem("user");

    if (!userJson) {
        return null;
    }

    return JSON.parse(userJson);
}

function isLoggedIn() {
    return localStorage.getItem("token") !== null;
}