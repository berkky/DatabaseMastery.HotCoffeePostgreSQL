(function () {
  "use strict";

  var isMobile = function () {
    return window.innerWidth < 992;
  };

  window.toggleSidebar = function () {
    if (isMobile()) {
      document.body.classList.toggle("sidebar-open");
      var open = document.body.classList.contains("sidebar-open");
      var toggle = document.getElementById("sidebarToggle");
      if (toggle) {
        toggle.setAttribute("aria-expanded", open ? "true" : "false");
        toggle.setAttribute("aria-label", open ? "Menüyü kapat" : "Menüyü aç");
      }
    } else {
      document.body.classList.toggle("sidebar-collapsed");
      localStorage.setItem(
        "sidebarCollapsed",
        document.body.classList.contains("sidebar-collapsed") ? "true" : "false");
    }
  };

  window.closeSidebar = function () {
    document.body.classList.remove("sidebar-open");
    var toggle = document.getElementById("sidebarToggle");
    if (toggle) {
      toggle.setAttribute("aria-expanded", "false");
      toggle.setAttribute("aria-label", "Menüyü aç");
    }
  };

  document.addEventListener("DOMContentLoaded", function () {
    if (!isMobile() && localStorage.getItem("sidebarCollapsed") === "true") {
      document.body.classList.add("sidebar-collapsed");
    }

    document.addEventListener("keydown", function (event) {
      if (event.key === "Escape") {
        closeSidebar();
      }
    });
  });

  window.addEventListener("resize", function () {
    if (!isMobile()) {
      document.body.classList.remove("sidebar-open");
    }
  });
})();
