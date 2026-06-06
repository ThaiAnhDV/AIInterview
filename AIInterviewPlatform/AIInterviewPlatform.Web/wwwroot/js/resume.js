const API_ORIGIN = API_BASE_URL.replace("/api", "");

function getResumeExtension(fileName) {
    return (fileName || "").split('.').pop().toLowerCase();
}

function getResumeMimeType(fileName) {
    const extension = getResumeExtension(fileName);

    switch (extension) {
        case "pdf":
            return "application/pdf";
        case "docx":
            return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        case "doc":
            return "application/msword";
        default:
            return "application/octet-stream";
    }
}

function canPreviewResume(fileName) {
    return getResumeExtension(fileName) === "pdf";
}

function getResumeActionLabel(fileName) {
    return canPreviewResume(fileName) ? "Preview" : "Download";
}

function showResumeFallbackMessage() {
    showToast(
        "This file type cannot be previewed in browser. The file will be downloaded instead.",
        "info"
    );
}

async function uploadResume() {

    const token = localStorage.getItem("token");

    const fileInput = document.getElementById("resumeFile");

    if (!fileInput.files || fileInput.files.length === 0) {

        showToast("Please select a resume file!", "error");
        return;
    }

    const file = fileInput.files[0];

    const formData = new FormData();

    formData.append("file", file);

    try {

        const response = await fetch(
            `${API_BASE_URL}/Resume/upload`,
            {
                method: "POST",
                headers: {
                    "Authorization": `Bearer ${token}`
                },
                body: formData
            });

        if (!response.ok) {

            const errorText = await response.text();

            console.error(errorText);

            showToast("Upload resume failed!", "error");

            return;
        }

        showToast("Resume uploaded successfully!", "success");

        fileInput.value = "";

        await loadResumes();
    }
    catch (error) {

        console.error(error);

        showToast("Cannot connect to server!", "error");
    }
}

async function loadResumes() {

    const token = localStorage.getItem("token");

    try {

        const response = await fetch(
            `${API_BASE_URL}/Resume/my-resumes`,
            {
                method: "GET",
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

        if (!response.ok) {

            showToast("Cannot load resumes!", "error");

            return;
        }

        const resumes = await response.json();

        const tableBody =
            document.getElementById("resumeTableBody");

        tableBody.innerHTML = "";

        if (!resumes || resumes.length === 0) {

            tableBody.innerHTML = `
                <tr>
                    <td colspan="5"
                        class="text-center text-muted">
                        No resumes uploaded yet.
                    </td>
                </tr>
            `;

            return;
        }

        resumes.forEach(resume => {

            const actionLabel = getResumeActionLabel(resume.fileName);

            tableBody.innerHTML += `
                <tr>

                    <td>${resume.fileName}</td>

                    <td>
                        ${resume.isActive
                    ? `<span class="badge badge-success">
                            ACTIVE
                       </span>`
                    : `<span class="badge badge-secondary">
                            INACTIVE
                       </span>`
                }
                    </td>

                    <td>
                        ${new Date(
                    resume.uploadedAt
                ).toLocaleString()}
                    </td>

                    <td>

                        <button
                            onclick="handleResumeViewClick(${resume.id}, '${resume.fileName}')"
                            class="btn btn-sm btn-info">

                            ${actionLabel.toUpperCase()}

                        </button>

                    </td>

                    <td>

                        <button
                            onclick="setActiveResume(${resume.id})"
                            class="btn btn-sm btn-primary">

                            SET ACTIVE

                        </button>

                        <button
                            onclick="downloadResume(${resume.id}, '${resume.fileName}')"
                            class="btn btn-sm btn-danger">

                            DOWNLOAD

                        </button>

                    </td>

                </tr>
            `;
        });
    }
    catch (error) {

        console.error(error);

        showToast("Cannot connect to server!", "error");
    }
}

async function handleResumeViewClick(resumeId, fileName) {
    if (canPreviewResume(fileName)) {
        await viewResume(resumeId, fileName);
        return;
    }

    showResumeFallbackMessage();
    await downloadResume(resumeId, fileName);
}

async function viewResume(resumeId, fileName) {
    const token = localStorage.getItem("token");
    const url = `${API_BASE_URL}/Resume/view/${resumeId}`;

    try {
        const response = await fetch(url, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) {
            const errorText = await response.text();
            console.error(errorText);
            showToast("Cannot open resume!", "error");
            return;
        }

        const blob = await response.blob();
        const mimeType = response.headers.get("content-type") || getResumeMimeType(fileName);
        const viewBlob = new Blob([blob], { type: mimeType });
        const objectUrl = window.URL.createObjectURL(viewBlob);

        const opened = window.open(objectUrl, "_blank", "noopener,noreferrer");

        if (!opened) {
            window.URL.revokeObjectURL(objectUrl);
            showToast("Pop-up blocked. Please allow pop-ups to view the resume.", "error");
            return;
        }

        window.setTimeout(() => {
            window.URL.revokeObjectURL(objectUrl);
        }, 1000);
    }
    catch (error) {
        console.error(error);
        showToast("Cannot connect to server!", "error");
    }
}

async function downloadResume(resumeId, fileName) {

    const token = localStorage.getItem("token");

    try {

        const response = await fetch(
            `${API_BASE_URL}/Resume/download/${resumeId}`,
            {
                method: "GET",
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

        if (!response.ok) {

            const errorText = await response.text();

            console.error(errorText);

            showToast("Cannot open resume!", "error");

            return;
        }

        const blob = await response.blob();

        const url =
            window.URL.createObjectURL(blob);

        const link =
            document.createElement("a");

        link.href = url;

        link.download = fileName;

        document.body.appendChild(link);

        link.click();

        link.remove();

        window.URL.revokeObjectURL(url);
    }
    catch (error) {

        console.error(error);

        showToast("Cannot connect to server!", "error");
    }
}

async function setActiveResume(resumeId) {

    const token = localStorage.getItem("token");

    try {

        const response = await fetch(
            `${API_BASE_URL}/Resume/set-active/${resumeId}`,
            {
                method: "PUT",
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

        if (!response.ok) {

            showToast("Cannot set active resume!", "error");

            return;
        }

        showToast("Active resume updated!", "success");

        await loadResumes();
    }
    catch (error) {

        console.error(error);

        showToast("Cannot connect to server!", "error");
    }
}

async function deleteResume(resumeId) {

    const confirmDelete = confirm(
        "Are you sure you want to delete this resume?"
    );

    if (!confirmDelete) {
        return;
    }

    const token = localStorage.getItem("token");

    try {

        const response = await fetch(
            `${API_BASE_URL}/Resume/${resumeId}`,
            {
                method: "DELETE",
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

        if (!response.ok) {

            showToast("Cannot delete resume!", "error");

            return;
        }

        showToast(
            "Resume deleted successfully!",
            "success"
        );

        await loadResumes();
    }
    catch (error) {

        console.error(error);

        showToast("Cannot connect to server!", "error");
    }
}

document.addEventListener("DOMContentLoaded", () => {

    loadResumes();
});