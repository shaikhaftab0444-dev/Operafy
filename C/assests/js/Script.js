// GLOBAL EVENT DELEGATION & MOBILE HEADER MENU
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
document.addEventListener("click", function (e) {
  // Mobile Header Menu Toggle
  const menuBtn = e.target.closest("#menuBtn, .mobile-menu-btn");
  if (menuBtn) {
    e.preventDefault();
    e.stopPropagation();
    const panel = document.getElementById("mobileMenu");
    if (panel) {
      panel.classList.toggle("active");
    }
    return;
  }

  // Close mobile menu when clicking outside
  const mobileMenu = document.getElementById("mobileMenu");
  if (mobileMenu && mobileMenu.classList.contains("active")) {
    if (!e.target.closest("#mobileMenu")) {
      mobileMenu.classList.remove("active");
    }
  }

  // Close dropdown menus when clicking outside
  if (!e.target.closest('.dropdown')) {
    document.querySelectorAll('.dropdown-menu.show').forEach(dropdown => {
      dropdown.classList.remove('show');
    });
  }
});


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// SIDEBAR BUTTON & ACCESSIBILITY INITIALIZATION
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
document.addEventListener("DOMContentLoaded", () => {
  const buttonPlaceholder = document.getElementById("Button-placeholder");

  if (buttonPlaceholder) {
    fetch("Components/Buttons/Button.html")
      .then(res => res.text())
      .then(data => {
        buttonPlaceholder.innerHTML = data;
        initSidebarNav();
      })
      .catch(err => {
        console.warn("Sidebar component load note:", err);
      });
  } else {
    initSidebarNav();
  }
});

function initSidebarNav() {
  const currentPage = location.pathname.split("/").pop();

  /* ===============================
     NORMAL NAV BUTTONS
  =============================== */
  document.querySelectorAll(".nav-button").forEach(btn => {
    if (btn.dataset.link === currentPage) {
      btn.classList.add("active");
    }

    btn.addEventListener("click", () => {
      if (btn.id) {
        sessionStorage.setItem("scrollBtnId", btn.id);
        sessionStorage.setItem("scrollType", "button");
      }

      const link = btn.dataset.link;
      if (link) window.location.href = link;
    });
  });

  /* ===============================
     DROPDOWN ITEMS
  =============================== */
  document.querySelectorAll(".dropdown-menu .dropper").forEach(item => {
    const onclickAttr = item.getAttribute("onclick");
    if (!onclickAttr) return;

    const match = onclickAttr.match(/'([^']+)'/);
    if (!match) return;

    const page = match[1];

    if (page === currentPage) {
      setDropdownActive(item);
    }

    item.addEventListener("click", (e) => {
      e.stopPropagation();

      if (item.id) {
        sessionStorage.setItem("scrollBtnId", item.id);
        sessionStorage.setItem("scrollType", "dropdown");
      }

      window.location.href = page;
    });
  });

  /* ===============================
     NEXT / PREVIOUS BUTTONS
  =============================== */
  document.querySelectorAll(".nav-next-prev").forEach(linkBtn => {
    linkBtn.addEventListener("click", () => {
      const targetHref = linkBtn.getAttribute("href");
      if (!targetHref) return;

      const targetPage = targetHref.split("/").pop();

      const sidebarBtn = document.querySelector(
        `.nav-button[data-link="${targetPage}"], .dropdown-menu .dropper[onclick*="${targetPage}"]`
      );

      if (sidebarBtn && sidebarBtn.id) {
        sessionStorage.setItem("scrollBtnId", sidebarBtn.id);
      }
    });
  });

  /* ===============================
     SCROLL AFTER PAGE LOAD
  =============================== */
  const savedBtnId = sessionStorage.getItem("scrollBtnId");

  if (savedBtnId) {
    const activeEl = document.getElementById(savedBtnId);

    if (activeEl) {
      document.querySelectorAll(".nav-button, .dropper").forEach(el => {
        el.classList.remove("active");
      });

      activeEl.classList.add("active");
      setDropdownActive(activeEl);

      activeEl.scrollIntoView({
        behavior: "smooth",
        block: "start"
      });

      setTimeout(() => {
        window.scrollBy({
          top: -150,
          behavior: "smooth"
        });
      }, 300);
    }

    sessionStorage.removeItem("scrollBtnId");
    sessionStorage.removeItem("scrollType");
  }
}

/* ===============================
   HELPER: OPEN DROPDOWN & ACTIVE
=============================== */
function setDropdownActive(item) {
  if (!item) return;
  const dropdownMenu = item.closest(".dropdown-menu");
  if (dropdownMenu) {
    dropdownMenu.classList.add("show");

    const parentBtn = dropdownMenu.previousElementSibling;
    if (parentBtn) parentBtn.classList.add("active");
  }
}

