using Game1.Components;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace Game1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddSingleton<Services.GameRoomService>();

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            var supportedCultures = new[] { "en", "cs" };
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.SetDefaultCulture(supportedCultures[0])
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);
                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new CookieRequestCultureProvider { CookieName = ".Culture" }
                };
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRequestLocalization();

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapGet("/culture/set", (HttpContext httpContext, string culture, string returnUrl) =>
            {
                if (!string.IsNullOrEmpty(culture))
                {
                    var requestCulture = new RequestCulture(culture, culture);
                    var cookieValue = CookieRequestCultureProvider.MakeCookieValue(requestCulture);
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax
                    };
                    // Share the preference across samzvi.site and its subdomains (e.g. game.samzvi.site).
                    if (!app.Environment.IsDevelopment())
                    {
                        cookieOptions.Domain = ".samzvi.site";
                    }

                    httpContext.Response.Cookies.Append(".Culture", cookieValue, cookieOptions);
                }

                if (string.IsNullOrEmpty(returnUrl) || !Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
                {
                    returnUrl = "/";
                }

                return Results.LocalRedirect(returnUrl);
            });

            app.Run();
        }
    }
}

