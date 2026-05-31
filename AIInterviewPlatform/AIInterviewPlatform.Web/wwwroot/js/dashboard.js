const API_BASE_URL = "https://localhost:7196/api";

let trendChart = null;

async function loadAllDashboardData() {
    showLoadingState();
    await Promise.all([
        loadReadinessDashboard(),
        loadSkillGapsDashboard(),
        loadHistoryDashboard()
    ]);
}

function showLoadingState() {
    document.getElementById('latestScore').textContent = '...';
    document.getElementById('matchedCount').textContent = '...';
    document.getElementById('missingCount').textContent = '...';
    document.getElementById('trendValue').textContent = '...';
}

async function loadReadinessDashboard() {
    try {
        const response = await fetch(`${API_BASE_URL}/dashboard/readiness`, {
            headers: getAuthHeaders()
        });

        if (!response.ok) {
            throw new Error('Failed to load readiness data');
        }

        const data = await response.json();
        updateReadinessStats(data);
    } catch (error) {
        console.error('Error loading readiness:', error);
        showEmptyReadinessStats();
    }
}

function updateReadinessStats(data) {
    const latestScoreEl = document.getElementById('latestScore');
    const latestScoreDateEl = document.getElementById('latestScoreDate');
    const trendValueEl = document.getElementById('trendValue');
    const improvementTextEl = document.getElementById('improvementText');

    if (data.latestScore) {
        latestScoreEl.textContent = `${data.latestScore.score}%`;
        latestScoreDateEl.textContent = formatDate(data.latestScore.calculatedAt);
    } else {
        latestScoreEl.textContent = 'N/A';
        latestScoreDateEl.textContent = 'No data';
    }

    const trendClass = data.trend?.toLowerCase() || 'stable';
    trendValueEl.innerHTML = `<span class="trend-indicator ${trendClass}">
        <i class="fas fa-${getTrendIcon(data.trend)}"></i>
        ${data.trend || 'N/A'}
    </span>`;

    if (data.improvementPercentage !== undefined && data.improvementPercentage !== 0) {
        const isPositive = data.improvementPercentage > 0;
        const badgeClass = isPositive ? 'positive' : 'negative';
        const sign = isPositive ? '+' : '';
        improvementTextEl.innerHTML = `<span class="improvement-badge ${badgeClass}">${sign}${data.improvementPercentage}%</span> vs previous`;
    } else {
        improvementTextEl.textContent = 'First analysis';
    }
}

function getTrendIcon(trend) {
    switch (trend?.toUpperCase()) {
        case 'IMPROVING': return 'arrow-up';
        case 'DECLINING': return 'arrow-down';
        default: return 'equals';
    }
}

function showEmptyReadinessStats() {
    document.getElementById('latestScore').textContent = 'N/A';
    document.getElementById('latestScoreDate').textContent = 'No data available';
    document.getElementById('trendValue').textContent = '--';
    document.getElementById('improvementText').textContent = 'Complete an analysis';
}

async function loadSkillGapsDashboard() {
    try {
        const response = await fetch(`${API_BASE_URL}/dashboard/skill-gaps`, {
            headers: getAuthHeaders()
        });

        if (!response.ok) {
            if (response.status === 404) {
                showEmptySkillGaps();
                return;
            }
            throw new Error('Failed to load skill gaps');
        }

        const data = await response.json();
        updateSkillGaps(data);
    } catch (error) {
        console.error('Error loading skill gaps:', error);
        showEmptySkillGaps();
    }
}

