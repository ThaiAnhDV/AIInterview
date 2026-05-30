let currentRoadmapId = null;

async function loadRoadmaps() {
    const token = localStorage.getItem("token");

    try {
        const response = await fetch(`${API_BASE_URL}/Roadmaps/my`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) {
            showToast("Cannot load roadmaps.", "error");
            return;
        }

        const roadmaps = await response.json();
        const container = document.getElementById("roadmapList");

        if (!roadmaps || roadmaps.length === 0) {
            container.innerHTML = `
                <div class="text-muted">
                    No roadmaps found.
                </div>
            `;
            return;
        }

        container.innerHTML = "";

        roadmaps.forEach(roadmap => {
            container.innerHTML += `
                <div class="d-flex justify-content-between align-items-center border rounded p-3 mb-2">
                    <div>
                        <strong>${roadmap.roadmapTitle}</strong>
                        <br />
                        <small>Status: ${roadmap.roadmapStatus}</small>
                        <br />
                        <small>Progress: ${Number(roadmap.completionPercentage).toFixed(2)}%</small>
                    </div>

                    <button class="btn btn-primary btn-sm"
                            onclick="loadRoadmapDetail(${roadmap.id})">
                        View
                    </button>
                </div>
            `;
        });

    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server.", "error");
    }
}

async function generateRoadmap() {
    const token = localStorage.getItem("token");

    const skillGapAnalysisId =
        document.getElementById("skillGapAnalysisId").value;

    if (!skillGapAnalysisId) {
        showToast("Please enter Skill Gap Analysis Id.", "error");
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/Roadmaps/generate`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({
                skillGapAnalysisId: Number(skillGapAnalysisId)
            })
        });

        if (!response.ok) {
            const errorText = await response.text();
            console.error(errorText);
            showToast("Cannot generate roadmap.", "error");
            return;
        }

        const roadmap = await response.json();

        showToast("Roadmap generated successfully.", "success");

        await loadRoadmaps();
        await loadRoadmapDetail(roadmap.id);

    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server.", "error");
    }
}

async function loadRoadmapDetail(id) {
    const token = localStorage.getItem("token");

    try {
        const response = await fetch(`${API_BASE_URL}/Roadmaps/${id}`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) {
            showToast("Cannot load roadmap detail.", "error");
            return;
        }

        const roadmap = await response.json();
        currentRoadmapId = roadmap.id;

        renderRoadmapDetail(roadmap);

    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server.", "error");
    }
}

function renderRoadmapDetail(roadmap) {
    const card = document.getElementById("roadmapDetailCard");
    const title = document.getElementById("roadmapTitle");
    const progressText = document.getElementById("roadmapProgress");
    const progressBar = document.getElementById("roadmapProgressBar");
    const milestoneContainer = document.getElementById("milestoneContainer");

    card.style.display = "block";

    title.innerText = roadmap.roadmapTitle;

    const progress = Number(roadmap.completionPercentage).toFixed(2);

    progressText.innerText = `${progress}%`;
    progressBar.style.width = `${progress}%`;
    progressBar.innerText = `${progress}%`;

    milestoneContainer.innerHTML = "";

    if (!roadmap.milestones || roadmap.milestones.length === 0) {
        milestoneContainer.innerHTML = `
            <div class="text-muted">
                No milestones found.
            </div>
        `;
        return;
    }

    roadmap.milestones.forEach(milestone => {
        milestoneContainer.innerHTML += `
            <div class="card mb-3 border-0 shadow-sm">
                <div class="card-header">
                    <strong>
                        ${milestone.milestoneOrder}. ${milestone.milestoneTitle}
                    </strong>

                    ${milestone.isCompleted
                ? `<span class="badge badge-success ml-2">Completed</span>`
                : `<span class="badge badge-warning ml-2">In Progress</span>`
            }
                </div>

                <div class="card-body">
                    ${milestone.activities.map(activity => `
                            <div class="d-flex justify-content-between align-items-center border-bottom py-2">
                                <div>
                                    <strong>${activity.activityTitle}</strong>
                                    <br />
                                    <small>${activity.activityDescription ?? ""}</small>
                                    <br />
                                    <span class="badge badge-info">
                                        ${activity.activityType ?? "OTHER"}
                                    </span>

                                    ${activity.isCompleted
                    ? `<span class="badge badge-success ml-1">Done</span>`
                    : `<span class="badge badge-secondary ml-1">Pending</span>`
                }
                                </div>

                                ${activity.isCompleted
                    ? `<button class="btn btn-secondary btn-sm" disabled>
                                               Completed
                                           </button>`
                    : `<button class="btn btn-success btn-sm"
                                                   onclick="completeActivity(${activity.id})">
                                               Mark Complete
                                           </button>`
                }
                            </div>
                        `).join("")
            }
                </div>
            </div>
        `;
    });
}

async function completeActivity(activityId) {
    const token = localStorage.getItem("token");

    try {
        const response = await fetch(
            `${API_BASE_URL}/RoadmapActivities/${activityId}/complete`,
            {
                method: "PUT",
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            }
        );

        if (!response.ok) {
            showToast("Cannot complete activity.", "error");
            return;
        }

        showToast("Activity completed.", "success");

        await loadRoadmaps();

        if (currentRoadmapId) {
            await loadRoadmapDetail(currentRoadmapId);
        }

    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server.", "error");
    }
}