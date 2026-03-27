(function () {
    const data = window.dashboardData || {};
    const locale = document.documentElement.lang || 'tr-TR';
    const getValue = (obj, pascalName, camelName) => {
        if (!obj) return null;
        if (Object.prototype.hasOwnProperty.call(obj, pascalName)) return obj[pascalName];
        if (Object.prototype.hasOwnProperty.call(obj, camelName)) return obj[camelName];
        return null;
    };

    const initLine = () => {
        const ctx = document.getElementById('dailySalesChart')?.getContext('2d');
        const rows = data.dailySales || [];
        if (!ctx || !rows.length) return;

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: rows.map(x => new Date(getValue(x, 'Date', 'date')).toLocaleDateString(locale, { day: 'numeric', month: 'short' })),                datasets: [{
                    label: 'Ciro',
                    data: rows.map(x => getValue(x, 'TotalSales', 'totalSales') || 0),
                    borderColor: '#38bdf8',
                    backgroundColor: 'rgba(56,189,248,.12)',
                    fill: true,
                    tension: .28
                }]
            },
            options: {
                plugins: { legend: { display: false } },
                scales: {
                    x: { ticks: { color: '#a1a1aa' }, grid: { color: 'rgba(255,255,255,.04)' } },
                    y: { ticks: { color: '#a1a1aa' }, grid: { color: 'rgba(255,255,255,.04)' } }
                }
            }
        });
    };

    const pieChart = (id, rows, labelKey) => {
        const ctx = document.getElementById(id)?.getContext('2d');
        if (!ctx || !rows?.length) return;

        const palette = ['#38bdf8', '#34d399', '#fbbf24', '#a78bfa', '#f87171', '#60a5fa'];
        new Chart(ctx, {
            type: 'pie',
            data: {
                labels: rows.map(x => getValue(x, labelKey, labelKey.charAt(0).toLowerCase() + labelKey.slice(1)) || 'Bilinmiyor'),                datasets: [{
                    data: rows.map(x => getValue(x, 'TotalSales', 'totalSales') || getValue(x, 'Percentage', 'percentage') || 0),
                    backgroundColor: palette
                }]
            },
            options: {
                plugins: {
                    legend: { labels: { color: '#a1a1aa', boxWidth: 10 } }
                }
            }
        });
    };

    const initTopProducts = () => {
        const ctx = document.getElementById('topProductsChart')?.getContext('2d');
        const rows = data.topProducts || [];
        if (!ctx || !rows.length) return;

        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: rows.map(x => {
                    const n = getValue(x, 'ProductName', 'productName') || '-';
                    return n.length > 16 ? `${n.slice(0, 16)}...` : n;
                }),
                datasets: [{
                    label: 'Adet',
                    data: rows.map(x => getValue(x, 'TotalQuantity', 'totalQuantity') || 0),
                    backgroundColor: '#38bdf8'
                }]
            },
            options: {
                plugins: { legend: { display: false } },
                scales: {
                    x: { ticks: { color: '#a1a1aa' }, grid: { display: false } },
                    y: { ticks: { color: '#a1a1aa' }, grid: { color: 'rgba(255,255,255,.04)' } }
                }
            }
        });
    };

    const initMonthlyCombo = () => {
        const ctx = document.getElementById('monthlyRevenueChart')?.getContext('2d');
        const rows = data.monthlyRevenue || [];
        if (!ctx || !rows.length) return;

        new Chart(ctx, {
            data: {
                labels: rows.map(x => getValue(x, 'MonthLabel', 'monthLabel')),
                datasets: [
                    {
                        type: 'bar',
                        label: 'Sipariş',
                        yAxisID: 'y1',
                        data: rows.map(x => getValue(x, 'OrderCount', 'orderCount') || 0),
                        backgroundColor: 'rgba(163, 230, 53, .6)'
                    },
                    {
                        type: 'line',
                        label: 'Ciro',
                        yAxisID: 'y',
                        data: rows.map(x => getValue(x, 'Revenue', 'revenue') || 0),
                        borderColor: '#38bdf8',
                        tension: .3
                    }
                ]
            },
            options: {
                scales: {
                    x: { ticks: { color: '#a1a1aa' } },
                    y: { position: 'left', ticks: { color: '#a1a1aa' }, grid: { color: 'rgba(255,255,255,.04)' } },
                    y1: { position: 'right', ticks: { color: '#a1a1aa' }, grid: { drawOnChartArea: false } }
                }
            }
        });
    };

    const initHeatmap = () => {
        const container = document.getElementById('hourlyHeatmap');
        const rows = data.hourlyHeatmap || [];
        if (!container || !rows.length) {
            if (container) container.innerHTML = '<div class="text-zinc-500 text-sm col-span-12">Isı haritası verisi bulunamadı.</div>';            return;
        }

        const max = Math.max(...rows.map(x => getValue(x, 'OrderCount', 'orderCount') || 0), 1);
        for (let day = 1; day <= 7; day++) {
            for (let h = 0; h < 24; h += 2) {
                const found = rows.find(r => (getValue(r, 'DayOfWeek', 'dayOfWeek') === day) && (getValue(r, 'Hour', 'hour') >= h) && (getValue(r, 'Hour', 'hour') < h + 2));
                const val = found ? (getValue(found, 'OrderCount', 'orderCount') || 0) : 0;
                const intensity = Math.max(0.08, val / max);
                const cell = document.createElement('div');
                cell.className = 'heat-cell';
                cell.style.backgroundColor = `rgba(56, 189, 248, ${intensity})`;
                cell.title = `Gün ${day} | ${h}:00-${h + 1}:59 => ${val} sipariş`;
                container.appendChild(cell);
            }
        }
    };

    const initDrillDown = () => {
        const canvas = document.getElementById('countrySalesChart');
        const rows = data.countrySales || [];
        if (!canvas || !rows.length) return;

        canvas.onclick = (evt) => {
            const points = Chart.getChart(canvas)?.getElementsAtEventForMode(evt, 'nearest', { intersect: true }, true) || [];
            if (!points.length) return;

            const idx = points[0].index;
            const country = getValue(rows[idx], 'Country', 'country');
            if (!country) return;

            const url = new URL(window.location.href);
            url.searchParams.set('country', country);
            window.location.assign(url.toString());
        };
    };

    initLine();
    pieChart('countrySalesChart', data.countrySales || [], 'Country');
    pieChart('citySalesChart', data.citySales || [], 'City');
    pieChart('segmentChart', data.segmentDistribution || [], 'Segment');
    initTopProducts();
    initMonthlyCombo();
    initHeatmap();
    initDrillDown();
})();