async function loadJobDescription() {
    const token = localStorage.getItem("token");
    const targetJobId = document.getElementById("targetJobId").value;

    if (!targetJobId) {
        showToast("Target job id is missing!", "error");
        return;
    }

    try {
        const response = await fetch(
            `${API_BASE_URL}/TargetJobs/${targetJobId}/job-description`,
            {
                method: "GET",
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            }
        );

        if (!response.ok) {
            return;
        }

        const jd = await response.json();

        if (jd && jd.content) {
            document.getElementById("jobDescriptionContent").value = jd.content;
        }
    } catch (error) {
        console.log(error);
    }
}

async function saveJobDescription() {
    const token = localStorage.getItem("token");
    const targetJobId = document.getElementById("targetJobId").value;
    const content = document.getElementById("jobDescriptionContent").value;

    if (!targetJobId) {
        showToast("Target job id is missing!", "error");
        return;
    }

    if (!content.trim()) {
        showToast("Job description is required!", "error");
        return;
    }

    try {
        const response = await fetch(
            `${API_BASE_URL}/TargetJobs/${targetJobId}/job-description`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                },
                body: JSON.stringify({
                    content: content
                })
            }
        );

        if (!response.ok) {
            showToast("Cannot save job description!", "error");
            return;
        }

        showToast("Job description saved successfully!", "success");
    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server!", "error");
    }
}