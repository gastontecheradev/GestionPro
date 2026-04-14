// ── Auto-dismiss alerts after 5 seconds ──
document.addEventListener('DOMContentLoaded', () => {
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            bsAlert.close();
        }, 5000);
    });

    // ── Highlight active nav link ──
    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.navbar-nav .nav-link').forEach(link => {
        const href = link.getAttribute('href');
        if (href && href !== '/' && currentPath.startsWith(href.toLowerCase())) {
            link.classList.add('active');
        }
    });

    // ── Animate elements on scroll ──
    const fadeElements = document.querySelectorAll('.fade-up');
    if (fadeElements.length > 0 && 'IntersectionObserver' in window) {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1 });

        fadeElements.forEach(el => observer.observe(el));
    }

    // ── Tooltip init ──
    const tooltipTriggerList = document.querySelectorAll('[title]');
    tooltipTriggerList.forEach(el => {
        if (el.closest('.btn-group')) {
            new bootstrap.Tooltip(el, { placement: 'top', trigger: 'hover' });
        }
    });
});

// ── Format currency inputs on blur ──
document.querySelectorAll('input[type="number"][step="0.01"]').forEach(input => {
    input.addEventListener('blur', () => {
        if (input.value) {
            input.value = parseFloat(input.value).toFixed(2);
        }
    });
});

console.log('%c GestionPro v1.0 ', 
    'background: #1e293b; color: #3b82f6; font-weight: bold; padding: 4px 8px; border-radius: 4px;');
