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

function logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("user");

    window.location.href = "/Auth/Login";
}