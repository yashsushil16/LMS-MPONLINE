// API Base URL
const API_BASE_URL = '/api';

// JWT Token Management
const TokenManager = {
    getToken() {
        return localStorage.getItem('jwt_token');
    },

    setToken(token) {
        localStorage.setItem('jwt_token', token);
    },

    removeToken() {
        localStorage.removeItem('jwt_token');
    },

    isAuthenticated() {
        return !!this.getToken();
    },

    getUserInfo() {
        const token = this.getToken();
        if (!token) return null;

        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return {
                userId: payload.sub,
                email: payload.email,
                fullName: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
                role: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
            };
        } catch (e) {
            return null;
        }
    }
};

// API Request Helper
async function apiRequest(endpoint, options = {}) {
    const token = TokenManager.getToken();
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers
    };

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    const config = {
        ...options,
        headers
    };

    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, config);

        if (response.status === 401) {
            TokenManager.removeToken();
            window.location.href = '/index.html';
            throw new Error('Unauthorized. Please login again.');
        }

        if (!response.ok) {
            let errorMessage = 'Request failed';
            try {
                const errorData = await response.json();
                // Handle ASP.NET validation errors format: { errors: { FieldName: ["msg"] } }
                if (errorData.errors) {
                    const messages = Object.values(errorData.errors).flat();
                    errorMessage = messages.join(', ') || errorData.title || 'Validation failed';
                } else {
                    errorMessage = errorData.message || errorData.title || 'Request failed';
                }
            } catch (_jsonErr) {
                // Response body wasn't valid JSON — use status text
                errorMessage = response.statusText || 'Request failed';
            }
            throw new Error(errorMessage);
        }

        // Handle empty responses (e.g. 204 No Content from DELETE)
        const text = await response.text();
        return text ? JSON.parse(text) : {};
    } catch (error) {
        console.error('API Error:', error);
        throw error;
    }
}


// Auth API
const AuthAPI = {
    async register(fullName, email, password) {
        return await apiRequest('/auth/register', {
            method: 'POST',
            body: JSON.stringify({ fullName, email, password })
        });
    },

    async login(email, password) {
        return await apiRequest('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ email, password })
        });
    },

    logout() {
        TokenManager.removeToken();
        window.location.href = '/index.html';
    }
};

// Admin API
const AdminAPI = {
    async getBooks() {
        return await apiRequest('/admin/books');
    },

    async addBook(title, author, isbn, totalCopies, publishedYear) {
        return await apiRequest('/admin/books', {
            method: 'POST',
            body: JSON.stringify({ title, author, isbn, totalCopies, publishedYear })
        });
    },

    async updateBook(id, title, author, isbn, totalCopies, publishedYear) {
        return await apiRequest(`/admin/books/${id}`, {
            method: 'PUT',
            body: JSON.stringify({ title, author, isbn, totalCopies, publishedYear })
        });
    },

    async deleteBook(id) {
        return await apiRequest(`/admin/books/${id}`, {
            method: 'DELETE'
        });
    },

    async getAllUsers() {
        return await apiRequest('/admin/users');
    },

    async getAllTransactions() {
        return await apiRequest('/admin/transactions');
    },

    async addUser(fullName, email, phone, password, role) {
        return await apiRequest('/admin/users', {
            method: 'POST',
            body: JSON.stringify({ fullName, email, phone, password, role: parseInt(role) })
        });
    },

    async updateUser(id, fullName, email, phone, role, password) {
        return await apiRequest(`/admin/users/${id}`, {
            method: 'PUT',
            body: JSON.stringify({ fullName, email, phone, role: role !== undefined ? parseInt(role) : undefined, password })
        });
    },

    async deleteUser(id) {
        return await apiRequest(`/admin/users/${id}`, {
            method: 'DELETE'
        });
    },

    async returnBook(transactionId) {
        return await apiRequest(`/admin/transactions/${transactionId}/return`, {
            method: 'POST'
        });
    },

    // Newspapers
    async getNewspapers() {
        return await apiRequest('/admin/newspapers');
    },

    async addNewspaper(title, publisher, language, publicationDate, copies) {
        return await apiRequest('/admin/newspapers', {
            method: 'POST',
            body: JSON.stringify({ title, publisher, language, publicationDate, copies: parseInt(copies) })
        });
    },

    async updateNewspaper(id, title, publisher, language, publicationDate, copies) {
        return await apiRequest(`/admin/newspapers/${id}`, {
            method: 'PUT',
            body: JSON.stringify({ title, publisher, language, publicationDate, copies: parseInt(copies) })
        });
    },

    async deleteNewspaper(id) {
        return await apiRequest(`/admin/newspapers/${id}`, {
            method: 'DELETE'
        });
    },

    // Magazines
    async getMagazines() {
        return await apiRequest('/admin/magazines');
    },

    async addMagazine(title, category, publisher, issueNumber, publishedYear, copies) {
        return await apiRequest('/admin/magazines', {
            method: 'POST',
            body: JSON.stringify({ title, category, publisher, issueNumber, publishedYear: parseInt(publishedYear), copies: parseInt(copies) })
        });
    },

    async updateMagazine(id, title, category, publisher, issueNumber, publishedYear, copies) {
        return await apiRequest(`/admin/magazines/${id}`, {
            method: 'PUT',
            body: JSON.stringify({ title, category, publisher, issueNumber, publishedYear: parseInt(publishedYear), copies: parseInt(copies) })
        });
    },

    async deleteMagazine(id) {
        return await apiRequest(`/admin/magazines/${id}`, {
            method: 'DELETE'
        });
    }
};

