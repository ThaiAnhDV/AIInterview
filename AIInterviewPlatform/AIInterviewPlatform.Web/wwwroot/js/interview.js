async function loadTargetJobsForInterview() {

    const token =
        localStorage.getItem("token");

    const response =
        await fetch(
            `${API_BASE_URL}/TargetJobs/my`,
            {
                headers: {
                    "Authorization":
                        `Bearer ${token}`
                }
            });

    const jobs =
        await response.json();

    const select =
        document.getElementById(
            "targetJobSelect");

    select.innerHTML = "";

    jobs.forEach(job => {

        select.innerHTML += `
            <option value="${job.id}">
                ${job.jobTitle}
            </option>
        `;
    });
}

async function startInterview() {

    const token =
        localStorage.getItem("token");

    const targetJobId =
        document.getElementById(
            "targetJobSelect").value;

    const response =
        await fetch(
            `${API_BASE_URL}/Interview/start`,
            {
                method: "POST",

                headers: {
                    "Content-Type":
                        "application/json",

                    "Authorization":
                        `Bearer ${token}`
                },

                body: JSON.stringify({
                    targetJobId:
                        Number(targetJobId)
                })
            });

    const session =
        await response.json();

    window.location.href =
        `/Interview/Session?id=${session.id}`;
}

async function loadInterviewSession() {

    const token =
        localStorage.getItem("token");

    const params =
        new URLSearchParams(
            window.location.search);

    const id =
        params.get("id");

    const response =
        await fetch(
            `${API_BASE_URL}/Interview/${id}`,
            {
                headers: {
                    "Authorization":
                        `Bearer ${token}`
                }
            });

    const session =
        await response.json();

    document.getElementById(
        "jobTitle").innerText =
        session.targetJobTitle;

    const container =
        document.getElementById(
            "questionContainer");

    container.innerHTML = "";

    session.questions.forEach(
        (q, index) => {

            container.innerHTML += `
                <div class="card mb-3">

                    <div class="card-body">

                        <h5>
                            Question ${index + 1}
                        </h5>

                        <p>
                            ${q.questionContent}
                        </p>

                    </div>

                </div>
            `;
        });
}

async function completeInterview() {

    const token =
        localStorage.getItem("token");

    const params =
        new URLSearchParams(
            window.location.search);

    const id =
        params.get("id");

    await fetch(
        `${API_BASE_URL}/Interview/${id}/complete`,
        {
            method: "POST",

            headers: {
                "Authorization":
                    `Bearer ${token}`
            }
        });

    alert(
        "Interview Completed!");

    window.location.href =
        "/Interview";
}