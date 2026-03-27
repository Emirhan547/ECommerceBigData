# ECommerceBigData 📊

> **Kurumsal e-ticaret operasyonları için büyük veri odaklı, görsel ve aksiyon alınabilir karar destek dashboard'u.**

`ECommerceBigData`; sipariş, müşteri, ürün, segment ve coğrafi kırılımları tek platformda birleştirerek yöneticilere ve analistlere **hızlı ve veri odaklı karar alma** imkânı sunan bir ASP.NET Core MVC uygulamasıdır.

Klasik raporlama yaklaşımının ötesine geçerek, **zaman serisi tabanlı satış tahmini (ML.NET)** ile ileriye dönük planlama yapılmasını sağlar.

---

## ✨ Projenin Amacı

Geleneksel e-ticaret panellerinde:

* Veriler dağınık olur
* Analizler gecikir
* Kararlar reaktif kalır

Bu proje, bu problemleri çözmek için tasarlanmıştır:

* **Merkezi KPI görünümü** ile dağınıklığı ortadan kaldırır
* **Optimize veri erişimi (Dapper)** ile hızlı sonuç üretir
* **Makine öğrenmesi ile tahminleme** yaparak geleceğe odaklanır
* **Dinamik filtreleme** ile detaylı analiz imkânı sunar

---

## 🚀 Öne Çıkan Özellikler

### 📊 Yönetici Dashboard

* Toplam ciro, sipariş sayısı, müşteri sayısı, ortalama sipariş tutarı
* Önceki dönemlerle karşılaştırmalı büyüme oranları
* Günlük ve aylık satış trendleri

### 📈 Analitik Modüller

* **Sales:** Satış trendleri ve dönemsel analiz
* **Customers:** Müşteri segmentasyonu ve değer analizi
* **Products:** Kategori performansı ve en çok satan ürünler
* **Geography:** Ülke ve şehir bazlı gelir dağılımı
* **Forecast (ML):** 3 / 6 / 12 aylık satış tahmini

### ⚙️ Operasyonel Kabiliyetler

* AJAX tabanlı hızlı sipariş arama
* Gelişmiş filtreleme (tarih, lokasyon, kategori, segment)
* Çoklu dil desteği (`tr-TR`, `en-US`)

---

## 🧠 Teknik Mimari

Proje, sürdürülebilir ve okunabilir bir yapı için katmanlı mimari ile tasarlanmıştır:

### Katmanlar

* **Presentation Layer (MVC)**
* **Repository Layer**
* **Data Access Layer**
* **ML Layer**

---

### 🔄 Veri Akışı

1. Kullanıcı filtre seçer
2. Controller, repository katmanına async çağrılar yapar
3. Veriler ViewModel içinde birleşir
4. Dashboard bileşenleri render edilir
5. Forecast modülünde ML pipeline çalışır

---

## 🧰 Kullanılan Teknolojiler

* **.NET 8 / ASP.NET Core MVC**
* **Dapper**
* **SQL Server**
* **ML.NET (Time Series)**
* **Razor Views**
* **Bootstrap & jQuery**
* **MemoryCache & Dependency Injection**

---

## 📁 Proje Yapısı

```text
ECommerceBigData/
├─ Controllers/
├─ Data/
│  ├─ Context/
│  └─ Repositories/
├─ Dtos/
├─ ML/
├─ Models/
├─ Views/
└─ wwwroot/
```

---

## 🤖 Makine Öğrenmesi (Forecast)

* 3 / 6 / 12 aylık tahmin
* MAE & RMSE metrikleri
* Zaman serisi bazlı analiz

---

## ⚡ Performans Yaklaşımı

* Asenkron veri çekme
* Dapper ile yüksek performans
* MemoryCache kullanımı
* Optimize SQL sorguları

---

## 🖼️ Uygulama Görselleri

> Dashboard ve analitik modüllerden örnek ekran görüntüleri

### 📊 Dashboard

<img width="1920" height="2781" alt="screencapture-localhost-7112-2026-03-28-00_25_13" src="https://github.com/user-attachments/assets/c2763f0c-e2b0-4a7c-ae3b-f9ddfc873309" />


---

### 📈 Sales

<img width="1920" height="2781" alt="screencapture-localhost-7112-2026-03-28-00_25_13" src="https://github.com/user-attachments/assets/ddae4945-b3b9-4e77-8568-eb3779996b45" />

---

### 👥 Customers

<img width="1920" height="2781" alt="screencapture-localhost-7112-2026-03-28-00_25_13" src="https://github.com/user-attachments/assets/dd09b334-5a91-4b1b-9545-3d17eb34c415" />

---

### 📦 Products

<img width="1904" height="831" alt="Ekran görüntüsü 2026-03-28 003133" src="https://github.com/user-attachments/assets/e414f17c-91a6-4a81-8bdf-65c695098165" />

---



### 🤖 Forecast

<img width="1890" height="860" alt="Ekran görüntüsü 2026-03-28 002958" src="https://github.com/user-attachments/assets/999ba58e-076e-41a9-af74-cc77e38bf6ce" />

---


