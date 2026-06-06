// API_BASE_URL is defined in api.js
console.log("dashboard.js loaded");
console.log("API_BASE_URL =", API_BASE_URL);

let dashboardData = null;
let isLoading = false;
let retryCount = 0;
const MAX_RETRIES = 3;

async function loadDashboard() {
    console.log("[Dashboard] loadDashboard called at", new Date().toISOString());
    console.log("[Dashboard] API_BASE_URL =", API_BASE_URL);
    
    const apiUrl = `${API_BASE_URL}/dashboard`;
    console.log("[Dashboard] Calling API:", apiUrl);

    if (isLoading) {
        console.log("[Dashboard] Already loading, skipping duplicate request");
        return;
    }
    
    isLoading = true;
    retryCount = 0;

    showLoadingState();
    hideAllStates();

    const token = localStorage.getItem("token");
    console.log("[Dashboard] Token from localStorage:", token ? "EXISTS" : "NULL");

    if (!token) {
        console.warn("[Dashboard] No authentication token found");
        showErrorState({
            title: "Authentication Required",
            description: "Please login to view your dashboard.",
            code: "AUTH_001",
            showLoginButton: true
        });
        return;
    }

    try {
        console.log("[Dashboard] Sending request with Bearer token...");
        
        const response = await fetch(apiUrl, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            }
        });

        console.log("[Dashboard] Response status:", response.status);
        console.log("[Dashboard] Response ok:", response.ok);
        console.log("[Dashboard] Response statusText:", response.statusText);

        if (!response.ok) {
            await handleHttpError(response);
            return;
        }

        const contentType = response.headers.get("content-type");
        console.log("[Dashboard] Content-Type:", contentType);
        
        if (!contentType || !contentType.includes("application/json")) {
            console.error("[Dashboard] Invalid content type received");
            showErrorState({
                title: "Invalid Response",
                description: "Received an unexpected response from the server.",
                code: "INVALID_RESPONSE"
            });
            return;
        }

        dashboardData = await response.json();
        console.log("[Dashboard] API response data:", dashboardData);
        console.log("[Dashboard] Rendering dashboard...");
        renderDashboard(dashboardData);
        console.log("[Dashboard] Dashboard loaded successfully");

    } catch (error) {
        handleNetworkError(error);
    } finally {
        isLoading = false;
        console.log("[Dashboard] Loading state ended at", new Date().toISOString());
    }
}

async function handleHttpError(response) {
    const status = response.status;
    const statusText = response.statusText;
    
    console.error(`[Dashboard] HTTP Error ${status}: ${statusText}`);

    switch (status) {
        case 401:
            console.warn("[Dashboard] 401 Unauthorized - Session expired");
            showErrorState({
                title: "Session Expired",
                description: "Your session has expired. Please login again to continue.",
                code: "HTTP_401",
                showLoginButton: true
            });
            localStorage.removeItem("token");
            localStorage.removeItem("user");
            break;

        case 403:
            console.warn("[Dashboard] 403 Forbidden - Access denied");
            showErrorState({
                title: "Access Denied",
                description: "You don't have permission to view this dashboard.",
                code: "HTTP_403"
            });
            break;

        case 404:
            console.warn("[Dashboard] 404 Not Found - Resource not available");
            showErrorState({
                title: "Dashboard Not Found",
                description: "The dashboard resource could not be found.",
                code: "HTTP_404"
            });
            break;

        case 500:
            console.error("[Dashboard] 500 Internal Server Error");
            let serverMessage = "Something went wrong on our end. Please try again later.";
            try {
                const errorData = await response.json();
                if (errorData.detail) {
                    serverMessage = errorData.detail;
                }
            } catch (e) {
                console.log("[Dashboard] Could not parse 500 error response");
            }
            showErrorState({
                title: "Server Error",
                description: serverMessage,
                code: "HTTP_500"
            });
            break;

        case 502:
        case 503:
        case 504:
            console.error(`[Dashboard] ${status} Server Unavailable`);
            showErrorState({
                title: "Service Unavailable",
                description: "The server is temporarily unavailable. Please try again in a few moments.",
                code: `HTTP_${status}`
            });
            break;

        default:
            console.error(`[Dashboard] Unhandled HTTP error: ${status}`);
            showErrorState({
                title: "Unable to load dashboard data",
                description: "Something went wrong while loading your dashboard.",
                code: `HTTP_${status}`
            });
    }
}