/* ===============================
   MONACO EDITOR INITIALIZATION (IF PRESENT)
=============================== */
if (typeof require !== "undefined") {
  require.config({
    paths: {
      vs: 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.44.0/min/vs'
    }
  });

  require(['vs/editor/editor.main'], function () {
    function getInitialCode(id) {
      return document.getElementById(id)?.textContent || '';
    }

    const editorConfigs = [
      { editorId: 'editor1', frameId: 'outputFrame1', codeId: 'code1' },
      { editorId: 'editor2', frameId: 'outputFrame2', codeId: 'code2' },
      { editorId: 'editor3', frameId: 'outputFrame3', codeId: 'code3' },
      { editorId: 'editor4', frameId: 'outputFrame4', codeId: 'code4' },
      { editorId: 'editor5', frameId: 'outputFrame5', codeId: 'code5' }
    ];

    editorConfigs.forEach(({ editorId, frameId, codeId }) => {
      const el = document.getElementById(editorId);
      if (!el) return;
      const editor = monaco.editor.create(el, {
        value: getInitialCode(codeId),
        language: 'html',
        theme: 'vs-dark',
        automaticLayout: true,
        wordWrap: 'on',
        scrollBeyondLastLine: false,
        contextmenu: false
      });

      const iframe = document.getElementById(frameId);
      if (iframe) {
        iframe.srcdoc = editor.getValue();
        editor.onDidChangeModelContent(() => {
          iframe.srcdoc = editor.getValue();
        });
      }
    });
  });
}

/* ===============================
   RESPONSIVE SIDEBAR TOGGLE
=============================== */
window.addEventListener('resize', function () {
  const width = window.innerWidth;
  const sidebarMenu = document.getElementById('sidebarMenu');
  if (!sidebarMenu) return;

  if (width <= 768) {
    sidebarMenu.classList.remove('show');
  } else {
    sidebarMenu.classList.add('show');
  }
});

/* ===============================
   HELPER FUNCTIONS (NAV & DROPDOWN)
=============================== */
function toggleDropdown(event, dropdownId) {
  if (event) {
    event.stopPropagation();
    event.preventDefault();
  }

  const dropdown = document.getElementById(dropdownId);
  if (!dropdown) return;

  document.querySelectorAll('.dropdown-menu').forEach(d => {
    if (d.id !== dropdownId) {
      d.classList.remove('show');
    }
  });

  dropdown.classList.toggle('show');
}

function navigateTo(link) {
  if (!link) return;

  const clickedButton = document.querySelector(`button[onclick*="${link}"]`);
  if (clickedButton) {
    document.querySelectorAll('.nav-button, .dropper').forEach(btn => {
      btn.classList.remove('active');
    });

    clickedButton.classList.add('active');

    const pageNumber = clickedButton.getAttribute('data-page');
    if (pageNumber) {
      localStorage.setItem('activePage', pageNumber);
    }
  }

  window.location.href = link;
}

/* ===============================
   CODE BLOCK & SELECTION HANDLERS
=============================== */
function toggleCode() {
  const codeBlock = document.getElementById("codeBlock");
  const switchLabel = document.getElementById("switchLabel");
  const toggleSwitch = document.getElementById("toggleSwitch");

  if (toggleSwitch && codeBlock && switchLabel) {
    if (toggleSwitch.checked) {
      codeBlock.style.display = "block";
      switchLabel.innerText = "Hide Code";
    } else {
      codeBlock.style.display = "none";
      switchLabel.innerText = "Show Code";
    }
  }
}

const codeBlockEl = document.getElementById("codeBlock");
if (codeBlockEl) {
  codeBlockEl.addEventListener("selectstart", e => {
    if (!codeBlockEl.classList.contains("unlocked")) {
      e.preventDefault();
    }
  });
}

document.addEventListener("keydown", e => {
  if (e.ctrlKey && e.code === "Backquote") {
    const codeBlock = document.getElementById("codeBlock");
    if (!codeBlock) return;

    e.preventDefault();
    codeBlock.classList.add("unlocked");

    setTimeout(() => {
      codeBlock.style.userSelect = "text";
      codeBlock.style.cursor = "text";

      codeBlock.querySelectorAll("span").forEach(span => {
        span.style.userSelect = "text";
        span.style.cursor = "text";
      });
    }, 10);

    console.log("✅ Unlocked selection");
  }
});


/* ===============================
   COPY BUTTON FUNCTIONALITY
=============================== */
document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll(".copy-btn").forEach(button => {
    button.addEventListener("click", function () {
      const container = this.closest(".code-container");
      if (!container) return;

      const codeEl = container.querySelector(".code-block, pre, code");
      if (!codeEl) return;

      const codeText = codeEl.innerText;

      navigator.clipboard.writeText(codeText).then(() => {
        const span = this.querySelector("span");
        if (span) {
          this.classList.add("copy-success", "copied");
          span.innerText = "Copied ✓";

          setTimeout(() => {
            this.classList.remove("copy-success", "copied");
            span.innerText = "Copy";
          }, 2000);
        }
      });
    });
  });
});
