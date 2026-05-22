async function register() {
    const fullName = document.getElementById("registerFullName").value;
    const email = document.getElementById("registerEmail").value;
    const password = document.getElementById("registerPassword").value;

    try {
        const response = await fetch(`${API_BASE_URL}/Auth/register`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                fullName,
                email,
                password
            })
        });

        const result = await response.json();

        if (response.ok) {
            showToast("Account created successfully!", "success");

            setTimeout(() => {
                window.location.href = "/Auth/Login";
            }, 1200);
        } else {
            showToast(result.message || "Register failed!", "error");
        }
    } catch (error) {
        showToast("Cannot connect to server!", "error");
    }
}

async function login() {
    const email = document.getElementById("loginEmail").value;
    const password = document.getElementById("loginPassword").value;

    try {
        const response = await fetch(`${API_BASE_URL}/Auth/login`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                email,
                password
            })
        });

        const result = await response.json();

        if (response.ok) {
            localStorage.setItem("token", result.data.token);
            localStorage.setItem("user", JSON.stringify(result.data));

            // Sync token to server session
            try {
                await fetch("/Auth/Login?handler=SetToken", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({ token: result.data.token })
                });
            } catch (err) {
                console.error("Failed to sync token to session", err);
            }

            showToast("Login successfully!", "success");

            setTimeout(() => {
                window.location.href = "/";
            }, 1200);
        } else {
            showToast(result.message || "Invalid email or password!", "error");
        }
    } catch (error) {
        showToast("Cannot connect to server!", "error");
    }
}

async function logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("user");

    try {
        await fetch("/Auth/Login?handler=ClearToken", {
            method: "POST"
        });
    } catch (err) {
        console.error("Failed to clear token from session", err);
    }

    window.location.href = "/Auth/Login";
}

// Auto-sync token to session when visiting login page
document.addEventListener("DOMContentLoaded", async () => {
    const currentPath = window.location.pathname.toLowerCase();
    if (currentPath === "/auth/login") {
        const token = localStorage.getItem("token");
        if (token) {
            try {
                const response = await fetch("/Auth/Login?handler=SetToken", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({ token: token })
                });
                if (response.ok) {
                    window.location.href = "/";
                }
            } catch (err) {
                console.error("Failed to auto-sync session token", err);
            }
        }
    }
});