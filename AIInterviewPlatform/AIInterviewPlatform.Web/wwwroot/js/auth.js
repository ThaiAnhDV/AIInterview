// =============================================================================
// AuthManager - Centralized Authentication State Management
// =============================================================================

const AuthManager = (function () {
    'use strict';

    // Private state
    let _state = {
        isAuthenticated: false,
        isInitialized: false,
        isValidating: false,
        user: null,
        token: null
    };

    // Event listeners for auth state changes
    const _listeners = [];

    // -------------------------------------------------------------------------
    // Private: Get token from localStorage
    // -------------------------------------------------------------------------
    function _getToken() {
        return localStorage.getItem('token');
    }

    // -------------------------------------------------------------------------
    // Private: Get user from localStorage
    // -------------------------------------------------------------------------
    function _getUser() {
        const userJson = localStorage.getItem('user');
        if (!userJson) return null;
        try {
            return JSON.parse(userJson);
        } catch {
            return null;
        }
    }

    // -------------------------------------------------------------------------
    // Private: Clear auth data from localStorage
    // -------------------------------------------------------------------------
    function _clearAuth() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        _state.token = null;
        _state.user = null;
        _state.isAuthenticated = false;
    }

    // -------------------------------------------------------------------------
    // Private: Store auth data in localStorage
    // -------------------------------------------------------------------------
    function _storeAuth(token, user) {
        localStorage.setItem('token', token);
        localStorage.setItem('user', JSON.stringify(user));
        _state.token = token;
        _state.user = user;
    }

    // -------------------------------------------------------------------------
    // Private: Notify all listeners of state change
    // -------------------------------------------------------------------------
    function _notifyListeners() {
        _listeners.forEach(callback => {
            try {
                callback(_state);
            } catch (e) {
                console.error('AuthManager listener error:', e);
            }
        });
    }

    // -------------------------------------------------------------------------
    // Private: Validate token with backend
    // -------------------------------------------------------------------------
    async function _validateTokenWithBackend(token) {
        if (!token) return { valid: false, user: null };

        try {
            const response = await fetch(`${API_BASE_URL}/Auth/me`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const result = await response.json();
                return {
                    valid: true,
                    user: result.data || result
                };
            } else if (response.status === 401) {
                // Token is invalid or expired
                return { valid: false, user: null };
            } else {
                // Other errors - treat as invalid
                return { valid: false, user: null };
            }
        } catch (error) {
            console.error('AuthManager: Token validation failed', error);
            // Network error - assume invalid for security
            return { valid: false, user: null };
        }
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------
    return {
        // -------------------------------------------------------------------------
        // Initialize auth state - MUST be called on app start
        // Validates existing token with backend
        // -------------------------------------------------------------------------
        async init() {
            if (_state.isInitialized) {
                return _state;
            }

            // Prevent multiple simultaneous validations
            if (_state.isValidating) {
                // Wait for existing validation to complete
                while (_state.isValidating) {
                    await new Promise(resolve => setTimeout(resolve, 50));
                }
                return _state;
            }

            _state.isValidating = true;

            const token = _getToken();

            if (!token) {
                // No token found
                _state.isAuthenticated = false;
                _state.isInitialized = true;
                _state.isValidating = false;
                _notifyListeners();
                return _state;
            }

            // Validate token with backend
            const validation = await _validateTokenWithBackend(token);

            if (validation.valid) {
                _state.isAuthenticated = true;
                _state.user = validation.user;
                _state.token = token;
            } else {
                // Token invalid or expired - clear it
                _clearAuth();
            }

            _state.isInitialized = true;
            _state.isValidating = false;
            _notifyListeners();

            return _state;
        },

        // -------------------------------------------------------------------------
        // Get current auth state (synchronous)
        // Returns cached state, not validated
        // -------------------------------------------------------------------------
        getState() {
            return { ..._state };
        },

        // -------------------------------------------------------------------------
        // Check if user is authenticated (synchronous)
        // Returns cached state - use isReady() first to ensure initialized
        // -------------------------------------------------------------------------
        isAuthenticated() {
            return _state.isAuthenticated;
        },

        // -------------------------------------------------------------------------
        // Check if initialization is complete
        // -------------------------------------------------------------------------
        isReady() {
            return _state.isInitialized;
        },

        // -------------------------------------------------------------------------
        // Get current user
        // -------------------------------------------------------------------------
        getUser() {
            return _state.user;
        },

        // -------------------------------------------------------------------------
        // Get token
        // -------------------------------------------------------------------------
        getToken() {
            return _state.token;
        },

        // -------------------------------------------------------------------------
        // Login - set auth state after successful login
        // -------------------------------------------------------------------------
        setAuth(token, user) {
            _storeAuth(token, user);
            _state.isAuthenticated = true;
            _state.isInitialized = true;
            _notifyListeners();
        },

        // -------------------------------------------------------------------------
        // Logout - clear auth state
        // -------------------------------------------------------------------------
        logout() {
            _clearAuth();
            _state.isInitialized = true;
            _notifyListeners();
            window.location.href = '/Auth/Login';
        },

        // -------------------------------------------------------------------------
        // Subscribe to auth state changes
        // -------------------------------------------------------------------------
        onAuthChange(callback) {
            if (typeof callback === 'function') {
                _listeners.push(callback);
            }
        },

        // -------------------------------------------------------------------------
        // Force re-validation (e.g., after token refresh)
        // -------------------------------------------------------------------------
        async revalidate() {
            _state.isInitialized = false;
            return await this.init();
        }
    };
})();


