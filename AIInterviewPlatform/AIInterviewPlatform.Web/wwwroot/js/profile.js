async function loadProfile() {
    const token = localStorage.getItem("token");

    try {
        const response = await fetch(`${API_BASE_URL}/Profile/me`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) {
            showToast("Please login again!", "error");

            setTimeout(() => {
                logout();
            }, 1000);

            return;
        }

        const profile = await response.json();

        document.getElementById("profileDisplayName").innerText =
            profile.fullName || "Unknown User";

        document.getElementById("profileDisplayEmail").innerText =
            profile.email || "";

        document.getElementById("profileDisplayEducation").innerText =
            profile.educationLevel || "Preparing for better interviews";

        document.getElementById("navbarUserName").innerText =
            profile.fullName || profile.email || "";

        document.getElementById("profileFullName").value =
            profile.fullName || "";

        document.getElementById("profileEmail").value =
            profile.email || "";

        document.getElementById("profilePhone").value =
            profile.phone || "";

        document.getElementById("profileEducationLevel").value =
            profile.educationLevel || "";

        document.getElementById("profileCareerGoal").value =
            profile.careerGoal || "";
    } catch (error) {
        showToast("Cannot load profile!", "error");
    }
}

async function updateProfile() {
    const token = localStorage.getItem("token");

    const data = {
        fullName: document.getElementById("profileFullName").value,
        phone: document.getElementById("profilePhone").value,
        educationLevel: document.getElementById("profileEducationLevel").value,
        careerGoal: document.getElementById("profileCareerGoal").value
    };

    try {
        const response = await fetch(`${API_BASE_URL}/Profile/me`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            showToast("Profile updated successfully!", "success");
            loadProfile();
        } else {
            showToast("Update profile failed!", "error");
        }
    } catch (error) {
        showToast("Cannot connect to server!", "error");
    }
}