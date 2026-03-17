function toggleSidebar() {
    document.querySelector('.sidebar')?.classList.toggle('open');
}

document.addEventListener('click', function (e) {
    var sidebar = document.querySelector('.sidebar');
    var toggle  = document.querySelector('.menu-toggle');
    if (sidebar && sidebar.classList.contains('open') &&
        !sidebar.contains(e.target) && !toggle?.contains(e.target)) {
        sidebar.classList.remove('open');
    }
});
// Cookie banner
function initCookieBanner() {
    if (!localStorage.getItem('cookieConsent')) {
        document.getElementById('cookie-banner')?.classList.add('show');
    }
}

function acceptCookies() {
    localStorage.setItem('cookieConsent', 'all');
    document.getElementById('cookie-banner')?.classList.remove('show');
}

function acceptNecessary() {
    localStorage.setItem('cookieConsent', 'necessary');
    document.getElementById('cookie-banner')?.classList.remove('show');
}

function denyCookies() {
    localStorage.setItem('cookieConsent', 'denied');
    document.getElementById('cookie-banner')?.classList.remove('show');
}

document.addEventListener('DOMContentLoaded', initCookieBanner);