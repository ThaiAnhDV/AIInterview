const TARGET_JOB_API = `${API_BASE_URL}/TargetJobs`;
console.log("==========================================");
console.log("[TARGET_JOB] ★★★ FRESH FILE LOADED ★★★ - @ " + new Date().toISOString());
console.log("==========================================");

// Store current job ID for modal operations
let currentJobId = null;

// ============================================
// DOM Ready - Initialize when DOM is ready
// ============================================
document.addEventListener('DOMContentLoaded', function() {
    console.log('[TargetJob] DOM ready');
    
    // Check if we're on the TargetJob page by looking for our container
    const pageContainer = document.getElementById('targetJobStats');
    if (!pageContainer) {
        console.log('[TargetJob] Not on TargetJob page, skipping initialization');
        return;
    }
    
    // Check login first
    if (typeof requireLogin === 'function') {
        if (!requireLogin()) {
            console.log('[TargetJob] Login required, redirecting...');
            return;
        }
    }
    
    // Verify all required elements exist
    const requiredIds = [
        'loadingState', 'emptyState', 'jobsList',
        'jobTitle', 'industry', 'experienceLevel', 'createJobBtn',
        'statTotalJobs', 'statWithJD', 'statPending', 'jobCountBadge',
        'jdModal', 'jdJobTitle', 'jobDescriptionContent',
        'deleteModal', 'deleteJobName', 'confirmDeleteBtn'
    ];
    
    const missing = requiredIds.filter(id => !document.getElementById(id));
    if (missing.length > 0) {
        console.error('[TargetJob] Missing elements:', missing);
    } else {
        console.log('[TargetJob] All elements verified, loading data...');
        loadTargetJobs();
    }
});

// ============================================
// Create Target Job
// ============================================
async function createTargetJob() {
    const token = localStorage.getItem("token");
    
    const jobTitleEl = document.getElementById("jobTitle");
    const industryEl = document.getElementById("industry");
    const experienceLevelEl = document.getElementById("experienceLevel");
    
    if (!jobTitleEl || !industryEl || !experienceLevelEl) {
        console.error('[TargetJob] Form elements not found');
        return;
    }
    
    const jobTitle = jobTitleEl.value.trim();
    const industry = industryEl.value.trim();
    const experienceLevel = experienceLevelEl.value;
    
    // Clear previous validation
    clearValidation();

    // Validate
    if (!jobTitle) {
        showFieldError(jobTitleEl, "Job title is required!");
        return;
    }

    // Show loading state
    const btn = document.getElementById("createJobBtn");
    if (!btn) {
        console.error('[TargetJob] Create button not found');
        return;
    }
    
    const originalText = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = '<span class="loading-spinner-ai loading-spinner-ai-sm mr-2"></span> Creating...';

    try {
        const response = await fetch(TARGET_JOB_API, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({
                jobTitle: jobTitle,
                industry: industry,
                experienceLevel: experienceLevel
            })
        });

        if (!response.ok) {
            showToast("Cannot create target job!", "error");
            return;
        }

        const result = await response.json();
        showToast("Target job created successfully!", "success");

        // Reset form
        jobTitleEl.value = "";
        industryEl.value = "";
        experienceLevelEl.value = "";

        // Refresh list and stats
        await loadTargetJobs();
    } catch (error) {
        console.error("[TargetJob] Error creating job:", error);
        showToast("Cannot connect to server!", "error");
    } finally {
        btn.disabled = false;
        btn.innerHTML = originalText;
    }
}

// ============================================
// Load Target Jobs
// ============================================
console.log("[TARGET_JOB] loadTargetJobs registered");
async function loadTargetJobs() {
    console.log("[LOAD] loadTargetJobs() started");
    
    const token = localStorage.getItem("token");
    console.log("[LOAD] Token:", token ? "present" : "MISSING");
    
    const loadingState = document.getElementById("loadingState");
    const emptyState = document.getElementById("emptyState");
    const jobsList = document.getElementById("jobsList");

    console.log("[LOAD] Elements:", { loadingState: !!loadingState, emptyState: !!emptyState, jobsList: !!jobsList });

    // Safety check - if elements don't exist, return early
    if (!loadingState || !emptyState || !jobsList) {
        console.error('[LOAD] Required elements not found');
        return;
    }

    loadingState.style.display = "block";
    emptyState.style.display = "none";
    jobsList.style.display = "none";

    try {
        console.log("[LOAD] Fetching from:", `${TARGET_JOB_API}/my`);
        const response = await fetch(`${TARGET_JOB_API}/my`, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });
        console.log("[LOAD] Response status:", response.status);

        if (!response.ok) {
            console.error("[LOAD] Response not OK:", response.status, response.statusText);
            showToast("Cannot load target jobs!", "error");
            loadingState.style.display = "none";
            return;
        }

        console.log("[LOAD] Parsing JSON...");
        const jobs = await response.json();
        console.log("[LOAD] Raw response:", jobs);
        console.log("[LOAD] Jobs type:", typeof jobs, Array.isArray(jobs));
        console.log("[LOAD] Jobs count:", jobs ? jobs.length : "N/A");
        
        loadingState.style.display = "none";

        // Update stats
        updateJobStats(jobs);

        if (!jobs || jobs.length === 0) {
            console.log("[LOAD] Empty list - showing empty state");
            emptyState.style.display = "block";
            jobsList.style.display = "none";
            return;
        }

        emptyState.style.display = "none";
        jobsList.style.display = "block";
        console.log("[LOAD] About to render jobs");

        // Render jobs - check if function exists first
        console.log("[LOAD] renderJobList type:", typeof renderJobList);
        if (typeof renderJobList !== 'function') {
            console.error("[LOAD] renderJobList is NOT a function!");
            return;
        }
        renderJobList(jobs);
        console.log("[LOAD] renderJobList completed");
    } catch (error) {
        console.error("[LOAD] Error:", error.message, error);
        showToast("Cannot connect to server!", "error");
        loadingState.style.display = "none";
    }
}

