# Language & Theme Switcher Reference

Extracted from SamzviWeb (.NET 9, Razor Pages) for reuse in a new project.
Contains: server-side EN/CZ localization switcher, client-side light/dark theme switcher, shared CSS, and the full color palette.

---

## 1. Language Switcher (EN/CZ cookie-based localization)

### Program.cs — register localization services
```csharp
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
```

### Program.cs — middleware (add after `app.UseRouting();`)
```csharp
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("cs") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
	DefaultRequestCulture = new RequestCulture("en"),
	SupportedCultures = supportedCultures,
	SupportedUICultures = supportedCultures,
	RequestCultureProviders = new List<IRequestCultureProvider>
	{
		new CookieRequestCultureProvider { CookieName = ".Culture" }
	}
});
```

### Controllers/CultureController.cs (full file)
```csharp
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace SamzviWeb.Controllers;

[Route("[controller]/[action]")]
public class CultureController : Controller
{
	[HttpGet]
	public IActionResult Set(string culture, string returnUrl = "/")
	{
		if (!string.IsNullOrEmpty(culture))
		{
			Response.Cookies.Append(
				".Culture",
				CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
				new CookieOptions
				{
					Expires = DateTimeOffset.UtcNow.AddYears(1),
					IsEssential = true,
					SameSite = SameSiteMode.Lax
				}
			);
		}

		// Ensure returnUrl is local to prevent open redirect
		if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
		{
			returnUrl = "/";
		}

		return LocalRedirect(returnUrl);
	}
}
```

### Layout markup (Razor) — flag icons via CDN
```razor
@{
	var currentCulture = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
	var currentPath = Context.Request.Path + Context.Request.QueryString;
}
<div class="language-switcher @(currentCulture == "cs" ? "cz-active" : "")">
	<a href="/Culture/Set?culture=en&returnUrl=@Uri.EscapeDataString(currentPath)" class="lang-btn" title="English"><span class="fi fi-us"></span></a>
	<a href="/Culture/Set?culture=cs&returnUrl=@Uri.EscapeDataString(currentPath)" class="lang-btn" title="Čeština"><span class="fi fi-cz"></span></a>
</div>
```

### `<head>` CDN link required for flags
```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flag-icons@7.2.3/css/flag-icons.min.css" />
```

---

## 2. Theme Switcher (light/dark via `data-theme` attribute + localStorage)

### Inline script in `<head>` (prevents flash of wrong theme on load)
```html
<script>
	(function() {
		var theme = localStorage.getItem('theme') || 'dark';
		document.documentElement.setAttribute('data-theme', theme);
	})();
</script>
```

### Layout markup (Razor)
```razor
<div class="theme-switcher">
	<span class="theme-btn theme-btn-light" title="Light mode">☀️</span>
	<span class="theme-btn theme-btn-dark" title="Dark mode">🌙</span>
</div>
```

---

## 3. wwwroot/js/site.js — theme + language client-side logic

```javascript
// Site-wide JavaScript functionality

// ── Dark Mode Toggle ──
// Works on every .theme-switcher instance (header + burger menu)
document.addEventListener('DOMContentLoaded', function() {
	function setTheme(next) {
		if ((document.documentElement.getAttribute('data-theme') || 'light') === next) return;
		document.documentElement.setAttribute('data-theme', next);
		localStorage.setItem('theme', next);
	}

	document.querySelectorAll('.theme-switcher').forEach(function(switcher) {
		switcher.addEventListener('click', function() {
			var current = document.documentElement.getAttribute('data-theme') || 'light';
			setTheme(current === 'light' ? 'dark' : 'light');
		});
	});
});

// ── Language Switcher ──
// Works on every .language-switcher instance (header + burger menu)
document.addEventListener('DOMContentLoaded', function() {
	document.querySelectorAll('.language-switcher').forEach(function(switcher) {
		switcher.addEventListener('click', function(e) {
			e.preventDefault();
			var isCzActive = switcher.classList.contains('cz-active');
			// Navigate to the other language
			var targetBtn = isCzActive
				? switcher.querySelector('.lang-btn:first-child')
				: switcher.querySelector('.lang-btn:last-child');
			if (!targetBtn) return;
			var targetUrl = targetBtn.href;

			// Animate all switcher instances simultaneously
			document.querySelectorAll('.language-switcher').forEach(function(s) {
				isCzActive ? s.classList.remove('cz-active') : s.classList.add('cz-active');
			});

			setTimeout(function() { window.location.href = targetUrl; }, 300);
		});
	});
});
```

---

## 4. Switcher CSS (pill-style, sliding highlight)

