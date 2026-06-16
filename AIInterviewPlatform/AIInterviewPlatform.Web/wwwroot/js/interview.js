async function loadTargetJobsForInterview() {
    const token = localStorage.getItem("token");

    try {
        showLoader();

        const response = await fetch(`${API_BASE_URL}/TargetJobs/my`, {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) {
            showToast("Cannot load target jobs.", "error");
            return;
        }

        const jobs = await response.json();
        const select = document.getElementById("targetJobSelect");

        select.innerHTML = "";

        if (!jobs || jobs.length === 0) {
            select.innerHTML = `<option value="">No target jobs found</option>`;
            return;
        }

        jobs.forEach(job => {
            select.innerHTML += `
                <option value="${job.id}">
                    ${job.jobTitle}
                </option>
            `;
        });
    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server.", "error");
    } finally {
        hideLoader();
    }
}

async function startMockInterview() {
    const targetJobId = document.getElementById("targetJobSelect").value;
    const skillGapAnalysisId = document.getElementById("skillGapSelect").value;

    if (!targetJobId) {
        showToast("Please select a target job.", "error");
        return;
    }

    if (!skillGapAnalysisId) {
        showToast("Please select a skill gap analysis.", "error");
        return;
    }

    hideAllViews();
    showView('loading');
    setButtonLoading(true);

    const token = localStorage.getItem("token");

    try {
        showLoader();

        const response = await axios.post(
            `${API_BASE_URL}/MockInterviews/generate`,
            {
                targetJobId: parseInt(targetJobId),
                skillGapAnalysisId: parseInt(skillGapAnalysisId)
            },
            {
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                }
            }
        );

        displayResults(response.data);
    } catch (error) {
        console.error('Interview generation failed:', error);
        showError(error);
    } finally {
        hideLoader();
        setButtonLoading(false);
    }
}

async function loadInterviewSession() {
    const token = localStorage.getItem("token");
    const params = new URLSearchParams(window.location.search);
    const id = params.get("id");

    try {
        showLoader();

        const response = await fetch(`${API_BASE_URL}/Interview/${id}`, {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) {
            showToast("Cannot load interview session.", "error");
            return;
        }

        const session = await response.json();

        document.getElementById("jobTitle").innerText =
            session.targetJobTitle;

        const feedbackLink = document.getElementById("feedbackLink");

        if (feedbackLink) {
            feedbackLink.href = `/Feedback?sessionId=${id}`;
        }

        const container = document.getElementById("questionContainer");

        container.innerHTML = "";

        session.questions.forEach((q, index) => {
            container.innerHTML += `
                <div class="card mb-3 shadow-sm">
                    <div class="card-body">
                        <h5 class="text-primary">
                            Question ${index + 1}
                        </h5>

                        <p>
                            ${q.questionContent}
                        </p>

                        <textarea
                            class="form-control answer-input"
                            rows="5"
                            data-question-id="${q.id}"
                            placeholder="Enter your answer here..."></textarea>
                    </div>
                </div>
            `;
        });
    } catch (error) {
        console.error(error);
        showToast("Cannot connect to server.", "error");
    } finally {
        hideLoader();
    }
}

async function completeInterview() {
    const token = localStorage.getItem("token");
    const params = new URLSearchParams(window.location.search);
    const id = params.get("id");

    const completeButton = document.getElementById("completeInterviewButton");
    if (completeButton) {
        completeButton.disabled = true;
        completeButton.classList.add("loading");
    }

    try {
        showLoader();

        await fetch(`${API_BASE_URL}/Interview/${id}/complete`, {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        showToast("Interview completed.", "success");

        window.location.href = "/Interview";
    } catch (error) {
        console.error(error);
        showToast("Cannot complete interview.", "error");
    } finally {
        hideLoader();

        if (completeButton) {
            completeButton.disabled = false;
            completeButton.classList.remove("loading");
        }
    }
}

async function submitInterviewAnswers() {
    const token = localStorage.getItem("token");
    const params = new URLSearchParams(window.location.search);
    const sessionId = params.get("id");
    const answers = document.querySelectorAll(".answer-input");

    const submitButton = document.getElementById("submitAnswersButton");
    if (submitButton) {
        submitButton.disabled = true;
        submitButton.classList.add("loading");
    }

    try {
        showLoader();

        for (const answer of answers) {
            const questionId = answer.dataset.questionId;
            const answerText = answer.value.trim();

            if (!answerText) {
                continue;
            }

            await fetch(`${API_BASE_URL}/interviews/${sessionId}/answers`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                },
                body: JSON.stringify({
                    interviewQuestionId: Number(questionId),
                    answerText: answerText
                })
            });
        }

        showToast("Answers submitted successfully.", "success");
    } catch (error) {
        console.error(error);
        showToast("Cannot submit answers.", "error");
    } finally {
        hideLoader();

        if (submitButton) {
            submitButton.disabled = false;
            submitButton.classList.remove("loading");
        }
    }
}