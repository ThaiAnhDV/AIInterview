async function loadSkillGapPage() {
    await loadResumesForAnalysis();
    await loadTargetJobsForAnalysis();
}

async function loadResumesForAnalysis() {
    const token = localStorage.getItem("token");

    const response = await fetch(`${API_BASE_URL}/Resume/my-resumes`, {
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });

    const resumes = await response.json();
    const select = document.getElementById("resumeSelect");

    select.innerHTML = "";

    resumes.forEach(resume => {
        select.innerHTML += `
            <option value="${resume.id}">
                ${resume.fileName}
            </option>
        `;
    });
}

async function loadTargetJobsForAnalysis() {
    const token = localStorage.getItem("token");

    const response = await fetch(`${API_BASE_URL}/TargetJobs/my`, {
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });

    const jobs = await response.json();
    const select = document.getElementById("targetJobSelect");

    select.innerHTML = "";

    jobs.forEach(job => {
        select.innerHTML += `
            <option value="${job.id}">
                ${job.jobTitle}
            </option>
        `;
    });

    if (jobs.length > 0) {
        await loadJobDescriptionForSelectedJob();
    }
}

async function loadJobDescriptionForSelectedJob() {
    const token = localStorage.getItem("token");
    const targetJobId = document.getElementById("targetJobSelect").value;

    if (!targetJobId) {
        return;
    }

    const response = await fetch(
        `${API_BASE_URL}/TargetJobs/${targetJobId}/job-description`,
        {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        }
    );

    if (!response.ok) {
        document.getElementById("jobDescriptionId").value = "";
        return;
    }

    const jd = await response.json();

    document.getElementById("jobDescriptionId").value = jd.id;
}

async function runSkillGapAnalysis() {
    const token = localStorage.getItem("token");

    const resumeId = document.getElementById("resumeSelect").value;
    const jobDescriptionId = document.getElementById("jobDescriptionId").value;

    if (!resumeId) {
        showToast("Please select a resume!", "error");
        return;
    }

    if (!jobDescriptionId) {
        showToast("This target job has no job description!", "error");
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/SkillGapAnalysis/analyze`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({
                resumeId: Number(resumeId),
                jobDescriptionId: Number(jobDescriptionId)
            })
        });

        if (!response.ok) {
            const errorText = await response.text();
            console.error(errorText);
            showToast("Cannot analyze skill gap!", "error");
            return;
        }

        const result = await response.json();

        renderAnalysisResult(result);

        showToast("Skill gap analysis completed!", "success");

    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server!", "error");
    }
}

function renderAnalysisResult(result) {
    document.getElementById("analysisResult").style.display = "block";

    document.getElementById("readinessScore").innerText =
        result.readinessScore.toFixed(2);

    const matchedContainer =
        document.getElementById("matchedSkillsContainer");

    matchedContainer.innerHTML = "";

    if (!result.matchedSkills || result.matchedSkills.length === 0) {
        matchedContainer.innerHTML =
            `<span class="text-muted">No matched skills.</span>`;
    } else {
        result.matchedSkills.forEach(skill => {
            matchedContainer.innerHTML += `
                <span class="badge badge-success mr-2 mb-2 p-2">
                    ${skill}
                </span>
            `;
        });
    }

    const missingContainer =
        document.getElementById("missingSkillsContainer");

    missingContainer.innerHTML = "";

    if (!result.missingSkills || result.missingSkills.length === 0) {
        missingContainer.innerHTML =
            `<span class="text-success">No missing skills. Great!</span>`;
    } else {
        result.missingSkills.forEach(item => {
            missingContainer.innerHTML += `
                <div class="alert alert-warning">
                    <strong>${item.skillName}</strong>
                    <br />
                    ${item.gapDescription ?? ""}
                </div>
            `;
        });
    }
}