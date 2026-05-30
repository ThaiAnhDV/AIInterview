function requireLogin() {
    const token = localStorage.getItem("token");

    console.log("=== guard.js requireLogin ===");
    console.log("Token from localStorage:", token ? "EXISTS" : "NULL/MISSING");
    console.log("Token value:", token);

    if (!token) {
        console.log("No token - Redirecting to /Auth/Login");
        window.location.href = "/Auth/Login";
        return false;
    }
    
    console.log("Token found - returning true");
    return true;
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