(function () {
    "use strict";

    const labels = {
        vi: {
            welcome: "Xin chào! Tôi là trợ lý AI của nền tảng. Bạn đang thắc mắc ở bước nào?",
            loginRequired: "Bạn cần đăng nhập để sử dụng trợ lý AI nhé.",
            placeholder: "Hỏi AI khi bạn thắc mắc...",
            error: "Mình chưa thể kết nối AI lúc này. Bạn thử lại sau một chút nhé.",
            empty: "Bạn hãy nhập câu hỏi trước khi gửi nhé.",
            title: "Bạn cần hỗ trợ?",
            eyebrow: "Trợ lý AI",
            open: "Mở trợ lý AI",
            close: "Đóng trợ lý AI",
            send: "Gửi câu hỏi"
        },
        en: {
            welcome: "Hi! I am your AI assistant for this platform. What step are you unsure about?",
            loginRequired: "Please sign in to use the AI assistant.",
            placeholder: "Ask AI when you need help...",
            error: "I cannot reach the AI service right now. Please try again in a moment.",
            empty: "Please type a question before sending.",
            title: "Need help?",
            eyebrow: "AI Assistant",
            open: "Open AI assistant",
            close: "Close AI assistant",
            send: "Send question"
        }
    };

    function currentLanguage() {
        return localStorage.getItem("appLanguage") || "vi";
    }

    function text(key) {
        return labels[currentLanguage()][key] || labels.vi[key] || key;
    }

    function isInterviewPage() {
        return window.location.pathname.toLowerCase().startsWith("/interview");
    }

    function pageContext() {
        const activeNav = document.querySelector(".nav-link.active span, .mobile-nav-link.active");
        const activeText = activeNav?.textContent?.trim();
        return activeText || document.title || window.location.pathname;
    }

    function token() {
        return window.AuthManager?.getState?.().token || localStorage.getItem("token");
    }

    function createMessage(content, type) {
        const message = document.createElement("div");
        message.className = `ai-assistant-message ${type}`;
        message.textContent = content;
        return message;
    }

    function createTyping() {
        const message = document.createElement("div");
        message.className = "ai-assistant-message bot is-typing";
        message.innerHTML = "<span></span><span></span><span></span>";
        return message;
    }

    function initAssistant() {
        const widget = document.getElementById("aiAssistantWidget");
        if (!widget) return;

        if (isInterviewPage()) {
            widget.classList.add("is-hidden");
            return;
        }

        const panel = document.getElementById("aiAssistantPanel");
        const toggle = document.getElementById("aiAssistantToggle");
        const close = document.getElementById("aiAssistantClose");
        const form = document.getElementById("aiAssistantForm");
        const input = document.getElementById("aiAssistantInput");
        const send = document.getElementById("aiAssistantSend");
        const messages = document.getElementById("aiAssistantMessages");

        if (!panel || !toggle || !close || !form || !input || !send || !messages) return;

        function syncLanguage() {
            input.placeholder = text("placeholder");
            toggle.setAttribute("aria-label", text("open"));
            close.setAttribute("aria-label", text("close"));
            send.setAttribute("aria-label", text("send"));
            document.querySelectorAll("[data-assistant-text]").forEach(function (element) {
                element.textContent = text(element.dataset.assistantText);
            });
        }

        function scrollToBottom() {
            messages.scrollTop = messages.scrollHeight;
        }

        function appendMessage(content, type) {
            messages.appendChild(createMessage(content, type));
            scrollToBottom();
        }

        function setOpen(isOpen) {
            widget.classList.toggle("is-open", isOpen);
            panel.setAttribute("aria-hidden", String(!isOpen));
            toggle.setAttribute("aria-expanded", String(isOpen));
            if (isOpen) {
                window.setTimeout(function () {
                    input.focus();
                }, 120);
            }
        }

        async function sendMessage(message) {
            if (!token()) {
                appendMessage(text("loginRequired"), "bot");
                return;
            }

            const typing = createTyping();
            messages.appendChild(typing);
            scrollToBottom();
            send.disabled = true;
            input.disabled = true;

            try {
            const languageCode = currentLanguage();
            console.log("[ASSISTANT_UI] selected language:", languageCode);
            console.log("[ASSISTANT_UI] localStorage keys:", Object.keys(localStorage));
            console.log("[ASSISTANT_UI] localStorage.appLanguage:", localStorage.getItem("appLanguage"));
            console.log("[ASSISTANT_UI] window.I18n?.getLanguage:", typeof window.I18n !== "undefined" ? window.I18n.getLanguage() : "I18n not loaded");

            const payload = {
                message,
                page: pageContext(),
                languageCode
            };
            console.log("[ASSISTANT_UI] payload JSON:", JSON.stringify(payload));

            const response = await fetch(`${API_BASE_URL}/AssistantChat/message`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token()}`
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                throw new Error(`Assistant request failed: ${response.status}`);
            }

            const result = await response.json();
            console.log("[ASSISTANT_UI] Received reply — languageCode:", result.languageCode, "reply:", result.reply?.substring(0, 80));
            appendMessage(result.reply || text("error"), "bot");
            } catch (error) {
                console.error("AI assistant error:", error);
                appendMessage(text("error"), "bot");
            } finally {
                typing.remove();
                send.disabled = false;
                input.disabled = false;
                input.focus();
                scrollToBottom();
            }
        }

        syncLanguage();
        appendMessage(text("welcome"), "bot");

        toggle.addEventListener("click", function () {
            setOpen(!widget.classList.contains("is-open"));
        });

        close.addEventListener("click", function () {
            setOpen(false);
        });

        form.addEventListener("submit", async function (event) {
            event.preventDefault();
            const message = input.value.trim();
            if (!message) {
                appendMessage(text("empty"), "bot");
                return;
            }

            input.value = "";
            appendMessage(message, "user");
            await sendMessage(message);
        });

        window.addEventListener("languagechange", function () {
            syncLanguage();
        });
    }

    document.addEventListener("DOMContentLoaded", initAssistant);
})();
