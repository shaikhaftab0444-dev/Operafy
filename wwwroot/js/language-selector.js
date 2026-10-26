// Global Language Selector & Software Translator
(function () {
    const supportedLanguages = [
        { code: 'en', name: 'English', flag: '🇬🇧' },
        { code: 'hi', name: 'हिंदी (Hindi)', flag: '🇮🇳' },
        { code: 'es', name: 'Español (Spanish)', flag: '🇪🇸' },
        { code: 'ar', name: 'العربية (Arabic)', flag: '🇸🇦', rtl: true },
        { code: 'fr', name: 'Français (French)', flag: '🇫🇷' },
        { code: 'de', name: 'Deutsch (German)', flag: '🇩🇪' },
        { code: 'ur', name: 'اردو (Urdu)', flag: '🇵🇰', rtl: true },
        { code: 'zh-CN', name: '中文 (Chinese)', flag: '🇨🇳' }
    ];

    function deleteCookie(name) {
        document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
        document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC;";
        const host = window.location.hostname;
        document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/; domain=" + host + ";";
        document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/; domain=." + host + ";";
    }

    function setCookie(name, value, days) {
        let expires = "";
        if (days) {
            let date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toUTCString();
        }
        const isSecure = window.location.protocol === 'https:';
        const secureFlag = isSecure ? "; Secure; SameSite=Lax" : "; SameSite=Lax";
        document.cookie = name + "=" + (value || "") + expires + "; path=/" + secureFlag;
    }

    function getCookie(name) {
        let nameEQ = name + "=";
        let ca = document.cookie.split(';');
        for (let i = 0; i < ca.length; i++) {
            let c = ca[i];
            while (c.charAt(0) === ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    }

    window.changeLanguage = function (langCode) {
        const langObj = supportedLanguages.find(l => l.code === langCode) || supportedLanguages[0];
        const aspNetCulture = (langCode === 'en' ? 'en-US' : langCode);
        const cookieVal = 'c=' + aspNetCulture + '|uic=' + aspNetCulture;
        
        // 1. Force cookie deletion on client before setting to prevent conflicts
        deleteCookie('.AspNetCore.Culture');
        deleteCookie('app_lang');
        deleteCookie('googtrans');

        // 2. Frontend Storage Synchronization: explicitly clear stored language values
        // Prevents the frontend from fighting the server's new state during language switches
        localStorage.removeItem('language');
        localStorage.removeItem('culture');
        localStorage.removeItem('googtrans'); // If using Google Translate fallback
        sessionStorage.clear();

        // 3. Save choice in hardened cookies & localStorage
        setCookie('.AspNetCore.Culture', cookieVal, 365);
        if (langCode === 'en') {
            deleteCookie('googtrans');
        } else {
            setCookie('googtrans', '/en/' + langCode, 365);
        }
        setCookie('app_lang', langCode, 365);
        localStorage.setItem('app_lang', langCode);

        // 4. Update document dir for RTL languages
        if (langObj.rtl) {
            document.documentElement.setAttribute('dir', 'rtl');
        } else {
            document.documentElement.removeAttribute('dir');
        }

        // 5. Clean current URL of any old lang_refresh parameter
        let currentReturnUrl = window.location.pathname + window.location.search;
        currentReturnUrl = currentReturnUrl.replace(/([?&])lang_refresh=\d+(&|$)/g, '$1').replace(/[?&]$/, '') || '/';

        // 6. Submit POST form to /Home/SetLanguage to perform server-side cookie overwrite and cache-busted redirect
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = '/Home/SetLanguage';
        form.style.display = 'none';

        const cultureInput = document.createElement('input');
        cultureInput.type = 'hidden';
        cultureInput.name = 'culture';
        cultureInput.value = aspNetCulture;
        form.appendChild(cultureInput);

        const returnUrlInput = document.createElement('input');
        returnUrlInput.type = 'hidden';
        returnUrlInput.name = 'returnUrl';
        returnUrlInput.value = currentReturnUrl;
        form.appendChild(returnUrlInput);

        document.body.appendChild(form);
        form.submit();
    };

    // Google Translate Initialization Callback
    window.googleTranslateElementInit = function () {
        new google.translate.TranslateElement({
            pageLanguage: 'en',
            includedLanguages: 'en,hi,es,ar,fr,de,ur,zh-CN',
            layout: google.translate.TranslateElement.InlineLayout.SIMPLE,
            autoDisplay: false
        }, 'google_translate_element');
    };

    // Inject strict CSS rules to suppress Google Translate Banner & Toolbar
    const hideGoogleBannerStyle = function () {
        if (document.getElementById('hide-google-translate-style')) return;
        const style = document.createElement('style');
        style.id = 'hide-google-translate-style';
        style.innerHTML = `
            .goog-te-banner-frame,
            .goog-te-banner-frame.skiptranslate,
            iframe.skiptranslate,
            iframe[class*="VIpgJd"],
            iframe[id*="goog-gt"],
            .VIpgJd-ZGain-xl0qp4-Ojan2-OWX0fe,
            #goog-gt-tt,
            .goog-te-balloon-frame,
            .goog-tooltip,
            .goog-tooltip:hover {
                display: none !important;
                visibility: hidden !important;
                opacity: 0 !important;
                height: 0 !important;
                width: 0 !important;
                pointer-events: none !important;
            }

            body {
                top: 0px !important;
                position: static !important;
                margin-top: 0px !important;
            }

            #google_translate_element,
            .goog-te-spinner-pos,
            .goog-te-combo {
                display: none !important;
            }

            font[style] {
                background-color: transparent !important;
                box-shadow: none !important;
            }
        `;
        document.head.appendChild(style);
    };

    hideGoogleBannerStyle();

    document.addEventListener("DOMContentLoaded", function () {
        hideGoogleBannerStyle();

        // Load Google Translate Script asynchronously
        if (!document.getElementById('google-translate-script')) {
            const gtScript = document.createElement('script');
            gtScript.id = 'google-translate-script';
            gtScript.src = '//translate.google.com/translate_a/element.js?cb=googleTranslateElementInit';
            document.head.appendChild(gtScript);
        }

        // Auto-restore saved language & RTL setting
        const savedLang = getCookie('app_lang') || localStorage.getItem('app_lang') || 'en';
        const langObj = supportedLanguages.find(l => l.code === savedLang);
        if (langObj && langObj.rtl) {
            document.documentElement.setAttribute('dir', 'rtl');
        }

        // Remove injected Google topbar iframes continuously
        setInterval(function () {
            document.body.style.top = '0px';
            const frames = document.querySelectorAll('iframe.skiptranslate, iframe[class*="VIpgJd"]');
            frames.forEach(f => {
                f.style.display = 'none';
                f.style.visibility = 'hidden';
                f.style.height = '0px';
            });
        }, 300);
    });
})();
