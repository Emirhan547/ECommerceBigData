(function () {
    const input = document.getElementById('dashboardSearchInput');
    const resultBox = document.getElementById('searchResults');
    const searchState = document.getElementById('searchState');

    if (!input || !resultBox || !searchState) return;

    const debounce = (fn, ms) => {
        let timer = null;
        return (...args) => {
            clearTimeout(timer);
            timer = setTimeout(() => fn(...args), ms);
        };
    };

    const escapeHtml = (value) => `${value ?? ''}`
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');

    const renderRows = (rows) => {
        if (!rows.length) {
            resultBox.classList.add('hidden');
            searchState.textContent = 'Sonuç bulunamadı.';
            return;
        }

        resultBox.innerHTML = rows.map(r => `
            <a class="block px-3 py-2 border-b border-zinc-800 hover:bg-zinc-900" href="/Orders/Index?query=${encodeURIComponent(r.orderId || r.OrderId)}">
                <div class="text-sm text-zinc-100">#${escapeHtml(r.orderId ?? r.OrderId)} - ${escapeHtml(r.customerName ?? r.CustomerName)}</div>
                <div class="text-xs text-zinc-500">${escapeHtml(r.city ?? r.City)}, ${escapeHtml(r.country ?? r.Country)} · ${escapeHtml(r.status ?? r.Status)}</div>
            </a>
        `).join('');

        resultBox.classList.remove('hidden');
        searchState.textContent = `${rows.length} sonuç bulundu.`;
    };

    const runSearch = debounce(async (q) => {
        const query = q.trim();
        if (!query) {
            resultBox.classList.add('hidden');
            resultBox.innerHTML = '';
            searchState.textContent = 'Arama yapmak için yazmaya başlayın.';
            return;
        }

        searchState.textContent = 'Aranıyor...';

        try {
            const response = await fetch(`/Dashboard/Search?q=${encodeURIComponent(query)}`, {
                headers: { 'Accept': 'application/json' }
            });
            if (!response.ok) throw new Error('Search request failed');
            const rows = await response.json();
            renderRows(Array.isArray(rows) ? rows : []);
        } catch (error) {
            resultBox.classList.add('hidden');
            searchState.textContent = 'Arama sırasında bir hata oluştu.';
        }
    }, 400);

    input.addEventListener('input', (e) => runSearch(e.target.value));
    document.addEventListener('click', (e) => {
        if (!resultBox.contains(e.target) && e.target !== input) {
            resultBox.classList.add('hidden');
        }
    });
})();