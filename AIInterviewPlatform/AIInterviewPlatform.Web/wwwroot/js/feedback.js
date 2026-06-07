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
            renderFeedback({ evaluations: [], averageScore: 0 });
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
            renderFeedback({ evaluations: [], averageScore: 0 });
            return;
        }

        for (const answer of answers) {
            await fetch(
                `${API_BASE_URL}/interviews/evaluate`,
                {
                    method: "POST",
                    headers: {
                        "Authorization": `Bearer ${token}`,
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({ answerId: answer.id })
                }
            );
        }

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

    if (!summary || !averageScore || !container) {
        console.error("Feedback page elements are missing.");
        return;
    }

    if (!result || !Array.isArray(result.evaluations) || result.evaluations.length === 0) {
        summary.style.display = "none";
        container.innerHTML = `
            <div class="text-muted">
                No feedback yet. Click "Evaluate All Answers".
            </div>
        `;
        return;
    }

    summary.style.display = "block";
    averageScore.innerText = Number(result.averageScore || 0).toFixed(2) + "%";

    container.innerHTML = "";

    result.evaluations.forEach((evaluation, index) => {
        const feedbackItems = Array.isArray(evaluation.feedbacks) ? evaluation.feedbacks : [];
        const groupedFeedback = feedbackItems.reduce((acc, item) => {
            const key = (item.feedbackType || "OVERALL").toUpperCase();
            acc[key] = acc[key] || [];
            acc[key].push(item.feedbackContent || "");
            return acc;
        }, {});

        const clarityFeedback = groupedFeedback.CLARITY || [];
        const structureFeedback = groupedFeedback.STRUCTURE || [];
        const relevanceFeedback = groupedFeedback.RELEVANCE || [];
        const overallFeedback = groupedFeedback.OVERALL || [];

        const feedbackText = overallFeedback[0]
            || feedbackItems.map(item => item.feedbackContent).filter(Boolean).join(" ")
            || evaluation.message
            || "No feedback available.";

        const strengths = clarityFeedback.concat(structureFeedback).map(s => `<li>${s}</li>`).join("");
        const weaknesses = relevanceFeedback.map(w => `<li>${w}</li>`).join("");
        const hasNumericScores = evaluation.clarityScore != null
            || evaluation.structureScore != null
            || evaluation.relevanceScore != null
            || evaluation.overallScore != null;
        const statusBadge = evaluation.success !== false && hasNumericScores
            ? `<span class="badge bg-success">Evaluated</span>`
            : `<span class="badge bg-warning text-dark">Evaluation Incomplete</span>`;

        container.innerHTML += `
            <div class="card mb-3 border-0 shadow-sm">
                <div class="card-header bg-dark text-white d-flex justify-content-between align-items-center">
                    <span>Answer ${index + 1}</span>
                    ${statusBadge}
                </div>

                <div class="card-body">
                    <div class="row text-center mb-3">
                        <div class="col-md-3">
                            <strong>Clarity</strong>
                            <div class="text-primary">${Number(evaluation.clarityScore || 0).toFixed(1)}</div>
                        </div>

                        <div class="col-md-3">
                            <strong>Structure</strong>
                            <div class="text-primary">${Number(evaluation.structureScore || 0).toFixed(1)}</div>
                        </div>

                        <div class="col-md-3">
                            <strong>Relevance</strong>
                            <div class="text-primary">${Number(evaluation.relevanceScore || 0).toFixed(1)}</div>
                        </div>

                        <div class="col-md-3">
                            <strong>Overall</strong>
                            <div class="text-success">${Number(evaluation.overallScore || 0).toFixed(1)}</div>
                        </div>
                    </div>

                    <h6>Feedback</h6>
                    <p>${feedbackText}</p>

                    <div class="row">
                        <div class="col-md-6">
                            <h6>Strengths</h6>
                            <ul>${strengths || "<li>None provided</li>"}</ul>
                        </div>
                        <div class="col-md-6">
                            <h6>Weaknesses / Relevance Notes</h6>
                            <ul>${weaknesses || "<li>None provided</li>"}</ul>
                        </div>
                    </div>
                </div>
            </div>
        `;
    });
}
