const API_ORIGIN = API_BASE_URL.replace("/api", "");

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
                            onclick="downloadResume(${resume.id}, '${resume.fileName}')"
                            class="btn btn-sm btn-info">

                            VIEW

                        </button>

                    </td>

                    <td>

                        <button
                            onclick="setActiveResume(${resume.id})"
                            class="btn btn-sm btn-primary">

                            SET ACTIVE

                        </button>

                        <button
                            onclick="deleteResume(${resume.id})"
                            class="btn btn-sm btn-danger">

                            DELETE

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