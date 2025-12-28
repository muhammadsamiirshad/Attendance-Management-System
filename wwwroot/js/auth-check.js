// JWT Token Management - Client-side validation
(function() {
    'use strict';

    // Check if JWT token exists for authenticated pages
    function checkAuthentication() {
        // Get all cookies
        const cookies = document.cookie.split(';');
        let hasJwtToken = false;
        let hasIdentityCookie = false;

        cookies.forEach(cookie => {
            const [name, value] = cookie.trim().split('=');
            if (name === 'jwt_token' && value) {
                hasJwtToken = true;
            }
            if (name === '.AspNetCore.Identity.Application' && value) {
                hasIdentityCookie = true;
            }
        });

        // If user appears to be logged in (has identity cookie) but no JWT token
        // This means token was manually deleted - force logout
        if (hasIdentityCookie && !hasJwtToken) {
            const currentPath = window.location.pathname.toLowerCase();
            
            // Skip check for public pages
            if (!currentPath.includes('/account/login') && 
                !currentPath.includes('/account/logout') &&
                !currentPath.includes('/home/') &&
                !currentPath.startsWith('/api/')) {
                
                console.warn('JWT token missing - redirecting to login');
                
                // Clear all cookies
                document.cookie.split(";").forEach(function(c) { 
                    document.cookie = c.replace(/^ +/, "").replace(/=.*/, "=;expires=" + new Date().toUTCString() + ";path=/"); 
                });
                
                // Redirect to login
                window.location.href = '/Account/Login';
            }
        }
    }

    // Run check on page load
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', checkAuthentication);
    } else {
        checkAuthentication();
    }

    // Periodically check authentication status (every 30 seconds)
    setInterval(checkAuthentication, 30000);

    // Handle token expiration
    function handleTokenExpiration() {
        fetch('/api/auth/me', {
            method: 'GET',
            credentials: 'include',
            headers: {
                'Accept': 'application/json'
            }
        })
        .then(response => {
            if (response.status === 401) {
                // Token expired or invalid
                console.warn('Token expired - redirecting to login');
                window.location.href = '/Account/Login';
            }
        })
        .catch(error => {
            console.error('Error checking authentication:', error);
        });
    }

    // Check token validity every 5 minutes
    setInterval(handleTokenExpiration, 300000);

})();
