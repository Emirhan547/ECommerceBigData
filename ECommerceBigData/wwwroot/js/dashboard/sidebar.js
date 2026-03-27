(() => {
    const sidebar = document.getElementById('dashboardSidebar');
    const toggleBtn = document.getElementById('dashboardSidebarToggle');

    if (!sidebar || !toggleBtn) {
        return;
    }

    toggleBtn.addEventListener('click', () => {
        sidebar.classList.toggle('is-open');
    });

    document.addEventListener('click', (event) => {
        const isMobile = window.innerWidth <= 1024;
        if (!isMobile || !sidebar.classList.contains('is-open')) {
            return;
        }

        const target = event.target;
        if (!(target instanceof Node)) {
            return;
        }

        if (!sidebar.contains(target) && !toggleBtn.contains(target)) {
            sidebar.classList.remove('is-open');
        }
    });
})();