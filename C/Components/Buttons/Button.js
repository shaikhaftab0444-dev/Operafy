document.addEventListener("DOMContentLoaded", () => {

  /* =============================
     SAFE QUERY
  ============================= */
  const navButtons = document.querySelectorAll(".nav-button") || [];
  const dropdownItems = document.querySelectorAll(".dropper") || [];

  /* =============================
     REMOVE ACTIVE SAFELY
  ============================= */
  function removeAllActive() {
    navButtons.forEach(btn => btn.classList.remove("active"));
    dropdownItems.forEach(item => item.classList.remove("active"));

    document.querySelectorAll(".dropdown-menu.show").forEach(menu => {
      menu.classList.remove("show");
    });
  }

  /* =============================
     NAV BUTTON CLICK
  ============================= */
  navButtons.forEach(btn => {
    btn.addEventListener("click", () => {
      removeAllActive();
      btn.classList.add("active");
    });
  }); 

  /* =============================
     DROPDOWN ITEM CLICK
  ============================= */
  dropdownItems.forEach(item => {
    item.addEventListener("click", (e) => {
      e.stopPropagation();

      removeAllActive();
      item.classList.add("active");

      const dropdownMenu = item.closest(".dropdown-menu");
      if (!dropdownMenu) return;

      dropdownMenu.classList.add("show");

      const parentBtn = dropdownMenu.previousElementSibling;
      if (parentBtn && parentBtn.classList.contains("nav-button")) {
        parentBtn.classList.add("active");
      }
    });
  });

  /* =============================
     PAGE LOAD ACTIVE (URL BASED)
  ============================= */
  const currentPage = location.pathname.split("/").pop();

  navButtons.forEach(btn => {
    if (btn.dataset.link === currentPage) {
      btn.classList.add("active");
    }
  });

  dropdownItems.forEach(item => {
    const onclickAttr = item.getAttribute("onclick");
    if (!onclickAttr) return;

    const match = onclickAttr.match(/'([^']+)'/);
    if (!match) return;

    if (match[1] === currentPage) {
      item.classList.add("active");

      const dropdownMenu = item.closest(".dropdown-menu");
      if (!dropdownMenu) return;

      dropdownMenu.classList.add("show");

      const parentBtn = dropdownMenu.previousElementSibling;
      if (parentBtn) parentBtn.classList.add("active");
    }
  });

  /* =============================
     CODE BLOCK SAFE HANDLING
  ============================= */
  const codeBlock = document.getElementById("codeBlock");
  if (codeBlock) {
    codeBlock.addEventListener("selectstart", e => {
      if (!codeBlock.classList.contains("unlocked")) {
        e.preventDefault();
      }
    });

    document.addEventListener("keydown", e => {
      if (e.ctrlKey && e.code === "Backquote") {
        codeBlock.classList.add("unlocked");
        codeBlock.style.userSelect = "text";
      }
    });
  }

});


