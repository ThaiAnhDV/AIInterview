async function loadSessionFeedback() {
    const token = localStorage.getItem("token");
    const sessionId = document.getElementById("sessionId").value;

    if (!sessionId) {
        showToast("Session id is missing!", "error");
        return;
    }

    try {
        const response = await fetch(
            `${API_BASE_URL}/AnswerEvaluation/session/${sessionId}`,
            {
                method: "GET",
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            }
        );

        if (!response.ok) {
            return;
        }

        const result = await response.json();

        renderFeedback(result);

    } catch (error) {
        console.error(error);
    }
}

async function evaluateAllAnswers() {
    const token = localStorage.getItem("token");
    const sessionId = document.getElementById("sessionId").value;

    if (!sessionId) {
        showToast("Session id is missing!", "error");
        return;
    }

    try {
        const answersResponse = await fetch(
            `${API_BASE_URL}/interviews/${sessionId}/answers`,
            {
                method: "GET",
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            }
        );

        if (!answersResponse.ok) {
            showToast("Cannot load answers!", "error");
            return;
        }

        const answers = await answersResponse.json();

        if (!answers || answers.length === 0) {
            showToast("No answers found for this session!", "error");
            return;
        }

        for (const answer of answers) {
            await fetch(
                `${API_BASE_URL}/AnswerEvaluation/${answer.id}/evaluate`,
                {
                    method: "POST",
                    headers: {
                        "Authorization": `Bearer ${token}`
                    }
                }
            );
        }

        showToast("Answers evaluated successfully!", "success");

        await loadSessionFeedback();

    } catch (error) {
        console.error(error);
        showToast("Cannot evaluate answers!", "error");
    }
}

function renderFeedback(result) {
    const summary = document.getElementById("feedbackSummary");
    const averageScore = document.getElementById("averageScore");
    const container = document.getElementById("feedbackContainer");

    if (!result || !result.evaluations || result.evaluations.length === 0) {
        summary.style.display = "none";
        container.innerHTML = `
            <div class="text-muted">
                No feedback yet. Click "Evaluate All Answers".
            </div>
        `;
        return;
    }

    summary.style.display = "block";
    averageScore.innerText = Number(result.averageScore).toFixed(2) + "%";

    container.innerHTML = "";

    result.evaluations.forEach((evaluation, index) => {
        container.innerHTML += `
            <div class="card mb-3 border-0 shadow-sm">
                <div class="card-header bg-dark text-white">
                    Answer ${index + 1}
                </div>

                <div class="card-body">

                    <div class="row text-center mb-3">
                        <div class="col-md-3">
                            <strong>Clarity</strong>
                            <div class="text-primary">${evaluation.clarityScore}</div>
                        </div>

                        <div class="col-md-3">
                            <strong>Structure</strong>
                            <div class="text-primary">${evaluation.structureScore}</div>
                        </div>

                        <div class="col-md-3">
                            <strong>Relevance</strong>
                            <div class="text-primary">${evaluation.relevanceScore}</div>
                        </div>

                        <div class="col-md-3">
                            <strong>Overall</strong>
                            <div class="text-success">${Number(evaluation.overallScore).toFixed(2)}</div>
                        </div>
                    </div>

                    <h6>Feedback</h6>

                    <ul>
                        ${evaluation.feedbacks.map(f => `
                            <li>
                                <strong>${f.feedbackType}</strong>:
                                ${f.feedbackContent}
                            </li>
                        `).join("")}
                    </ul>

                </div>
            </div>
        `;
    });
}