function handleNetworkError(error) {
    console.error("[Dashboard] Network error:", error);
    console.error("[Dashboard] Error name:", error.name);
    console.error("[Dashboard] Error message:", error.message);

    if (error.name === "AbortError") {
        console.warn("[Dashboard] Request was aborted (timeout or cancelled)");
        showErrorState({
            title: "Request Timeout",
            description: "The request took too long. Please check your connection and try again.",
            code: "TIMEOUT"
        });
    } else if (error.name === "TypeError" && error.message.includes("fetch")) {
        console.error("[Dashboard] Fetch API error - likely network issue");
        showErrorState({
            title: "Connection Error",
            description: "Unable to connect to the server. Please check your internet connection.",
            code: "NETWORK_ERROR"
        });
    } else {
        console.error("[Dashboard] Unexpected error:", error);
        showErrorState({
            title: "Unable to load dashboard data",
            description: "Something went wrong while loading your dashboard.",
            code: "UNKNOWN_ERROR"
        });
    }
}

function showLoadingState() {
    console.log("[Dashboard] Showing loading state");
    document.getElementById("dashboardLoading").style.display = "flex";
    document.getElementById("dashboardContent").style.display = "none";
    document.getElementById("dashboardError").style.display = "none";
    document.getElementById("dashboardEmpty").style.display = "none";
}

function hideAllStates() {
    document.getElementById("dashboardLoading").style.display = "none";
    document.getElementById("dashboardContent").style.display = "none";
    document.getElementById("dashboardError").style.display = "none";
    document.getElementById("dashboardEmpty").style.display = "none";
}

function showEmptyState() {
    console.log("[Dashboard] Showing empty state");
    document.getElementById("dashboardLoading").style.display = "none";
    document.getElementById("dashboardContent").style.display = "none";
    document.getElementById("dashboardError").style.display = "none";
    document.getElementById("dashboardEmpty").style.display = "flex";
}

function showErrorState(errorInfo) {
    console.log("[Dashboard] Showing error state:", errorInfo);
    
    document.getElementById("dashboardLoading").style.display = "none";
    document.getElementById("dashboardContent").style.display = "none";
    document.getElementById("dashboardEmpty").style.display = "none";
    document.getElementById("dashboardError").style.display = "flex";

    const errorTitle = document.querySelector("#dashboardError .error-title");
    const errorDescription = document.querySelector("#dashboardError .error-description");
    const errorCode = document.getElementById("errorCode");
    const retryButton = document.getElementById("retryButton");
    const loginButton = document.getElementById("loginButton");

    if (errorTitle) errorTitle.textContent = errorInfo.title || "Unable to load dashboard data";
    if (errorDescription) errorDescription.textContent = errorInfo.description || "Something went wrong while loading your dashboard.";
    
    if (errorCode) {
        if (errorInfo.code) {
            errorCode.textContent = errorInfo.code;
            errorCode.style.display = "inline-block";
        } else {
            errorCode.style.display = "none";
        }
    }

    if (retryButton) {
        retryButton.disabled = false;
        retryButton.innerHTML = '<i class="fas fa-redo"></i> Retry';
    }
    
    if (loginButton) {
        loginButton.style.display = errorInfo.showLoginButton ? "inline-flex" : "none";
    }

    console.log("[Dashboard] Error state displayed");
}

