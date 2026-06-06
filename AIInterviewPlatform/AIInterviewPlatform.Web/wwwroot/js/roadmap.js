// ===== ROADMAP MODULE =====
console.log("Roadmap script loaded");

let currentRoadmapId = null;

// DOM Element Cache
const Elements = {
    get: function(id) {
        const el = document.getElementById(id);
        if (!el) {
            console.error(`Element with id "${id}" not found`);
        }
        return el;
    }
};

document.addEventListener("DOMContentLoaded", async () => {
    console.log("DOMContentLoaded fired");
    try {
        await loadSkillGapAnalyses();
    } catch (error) {
        console.error("loadSkillGapAnalyses failed:", error);
    }
});

// ===== ROADMAP LIST =====
async function loadRoadmaps() {
    showRoadmapsLoading();
    
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
            hideAllRoadmapStates();
            return;
        }

        const data = await response.json();
        
        // Handle both array and wrapped response
        const roadmaps = Array.isArray(data) ? data : (data.data || []);

        hideAllRoadmapStates();

        if (!roadmaps || roadmaps.length === 0) {
            showRoadmapsEmpty();
            return;
        }

        renderRoadmaps(roadmaps);

    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server.", "error");
        hideAllRoadmapStates();
    }
}

function showRoadmapsLoading() {
    const el = Elements.get("roadmapsLoading");
    const listEl = Elements.get("roadmapsList");
    const emptyEl = Elements.get("roadmapsEmpty");
    
    if (el) el.style.display = "grid";
    if (emptyEl) emptyEl.style.display = "none";
    if (listEl) listEl.style.display = "none";
}

function showRoadmapsEmpty() {
    const el = Elements.get("roadmapsEmpty");
    const loadingEl = Elements.get("roadmapsLoading");
    const listEl = Elements.get("roadmapsList");
    
    if (loadingEl) loadingEl.style.display = "none";
    if (el) el.style.display = "block";
    if (listEl) listEl.style.display = "none";
}

function hideAllRoadmapStates() {
    const loadingEl = Elements.get("roadmapsLoading");
    const emptyEl = Elements.get("roadmapsEmpty");
    
    if (loadingEl) loadingEl.style.display = "none";
    if (emptyEl) emptyEl.style.display = "none";
}

function renderRoadmaps(roadmaps) {
    const container = Elements.get("roadmapsList");
    
    if (!container) {
        console.error("Cannot render roadmaps: container element not found");
        return;
    }
    
    container.style.display = "grid";
    container.innerHTML = roadmaps.map(roadmap => createRoadmapCard(roadmap)).join('');
}

function createRoadmapCard(roadmap) {
    const progress = Number(roadmap.completionPercentage || 0).toFixed(0);
    const circumference = 2 * Math.PI * 25;
    const offset = circumference - (progress / 100) * circumference;
    const createdDate = new Date(roadmap.createdAt).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
    });
    const statusBadge = getStatusBadge(roadmap.roadmapStatus);

    return `
        <div class="roadmap-card" onclick="console.log('roadmap card clicked', ${roadmap.id}); loadRoadmapDetail(${roadmap.id})">
            <div class="roadmap-card-header">
                <div>
                    <h3 class="roadmap-card-title">${escapeHtml(roadmap.roadmapTitle || 'Untitled Roadmap')}</h3>
                    <span class="roadmap-card-date">${createdDate}</span>
                </div>
                <div class="progress-ring-container">
                    <svg class="progress-ring" width="64" height="64">
                        <circle class="progress-ring-bg" cx="32" cy="32" r="25"/>
                        <circle class="progress-ring-fill" cx="32" cy="32" r="25" 
                                style="stroke-dashoffset: ${offset}"/>
                    </svg>
                    <span class="progress-ring-text">${progress}%</span>
                </div>
            </div>
            <div class="roadmap-card-stats">
                <div class="roadmap-stat">
                    <i class="fas fa-flag-checkered"></i>
                    <span>${roadmap.totalMilestones || 0} milestones</span>
                </div>
                <div class="roadmap-stat">
                    <i class="fas fa-tasks"></i>
                    <span>${roadmap.totalActivities || 0} activities</span>
                </div>
            </div>
            <div style="margin-top: 16px;">
                ${statusBadge}
            </div>
        </div>
    `;
}

