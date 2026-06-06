async function loadProfile() {
    const token = localStorage.getItem("token");
    
    console.log("[Profile] Loading profile data...");
    console.log("[Profile] Token:", token ? "EXISTS" : "NULL/MISSING");
    console.log("[Profile] API_BASE_URL:", API_BASE_URL);

    if (!token) {
        console.log("[Profile] NO TOKEN - Redirecting to login");
        showToast("Please login again!", "error");
        setTimeout(() => {
            logout();
        }, 1000);
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/Profile/me`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        console.log("[Profile] Response Status:", response.status);

        if (!response.ok) {
            const errorText = await response.text();
            console.log("[Profile] Error Response:", errorText);
            
            showToast("Please login again!", "error");
            setTimeout(() => {
                logout();
            }, 1000);
            return;
        }

        const profile = await response.json();
        console.log("[Profile] Profile Data:", profile);

        // Update display fields
        document.getElementById("profileDisplayName").textContent =
            profile.fullName || "Unknown User";

        document.getElementById("profileDisplayEmail").textContent =
            profile.email || "";

        document.getElementById("profileDisplayEducation").textContent =
            profile.educationLevel || "Preparing for better interviews";

        // Update avatar initials
        const displayName = profile.fullName || profile.email || "U";
        const initials = displayName
            .split(" ")
            .map(n => n[0])
            .join("")
            .substring(0, 2)
            .toUpperCase();
        
        const initialsElement = document.getElementById("profileAvatarInitials");
        if (initialsElement) {
            initialsElement.textContent = initials;
        }

        // Update form fields
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
            
        console.log("[Profile] Profile loaded successfully");
    } catch (error) {
        console.error("[Profile] Error loading profile:", error);
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

    console.log("[Profile] Updating profile:", data);

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
        console.error("[Profile] Error updating profile:", error);
        showToast("Cannot connect to server!", "error");
    }
}