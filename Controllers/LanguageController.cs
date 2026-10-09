using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace ERP_System.Controllers
{
    public class LanguageController : Controller
    {
        [HttpPost]
        [HttpGet]
        public IActionResult SetLanguage(string culture, string? returnUrl = null)
        {
            var supportedCultures = new[] { "en-US", "ar", "hi", "es", "fr", "de", "ur", "zh-CN", "en" };
            if (string.IsNullOrWhiteSpace(culture) || !supportedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase))
            {
                culture = "en-US";
            }

            if (string.Equals(culture, "en", StringComparison.OrdinalIgnoreCase))
            {
                culture = "en-US";
            }

            // Determine if request is HTTPS directly or behind reverse proxy
            var isHttpsOrProxy = Request.IsHttps || 
                                 string.Equals(Request.Headers["X-Forwarded-Proto"], "https", StringComparison.OrdinalIgnoreCase);

            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Path = "/",
                SameSite = SameSiteMode.Lax,
                Secure = isHttpsOrProxy,
                HttpOnly = false
            };

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                cookieOptions
            );

            Response.Cookies.Append("app_lang", culture, cookieOptions);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers["Accept"].ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = true, culture = culture });
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri) && refererUri.Host == Request.Host.Host)
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [HttpGet]
        public IActionResult ChangeLanguage(string culture, string? returnUrl = null)
        {
            return SetLanguage(culture, returnUrl);
        }
    }
}
