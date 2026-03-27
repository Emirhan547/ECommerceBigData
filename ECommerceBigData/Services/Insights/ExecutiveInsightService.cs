using ECommerceBigData.Models;

namespace ECommerceBigData.Services.Insights;

public class ExecutiveInsightService : IExecutiveInsightService
{
    public Task<ExecutiveInsightResult> GenerateAsync(DashboardViewModel model)
    {
        var gelirTrend = model.RevenueGrowthRate >= 0 ? "gelir artış eğiliminde" : "gelir düşüş eğiliminde";
        var siparisTrend = model.OrdersGrowthRate >= 0 ? "sipariş hacmi büyüyor" : "sipariş hacmi daralıyor";
        var musteriTrend = model.CustomersGrowthRate >= 0 ? "müşteri kazanımı olumlu" : "müşteri kazanımı zayıf";

        var risk = model.KpiMetrics.RefundRate > 8
            ? "İade oranı yüksek. Operasyonel kalite ve ürün eşleşmesi gözden geçirilmeli."
            : "İade oranı kontrol altında. Risk seviyesi düşük-orta bandında.";

        var action = model.KpiMetrics.ConversionRate < 2.5
            ? "Dönüşüm oranını artırmak için ürün detay sayfalarında kampanya ve sosyal kanıt bileşenleri güçlendirilmeli."
            : "Dönüşüm oranı sağlıklı. Büyümeyi hızlandırmak için yüksek CLV segmentinde kişiselleştirilmiş çapraz satış önerilir.";

        var summary = $"Son dönem verilerine göre {gelirTrend}, {siparisTrend} ve {musteriTrend}. Ortalama sepet tutarı {model.Summary.AvgOrderValue:C2} seviyesinde.";

        return Task.FromResult(new ExecutiveInsightResult
        {
            YoneticiOzeti = summary,
            RiskDegerlendirmesi = risk,
            AksiyonOnerileri = action,
            UretimZamani = DateTime.UtcNow
        });
    }
}