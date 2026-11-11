// Global Language Selector & Software Translator
(function () {
    const supportedLanguages = [
        { code: 'en-US', short: 'en', name: 'English', flag: '🇬🇧' },
        { code: 'hi-IN', short: 'hi', name: 'Hindi (हिन्दी)', flag: '🇮🇳' },
        { code: 'ur-PK', short: 'ur', name: 'Urdu (اردو)', flag: '🇵🇰', rtl: true },
        { code: 'mr-IN', short: 'mr', name: 'Marathi (मराठी)', flag: '🇮🇳' },
        { code: 'ar-SA', short: 'ar', name: 'Arabic (العربية)', flag: '🇸🇦', rtl: true }
    ];

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

    function switchLanguage(cultureCode) {
        if (!cultureCode) cultureCode = 'en-US';
        
        // Set cookie directly in JavaScript as well to avoid race conditions
        const date = new Date();
        date.setTime(date.getTime() + (365 * 24 * 60 * 60 * 1000));
        const expires = "; expires=" + date.toUTCString();
        
        // Write both AspNetCore culture and localStorage with SameSite=Lax (safe for HTTP)
        document.cookie = ".AspNetCore.Culture=c=" + cultureCode + "|uic=" + cultureCode + expires + "; path=/; SameSite=Lax";
        localStorage.setItem("user_selected_language", cultureCode);

        // Also sync Google Translate googtrans and app_lang cookies
        const shortLang = cultureCode.split('-')[0];
        document.cookie = "googtrans=/en/" + shortLang + expires + "; path=/; SameSite=Lax";
        document.cookie = "app_lang=" + cultureCode + expires + "; path=/; SameSite=Lax";
        localStorage.setItem("app_lang", shortLang);

        // POST/GET to backend controller and reload cleanly
        window.location.href = '/Home/SetLanguage?culture=' + encodeURIComponent(cultureCode) + '&returnUrl=' + encodeURIComponent(window.location.pathname);
    }

    window.switchLanguage = switchLanguage;

    window.changeLanguage = function (langCode) {
        const codeMap = {
            'en': 'en-US',
            'hi': 'hi-IN',
            'ur': 'ur-PK',
            'mr': 'mr-IN',
            'ar': 'ar-SA'
        };
        switchLanguage(codeMap[langCode] || langCode);
    };

    // Google Translate Initialization Callback
    window.googleTranslateElementInit = function () {
        new google.translate.TranslateElement({
            pageLanguage: 'en',
            includedLanguages: 'en,hi,ur,mr,ar,es,fr,de,zh-CN',
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

        // Update labels on UI
        const langMap = {
            'en-US': '🇬🇧 English',
            'en': '🇬🇧 English',
            'hi-IN': '🇮🇳 Hindi (हिन्दी)',
            'hi': '🇮🇳 Hindi (हिन्दी)',
            'ur-PK': '🇵🇰 Urdu (اردو)',
            'ur': '🇵🇰 Urdu (اردو)',
            'mr-IN': '🇮🇳 Marathi (मराठी)',
            'mr': '🇮🇳 Marathi (मराठी)',
            'ar-SA': '🇸🇦 Arabic (العربية)',
            'ar': '🇸🇦 Arabic (العربية)'
        };
        const current = localStorage.getItem("user_selected_language") || getCookie('app_lang') || 'en-US';
        
        const label = document.getElementById("currentLangLabel");
        if (label && langMap[current]) {
            label.textContent = langMap[current];
        }
        const landingLabel = document.getElementById("landingLangLabel");
        if (landingLabel && langMap[current]) {
            landingLabel.textContent = langMap[current];
        }

        // Apply RTL if Arabic or Urdu
        if (current.startsWith('ar') || current.startsWith('ur')) {
            document.documentElement.setAttribute('dir', 'rtl');
        } else {
            document.documentElement.removeAttribute('dir');
        }

        // Load Google Translate Script asynchronously
        if (!document.getElementById('google-translate-script')) {
            const gtScript = document.createElement('script');
            gtScript.id = 'google-translate-script';
            gtScript.src = '//translate.google.com/translate_a/element.js?cb=googleTranslateElementInit';
            document.head.appendChild(gtScript);
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
