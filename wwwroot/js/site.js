// Site-wide JavaScript functionality
//
// NOTE: Blazor Web Apps navigate between pages using "enhanced navigation"
// (see blazor.web.js), which fetches and merges the new page's content
// without firing a full browser reload/DOMContentLoaded. That means:
//   1. Any state applied only once at initial load (e.g. the inline theme
//      script in App.razor's <head>) will NOT be re-applied after navigating.
//   2. Elements re-rendered by the server (e.g. the theme/language switchers
//      in MainLayout) are brand-new DOM nodes after each navigation, so any
//      click handlers bound at DOMContentLoaded need to be re-bound.
// To keep behavior correct across navigations, all init logic below runs
// both on DOMContentLoaded (first load) and via Blazor.addEventListener('enhancedload', ...)
// (every subsequent enhanced navigation). This requires blazor.web.js to be
// loaded/executed *before* this script so window.Blazor already exists.

// ── Dark Mode Toggle ──
// Works on every .theme-switcher instance
(function () {
    function applyStoredTheme() {
        try {
            var theme = localStorage.getItem('theme') || 'dark';
            document.documentElement.setAttribute('data-theme', theme);
        } catch (e) { }
    }

    function setTheme(next) {
        if ((document.documentElement.getAttribute('data-theme') || 'dark') === next) return;
        document.documentElement.setAttribute('data-theme', next);
        localStorage.setItem('theme', next);
        var domainAttr = location.hostname.endsWith('samzvi.site') ? ';domain=.samzvi.site' : '';
        document.cookie = `theme=${next};path=/;max-age=${60 * 60 * 24 * 365};samesite=lax${domainAttr}`;
    }

    function initThemeSwitcher() {
        document.querySelectorAll('.theme-switcher').forEach(function (switcher) {
            if (switcher.dataset.themeBound) return;
            switcher.dataset.themeBound = 'true';
            switcher.addEventListener('click', function () {
                var current = document.documentElement.getAttribute('data-theme') || 'dark';
                setTheme(current === 'light' ? 'dark' : 'light');
            });
        });
    }

    function init() {
        applyStoredTheme();
        initThemeSwitcher();
    }

    document.addEventListener('DOMContentLoaded', init);
    if (window.Blazor && window.Blazor.addEventListener) {
        window.Blazor.addEventListener('enhancedload', init);
    }
})();

// ── Language Switcher ──
// Works on every .language-switcher instance
(function () {
    function initLanguageSwitcher() {
        document.querySelectorAll('.language-switcher').forEach(function (switcher) {
            if (switcher.dataset.langBound) return;
            switcher.dataset.langBound = 'true';
            switcher.addEventListener('click', function (e) {
                e.preventDefault();
                var isCzActive = switcher.classList.contains('cz-active');
                // Navigate to the other language
                var targetBtn = isCzActive
                    ? switcher.querySelector('.lang-btn:first-child')
                    : switcher.querySelector('.lang-btn:last-child');
                if (!targetBtn) return;
                var targetUrl = targetBtn.href;

                // Animate all switcher instances simultaneously
                document.querySelectorAll('.language-switcher').forEach(function (s) {
                    isCzActive ? s.classList.remove('cz-active') : s.classList.add('cz-active');
                });

                setTimeout(function () { window.location.href = targetUrl; }, 300);
            });
        });
    }

    document.addEventListener('DOMContentLoaded', initLanguageSwitcher);
    if (window.Blazor && window.Blazor.addEventListener) {
        window.Blazor.addEventListener('enhancedload', initLanguageSwitcher);
    }
})();

// ── Game Cookies ──
// Small helper used by Home/Game pages (via JS interop) to persist the
// current player's id per room without exposing it in the URL.
window.gameCookies = {
    set: function (name, value, days) {
        var expires = '';
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = '; expires=' + date.toUTCString();
        }
        document.cookie = name + '=' + encodeURIComponent(value) + expires + '; path=/; samesite=lax';
    },
    get: function (name) {
        var match = document.cookie.match('(^|;\\s*)' + name + '=([^;]*)');
        return match ? decodeURIComponent(match[2]) : null;
    }
};
