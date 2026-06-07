(function () {
    "use strict";

    function getToken() {
        return localStorage.getItem("token");
    }

    function getStoredUser() {
        try {
            return JSON.parse(localStorage.getItem("user") || "null");
        } catch {
            return null;
        }
    }

    function setText(id, value) {
        const element = document.getElementById(id);
        if (element) element.textContent = value ?? "--";
    }

    function hide(id) {
        const element = document.getElementById(id);
        if (element) element.style.display = "none";
    }

    function show(id, display = "block") {
        const element = document.getElementById(id);
        if (element) element.style.display = display;
    }

    function setGeminiStatus(isConfigured) {
        const status = document.getElementById("geminiStatus");
        if (!status) return;

        status.className = `admin-status ${isConfigured ? "ok" : "warn"}`;
        status.innerHTML = isConfigured
            ? '<i class="fas fa-check-circle"></i> Đã kết nối'
            : '<i class="fas fa-exclamation-triangle"></i> Chưa cấu hình';

        setText(
            "geminiNote",
            isConfigured
                ? "Gemini API đã sẵn sàng trả lời cho trợ lý AI."
                : "Chỉ admin thấy cảnh báo này. User thường sẽ nhận hướng dẫn fallback thân thiện."
        );
    }

    function setDenied(message) {
        hide("adminLoader");
        hide("adminPage");
        show("adminDenied");

        const deniedText = document.querySelector("#adminDenied p");
        if (deniedText && message) deniedText.textContent = message;
    }

    function setReady() {
        hide("adminLoader");
        hide("adminDenied");
        show("adminPage");
    }

    async function fetchAdminStatus() {
        const token = getToken();
        if (!token) {
            setDenied("Bạn chưa đăng nhập. Hãy đăng nhập bằng tài khoản admin.");
            return null;
        }

        const response = await fetch(`${API_BASE_URL}/Admin/system-status`, {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (response.status === 401 || response.status === 403) {
            setDenied("Token hiện tại không có quyền ADMIN. Hãy đăng xuất và đăng nhập lại bằng tài khoản admin.");
            return null;
        }

        if (!response.ok) {
            throw new Error(`Cannot load admin status: ${response.status}`);
        }

        return await response.json();
    }

    function fillCurrentAdmin() {
        const user = getStoredUser() || {};
        setText("adminEmail", user.email || user.Email || "--");
        setText("adminName", user.fullName || user.FullName || user.name || "--");
        setText("adminRole", user.role || user.Role || "ADMIN");
    }

    async function loadStatus() {
        const button = document.getElementById("adminRefreshBtn");
        if (button) button.disabled = true;

        try {
            const data = await fetchAdminStatus();
            if (!data) return;

            setReady();
            fillCurrentAdmin();

            const environment = data.server?.environment || "--";
            setGeminiStatus(Boolean(data.gemini?.configured));
            setText("geminiModel", data.gemini?.model || "--");
            setText("geminiKeyPreview", data.gemini?.keyPreview || "--");
            setText("totalUsers", data.users?.total ?? "--");
            setText("totalAdmins", data.users?.admins ?? "--");
            setText("serverEnvironment", environment);
            setText("serverEnvironmentCard", environment);
            setText(
                "checkedAt",
                data.server?.checkedAt
                    ? new Date(data.server.checkedAt).toLocaleString("vi-VN")
                    : "--"
            );
        } catch (error) {
            console.error("Admin status error:", error);
            setDenied("Không thể tải dữ liệu admin. Hãy kiểm tra API server hoặc đăng nhập lại.");
        } finally {
            if (button) button.disabled = false;
        }
    }

    function logout() {
        localStorage.removeItem("token");
        localStorage.removeItem("user");
        window.location.href = "/Auth/Login";
    }

    document.addEventListener("DOMContentLoaded", function () {
        document.getElementById("adminRefreshBtn")?.addEventListener("click", loadStatus);
        document.getElementById("adminLogoutBtn")?.addEventListener("click", logout);
        loadStatus();
    });
})();
