async function loadSkillGapPage() {
    await loadResumesForAnalysis();
    await loadTargetJobsForAnalysis();
}

console.log("SKILL-GAP VERSION 999");

function escapeHtml(text) {
    const div = document.createElement("div");
    div.textContent = text ?? "";
    return div.innerHTML;
}

async function loadResumesForAnalysis() {
    const token = localStorage.getItem("token");

    try {
        showLoader();

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
    } catch (error) {
        console.error("Failed to load resumes for analysis:", error);
        showToast("Cannot connect to server!", "error");
    } finally {
        hideLoader();
    }
}

async function loadTargetJobsForAnalysis() {
    const token = localStorage.getItem("token");

    try {
        showLoader();

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
    } catch (error) {
        console.error("Failed to load target jobs for analysis:", error);
        showToast("Cannot connect to server!", "error");
    } finally {
        hideLoader();
    }
}

async function loadJobDescriptionForSelectedJob() {
    const token = localStorage.getItem("token");
    const targetJobId = document.getElementById("targetJobSelect").value;

    if (!targetJobId) {
        return;
    }

    try {
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
    } catch (error) {
        console.error("Failed to load job description:", error);
    }
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

    const analyzeButton = document.getElementById("analyzeSkillGapButton");
    if (analyzeButton) {
        analyzeButton.disabled = true;
        analyzeButton.classList.add("loading");
        analyzeButton.dataset.originalHtml = analyzeButton.innerHTML;
        analyzeButton.innerHTML = `<i class="fas fa-spinner fa-spin mr-2"></i> Analyzing...`;
    }

    try {
        showLoader();

        const response = await fetch(`${API_BASE_URL}/SkillGapAnalysis/analyze-detailed`, {
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
            console.error("HTTP Error:", errorText);
            showToast("Cannot analyze skill gap!", "error");
            return;
        }

        const result = await response.json();

        renderAnalysisResult(result);

        showToast("Skill gap analysis completed!", "success");
    } catch (error) {
        console.error("Network Error:", error);
        showToast("Cannot connect to server!", "error");
    } finally {
        hideLoader();

        if (analyzeButton) {
            analyzeButton.disabled = false;
            analyzeButton.classList.remove("loading");
            analyzeButton.innerHTML = analyzeButton.dataset.originalHtml || `<i class="fas fa-play mr-2"></i>Analyze Skill Gap`;
        }
    }
}

function renderAnalysisResult(result) {
    document.getElementById("analysisResult").style.display = "block";

    document.getElementById("readinessScore").innerText =
        result.readinessScore.toFixed(2);

    const resumeSkillsContainer = document.getElementById("resumeSkillsContainer");
    const requiredSkillsContainer = document.getElementById("requiredSkillsContainer");
    const matchedContainer = document.getElementById("matchedSkillsContainer");
    const missingContainer = document.getElementById("missingSkillsContainer");

    const renderTextList = (items, emptyMessage, isMissing = false) => {
        if (!items || items.length === 0) {
            return `<span class="text-ai-muted">${emptyMessage}</span>`;
        }

        return items.map(item => {
            const skillName = typeof item === "string" ? item : (item.skillName || "");
            const icon = isMissing ? "✗" : "✓";
            const itemClass = isMissing ? "text-ai-danger" : "text-ai-success";

            return `
                <div class="mb-2 ${itemClass}">
                    <span class="mr-2">${icon}</span>${escapeHtml(skillName)}
                </div>
            `;
        }).join("");
    };

    if (resumeSkillsContainer) {
        resumeSkillsContainer.innerHTML = renderTextList(result.resumeSkills, "No resume skills found.");
    }

    if (requiredSkillsContainer) {
        requiredSkillsContainer.innerHTML = renderTextList(result.requiredSkills, "No required skills found.");
    }

    if (matchedContainer) {
        matchedContainer.innerHTML = renderTextList(result.matchedSkills, "No matched skills.");
    }

    if (missingContainer) {
        missingContainer.innerHTML = renderTextList(result.missingSkills, "No missing skills. Great!", true);
    }
}