function hasDashboardData(data) {
    if (!data) return false;

    const readiness = data.readiness || {};
    const skillGaps = data.skillGaps || {};
    const interviews = data.interviews || {};
    const roadmapProgress = data.roadmapProgress || {};
    const recentFeedbacks = data.recentFeedbacks || [];

    const readinessScore = readiness.currentScore ?? 0;
    const missingSkillsCount = skillGaps.totalMissingSkills ?? 0;
    const interviewCount = interviews.totalInterviews ?? 0;
    const roadmapCount = roadmapProgress.totalRoadmaps ?? 0;
    const hasFeedbacks = recentFeedbacks && recentFeedbacks.length > 0;

    return readinessScore > 0 || missingSkillsCount > 0 || interviewCount > 0 || roadmapCount > 0 || hasFeedbacks;
}

function renderDashboard(data) {
    console.log("[Dashboard] renderDashboard called with data:", data);
    
    document.getElementById("dashboardLoading").style.display = "none";
    document.getElementById("dashboardError").style.display = "none";

    if (!hasDashboardData(data)) {
        console.log("[Dashboard] No dashboard data found, showing empty state");
        showEmptyState();
        return;
    }

    console.log("[Dashboard] Dashboard has data, showing content");
    document.getElementById("dashboardEmpty").style.display = "none";
    document.getElementById("dashboardContent").style.display = "block";

    console.log("[Dashboard] Rendering individual components...");
    renderReadinessScore(data.readiness);
    renderSkillGaps(data.skillGaps);
    renderInterviewStats(data.interviews);
    renderRoadmapProgress(data.roadmapProgress);
    renderRecentFeedback(data.recentFeedbacks);
    console.log("[Dashboard] All components rendered");
}

function renderReadinessScore(readiness) {
    const currentScore = readiness.currentScore ?? 0;
    const percentage = Math.min(Math.max(currentScore, 0), 100);

    document.getElementById("kpiReadinessScore").textContent = currentScore.toFixed(0) || "0";
    document.getElementById("readinessScoreValue2").textContent = currentScore.toFixed(0) || "0";
    document.getElementById("readinessScoreValue3").textContent = currentScore.toFixed(0) || "0";

    const circumference = 2 * Math.PI * 54;
    const offset = circumference - (percentage / 100) * circumference;
    const progressCircle = document.getElementById("readinessProgressCircle");
    if (progressCircle) {
        progressCircle.style.strokeDashoffset = offset;
    }

    const scoreColor = getScoreColor(percentage);
    if (progressCircle) {
        progressCircle.style.stroke = scoreColor;
    }

    const trend = readiness.trend || "STABLE";
    const trendElement = document.getElementById("readinessTrend");
    const improvementElement = document.getElementById("readinessImprovement");

    if (trend === "IMPROVING") {
        trendElement.innerHTML = '<i class="fas fa-arrow-up text-success"></i> Improving';
        trendElement.className = "badge badge-success";
    } else if (trend === "DECLINING") {
        trendElement.innerHTML = '<i class="fas fa-arrow-down text-danger"></i> Declining';
        trendElement.className = "badge badge-danger";
    } else {
        trendElement.innerHTML = '<i class="fas fa-minus text-muted"></i> Stable';
        trendElement.className = "badge badge-secondary";
    }

    if (readiness.improvementPercentage !== undefined && readiness.improvementPercentage !== 0) {
        const sign = readiness.improvementPercentage > 0 ? "+" : "";
        improvementElement.textContent = `${sign}${readiness.improvementPercentage.toFixed(1)}% vs previous`;
    } else {
        improvementElement.textContent = "First analysis";
    }

    document.getElementById("previousScore").textContent = readiness.previousScore?.toFixed(1) || "-";

    if (readiness.calculatedAt) {
        const date = new Date(readiness.calculatedAt);
        document.getElementById("lastUpdated").textContent = date.toLocaleDateString("en-US", {
            month: "short",
            day: "numeric",
            year: "numeric"
        });
    }
}