```css
/* Both switchers share this "sliding pill" pattern */
.language-switcher, .theme-switcher {
	display: flex;
	align-items: center;
	background: var(--switcher-bg);
	border-radius: 20px;
	padding: 3px;
	position: relative;
	cursor: pointer;
}

.language-switcher::before, .theme-switcher::before {
	content: '';
	position: absolute;
	width: calc(50% - 3px);
	height: calc(100% - 6px);
	background: var(--color-primary);
	border-radius: 18px;
	transition: transform 0.3s ease;
	top: 3px;
	left: 3px;
}

.language-switcher.cz-active::before,
[data-theme="dark"] .theme-switcher::before {
	transform: translateX(100%);
}

.lang-btn, .theme-btn {
	padding: 4px 6px;
	border-radius: 18px;
	font-size: 1.1rem;
	line-height: 1;
	position: relative;
	z-index: 1;
	transition: opacity 0.3s ease;
	user-select: none;
	cursor: pointer;
}

.lang-btn {
	text-decoration: none;
	background: transparent;
	display: flex;
	align-items: center;
}

/* EN/🇺🇸 and ☀️ active by default */
.lang-btn:first-child, .theme-btn-light { opacity: 1; }
.lang-btn:last-child, .theme-btn-dark { opacity: 0.45; }

/* CZ/🇨🇿 and 🌙 active */
.language-switcher.cz-active .lang-btn:first-child,
[data-theme="dark"] .theme-btn-light { opacity: 0.45; }

.language-switcher.cz-active .lang-btn:last-child,
[data-theme="dark"] .theme-btn-dark { opacity: 1; }
```

---

## 5. Color Palette (CSS custom properties — wwwroot/css/theme.css)

### Light theme (`:root`)
```css
:root {
	/* Backgrounds */
	--bg-body: #ffffff;
	--bg-surface: #f8f9fa;
	--bg-surface-alt: #e9ecef;
	--bg-elevated: #ffffff;

	/* Text */
	--text-primary: #333333;
	--text-secondary: #555555;
	--text-muted: #6c757d;
	--text-on-primary: #ffffff;
	--text-on-danger: #ffffff;

	/* Borders */
	--border-default: #dee2e6;
	--border-light: #e0e0e0;
	--border-strong: #cccccc;

	/* Brand / accent */
	--color-primary: #007bff;
	--color-primary-hover: #0056b3;
	--color-primary-soft: #e7f1ff;
	--color-success: #28a745;
	--color-success-hover: #1e7e34;
	--color-danger: #dc3545;
	--color-danger-hover: #c82333;
	--color-warning: #ffc107;
	--color-secondary: #6c757d;
	--color-secondary-hover: #545b62;
	--color-orange: orange;

	/* Alerts */
	--alert-success-bg: #d4edda;
	--alert-success-text: #155724;
	--alert-success-border: #c3e6cb;
	--alert-danger-bg: #f8d7da;
	--alert-danger-text: #721c24;
	--alert-danger-border: #f5c6cb;
	--alert-info-bg: #d1ecf1;
	--alert-info-text: #0c5460;
	--alert-info-border: #bee5eb;
	--alert-warning-bg: #fff3cd;
	--alert-warning-text: #856404;
	--alert-warning-border: #ffeeba;

	/* Inputs */
	--input-bg: #ffffff;
	--input-border: #dddddd;
	--input-focus-border: #007bff;
	--input-focus-ring: rgba(0, 123, 255, 0.25);

	/* Shadows */
	--shadow-sm: 0 2px 4px rgba(0, 0, 0, 0.1);
	--shadow-md: 0 4px 12px rgba(0, 0, 0, 0.1);
	--shadow-lg: 0 4px 12px rgba(0, 0, 0, 0.3);

	/* Layout */
	--header-bg: #ffffff;
	--footer-bg: #ffffff;
	--header-border: #cccccc;
	--logo-color: #000000;
	--footer-text: dimgrey;

	/* Switcher */
	--switcher-bg: #f0f0f0;
	--switcher-inactive: #666666;
	--switcher-inactive-hover: #333333;

	/* Tables */
	--table-header-bg: #e9ecef;
	--table-row-hover: #f8f9fa;
	--table-border: #dee2e6;

	/* Checkers board (game-specific, optional) */
	--board-light-square: #f0d9b5;
	--board-dark-square: #b58863;
	--board-border: #333333;

	/* Misc */
	--link-color: #007bff;
	--code-bg: #f8f9fa;
	--highlight-purple-bg: #f3e5f5;
	--highlight-purple-border: #9c27b0;
	--highlight-purple-text: #4a148c;
	--highlight-blue-bg: #e3f2fd;
	--highlight-blue-border: #2196f3;
	--highlight-blue-text: #0d47a1;
	--highlight-blue-code-bg: #bbdefb;
	--your-turn-bg: linear-gradient(to right, #fff3cd, transparent);
	--your-turn-border: #ffc107;
	--invite-bg: linear-gradient(to right, #e7f3ff, transparent);
	--invite-status: #0066cc;
}
```