function updateSkillGaps(data) {
    const matchedContainer = document.getElementById('matchedSkillsContainer');
    const missingContainer = document.getElementById('missingSkillsContainer');
    const matchedBadge = document.getElementById('matchedBadge');
    const missingBadge = document.getElementById('missingBadge');

    if (!data || !data.missingSkills) {
        showEmptySkillGaps();
        return;
    }

    const totalMissing = data.missingSkills.length;
    const matchedCount = calculateMatchedCount(data);
    
    document.getElementById('matchedCount').textContent = matchedCount;
    document.getElementById('missingCount').textContent = totalMissing;
    matchedBadge.textContent = matchedCount;
    missingBadge.textContent = totalMissing;

    if (data.missingSkills && data.missingSkills.length > 0) {
        renderMissingSkills(data.missingSkills);
    } else {
        missingContainer.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-check-circle"></i>
                <h5>All Skills Matched!</h5>
                <p>Great job! You have all required skills.</p>
            </div>
        `;
    }
}

function calculateMatchedCount(data) {
    return data.matchedSkills?.length || 0;
}

function renderMissingSkills(skills) {
    const container = document.getElementById('missingSkillsContainer');
    const grid = document.createElement('div');
    grid.className = 'skills-grid';

    skills.forEach(skill => {
        const tag = document.createElement('span');
        tag.className = 'skill-tag missing';
        const skillName = skill.skillName || skill;
        const gapLevel = skill.gapLevel || 'HIGH';
        tag.innerHTML = `<i class="fas fa-exclamation-circle"></i> ${skillName}`;
        tag.title = `${gapLevel} - ${skill.gapDescription || 'Missing skill'}`;
        grid.appendChild(tag);
    });

    container.innerHTML = '';
    container.appendChild(grid);
}

function showEmptySkillGaps() {
    document.getElementById('matchedCount').textContent = '--';
    document.getElementById('missingCount').textContent = '--';
    document.getElementById('matchedBadge').textContent = '0';
    document.getElementById('missingBadge').textContent = '0';
    
    document.getElementById('matchedSkillsContainer').innerHTML = `
        <div class="empty-state">
            <i class="fas fa-check-circle"></i>
            <h5>No Matched Skills</h5>
            <p>Complete an analysis to see your matched skills</p>
        </div>
    `;
    
    document.getElementById('missingSkillsContainer').innerHTML = `
        <div class="empty-state">
            <i class="fas fa-exclamation-triangle"></i>
            <h5>No Missing Skills</h5>
            <p>Complete an analysis to see skill gaps</p>
        </div>
    `;
}

async function loadHistoryDashboard() {
    try {
        const response = await fetch(`${API_BASE_URL}/dashboard/history`, {
            headers: getAuthHeaders()
        });

        if (!response.ok) {
            throw new Error('Failed to load history');
        }

        const data = await response.json();
        updateHistory(data);
    } catch (error) {
        console.error('Error loading history:', error);
    }
}

function updateHistory(data) {
    renderRecentAnalyses(data.analyses || []);
    renderTrendChart(data.readinessTimeline || []);
}

function renderRecentAnalyses(analyses) {
    const container = document.getElementById('recentAnalysesList');

    if (!analyses || analyses.length === 0) {
        container.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-chart-bar"></i>
                <h5>No History Yet</h5>
                <p>Complete a skill gap analysis to see your progress</p>
            </div>
        `;
        return;
    }

    const list = document.createElement('div');
    analyses.slice(0, 5).forEach(analysis => {
        const scoreClass = analysis.readinessScore >= 70 ? 'high' : 
                          analysis.readinessScore >= 40 ? 'medium' : 'low';

        const item = document.createElement('div');
        item.className = 'history-item';
        item.innerHTML = `
            <div class="history-score ${scoreClass}">${analysis.readinessScore}%</div>
            <div class="history-details">
                <div class="history-date">${formatDate(analysis.createdAt)}</div>
                <div class="history-meta">
                    <i class="fas fa-file-alt"></i> Resume #${analysis.resumeId} |
                    <i class="fas fa-briefcase"></i> JD #${analysis.jobDescriptionId}
                </div>
            </div>
        `;
        list.appendChild(item);
    });

    container.innerHTML = '';
    container.appendChild(list);
}

function renderTrendChart(timeline) {
    const ctx = document.getElementById('trendChart');
    if (!ctx) return;
    
    const chartCtx = ctx.getContext('2d');

    if (trendChart) {
        trendChart.destroy();
    }

    const chartData = prepareChartData(timeline);

    trendChart = new Chart(chartCtx, {
        type: 'line',
        data: {
            labels: chartData.labels,
            datasets: [{
                label: 'Readiness Score',
                data: chartData.scores,
                borderColor: '#ff6ec4',
                backgroundColor: createGradient(chartCtx),
                fill: true,
                tension: 0.4,
                pointBackgroundColor: '#ff6ec4',
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                pointRadius: 5,
                pointHoverRadius: 7
            }]
        },
        options: getChartOptions()
    });
}

function prepareChartData(timeline) {
    if (!timeline || timeline.length === 0) {
        return { labels: ['No Data'], scores: [0] };
    }

    return {
        labels: timeline.map(t => formatDateShort(t.date)),
        scores: timeline.map(t => t.score)
    };
}

function createGradient(ctx) {
    const gradient = ctx.createLinearGradient(0, 0, 0, 300);
    gradient.addColorStop(0, 'rgba(255, 110, 196, 0.3)');
    gradient.addColorStop(1, 'rgba(255, 110, 196, 0.0)');
    return gradient;
}

function getChartOptions() {
    return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: false
            },
            tooltip: {
                backgroundColor: 'rgba(25, 25, 50, 0.95)',
                titleColor: '#fff',
                bodyColor: 'rgba(255, 255, 255, 0.8)',
                borderColor: 'rgba(255, 110, 196, 0.3)',
                borderWidth: 1,
                padding: 12,
                displayColors: false,
                callbacks: {
                    label: function(context) {
                        return `Score: ${context.parsed.y}%`;
                    }
                }
            }
        },
        scales: {
            y: {
                beginAtZero: true,
                max: 100,
                grid: {
                    color: 'rgba(255, 255, 255, 0.1)'
                },
                ticks: {
                    color: 'rgba(255, 255, 255, 0.6)',
                    callback: function(value) {
                        return value + '%';
                    }
                }
            },
            x: {
                grid: {
                    display: false
                },
                ticks: {
                    color: 'rgba(255, 255, 255, 0.6)'
                }
            }
        }
    };
}

function getAuthHeaders() {
    const token = localStorage.getItem('token');
    return {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    };
}

function formatDate(dateString) {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function formatDateShort(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric'
    });
}

window.loadAllDashboardData = loadAllDashboardData;
window.loadReadinessDashboard = loadReadinessDashboard;
window.loadSkillGapsDashboard = loadSkillGapsDashboard;
window.loadHistoryDashboard = loadHistoryDashboard;