function getScoreColor(percentage) {
    if (percentage >= 70) return "#4caf50";
    if (percentage >= 40) return "#ff9800";
    return "#f44336";
}

function renderSkillGaps(skillGaps) {
    document.getElementById("missingSkillsCount").textContent = skillGaps.totalMissingSkills || 0;
    document.getElementById("highPriorityCount").textContent = skillGaps.highPriorityCount || 0;
    document.getElementById("mediumPriorityCount").textContent = skillGaps.mediumPriorityCount || 0;
    document.getElementById("lowPriorityCount").textContent = skillGaps.lowPriorityCount || 0;

    const container = document.getElementById("missingSkillsList");
    container.innerHTML = "";

    const skills = skillGaps.missingSkills || [];
    const topSkills = skills.slice(0, 5);

    if (topSkills.length === 0) {
        container.innerHTML = `
            <div class="text-center text-muted py-4">
                <i class="fas fa-check-circle fa-2x mb-2 text-success"></i>
                <p class="mb-0">All skills matched!</p>
            </div>
        `;
        return;
    }

    topSkills.forEach(skill => {
        const priorityClass = getPriorityClass(skill.priority);
        const priorityLabel = getPriorityLabel(skill.priority);

        const skillHtml = `
            <div class="skill-gap-item">
                <div class="d-flex justify-content-between align-items-start mb-2">
                    <div>
                        <h6 class="mb-1 text-white">${escapeHtml(skill.skillName)}</h6>
                        ${skill.skillType ? `<small class="text-muted">${escapeHtml(skill.skillType)}</small>` : ""}
                    </div>
                    <span class="badge ${priorityClass}">${priorityLabel}</span>
                </div>
                <div class="progress mb-2" style="height: 6px;">
                    <div class="progress-bar" style="width: ${getPriorityWidth(skill.priority)}%"></div>
                </div>
                ${skill.gapDescription ? `<small class="text-muted">${escapeHtml(skill.gapDescription)}</small>` : ""}
            </div>
        `;
        container.innerHTML += skillHtml;
    });

    if (skills.length > 5) {
        container.innerHTML += `
            <div class="text-center mt-3">
                <small class="text-muted">+${skills.length - 5} more skills to improve</small>
            </div>
        `;
    }
}

function getPriorityClass(priority) {
    switch (priority) {
        case 1: return "badge-danger";
        case 2: return "badge-warning";
        case 3: return "badge-info";
        default: return "badge-secondary";
    }
}

function getPriorityLabel(priority) {
    switch (priority) {
        case 1: return "Critical";
        case 2: return "High";
        case 3: return "Medium";
        default: return "Low";
    }
}

function getPriorityWidth(priority) {
    return (5 - priority) * 20 + 20;
}

function renderInterviewStats(interviews) {
    document.getElementById("totalInterviews").textContent = interviews.totalInterviews || 0;
    document.getElementById("completedInterviews").textContent = interviews.completedInterviews || 0;
    document.getElementById("pendingInterviews").textContent = interviews.pendingInterviews || 0;
    document.getElementById("averageScore").textContent = (interviews.averageScore || 0).toFixed(1);

    const highestScore = interviews.highestScore;
    const lowestScore = interviews.lowestScore;

    document.getElementById("highestScore").textContent = highestScore ? highestScore.toFixed(1) : "-";
    document.getElementById("lowestScore").textContent = lowestScore ? lowestScore.toFixed(1) : "-";
}

