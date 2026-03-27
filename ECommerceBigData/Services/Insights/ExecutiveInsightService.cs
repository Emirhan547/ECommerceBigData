using ECommerceBigData.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerceBigData.Services.Insights;

public class ExecutiveInsightService : IExecutiveInsightService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExecutiveInsightService> _logger;

    public ExecutiveInsightService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ExecutiveInsightService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ExecutiveInsightResult> GenerateAsync(DashboardViewModel model)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return BuildFallback(model, "OpenAI API anahtarı bulunamadı. Kural tabanlı özet üretildi.");
        }

        try
        {
            var prompt = BuildPrompt(model);
            var aiResult = await GenerateWithOpenAiAsync(prompt, apiKey);

            if (aiResult is null)
            {
                return BuildFallback(model, "AI yanıtı boş döndü. Kural tabanlı özet üretildi.");
            }

            return aiResult;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Executive summary AI çağrısı başarısız oldu.");
            return BuildFallback(model, "AI çağrısı başarısız olduğu için kural tabanlı özet üretildi.");
        }
    }

    private async Task<ExecutiveInsightResult?> GenerateWithOpenAiAsync(string prompt, string apiKey)
    {
        var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";
        var endpoint = _configuration["OpenAI:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";

        var requestBody = new
        {
            model,
            temperature = 0.3,
            messages = new object[]
            {
                new { role = "system", content = "Sen bir e-ticaret veri analisti asistanısın. Kısa, net ve iş odaklı Türkçe yaz." },
                new { role = "user", content = prompt }
            }
        };

        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("OpenAI API başarısız döndü. StatusCode={StatusCode}", response.StatusCode);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        var rawText = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(rawText))
        {
            return null;
        }

        return ParseInsight(rawText);
    }

    private static ExecutiveInsightResult ParseInsight(string raw)
    {
        var lines = raw
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        string Pick(int index, string fallback) => index < lines.Count ? TrimPrefix(lines[index]) : fallback;

        return new ExecutiveInsightResult
        {
            YoneticiOzeti = Pick(0, "AI özet üretilemedi."),
            RiskDegerlendirmesi = Pick(1, "Risk değerlendirmesi alınamadı."),
            AksiyonOnerileri = Pick(2, "Aksiyon önerisi alınamadı."),
            UretimZamani = DateTime.UtcNow
        };
    }

    private static string TrimPrefix(string line)
    {
        var cleaned = line.Trim().TrimStart('-', '*', '1', '2', '3', '.', ':', ')', ' ');
        return string.IsNullOrWhiteSpace(cleaned) ? line.Trim() : cleaned;
    }

    private static string BuildPrompt(DashboardViewModel model)
    {
        return $"""
Aşağıdaki metriklere göre 3 satırlık çıktı üret:
1) Yönetici özeti
2) Risk değerlendirmesi
3) Aksiyon önerisi

Metrikler:
- Gelir büyümesi: {model.RevenueGrowthRate:F1}%
- Sipariş büyümesi: {model.OrdersGrowthRate:F1}%
- Müşteri büyümesi: {model.CustomersGrowthRate:F1}%
- Toplam gelir: {model.Summary.TotalRevenue:F2}
- Toplam sipariş: {model.Summary.TotalOrders}
- Ortalama sepet: {model.Summary.AvgOrderValue:F2}
- Dönüşüm oranı: {model.KpiMetrics.ConversionRate:F2}%
- İade oranı: {model.KpiMetrics.RefundRate:F2}%
- Ortalama teslimat: {model.KpiMetrics.AvgDeliveryDays:F1} gün
- Ortalama CLV: {model.KpiMetrics.AvgCLV:F2}

Kurallar:
- Sadece Türkçe yaz.
- Her satır en fazla 22 kelime olsun.
- Teknik jargon az olsun.
""";
    }

    private static ExecutiveInsightResult BuildFallback(DashboardViewModel model, string reason)
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

        var summary = $"{reason} Son dönem verilerine göre {gelirTrend}, {siparisTrend} ve {musteriTrend}. Ortalama sepet tutarı {model.Summary.AvgOrderValue:C2}.";

        return new ExecutiveInsightResult
        {
            YoneticiOzeti = summary,
            RiskDegerlendirmesi = risk,
            AksiyonOnerileri = action,
            UretimZamani = DateTime.UtcNow
    };
}

}