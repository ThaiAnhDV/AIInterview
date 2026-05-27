const TARGET_JOB_API = `${API_BASE_URL}/TargetJobs`;

async function createTargetJob() {
    const token = localStorage.getItem("token");

    const jobTitle = document.getElementById("jobTitle").value;
    const industry = document.getElementById("industry").value;
    const experienceLevel = document.getElementById("experienceLevel").value;

    if (!jobTitle.trim()) {
        showToast("Job title is required!", "error");
        return;
    }

    try {
        const response = await fetch(TARGET_JOB_API, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({
                jobTitle,
                industry,
                experienceLevel
            })
        });

        if (!response.ok) {
            showToast("Cannot create target job!", "error");
            return;
        }

        showToast("Target job created successfully!", "success");

        document.getElementById("jobTitle").value = "";
        document.getElementById("industry").value = "";
        document.getElementById("experienceLevel").value = "";

        await loadTargetJobs();
    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server!", "error");
    }
}

async function loadTargetJobs() {
    const token = localStorage.getItem("token");

    try {
        const response = await fetch(`${TARGET_JOB_API}/my`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) {
            showToast("Cannot load target jobs!", "error");
            return;
        }

        const jobs = await response.json();
        const tableBody = document.getElementById("targetJobTableBody");

        tableBody.innerHTML = "";

        if (!jobs || jobs.length === 0) {
            tableBody.innerHTML = `
                <tr>
                    <td colspan="5" class="text-center text-muted">
                        No target jobs yet.
                    </td>
                </tr>
            `;
            return;
        }

        jobs.forEach(job => {
            tableBody.innerHTML += `
                <tr>
                    <td>${job.jobTitle}</td>
                    <td>${job.industry ?? ""}</td>
                    <td>${job.experienceLevel ?? ""}</td>
                    <td>${new Date(job.createdAt).toLocaleString()}</td>
                    <td>
                        <a href="/TargetJob/JobDescription?targetJobId=${job.id}"
                           class="btn btn-sm btn-info">
                            JD
                        </a>

                        <button onclick="deleteTargetJob(${job.id})"
                                class="btn btn-sm btn-danger">
                            Delete
                        </button>
                    </td>
                </tr>
            `;
        });
    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server!", "error");
    }
}

async function deleteTargetJob(jobId) {
    const confirmDelete = confirm("Are you sure you want to delete this target job?");

    if (!confirmDelete) {
        return;
    }

    const token = localStorage.getItem("token");

    try {
        const response = await fetch(`${TARGET_JOB_API}/${jobId}`, {
            method: "DELETE",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) {
            showToast("Cannot delete target job!", "error");
            return;
        }

        showToast("Target job deleted successfully!", "success");

        await loadTargetJobs();
    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server!", "error");
    }
}