// ============================================
// Update Stats
// ============================================
function updateJobStats(jobs) {
    if (!jobs) return;
    
    const totalJobs = jobs.length;
    const withJD = jobs.filter(j => j.jobDescription && j.jobDescription.trim().length > 0).length;
    const pending = totalJobs - withJD;

    const statTotalJobs = document.getElementById("statTotalJobs");
    const statWithJD = document.getElementById("statWithJD");
    const statPending = document.getElementById("statPending");
    const jobCountBadge = document.getElementById("jobCountBadge");

    if (statTotalJobs) statTotalJobs.textContent = totalJobs;
    if (statWithJD) statWithJD.textContent = withJD;
    if (statPending) statPending.textContent = pending;
    if (jobCountBadge) jobCountBadge.textContent = totalJobs;
}

// ============================================
// Render Job List
// ============================================
console.log("[TARGET_JOB] renderJobList registered");
function renderJobList(jobs) {
    console.log("[RENDER] renderJobList() started", { jobs });
    
    const container = document.getElementById("jobsList");
    console.log("[RENDER] Container:", !!container);
    
    // Safety check
    if (!container) {
        console.error('[RENDER] jobsList container not found');
        return;
    }
    
    console.log("[RENDER] About to map jobs, count:", jobs.length);
    const html = jobs.map(job => `
        <div class="job-row" data-job-id="${job.id}">
            <div class="job-row-content">
                <div class="job-row-info">
                    <div class="job-row-title">
                        <i class="fas fa-briefcase"></i>
                        ${escapeHtml(job.jobTitle)}
                    </div>
                    <div class="job-row-meta">
                        ${job.industry ? `
                            <span class="job-meta-item">
                                <i class="fas fa-building"></i>
                                ${escapeHtml(job.industry)}
                            </span>
                        ` : ''}
                        ${job.experienceLevel ? `
                            <span class="job-meta-item">
                                <i class="fas fa-layer-group"></i>
                                ${escapeHtml(job.experienceLevel)}
                            </span>
                        ` : ''}
                        <span class="job-meta-item">
                            <i class="fas fa-calendar"></i>
                            ${formatDate(job.createdAt)}
                        </span>
                        ${job.jobDescription && job.jobDescription.trim().length > 0 ? `
                            <span class="badge-ai badge-ai-success badge-ai-sm">
                                <i class="fas fa-check mr-1"></i> JD Added
                            </span>
                        ` : `
                            <span class="badge-ai badge-ai-warning badge-ai-sm">
                                <i class="fas fa-clock mr-1"></i> JD Pending
                            </span>
                        `}
                    </div>
                </div>
                <div class="job-row-actions">
                    <a href="/TargetJob/JobDescription?targetJobId=${job.id}" 
                       class="btn-secondary-ai btn-sm"
                       title="Manage Job Description">
                        <i class="fas fa-file-alt mr-1"></i>
                        JD
                    </a>
                    <button onclick="promptDeleteJob(${job.id}, '${escapeHtml(job.jobTitle)}')"
                            class="btn-danger-ai btn-sm"
                            title="Delete Job">
                        <i class="fas fa-trash mr-1"></i>
                        Delete
                    </button>
                </div>
            </div>
        </div>
    `).join("");
    
    console.log("[RENDER] Generated HTML length:", html.length);
    container.innerHTML = html;
    console.log("[RENDER] innerHTML set, container children:", container.children.length);
}

// ============================================
// Delete Job Functions
// ============================================
function promptDeleteJob(jobId, jobTitle) {
    currentJobId = jobId;
    const deleteJobNameEl = document.getElementById("deleteJobName");
    if (deleteJobNameEl) {
        deleteJobNameEl.textContent = jobTitle;
    }
    openModal("deleteModal");
}

