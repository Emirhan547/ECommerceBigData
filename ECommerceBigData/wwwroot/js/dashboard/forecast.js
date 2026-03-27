(() => {
    const locale = document.documentElement.lang || 'tr-TR';
    const ctx = document.getElementById('forecastChart')?.getContext('2d');
    const points = (window.forecastData?.points || window.forecastData?.Points || [])
        .map(x => ({
            monthStart: x.monthStart || x.MonthStart,
            revenue: x.revenue ?? x.Revenue ?? 0,
            orders: x.orders ?? x.Orders ?? 0,
            isForecast: x.isForecast ?? x.IsForecast ?? false
        }))
        .sort((a, b) => new Date(a.monthStart) - new Date(b.monthStart));

    if (!ctx || !points.length) {
        return;
    }

    const labels = points.map(x => new Date(x.monthStart).toLocaleDateString(locale, { month: 'short', year: '2-digit' }));

    new Chart(ctx, {
        data: {
            labels,
            datasets: [
                {
                    type: 'line',
                    label: 'Ciro',
                    yAxisID: 'y',
                    data: points.map(x => x.revenue),
                    borderColor: '#38bdf8',
                    segment: {
                        borderDash: c => points[c.p1DataIndex]?.isForecast ? [5, 5] : undefined
                    }
                },
                {
                    type: 'bar',
                    label: 'Sipariş',
                    yAxisID: 'y1',
                    data: points.map(x => x.orders),
                    backgroundColor: points.map(x => x.isForecast ? 'rgba(251,191,36,.55)' : 'rgba(52,211,153,.55)')
                }
            ]
        },
        options: {
            scales: {
                y: { position: 'left', ticks: { color: '#a1a1aa' }, grid: { color: 'rgba(255,255,255,.04)' } },
                y1: { position: 'right', ticks: { color: '#a1a1aa' }, grid: { drawOnChartArea: false } }
            }
        }
    });
})();