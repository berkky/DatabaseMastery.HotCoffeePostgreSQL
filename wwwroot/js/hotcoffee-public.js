(function () {
  "use strict";

  function ready(callback) {
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", callback);
      return;
    }

    callback();
  }

  function prefersReducedMotion() {
    return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  }

  function markAllRevealsVisible() {
    document.querySelectorAll(".hc-reveal").forEach(function (item) {
      item.classList.add("is-visible");
    });
  }

  function isRevealCandidateInView(element) {
    var rect = element.getBoundingClientRect();
    var viewportHeight = window.innerHeight || document.documentElement.clientHeight;
    return rect.top < viewportHeight * 0.94 && rect.bottom > 0;
  }

  function initMobileNav() {
    var toggle = document.getElementById("hcNavToggle");
    var nav = document.getElementById("hcPrimaryNav");
    if (!toggle || !nav) {
      return;
    }

    function setOpen(open) {
      toggle.setAttribute("aria-expanded", open ? "true" : "false");
      toggle.setAttribute("aria-label", open ? "Menüyü kapat" : "Menüyü aç");
      nav.classList.toggle("is-open", open);
    }

    toggle.addEventListener("click", function () {
      var open = toggle.getAttribute("aria-expanded") !== "true";
      setOpen(open);
    });

    nav.querySelectorAll("a").forEach(function (link) {
      link.addEventListener("click", function () {
        setOpen(false);
      });
    });

    document.addEventListener("keydown", function (event) {
      if (event.key === "Escape") {
        setOpen(false);
      }
    });
  }

  function initHeroMotion() {
    var hero = document.querySelector("[data-hc-hero]");
    if (!hero) {
      return;
    }

    hero.classList.add("is-intro-ready");

    if (prefersReducedMotion()) {
      return;
    }

    var layers = hero.querySelector("[data-hc-hero-layers]");
    if (!layers) {
      return;
    }

    var coarsePointer = window.matchMedia("(pointer: coarse)").matches;
    var narrow = window.matchMedia("(max-width: 768px)").matches;
    if (coarsePointer || narrow) {
      return;
    }

    var targetX = 0;
    var targetY = 0;
    var currentX = 0;
    var currentY = 0;
    var rafId = 0;
    var active = false;

    function animate() {
      currentX += (targetX - currentX) * 0.08;
      currentY += (targetY - currentY) * 0.08;
      layers.style.transform = "translate3d(" + currentX.toFixed(2) + "px," + currentY.toFixed(2) + "px,0)";
      rafId = window.requestAnimationFrame(animate);
    }

    hero.addEventListener("mousemove", function (event) {
      var rect = hero.getBoundingClientRect();
      var ratioX = (event.clientX - rect.left) / rect.width - 0.5;
      var ratioY = (event.clientY - rect.top) / rect.height - 0.5;
      targetX = Math.max(-6, Math.min(6, ratioX * 12));
      targetY = Math.max(-6, Math.min(6, ratioY * 10));

      if (!active) {
        active = true;
        rafId = window.requestAnimationFrame(animate);
      }
    });

    hero.addEventListener("mouseleave", function () {
      targetX = 0;
      targetY = 0;
    });
  }

  function initScrollReveal() {
    var items = document.querySelectorAll(".hc-reveal");
    if (!items.length) {
      return;
    }

    if (prefersReducedMotion() || !("IntersectionObserver" in window)) {
      markAllRevealsVisible();
      return;
    }

    var observer = new IntersectionObserver(
      function (entries) {
        entries.forEach(function (entry) {
          if (entry.isIntersecting) {
            entry.target.classList.add("is-visible");
            observer.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.12, rootMargin: "0px 0px -6% 0px" }
    );

    items.forEach(function (item) {
      if (isRevealCandidateInView(item)) {
        item.classList.add("is-visible");
      }

      observer.observe(item);
    });

    document.documentElement.classList.add("hc-motion-ready");
  }

  function initCounters() {
    var counters = document.querySelectorAll("[data-hc-count-to]");
    if (!counters.length) {
      return;
    }

    if (prefersReducedMotion()) {
      return;
    }

    counters.forEach(function (counter) {
      var finalValue = parseInt(counter.getAttribute("data-hc-count-to") || "0", 10);
      if (!Number.isFinite(finalValue) || finalValue <= 0) {
        return;
      }

      var start = performance.now();
      var duration = 900;

      function tick(now) {
        var progress = Math.min(1, (now - start) / duration);
        var eased = 1 - Math.pow(1 - progress, 3);
        var value = Math.round(finalValue * eased);
        counter.textContent = String(value);
        if (progress < 1) {
          window.requestAnimationFrame(tick);
        } else {
          counter.textContent = String(finalValue);
        }
      }

      counter.textContent = "0";
      window.requestAnimationFrame(tick);
    });
  }

  ready(function () {
    try {
      initMobileNav();
    } catch (error) {
      /* mobile nav failure must not block menu visibility */
    }

    try {
      initHeroMotion();
    } catch (error) {
      /* hero motion is decorative */
    }

    try {
      initScrollReveal();
    } catch (error) {
      markAllRevealsVisible();
    }

    try {
      initCounters();
    } catch (error) {
      /* counter animation is decorative */
    }
  });
})();
