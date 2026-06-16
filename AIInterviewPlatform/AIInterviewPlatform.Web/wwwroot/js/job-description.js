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

        if (jd && jd.id) {
            document.getElementById("jobDescriptionId").value = jd.id;
        }

        if (jd && jd.content) {
            document.getElementById("jobDescriptionContent").value = jd.content;
        }

        if (jd && jd.id) {
            await loadRequiredSkills(jd.id);
        }

    } catch (error) {
        console.error(error);
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

        const jd = await response.json();

        if (jd && jd.id) {
            document.getElementById("jobDescriptionId").value = jd.id;
        }

        showToast("Job description saved successfully!", "success");

    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server!", "error");
    }
}

window.showExtractLoading = function () {
    const container = document.getElementById("requiredSkillsContainer");
    if (!container) return;
    container.innerHTML = `
        <div class="text-center py-3">
            <i class="fas fa-spinner fa-spin fa-2x text-primary"></i>
            <div class="mt-2 text-muted">Extracting skills...</div>
        </div>
    `;
};

window.hideExtractLoading = function () {
    const container = document.getElementById("requiredSkillsContainer");
    if (!container) return;
    container.innerHTML = `
        <div class="text-muted">
            No skills extracted yet.
        </div>
    `;
};

async function extractSkills() {
    console.log('EXTRACT CLICK');
showLoader();
    const token = localStorage.getItem("token");
    let jobDescriptionId = document.getElementById("jobDescriptionId").value;

    if (!jobDescriptionId) {
        await saveJobDescription();
        jobDescriptionId = document.getElementById("jobDescriptionId").value;
    }

    if (!jobDescriptionId) {
        showToast("Please save job description first!", "error");
        return;
    }

    //window.showExtractLoading();
    showLoader();

    try {
        const response = await fetch(
            `${API_BASE_URL}/Skills/extract/${jobDescriptionId}`,
            {
                method: "POST",
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            }
        );

        if (!response.ok) {
            showToast("Cannot extract skills!", "error");
            return;
        }

        showToast("Skills extracted successfully!", "success");

        await loadRequiredSkills(jobDescriptionId);
    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server!", "error");
    } finally {
        hideLoader();
        window.hideExtractLoading();
    }
}

async function loadRequiredSkills(jobDescriptionId) {
    const token = localStorage.getItem("token");

    try {
        const response = await fetch(
            `${API_BASE_URL}/Skills/required/${jobDescriptionId}`,
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

        const skills = await response.json();
        const container = document.getElementById("requiredSkillsContainer");

        if (!skills || skills.length === 0) {
            container.innerHTML = `
                <div class="text-muted">
                    No skills extracted yet.
                </div>
            `;
            return;
        }

        container.innerHTML = "";

        skills.forEach(skill => {
            container.innerHTML += `
                <span class="badge badge-primary mr-2 mb-2 p-2">
                    ${skill.skillName}
                </span>
            `;
        });

    } catch (error) {
        console.error(error);
    }
}