// User API
const UserAPI = {
    async getAvailableBooks() {
        return await apiRequest('/user/books/available');
    },

    async borrowBook(bookId) {
        return await apiRequest(`/user/books/${bookId}/borrow`, {
            method: 'POST'
        });
    },

    async returnBook(transactionId) {
        return await apiRequest(`/user/transactions/${transactionId}/return`, {
            method: 'POST'
        });
    },

    async getNewspapers() {
        return await apiRequest('/user/newspapers');
    },

    async getMagazines() {
        return await apiRequest('/user/magazines');
    },

    async getMyTransactions() {
        return await apiRequest('/user/transactions');
    }
};

// UI Helpers
const UI = {
    showAlert(message, type = 'info') {
        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type}`;
        alertDiv.textContent = message;

        let alertRegion = document.querySelector('.alert-region');
        if (!alertRegion) {
            alertRegion = document.createElement('div');
            alertRegion.className = 'alert-region';
            alertRegion.setAttribute('aria-live', 'polite');
            document.body.appendChild(alertRegion);
        }

        alertRegion.appendChild(alertDiv);
        setTimeout(() => alertDiv.remove(), 5000);
    },

    showLoading(elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.innerHTML = '<div class="loading">Loading...</div>';
        }
    },

    showEmptyState(elementId, message) {
        const element = document.getElementById(elementId);
        if (element) {
            element.innerHTML = `
                <div class="empty-state">
                    <h3>${message}</h3>
                </div>
            `;
        }
    },

    formatDate(dateString) {
        if (!dateString) return 'N/A';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    },

    formatStatus(status) {
        const isIssued = status === 'Issued' || status === 1;
        const isReturned = status === 'Returned' || status === 2;
        const isPending = status === 'Pending' || status === 0;

        let badgeClass = 'badge-info';
        let label = 'Issued';

        if (isReturned) { badgeClass = 'badge-success'; label = 'Returned'; }
        else if (isPending) { badgeClass = 'badge-warning'; label = 'Pending'; }
        else if (isIssued) { badgeClass = 'badge-info'; label = 'Issued'; }

        return `<span class="badge ${badgeClass}">${label}</span>`;
    }
};

// Route Protection
function checkAuth() {
    if (!TokenManager.isAuthenticated()) {
        window.location.href = '/index.html';
        return false;
    }
    return true;
}

function checkAdminAuth() {
    const userInfo = TokenManager.getUserInfo();
    if (!userInfo || userInfo.role !== 'Admin') {
        window.location.href = '/index.html';
        return false;
    }
    return true;
}

function checkUserAuth() {
    const userInfo = TokenManager.getUserInfo();
    if (!userInfo || userInfo.role !== 'User') {
        window.location.href = '/index.html';
        return false;
    }
    return true;
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    // Check if user is on the wrong dashboard
    const path = window.location.pathname;
    const userInfo = TokenManager.getUserInfo();

    if (path.includes('admin-dashboard.html') && userInfo?.role !== 'Admin') {
        window.location.href = '/user-dashboard.html';
    }

    if (path.includes('user-dashboard.html') && userInfo?.role === 'Admin') {
        window.location.href = '/admin-dashboard.html';
    }
});
