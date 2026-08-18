let copyBlocked = true;

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
    { editorId: 'editor5', frameId: 'outputFrame5', codeId: 'code5' },
    { editorId: 'editor6', frameId: 'outputFrame6', codeId: 'code6' },
    { editorId: 'editor7', frameId: 'outputFrame7', codeId: 'code7' },
    { editorId: 'editor8', frameId: 'outputFrame8', codeId: 'code8' },
    { editorId: 'editor9', frameId: 'outputFrame9', codeId: 'code9' },
    { editorId: 'editor10', frameId: 'outputFrame10', codeId: 'code10' },
    { editorId: 'editor11', frameId: 'outputFrame11', codeId: 'code11' },
    { editorId: 'editor12', frameId: 'outputFrame12', codeId: 'code12' },
    { editorId: 'editor13', frameId: 'outputFrame13', codeId: 'code13' },
    { editorId: 'editor14', frameId: 'outputFrame14', codeId: 'code14' },
    { editorId: 'editor15', frameId: 'outputFrame15', codeId: 'code15' },
    { editorId: 'editor16', frameId: 'outputFrame16', codeId: 'code16' },
    { editorId: 'editor17', frameId: 'outputFrame17', codeId: 'code17' }
  ];

  const editors = [];

  editorConfigs.forEach(({ editorId, frameId, codeId }) => {
    const editor = monaco.editor.create(document.getElementById(editorId), {
      value: getInitialCode(codeId),
      language: 'html',
      theme: 'vs-dark',
      automaticLayout: true,
      wordWrap: 'on',
      scrollBeyondLastLine: false,
      contextmenu: false
    });


    // Update iframe with editor content
    const iframe = document.getElementById(frameId);
    iframe.srcdoc = editor.getValue();
    editor.onDidChangeModelContent(() => {
      iframe.srcdoc = editor.getValue();
    });

    editors.push(editor);
  });




document.addEventListener("DOMContentLoaded", () => {

  /* ===============================
     LOAD BUTTON HTML
  =============================== */
  fetch("Components/Buttons/Button.html")
    .then(res => res.text())
    .then(data => {

      const placeholder = document.getElementById("Button-placeholder");
      if (!placeholder) return;

      placeholder.innerHTML = data;

      const currentPage = location.pathname.split("/").pop();

      /* ===============================
         NORMAL NAV BUTTONS
      =============================== */
      document.querySelectorAll(".nav-button").forEach(btn => {

        if (btn.dataset.link === currentPage) {
          btn.classList.add("active");
        }

        btn.addEventListener("click", () => {
          sessionStorage.setItem("scrollBtnId", btn.id);

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

        item.addEventListener("click", e => {
          e.stopPropagation();
          sessionStorage.setItem("scrollBtnId", item.id);
          window.location.href = page;
        });
      });

      /* ===============================
         NEXT / PREVIOUS BUTTONS
      =============================== */
      document.querySelectorAll(".nav-next-prev").forEach(linkBtn => {

        linkBtn.addEventListener("click", () => {

          const href = linkBtn.getAttribute("href");
          if (!href) return;

          const targetPage = href.split("/").pop();

          const sidebarBtn = document.querySelector(
            `.nav-button[data-link="${targetPage}"],
             .dropdown-menu .dropper[onclick*="${targetPage}"]`
          );

          if (sidebarBtn?.id) {
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

          document
            .querySelectorAll(".nav-button, .dropper")
            .forEach(el => el.classList.remove("active"));

          activeEl.classList.add("active");
          setDropdownActive(activeEl);

          activeEl.scrollIntoView({ behavior: "smooth", block: "start" });

          setTimeout(() => {
            window.scrollBy({ top: -150, behavior: "smooth" });
          }, 300);
        }

        sessionStorage.removeItem("scrollBtnId");
      }

    });
});


/* ===============================
   HELPER: DROPDOWN ACTIVE
=============================== */
function setDropdownActive(item) {
  const dropdownMenu = item.closest(".dropdown-menu");
  if (!dropdownMenu) return;

  dropdownMenu.classList.add("show");
  const parentBtn = dropdownMenu.previousElementSibling;
  if (parentBtn) parentBtn.classList.add("active");
}


/* ===============================
   SIDEBAR RESPONSIVE
=============================== */
window.addEventListener("resize", () => {
  const sidebarMenu = document.getElementById("sidebarMenu");
  if (!sidebarMenu) return;

  sidebarMenu.classList.toggle("show", window.innerWidth > 768);
});


/* ===============================
   CODE BLOCK LOCK / UNLOCK
=============================== */
const codeBlock = document.getElementById("codeBlock");
if (codeBlock) {

  codeBlock.addEventListener("selectstart", e => {
    if (!codeBlock.classList.contains("unlocked")) {
      e.preventDefault();
    }
  });

  document.addEventListener("keydown", e => {
    if (e.ctrlKey && e.code === "Backquote") {
      e.preventDefault();

      codeBlock.classList.add("unlocked");
      codeBlock.style.userSelect = "text";

      codeBlock.querySelectorAll("span").forEach(span => {
        span.style.userSelect = "text";
      });

      console.log("✅ Code unlocked");
    }
  });
}});
