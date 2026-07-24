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

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            var supportedCultures = new[] { "en", "cs" };
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.SetDefaultCulture(supportedCultures[0])
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);
                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new CookieRequestCultureProvider()
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

            app.MapGet("/culture/set", (HttpContext httpContext, string culture, string redirectUri) =>
            {
                if (!string.IsNullOrEmpty(culture))
                {
                    var requestCulture = new RequestCulture(culture, culture);
                    var cookieValue = CookieRequestCultureProvider.MakeCookieValue(requestCulture);
                    httpContext.Response.Cookies.Append(
                        CookieRequestCultureProvider.DefaultCookieName,
                        cookieValue,
                        new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddYears(1),
                            IsEssential = true,
                            SameSite = SameSiteMode.Lax
                        });
                }

                return Results.LocalRedirect(redirectUri);
            });

            app.Run();
        }
    }
}