async function confirmDelete() {
    if (!currentJobId) return;

    const btn = document.getElementById("confirmDeleteBtn");
    if (!btn) return;
    
    btn.disabled = true;
    btn.innerHTML = '<span class="loading-spinner-ai loading-spinner-ai-sm mr-2"></span> Deleting...';

    const token = localStorage.getItem("token");

    try {
        const response = await fetch(`${TARGET_JOB_API}/${currentJobId}`, {
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
        closeDeleteModal();
        await loadTargetJobs();
    } catch (error) {
        console.error("[TargetJob] Error deleting job:", error);
        showToast("Cannot connect to server!", "error");
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i class="fas fa-trash mr-2"></i> Delete';
    }
}

function closeDeleteModal() {
    currentJobId = null;
    closeModal("deleteModal");
}

// ============================================
// Form Reset
// ============================================
function resetJobForm() {
    const jobTitleEl = document.getElementById("jobTitle");
    const industryEl = document.getElementById("industry");
    const experienceLevelEl = document.getElementById("experienceLevel");
    
    if (jobTitleEl) jobTitleEl.value = "";
    if (industryEl) industryEl.value = "";
    if (experienceLevelEl) experienceLevelEl.value = "";
    
    clearValidation();
}

// ============================================
// Job Description Modal
// ============================================
function openJdModal(jobId, jobTitle) {
    currentJobId = jobId;
    
    const jdJobTitleEl = document.getElementById("jdJobTitle");
    const jdModalEl = document.getElementById("jdModal");
    
    if (jdJobTitleEl) jdJobTitleEl.textContent = jobTitle;
    if (jdModalEl) {
        jdModalEl.style.display = "flex";
        document.body.style.overflow = "hidden";
    }
}

function closeJdModal() {
    currentJobId = null;
    const jdModalEl = document.getElementById("jdModal");
    if (jdModalEl) {
        jdModalEl.style.display = "none";
        document.body.style.overflow = "";
    }
}

async function saveJobDescription() {
    const contentEl = document.getElementById("jobDescriptionContent");
    if (!contentEl) return;
    
    const content = contentEl.value;
    const token = localStorage.getItem("token");

    if (!content.trim()) {
        showToast("Job description cannot be empty!", "error");
        return;
    }

    try {
        const response = await fetch(`${TARGET_JOB_API}/${currentJobId}/description`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({ jobDescription: content })
        });

        if (!response.ok) {
            showToast("Cannot save job description!", "error");
            return;
        }

        showToast("Job description saved successfully!", "success");
        closeJdModal();
        await loadTargetJobs();
    } catch (error) {
        console.error("[TargetJob] Error saving JD:", error);
        showToast("Cannot connect to server!", "error");
    }
}

// ============================================
// Modal Utilities
// ============================================
function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = "flex";
        document.body.style.overflow = "hidden";
    }
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = "none";
        document.body.style.overflow = "";
    }
}

// ============================================
// Validation Helpers
// ============================================
function showFieldError(input, message) {
    if (!input) return;
    
    input.classList.add("form-validation");
    
    // Remove existing message
    const existingMsg = input.parentElement.querySelector(".validation-message");
    if (existingMsg) existingMsg.remove();
    
    // Add new message
    const msg = document.createElement("div");
    msg.className = "validation-message";
    msg.innerHTML = `<i class="fas fa-exclamation-circle"></i>${message}`;
    input.parentElement.appendChild(msg);
    
    input.addEventListener("input", clearFieldValidation, { once: true });
}

function clearFieldValidation() {
    this.classList.remove("form-validation");
    const msg = this.parentElement.querySelector(".validation-message");
    if (msg) msg.remove();
}

function clearValidation() {
    document.querySelectorAll(".form-validation").forEach(el => {
        el.classList.remove("form-validation");
    });
    document.querySelectorAll(".validation-message").forEach(el => {
        el.remove();
    });
}

// ============================================
// Utility Functions
// ============================================
function escapeHtml(text) {
    if (!text) return "";
    const div = document.createElement("div");
    div.textContent = text;
    return div.innerHTML;
}

function formatDate(dateString) {
    if (!dateString) return "-";
    const date = new Date(dateString);
    return date.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric"
    });
}

// ============================================
// Event Listeners
// ============================================

// Close modals on escape key
document.addEventListener("keydown", function(e) {
    if (e.key === "Escape") {
        closeJdModal();
        closeDeleteModal();
    }
});

// Close modals on backdrop click
document.addEventListener("click", function(e) {
    if (e.target.classList.contains("modal-ai")) {
        closeJdModal();
        closeDeleteModal();
    }
});

console.log('[TargetJob] Script loaded successfully');