// ===== ROADMAP DETAIL =====
async function loadRoadmapDetail(id) {
    showMilestoneLoading();
    showRoadmapDetail();
    
    const token = localStorage.getItem("token");
    currentRoadmapId = id;

    try {
        const response = await fetch(`${API_BASE_URL}/Roadmaps/${id}`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) {
            showToast("Cannot load roadmap detail.", "error");
            hideMilestoneLoading();
            return;
        }

        const data = await response.json();
        const roadmap = data.data || data;
        
        renderRoadmapDetail(roadmap);

    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server.", "error");
        hideMilestoneLoading();
    }
}

function showMilestoneLoading() {
    const loadingEl = Elements.get("milestoneLoading");
    const containerEl = Elements.get("milestoneContainer");
    
    if (loadingEl) loadingEl.style.display = "block";
    if (containerEl) containerEl.style.display = "none";
}

function hideMilestoneLoading() {
    const loadingEl = Elements.get("milestoneLoading");
    const containerEl = Elements.get("milestoneContainer");
    
    if (loadingEl) loadingEl.style.display = "none";
    if (containerEl) containerEl.style.display = "block";
}

function showRoadmapDetail() {
    const detailPanel = document.getElementById("roadmapDetailPanel");
    console.log("detail panel found", !!detailPanel);

    if (!detailPanel) {
        console.error("roadmapDetailPanel not found");
        return;
    }

    detailPanel.classList.add("active");
    detailPanel.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

function closeRoadmapDetail() {
    const detailPanel = document.getElementById("roadmapDetailPanel");

    if (detailPanel) {
        detailPanel.classList.remove("active");
    }
    currentRoadmapId = null;
}

function renderRoadmapDetail(roadmap) {
    const progress = Number(roadmap.completionPercentage || 0).toFixed(0);
    
    const titleEl = Elements.get("roadmapDetailTitle");
    const progressBarEl = Elements.get("roadmapDetailProgressBar");
    const progressTextEl = Elements.get("roadmapDetailProgressText");
    
    if (titleEl) titleEl.textContent = roadmap.roadmapTitle || 'Untitled Roadmap';
    if (progressBarEl) progressBarEl.style.width = `${progress}%`;
    if (progressTextEl) progressTextEl.textContent = `${progress}%`;

    hideMilestoneLoading();

    const detailPanel = document.getElementById("roadmapDetailPanel");
    console.log("detail panel found", !!detailPanel);

    if (!detailPanel) {
        console.error("roadmapDetailPanel not found");
        return;
    }

    if (!roadmap.milestones || roadmap.milestones.length === 0) {
        detailPanel.innerHTML = `
            <div class="empty-state">
                <div class="empty-state-icon">
                    <i class="fas fa-flag"></i>
                </div>
                <h3>No Milestones Yet</h3>
                <p>This roadmap doesn't have any milestones yet.</p>
            </div>
        `;
        console.log("detail rendered");
        return;
    }

    detailPanel.innerHTML = roadmap.milestones
        .sort((a, b) => a.milestoneOrder - b.milestoneOrder)
        .map(milestone => createMilestoneCard(milestone))
        .join('');

    console.log("detail rendered");
}

function createMilestoneCard(milestone) {
    const statusClass = getMilestoneStatusClass(milestone);
    const statusBadge = getMilestoneStatusBadge(milestone);
    const markerClass = milestone.isCompleted ? 'completed' : '';
    const activities = milestone.activities || [];

    return `
        <div class="milestone-card">
            <div class="milestone-marker ${markerClass}">
                ${milestone.isCompleted ? '<i class="fas fa-check"></i>' : milestone.milestoneOrder}
            </div>
            <div class="milestone-content">
                <div class="milestone-header">
                    <h4 class="milestone-title">${escapeHtml(milestone.milestoneTitle)}</h4>
                    <div class="milestone-actions">
                        ${statusBadge}
                    </div>
                </div>
                <div class="activities-list">
                    ${activities.length > 0 
                        ? activities.map(activity => createActivityItem(activity)).join('')
                        : '<p class="text-muted" style="padding: 16px 0; margin: 0;">No activities in this milestone.</p>'
                    }
                </div>
            </div>
        </div>
    `;
}

function createActivityItem(activity) {
    const completedClass = activity.isCompleted ? 'completed' : '';
    const checkboxClass = activity.isCompleted ? 'completed' : '';
    const typeBadge = getActivityTypeBadge(activity.activityType);

    return `
        <div class="activity-item ${completedClass}">
            <div class="activity-checkbox ${checkboxClass}" onclick="toggleActivity(event, ${activity.id}, ${!activity.isCompleted})">
                ${activity.isCompleted ? '<i class="fas fa-check"></i>' : ''}
            </div>
            <div class="activity-content">
                <h5 class="activity-title">${escapeHtml(activity.activityTitle)}</h5>
                ${activity.activityDescription ? `<p class="activity-description">${escapeHtml(activity.activityDescription)}</p>` : ''}
                <div class="activity-meta">
                    ${typeBadge}
                </div>
            </div>
            <div class="activity-action">
                ${getActivityActionButton(activity)}
            </div>
        </div>
    `;
}

// ===== STATUS HELPERS =====
function getStatusBadge(status) {
    const statusMap = {
        'ACTIVE': '<span class="status-badge status-in-progress"><i class="fas fa-circle"></i> Active</span>',
        'COMPLETED': '<span class="status-badge status-completed"><i class="fas fa-check-circle"></i> Completed</span>',
        'ARCHIVED': '<span class="status-badge status-not-started"><i class="fas fa-archive"></i> Archived</span>'
    };
    return statusMap[status?.toUpperCase()] || statusMap['ACTIVE'];
}

function getMilestoneStatusBadge(milestone) {
    if (milestone.isCompleted) {
        return '<span class="status-badge status-completed"><i class="fas fa-check-circle"></i> Completed</span>';
    }
    
    const completedCount = milestone.activities?.filter(a => a.isCompleted).length || 0;
    const totalCount = milestone.activities?.length || 0;
    
    if (completedCount > 0) {
        return `<span class="status-badge status-in-progress"><i class="fas fa-spinner"></i> In Progress (${completedCount}/${totalCount})</span>`;
    }
    
    return '<span class="status-badge status-not-started"><i class="fas fa-circle"></i> Not Started</span>';
}

function getMilestoneStatusClass(milestone) {
    if (milestone.isCompleted) return 'status-completed';
    
    const hasCompleted = milestone.activities?.some(a => a.isCompleted);
    return hasCompleted ? 'status-in-progress' : 'status-not-started';
}

function getActivityTypeBadge(type) {
    const iconMap = {
        'READING': 'fas fa-book',
        'PRACTICE': 'fas fa-laptop-code',
        'MOCK_INTERVIEW': 'fas fa-comments',
        'QUIZ': 'fas fa-question-circle',
        'OTHER': 'fas fa-tasks'
    };
    const icon = iconMap[type?.toUpperCase()] || iconMap['OTHER'];
    const label = type?.replace(/_/g, ' ') || 'Other';
    
    return `<span class="activity-type-badge"><i class="${icon}"></i> ${label}</span>`;
}

function getActivityActionButton(activity) {
    if (activity.isCompleted) {
        return `<button class="btn-action btn-completed" disabled>
                    <i class="fas fa-check"></i> Completed
                </button>`;
    }
    
    return `<button class="btn-action btn-complete" onclick="completeActivity(${activity.id})">
                <i class="fas fa-check"></i> Mark Complete
            </button>`;
}

// ===== ACTIONS =====
async function loadSkillGapAnalyses() {
    console.log("loadSkillGapAnalyses started");

    const token = localStorage.getItem("token");
    console.log("Authorization token exists:", !!token);

    const selectEl = Elements.get("skillGapAnalysisId");
    console.log("Dropdown found:", !!selectEl, selectEl);

    if (!selectEl) {
        return;
    }

    const duplicateCount = document.querySelectorAll("#skillGapAnalysisId").length;
    console.log("skillGapAnalysisId duplicate count:", duplicateCount);

    try {
        const url = `${API_BASE_URL}/SkillGapAnalysis/my-analyses`;
        console.log("Fetch started:", url);

        const response = await fetch(url, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        console.log("Fetch completed:", response.status, response.statusText);

        if (!response.ok) {
            showToast("Cannot load skill gap analyses.", "error");
            selectEl.innerHTML = '<option value="">No analyses available</option>';
            return;
        }

        const analyses = await response.json();
        console.log("Response payload:", analyses);

        const items = Array.isArray(analyses)
            ? analyses
            : (analyses?.data || analyses?.result || analyses?.items || analyses?.value || []);

        console.log("Parsed items count:", items.length);

        selectEl.innerHTML = '<option value="">Select a skill gap analysis</option>';
        let addedCount = 0;

        items.forEach(analysis => {
            const readinessScore = analysis.readinessScore ?? analysis.ReadinessScore ?? 0;
            const analysisId = analysis.id ?? analysis.Id;

            if (analysisId == null) {
                return;
            }

            const option = document.createElement("option");
            option.value = analysisId;
            option.textContent = `Analysis #${analysisId} - Score ${readinessScore}%`;
            selectEl.appendChild(option);
            addedCount++;
        });

        console.log("Options added:", addedCount);
    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server.", "error");
        selectEl.innerHTML = '<option value="">No analyses available</option>';
    }
}

async function generateRoadmap() {
    const token = localStorage.getItem("token");
    const inputEl = Elements.get("skillGapAnalysisId");
    
    if (!inputEl) {
        showToast("Form element not found.", "error");
        return;
    }
    
    const skillGapAnalysisId = inputEl.value;

    if (!skillGapAnalysisId) {
        showToast("Please select a Skill Gap Analysis.", "error");
        return;
    }

    const btn = event?.target?.closest?.('button');
    if (!btn) {
        showToast("Button element not found.", "error");
        return;
    }
    
    const originalText = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i> Generating...';

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

        const data = await response.json();
        const roadmap = data.data || data;

        showToast("Roadmap generated successfully.", "success");
        
        if (inputEl) inputEl.value = '';
        
        await loadRoadmaps();
        await loadRoadmapDetail(roadmap.id);

    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server.", "error");
    } finally {
        if (btn) {
            btn.disabled = false;
            btn.innerHTML = originalText;
        }
    }
}

async function completeActivity(activityId) {
    const token = localStorage.getItem("token");

    try {
        const response = await fetch(
            `${API_BASE_URL}/Roadmap/complete-activity/${activityId}`,
            {
                method: "POST",
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

        if (currentRoadmapId) {
            await loadRoadmapDetail(currentRoadmapId);
        }

        await loadRoadmaps();

    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server.", "error");
    }
}

function toggleActivity(event, activityId, shouldComplete) {
    if (!event) return;
    event.stopPropagation();
    if (shouldComplete) {
        completeActivity(activityId);
    }
}

// ===== UTILITIES =====
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// ===== INITIALIZATION =====
console.log("Roadmap module loaded");
