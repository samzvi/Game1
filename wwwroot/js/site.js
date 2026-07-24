// Site-wide JavaScript functionality

// ── Dark Mode Toggle ──
// Works on every .theme-switcher instance
document.addEventListener('DOMContentLoaded', function () {
    function setTheme(next) {
        if ((document.documentElement.getAttribute('data-theme') || 'dark') === next) return;
        document.documentElement.setAttribute('data-theme', next);
        localStorage.setItem('theme', next);
        document.cookie = `theme=${next};path=/;max-age=${60 * 60 * 24 * 365};samesite=lax`;
    }

    document.querySelectorAll('.theme-switcher').forEach(function (switcher) {
        switcher.addEventListener('click', function () {
            var current = document.documentElement.getAttribute('data-theme') || 'dark';
            setTheme(current === 'light' ? 'dark' : 'light');
        });
    });
});

// ── Language Switcher ──
// Works on every .language-switcher instance
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.language-switcher').forEach(function (switcher) {
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
});