// =============================================================================
// Legacy Functions - Kept for backward compatibility
// =============================================================================

async function register() {

    const fullName =
        document.getElementById("registerFullName")
            .value
            .trim();

    const email =
        document.getElementById("registerEmail")
            .value
            .trim();

    const password =
        document.getElementById("registerPassword")
            .value;

    const agree =
        document.getElementById("customCheckRegister")
            .checked;

    if (!fullName) {
        showToast("Full name is required.", "error");
        return;
    }

    if (!email) {
        showToast("Email is required.", "error");
        return;
    }

    const emailPattern =
        /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!emailPattern.test(email)) {
        showToast("Invalid email format.", "error");
        return;
    }

    if (!password) {
        showToast("Password is required.", "error");
        return;
    }

    if (password.length < 6) {
        showToast(
            "Password must be at least 6 characters.",
            "error"
        );
        return;
    }

    if (!agree) {
        showToast(
            "Please accept Privacy Policy.",
            "error"
        );
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/Auth/register`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                fullName,
                email,
                password
            })
        });

        const result = await response.json();

        if (response.ok) {
            showToast("Account created successfully!", "success");

            setTimeout(() => {
                window.location.href = "/Auth/Login";
            }, 1200);
        } else {

            if (result.errors && Array.isArray(result.errors)) {
                showToast(result.errors.join("<br>"), "error");
            }
            else {
                showToast(
                    result.message || "Register failed!",
                    "error"
                );
            }
        }
    } catch (error) {
        showToast("Cannot connect to server!", "error");
    }
}

function setLoginLoading(isLoading) {
    const overlay = document.getElementById("login-loading-overlay");
    const submitBtn = document.getElementById("loginSubmitBtn");

    if (overlay) {
        overlay.classList.toggle("is-active", isLoading);
        overlay.setAttribute("aria-hidden", isLoading ? "false" : "true");
        overlay.style.display = isLoading ? "flex" : "none";
    }

    if (submitBtn) {
        submitBtn.disabled = isLoading;
    }
}

async function login() {
    const email = document.getElementById("loginEmail").value.trim();
    const password = document.getElementById("loginPassword").value;

    if (!email || !password) {
        showToast("Please enter email and password.", "error");
        return;
    }

    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!emailPattern.test(email)) {
        showToast("Email format is invalid.", "error");
        return;
    }

    setLoginLoading(true);

    try {
        const response = await fetch(`${API_BASE_URL}/Auth/login`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                email,
                password
            })
        });

        let result = null;

        try {
            result = await response.json();
        } catch {
            result = null;
        }

        if (response.ok) {

            if (!result?.data?.token) {
                setLoginLoading(false);

                showToast(
                    "Login failed: invalid server response.",
                    "error"
                );

                return;
            }

            // Use AuthManager to set auth state
            AuthManager.setAuth(result.data.token, result.data);

            showToast(
                "Login successfully!",
                "success"
            );

            setTimeout(() => {
                window.location.href = "/";
            }, 1200);

        } else {

            setLoginLoading(false);

            let serverMessage = "";

            if (result?.errors && Array.isArray(result.errors)) {
                serverMessage = result.errors.join(", ");
            }
            else {
                serverMessage =
                    result?.message ||
                    result?.detail ||
                    result?.title ||
                    "";
            }

            const lower = serverMessage.toLowerCase();

            if (
                response.status === 401 ||
                response.status === 400 ||
                lower.includes("invalid")
            ) {

                showToast(
                    serverMessage || "Login failed.",
                    "error"
                );

            } else {

                showToast(
                    serverMessage || "Login failed.",
                    "error"
                );
            }
        }

    } catch (error) {

        setLoginLoading(false);

        showToast(
            "Cannot connect to server!",
            "error"
        );
    }
}

function logout() {
    AuthManager.logout();
}


// =============================================================================
// Guard Functions - Updated to use AuthManager
// =============================================================================

async function requireLogin() {
    console.log("=== Guard: requireLogin ===");

    // Wait for auth to be initialized
    if (!AuthManager.isReady()) {
        console.log("Auth not ready, waiting...");
        await AuthManager.init();
    }

    if (!AuthManager.isAuthenticated()) {
        console.log("Not authenticated - Redirecting to /Auth/Login");
        window.location.href = "/Auth/Login";
        return false;
    }

    console.log("Authenticated - returning true");
    return true;
}

function getCurrentUser() {
    return AuthManager.getUser();
}

function isLoggedIn() {
    return AuthManager.isAuthenticated();
}
