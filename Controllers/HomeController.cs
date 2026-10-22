using System.Diagnostics;
using ERP_System.Models;
using Microsoft.AspNetCore.Mvc;
using ERP_System.Data;
using Microsoft.AspNetCore.Localization;

namespace ERP_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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

            // 1. Force Cookie Deletion Before Setting: Explicitly delete old cookies with Path="/"
            Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName, new CookieOptions { Path = "/" });
            Response.Cookies.Delete("app_lang", new CookieOptions { Path = "/" });

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

            // 2. Anti-Caching Headers: Ensure proxies and browsers never cache this response
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // 4. Fallback Protection: Sanitize returnUrl to prevent LocalRedirect exceptions
            string safeReturnUrl = "/";
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                safeReturnUrl = returnUrl;
            }
            else
            {
                var referer = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer) && 
                    Uri.TryCreate(referer, UriKind.Absolute, out var refererUri) && 
                    string.Equals(refererUri.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase))
                {
                    safeReturnUrl = refererUri.PathAndQuery;
                    if (!Url.IsLocalUrl(safeReturnUrl))
                    {
                        safeReturnUrl = "/";
                    }
                }
            }

            // Strip any existing lang_refresh parameter to prevent compounding query strings
            safeReturnUrl = System.Text.RegularExpressions.Regex.Replace(safeReturnUrl, @"([?&])lang_refresh=\d+(&|$)", "$1");
            safeReturnUrl = safeReturnUrl.TrimEnd('?', '&');
            if (string.IsNullOrEmpty(safeReturnUrl))
            {
                safeReturnUrl = "/";
            }

            // 3. Cache-Busting Redirect: Append dynamic timestamp to force CDN/browser bypass
            string separator = safeReturnUrl.Contains('?') ? "&" : "?";
            string noCacheUrl = $"{safeReturnUrl}{separator}lang_refresh={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            if (!Url.IsLocalUrl(noCacheUrl))
            {
                noCacheUrl = $"/?lang_refresh={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers["Accept"].ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = true, culture = culture, redirectUrl = noCacheUrl });
            }

            return LocalRedirect(noCacheUrl);
        }
    }
}
