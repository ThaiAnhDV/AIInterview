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

function setLoginLoading(isLoading) {
    const overlay = document.getElementById("login-loading-overlay");
    const submitBtn = document.getElementById("loginSubmitBtn");

    if (overlay) {
        overlay.classList.toggle("is-active", isLoading);
        overlay.setAttribute("aria-hidden", isLoading ? "false" : "true");
        overlay.style.display = isLoading ? "flex" : "none";
    }

    if (submitBtn) {
        submitBtn.disabled = isLoading;
    }
}

async function login() {
    const email = document.getElementById("loginEmail").value.trim();
    const password = document.getElementById("loginPassword").value;

    if (!email || !password) {
        showToast("Please enter email and password.", "error");
        return;
    }

    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!emailPattern.test(email)) {
        showToast("Email format is invalid.", "error");
        return;
    }

    setLoginLoading(true);

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

        let result = null;

        try {
            result = await response.json();
        } catch {
            result = null;
        }

        if (response.ok) {

            if (!result?.data?.token) {
                setLoginLoading(false);

                showToast(
                    "Login failed: invalid server response.",
                    "error"
                );

                return;
            }

            localStorage.setItem(
                "token",
                result.data.token
            );

            localStorage.setItem(
                "user",
                JSON.stringify(result.data)
            );

            showToast(
                "Login successfully!",
                "success"
            );

            /*
             IMPORTANT:
             keep overlay visible until redirect
            */

            setTimeout(() => {
                window.location.href = "/";
            }, 1200);

        } else {

            setLoginLoading(false);

            const serverMessage =
                result?.message ||
                result?.detail ||
                result?.title ||
                "";

            const lower = serverMessage.toLowerCase();

            if (
                response.status === 401 ||
                response.status === 400 ||
                lower.includes("invalid")
            ) {

                showToast(
                    "Email or password is invalid.",
                    "error"
                );

            } else {

                showToast(
                    serverMessage || "Login failed.",
                    "error"
                );
            }
        }

    } catch (error) {

        setLoginLoading(false);

        showToast(
            "Cannot connect to server!",
            "error"
        );
    }
}

function logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("user");

    window.location.href = "/Auth/Login";
}