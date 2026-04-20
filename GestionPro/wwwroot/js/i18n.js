/**
 * GestionPro — i18n Engine
 * Soporta ES / EN con toggle en navbar.
 * Traducciones via atributos data-i18n en el HTML.
 */
(function () {
  'use strict';

  const STORAGE_KEY = 'gp_lang';
  const DEFAULT_LANG = 'es';
  let translations = {};
  let currentLang = localStorage.getItem(STORAGE_KEY) || DEFAULT_LANG;

  // ── Cargar JSON de traducciones ──────────────────────────────────────────
  async function loadTranslations() {
    try {
      const resp = await fetch('/js/i18n.json');
      translations = await resp.json();
    } catch (e) {
      console.warn('i18n: no se pudo cargar i18n.json', e);
    }
  }

  // ── Obtener texto ────────────────────────────────────────────────────────
  function t(key) {
    return (translations[currentLang] && translations[currentLang][key]) ||
           (translations[DEFAULT_LANG] && translations[DEFAULT_LANG][key]) ||
           key;
  }

  // ── Aplicar traducciones al DOM ──────────────────────────────────────────
  function applyTranslations(root) {
    const scope = root || document;

    // Texto simple
    scope.querySelectorAll('[data-i18n]').forEach(el => {
      const key = el.getAttribute('data-i18n');
      el.textContent = t(key);
    });

    // Placeholder
    scope.querySelectorAll('[data-i18n-placeholder]').forEach(el => {
      el.placeholder = t(el.getAttribute('data-i18n-placeholder'));
    });

    // Title (tooltip)
    scope.querySelectorAll('[data-i18n-title]').forEach(el => {
      el.title = t(el.getAttribute('data-i18n-title'));
    });

    // Atributo value (para submit buttons, select options, etc.)
    scope.querySelectorAll('[data-i18n-value]').forEach(el => {
      el.value = t(el.getAttribute('data-i18n-value'));
    });

    // confirm() inline — reemplazar el texto del atributo onsubmit
    // Esto se maneja desde el servidor con data-i18n-confirm en el elemento form
    scope.querySelectorAll('[data-i18n-confirm]').forEach(el => {
      const key = el.getAttribute('data-i18n-confirm');
      el.setAttribute('onsubmit', `return confirm('${t(key)}')`);
    });

    // lang en <html>
    document.documentElement.lang = currentLang;
  }

  // ── Actualizar toggle visual ─────────────────────────────────────────────
  function updateToggles() {
    document.querySelectorAll('.gp-lang-toggle').forEach(toggle => {
      const esBtn = toggle.querySelector('[data-lang="es"]');
      const enBtn = toggle.querySelector('[data-lang="en"]');
      if (!esBtn || !enBtn) return;

      if (currentLang === 'es') {
        esBtn.classList.add('active');
        enBtn.classList.remove('active');
      } else {
        enBtn.classList.add('active');
        esBtn.classList.remove('active');
      }
    });
  }

  // ── Cambiar idioma ───────────────────────────────────────────────────────
  window.gpSetLang = function (lang) {
    if (!translations[lang]) return;
    currentLang = lang;
    localStorage.setItem(STORAGE_KEY, lang);
    applyTranslations();
    updateToggles();
  };

  // ── Init ─────────────────────────────────────────────────────────────────
  async function init() {
    await loadTranslations();

    // Solo aplicar traducciones si no es español (idioma base del HTML)
    // En inglés hay que traducir todo; en español el HTML ya tiene el texto correcto
    if (currentLang !== DEFAULT_LANG) {
      applyTranslations();
    }

    updateToggles();

    // Delegación de eventos en todos los toggles
    document.addEventListener('click', function (e) {
      const btn = e.target.closest('[data-lang]');
      if (btn && btn.closest('.gp-lang-toggle')) {
        e.preventDefault();
        window.gpSetLang(btn.getAttribute('data-lang'));
      }
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