function renderRoadmapProgress(progress) {
    if (!progress || progress.totalRoadmaps === 0) {
        document.getElementById("roadmapStats").innerHTML = `
            <div class="text-center text-muted py-4">
                <i class="fas fa-map fa-2x mb-2"></i>
                <p class="mb-0">No roadmaps yet</p>
                <a href="/Roadmap" class="btn btn-sm btn-primary mt-2">Create Roadmap</a>
            </div>
        `;
        return;
    }

    document.getElementById("totalRoadmaps").textContent = progress.totalRoadmaps;
    document.getElementById("completedMilestones").textContent = progress.completedMilestones || 0;
    document.getElementById("totalMilestones").textContent = progress.totalMilestones || 0;
    document.getElementById("overallProgress").textContent = (progress.overallProgressPercentage || 0).toFixed(0) + "%";

    const progressBar = document.getElementById("roadmapProgressBar");
    if (progressBar) {
        progressBar.style.width = (progress.overallProgressPercentage || 0) + "%";
    }

    const activeRoadmapEl = document.getElementById("activeRoadmapTitle");
    if (activeRoadmapEl) {
        activeRoadmapEl.textContent = progress.activeRoadmapTitle || "N/A";
    }
}

function renderRecentFeedback(feedbacks) {
    const container = document.getElementById("recentFeedbackList");
    container.innerHTML = "";

    if (!feedbacks || feedbacks.length === 0) {
        container.innerHTML = `
            <div class="text-center text-muted py-4">
                <i class="fas fa-comment fa-2x mb-2"></i>
                <p class="mb-0">No recent feedback</p>
            </div>
        `;
        return;
    }

    feedbacks.forEach(feedback => {
        const typeColors = getFeedbackTypeColors(feedback.feedbackType);
        const date = new Date(feedback.createdAt);

        const feedbackHtml = `
            <div class="feedback-item">
                <div class="d-flex gap-3">
                    <div class="feedback-icon ${typeColors.bg}">
                        <i class="fas fa-comment ${typeColors.text}"></i>
                    </div>
                    <div class="flex-grow-1">
                        <div class="d-flex justify-content-between align-items-start mb-1">
                            <span class="badge ${typeColors.badge}">${feedback.feedbackType}</span>
                            <div class="feedback-meta">
                                ${feedback.score !== null ? `<span class="me-2"><i class="fas fa-star text-warning"></i> ${feedback.score.toFixed(1)}</span>` : ""}
                                <small class="text-muted">${date.toLocaleDateString()}</small>
                            </div>
                        </div>
                        <p class="mb-0 text-light">${escapeHtml(feedback.content)}</p>
                    </div>
                </div>
            </div>
        `;
        container.innerHTML += feedbackHtml;
    });
}

function getFeedbackTypeColors(type) {
    switch (type) {
        case "IMPROVEMENT":
            return { bg: "bg-warning", text: "text-warning", badge: "badge-warning" };
        case "STRENGTH":
            return { bg: "bg-success", text: "text-success", badge: "badge-success" };
        case "TIP":
            return { bg: "bg-info", text: "text-info", badge: "badge-info" };
        case "WARNING":
            return { bg: "bg-danger", text: "text-danger", badge: "badge-danger" };
        default:
            return { bg: "bg-secondary", text: "text-secondary", badge: "badge-secondary" };
    }
}

function escapeHtml(text) {
    if (!text) return "";
    const div = document.createElement("div");
    div.textContent = text;
    return div.innerHTML;
}

function refreshDashboard() {
    console.log("[Dashboard] refreshDashboard called");
    retryDashboard();
}

function retryDashboard() {
    console.log("[Dashboard] retryDashboard called, retry count:", retryCount);
    
    const retryButton = document.getElementById("retryButton");
    if (retryButton) {
        retryButton.disabled = true;
        retryButton.innerHTML = '<div class="error-spinner"></div> Retrying...';
    }
    
    retryCount++;
    console.log(`[Dashboard] Retry attempt #${retryCount}`);
    
    setTimeout(() => {
        loadDashboard();
    }, 300);
}

function navigateToUpload() {
    window.location.href = "/Resume";
}

function navigateToSkillGap() {
    window.location.href = "/SkillGap";
}

window.loadDashboard = loadDashboard;
window.refreshDashboard = refreshDashboard;
window.retryDashboard = retryDashboard;
window.navigateToUpload = navigateToUpload;
window.navigateToSkillGap = navigateToSkillGap;
