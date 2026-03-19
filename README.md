# AYDA Müþavirlik - Profesyonel Mali Müþavirlik Yazýlýmý

[![.NET 9](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Server-blue)](https://blazor.net/)
[![MudBlazor](https://img.shields.io/badge/MudBlazor-8.5-green)](https://mudblazor.com/)
[![License](https://img.shields.io/badge/License-MIT-yellow)](LICENSE)

<p align="center">
  <img src="docs/logo.png" alt="AYDA Müþavirlik Logo" width="200"/>
</p>

**AYDA Müþavirlik**, mali müþavirlik ofisleri için geliþtirilmiþ modern, kullanýcý dostu ve kapsamlý bir muhasebe yazýlýmýdýr. Logo, Zirve ve Uyumsoft gibi önde gelen muhasebe yazýlýmlarýnýn en iyi özelliklerinden ilham alýnarak tasarlanmýþtýr.

## ?? Özellikler

### ?? Muhasebe Modülü
- **Tek Düzen Hesap Planý** - Türk muhasebe standartlarýna uygun
- **Fiþ Giriþi** - Mahsup, tahsilat, ödeme, alýþ/satýþ faturalarý
- **Yevmiye Defteri** - Otomatik oluþturma ve yazdýrma
- **Mizan** - Anlýk bakiye takibi
- **Bilanço & Gelir Tablosu** - Otomatik oluþturma

### ?? Finansal Analiz (Profesyonel)
- **14 Finansal Oran** - Likidite, karlýlýk, faaliyet, borçluluk
- **Trend Grafikleri** - Gelir/gider/kar eðilimleri
- **Bilanço Analizi** - Varlýk/kaynak daðýlýmý
- **Nakit Akýþ Analizi** - Faaliyetlerden, yatýrýmlardan, finansmandan
- **KPI Dashboard** - Anlýk performans göstergeleri

### ?? Firma Yönetimi
- **Çoklu Firma Desteði** - Sýnýrsýz firma tanýmlama
- **Müþteri Bilgileri** - Vergi no, MERSÝS, ticaret sicil
- **Ýletiþim Yönetimi** - Yetkili kiþiler, telefon, e-posta

### ?? Kullanýcý Yönetimi
- **Rol Tabanlý Yetkilendirme** - Admin, Yönetici, Muhasebeci, Denetçi
- **Güvenli Giriþ** - SHA256 þifreleme
- **Oturum Takibi** - Son giriþ, baþarýsýz giriþ sayýsý

### ?? Raporlama
- **Mali Tablolar** - Bilanço, gelir tablosu
- **Yasal Raporlar** - E-defter uyumlu
- **Grafik Raporlar** - Görsel analizler
- **Excel Export** - Tüm raporlar dýþa aktarýlabilir

## ??? Ekran Görüntüleri

<table>
  <tr>
    <td><img src="docs/screenshots/login.png" alt="Giriþ Ekraný" width="300"/></td>
    <td><img src="docs/screenshots/dashboard.png" alt="Dashboard" width="300"/></td>
  </tr>
  <tr>
    <td align="center">Giriþ Ekraný</td>
    <td align="center">Ana Sayfa / Dashboard</td>
  </tr>
  <tr>
    <td><img src="docs/screenshots/financial-analysis.png" alt="Finansal Analiz" width="300"/></td>
    <td><img src="docs/screenshots/accounting.png" alt="Muhasebe" width="300"/></td>
  </tr>
  <tr>
    <td align="center">Finansal Analiz</td>
    <td align="center">Muhasebe Fiþleri</td>
  </tr>
</table>

## ?? Hýzlý Baþlangýç

### Gereksinimler
- .NET 9.0 SDK
- Visual Studio 2022 (17.8+) veya VS Code
- SQL Server / SQLite (opsiyonel)

### Kurulum

```bash
# Repository'yi klonlayýn
git clone https://github.com/karamur/AydaMusavirlik.git
cd AydaMusavirlik

# Baðýmlýlýklarý yükleyin
dotnet restore

# Uygulamayý çalýþtýrýn
cd AydaMusavirlik.Web
dotnet run
```

### Docker ile Çalýþtýrma

```bash
docker build -t ayda-musavirlik .
docker run -p 5050:80 ayda-musavirlik
```

## ?? Proje Yapýsý

```
AydaMusavirlik/
??? AydaMusavirlik.Web/          # Blazor Web Uygulamasý
?   ??? Components/
?   ?   ??? Layout/              # Ana düzen bileþenleri
?   ?   ??? Pages/               # Sayfalar
?   ?       ??? Login.razor
?   ?       ??? Home.razor
?   ?       ??? Companies.razor
?   ?       ??? FinancialAnalysis.razor
?   ?       ??? ...
?   ??? Models/                  # Veri modelleri
?   ?   ??? Common/
?   ?   ??? Accounting/
?   ?   ??? Financial/
?   ??? Services/                # Ýþ mantýðý servisleri
?   ?   ??? AuthService.cs
?   ?   ??? AccountingService.cs
?   ?   ??? FinancialAnalysisService.cs
?   ?   ??? ...
?   ??? Program.cs
??? docs/                        # Dokümantasyon
??? README.md
```

## ?? Test Kullanýcýlarý

| Kullanýcý | Þifre | Rol | Yetki |
|-----------|-------|-----|-------|
| `admin` | `admin` | Admin | Tam eriþim |
| `muhasebe` | `muhasebe123` | Muhasebeci | Muhasebe modülü |
| `yonetici` | `yonetici123` | Yönetici | Raporlar + Yönetim |

## ?? Finansal Oranlar

### Likidite Oranlarý
| Oran | Formül | Ýdeal Deðer |
|------|--------|-------------|
| Cari Oran | Dönen Varlýklar / K.V. Borçlar | ? 2.0 |
| Asit-Test Oraný | (D.V. - Stoklar) / K.V. Borçlar | ? 1.0 |
| Nakit Oraný | Nakit / K.V. Borçlar | ? 0.5 |

### Karlýlýk Oranlarý
| Oran | Formül | Ýdeal Deðer |
|------|--------|-------------|
| Brüt Kar Marjý | Brüt Kar / Satýþlar × 100 | ? %30 |
| Net Kar Marjý | Net Kar / Satýþlar × 100 | ? %15 |
| ROA | Net Kar / Toplam Aktif × 100 | ? %10 |
| ROE | Net Kar / Özsermaye × 100 | ? %15 |

## ??? Teknoloji Yýðýný

- **Frontend:** Blazor Server, MudBlazor 8.5
- **Backend:** ASP.NET Core 9.0
- **Veritabaný:** Entity Framework Core 9, SQLite
- **Logging:** Serilog
- **Authentication:** Custom JWT-like session
- **Charts:** MudBlazor Charts

## ?? Kurulum Paketi

Setup dosyalarý aþaðýdaki dizinde oluþturulur:
```
C:\ARYAMusavirlik\kurulum\
??? AydaMusavirlik-Setup.exe
??? AydaMusavirlik-Portable.zip
??? README.txt
```

## ??? Yol Haritasý

### v1.0 (Mevcut)
- [x] Kullanýcý giriþi ve yetkilendirme
- [x] Dashboard ve KPI kartlarý
- [x] Firma yönetimi
- [x] Hesap planý
- [x] Muhasebe fiþleri
- [x] Profesyonel finansal analiz
- [x] 14 finansal oran hesaplama

### v1.1 (Planlanan)
- [ ] Yevmiye defteri
- [ ] Mizan raporu
- [ ] Bilanço oluþturma
- [ ] Gelir tablosu
- [ ] Excel dýþa aktarým

### v1.2 (Gelecek)
- [ ] E-Defter entegrasyonu
- [ ] E-Fatura entegrasyonu
- [ ] Bordro modülü
- [ ] Cari hesaplar
- [ ] Banka entegrasyonu

## ?? Katkýda Bulunma

1. Fork yapýn
2. Feature branch oluþturun (`git checkout -b feature/YeniOzellik`)
3. Commit yapýn (`git commit -m 'Yeni özellik eklendi'`)
4. Push yapýn (`git push origin feature/YeniOzellik`)
5. Pull Request açýn

## ?? Lisans

Bu proje MIT lisansý altýnda lisanslanmýþtýr. Detaylar için [LICENSE](LICENSE) dosyasýna bakýn.

## ?? Ýletiþim

- **Geliþtirici:** Murat K.
- **E-posta:** info@aydamusavirlik.com
- **Website:** [aydamusavirlik.com](https://aydamusavirlik.com)

---

<p align="center">
  <b>AYDA Müþavirlik</b> - Profesyonel Mali Müþavirlik Çözümü<br>
  <i>Logo, Zirve ve Uyumsoft'un en iyi özelliklerinden ilham alýnarak geliþtirilmiþtir.</i>
</p>

---

## ?? Ýlham Alýnan Özellikler

### Logo Yazýlým'dan
- Kullanýcý dostu arayüz tasarýmý
- Modüler yapý
- Hýzlý veri giriþi

### Zirve Mali Müþavir'den
- Tek düzen hesap planý entegrasyonu
- Kapsamlý raporlama sistemi
- E-defter uyumluluðu

### Uyumsoft'tan
- Modern web tabanlý mimari
- Responsive tasarým
- Bulut desteði