### Dark theme (`[data-theme="dark"]`) — GitHub-style deep dark-blue
```css
[data-theme="dark"] {
	/* Backgrounds */
	--bg-body: #0d1117;
	--bg-surface: #161b22;
	--bg-surface-alt: #1c2333;
	--bg-elevated: #1c2333;

	/* Text */
	--text-primary: #e6edf3;
	--text-secondary: #b1bac4;
	--text-muted: #8b949e;
	--text-on-primary: #ffffff;
	--text-on-danger: #ffffff;

	/* Borders */
	--border-default: #30363d;
	--border-light: #21262d;
	--border-strong: #484f58;

	/* Brand / accent */
	--color-primary: #58a6ff;
	--color-primary-hover: #79b8ff;
	--color-primary-soft: #1c3a5e;
	--color-success: #3fb950;
	--color-success-hover: #56d364;
	--color-danger: #f85149;
	--color-danger-hover: #ff6b63;
	--color-warning: #d29922;
	--color-secondary: #484f58;
	--color-secondary-hover: #6e7681;
	--color-orange: #e87b35;

	/* Alerts */
	--alert-success-bg: #0d2818;
	--alert-success-text: #3fb950;
	--alert-success-border: #1b4332;
	--alert-danger-bg: #2d0d0d;
	--alert-danger-text: #f85149;
	--alert-danger-border: #4c1d1d;
	--alert-info-bg: #0d1d2d;
	--alert-info-text: #58a6ff;
	--alert-info-border: #1c3a5e;
	--alert-warning-bg: #2d2200;
	--alert-warning-text: #d29922;
	--alert-warning-border: #4d3800;

	/* Inputs */
	--input-bg: #0d1117;
	--input-border: #30363d;
	--input-focus-border: #58a6ff;
	--input-focus-ring: rgba(88, 166, 255, 0.3);

	/* Shadows */
	--shadow-sm: 0 2px 4px rgba(0, 0, 0, 0.3);
	--shadow-md: 0 4px 12px rgba(0, 0, 0, 0.4);
	--shadow-lg: 0 4px 12px rgba(0, 0, 0, 0.6);

	/* Layout */
	--header-bg: #161b22;
	--footer-bg: #161b22;
	--header-border: #30363d;
	--logo-color: #e6edf3;
	--footer-text: #8b949e;

	/* Switcher */
	--switcher-bg: #21262d;
	--switcher-inactive: #8b949e;
	--switcher-inactive-hover: #e6edf3;

	/* Tables */
	--table-header-bg: #1c2333;
	--table-row-hover: #1c2333;
	--table-border: #30363d;

	/* Checkers board (game-specific, optional) */
	--board-light-square: #4a5568;
	--board-dark-square: #2d3748;
	--board-border: #58a6ff;

	/* Misc */
	--link-color: #58a6ff;
	--code-bg: #161b22;
	--highlight-purple-bg: #2d1b3d;
	--highlight-purple-border: #9c27b0;
	--highlight-purple-text: #ce93d8;
	--highlight-blue-bg: #0d1d2d;
	--highlight-blue-border: #58a6ff;
	--highlight-blue-text: #90caf9;
	--highlight-blue-code-bg: #1c3a5e;
	--your-turn-bg: linear-gradient(to right, #2d2200, transparent);
	--your-turn-border: #d29922;
	--invite-bg: linear-gradient(to right, #0d1d2d, transparent);
	--invite-status: #58a6ff;
}
```

---

## 6. Full head snippet (theme CSS load order matters)
```html
<script>
	// Apply theme immediately to prevent flash of wrong theme
	(function() {
		var theme = localStorage.getItem('theme') || 'dark';
		document.documentElement.setAttribute('data-theme', theme);
	})();
</script>

<link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
<link rel="stylesheet" href="~/css/theme.css" asp-append-version="true" />
<link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flag-icons@7.2.3/css/flag-icons.min.css" />
```

---

## Architecture Notes

- **Theme system**: Driven entirely by CSS custom properties toggled via the `data-theme="dark"` attribute on `<html>`. State persisted in `localStorage['theme']`. Defaults to `'dark'` if unset.
- **Language system**: Server-side only — no client-side i18n library. Uses ASP.NET Core's built-in `RequestLocalization` middleware + a cookie (`.Culture`) read by `CookieRequestCultureProvider`. Clicking a flag navigates to `/Culture/Set?culture=xx&returnUrl=...`, which sets the cookie and redirects back.
- **Shared UI pattern**: Both switchers are "sliding pill" toggles — two options side by side, with a highlighted background that slides via CSS `transform: translateX(100%)` based on state class (`cz-active`) or attribute (`[data-theme="dark"]`).
- **Dependencies**: `flag-icons` CSS (CDN) for country flags. No other third-party libraries required.
- **Reusability**: Place `theme.css` before `site.css` in `<head>` load order since `site.css` (and other page-specific stylesheets) consume the `--*` custom properties defined in `theme.css`.
