using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace ERP_System.Controllers
{
    public class LanguageController : Controller
    {
        [HttpGet]
        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult SetLanguage(string culture, string returnUrl = "/")
        {
            if (string.IsNullOrWhiteSpace(culture))
            {
                culture = "en-US";
            }

            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Path = "/",
                HttpOnly = false, // Allow client scripts to synchronize
                Secure = Request.IsHttps, // ONLY require HTTPS if currently on HTTPS
                SameSite = SameSiteMode.Lax // Lax allows cookie persistence on HTTP redirects
            };

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                cookieOptions
            );

            // Also set standard googtrans cookie if Google Translate script exists
            Response.Cookies.Append("googtrans", $"/en/{culture.Split('-')[0]}", cookieOptions);
            Response.Cookies.Append("app_lang", culture, cookieOptions);

            // Anti-Caching Headers: Ensure proxies, browsers, and CDNs never cache this response
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers["Accept"].ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                var targetUrl = (Url.IsLocalUrl(returnUrl) && !string.IsNullOrEmpty(returnUrl)) ? returnUrl.Split('?')[0] : "/";
                return Json(new { success = true, culture = culture, redirectUrl = targetUrl });
            }

            if (Url.IsLocalUrl(returnUrl) && !string.IsNullOrEmpty(returnUrl))
            {
                // Remove old lang_refresh param if exists to prevent infinite reload loops
                var cleanUrl = returnUrl.Split('?')[0];
                return Redirect(cleanUrl);
            }

            return Redirect("/");
        }

        [HttpGet]
        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult ChangeLanguage(string culture, string returnUrl = "/")
        {
            return SetLanguage(culture, returnUrl);
        }
    }
}
