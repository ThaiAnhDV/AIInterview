async function register() {
    const fullName = document.getElementById("registerFullName").value;
    const email = document.getElementById("registerEmail").value;
    const password = document.getElementById("registerPassword").value;

    const response = await fetch(`${API_BASE_URL}/Auth/register`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            fullName: fullName,
            email: email,
            password: password
        })
    });

    const result = await response.json();

    if (response.ok) {
        alert("Register successfully!");
        window.location.href = "/auth/login.html";
    } else {
        alert(result.message || "Register failed!");
    }
}

async function login() {
    const email = document.getElementById("loginEmail").value;
    const password = document.getElementById("loginPassword").value;

    const response = await fetch(`${API_BASE_URL}/Auth/login`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            email: email,
            password: password
        })
    });

    const result = await response.json();

    if (response.ok) {
        localStorage.setItem("token", result.data.token);
        localStorage.setItem("user", JSON.stringify(result.data));

        alert("Login successfully!");
        window.location.href = "/profile/profile.html";
    } else {
        alert(result.message || "Login failed!");
    }
}

function logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("user");

    window.location.href = "/auth/login.html";
}