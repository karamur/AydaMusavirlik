# ?? AYDA Müþavirlik - Profesyonel Mali Müþavirlik Yazýlýmý

<p align="center">
  <img src="docs/logo.png" alt="AYDA Müþavirlik Logo" width="200"/>
</p>

<p align="center">
  <strong>Allbatros Software Solutions</strong><br>
  <a href="http://www.allglb.com">www.allglb.com</a>
</p>

<p align="center">
  <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-9.0-purple" alt=".NET 9"></a>
  <a href="https://blazor.net/"><img src="https://img.shields.io/badge/Blazor-Server-blue" alt="Blazor"></a>
  <a href="https://mudblazor.com/"><img src="https://img.shields.io/badge/MudBlazor-8.5-green" alt="MudBlazor"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-yellow" alt="License"></a>
  <a href="#"><img src="https://img.shields.io/badge/Platform-Web%20%7C%20Desktop-orange" alt="Platform"></a>
</p>

---

**AYDA Müþavirlik**, mali müþavirlik ofisleri ve muhasebe bürolarý için geliþtirilmiþ kapsamlý, modern ve kullanýcý dostu bir ERP çözümüdür. **Logo**, **Zirve Mali Müþavir** ve **Uyumsoft** gibi önde gelen muhasebe yazýlýmlarýnýn en iyi özelliklerinden ilham alýnarak tasarlanmýþtýr.

## ?? Ýçindekiler

- [Özellikler](#-özellikler)
- [Mimari](#?-mimari)
- [Kurulum](#-kurulum)
- [Proje Yapýsý](#-proje-yapýsý)
- [Modüller](#-modüller)
- [Ekran Görüntüleri](#-ekran-görüntüleri)
- [Teknoloji Yýðýný](#?-teknoloji-yýðýný)
- [Yol Haritasý](#?-yol-haritasý)
- [Katkýda Bulunma](#-katkýda-bulunma)
- [Lisans](#-lisans)
- [Ýletiþim](#-iletiþim)

---

## ?? Özellikler

### ?? Muhasebe Modülü
| Özellik | Açýklama |
|---------|----------|
| **Tek Düzen Hesap Planý** | Türk muhasebe standartlarýna uygun 7 sýnýflý hesap planý |
| **Fiþ Giriþi** | Mahsup, tahsilat, ödeme, alýþ/satýþ faturalarý |
| **Yevmiye Defteri** | Otomatik oluþturma ve yazdýrma |
| **Mizan** | Anlýk bakiye takibi ve raporlama |
| **Bilanço & Gelir Tablosu** | Otomatik mali tablo oluþturma |

### ?? Finansal Analiz (Profesyonel)
| Kategori | Oranlar |
|----------|---------|
| **Likidite** | Cari Oran, Asit-Test Oraný, Nakit Oraný |
| **Karlýlýk** | Brüt Kar Marjý, Net Kar Marjý, ROA, ROE |
| **Faaliyet** | Alacak Devir Hýzý, Stok Devir Hýzý, Aktif Devir Hýzý |
| **Borçluluk** | Borç Oraný, Borç/Özsermaye, Faiz Karþýlama Oraný |

#### Grafik ve Raporlar
- ?? Gelir/Gider/Kar Trend Grafikleri (Line Chart)
- ?? Gider Daðýlýmý (Donut Chart)
- ?? Bilanço Analizi (Bar Chart)
- ?? Nakit Akýþ Analizi (Stacked Bar Chart)
- ?? KPI Dashboard Kartlarý

### ?? Randevu ve Takvim Sistemi
| Özellik | Açýklama |
|---------|----------|
| **Randevu Yönetimi** | Toplantý, danýþmanlýk, denetim randevularý |
| **Vergi Takvimi** | KDV, Muhtasar, SGK, Geçici Vergi tarihleri |
| **Hatýrlatýcýlar** | E-posta, SMS, uygulama içi bildirimler |
| **Çakýþma Kontrolü** | Otomatik randevu çakýþma tespiti |
| **Müsaitlik Takvimi** | Boþ zaman dilimlerini görme |

#### Randevu Türleri
- ?? Toplantý
- ?? Danýþmanlýk Görüþmesi
- ?? Vergi Son Tarihi
- ?? Denetim
- ?? Eðitim
- ?? Mahkeme Duruþmasý
- ?? Banka Randevusu
- ?? SGK Ýþlemleri
- ??? Vergi Dairesi

### ?? Firma Yönetimi
- Çoklu firma desteði
- Vergi no, MERSÝS, ticaret sicil bilgileri
- Ýletiþim kiþileri yönetimi
- Firma bazlý raporlama

### ?? Kullanýcý Yönetimi
- Rol tabanlý yetkilendirme (Admin, Yönetici, Muhasebeci, Denetçi)
- SHA256 þifreleme
- Oturum takibi
- Aktivite loglarý

---

## ??? Mimari

```
???????????????????????????????????????????????????????????????
?                    AYDA Müþavirlik Solution                 ?
???????????????????????????????????????????????????????????????
?                                                             ?
?  ???????????????  ???????????????  ??????????????????????? ?
?  ?   Web App   ?  ?  Desktop    ?  ?       API           ? ?
?  ?  (Blazor)   ?  ?   (WPF)     ?  ?   (ASP.NET Core)    ? ?
?  ???????????????  ???????????????  ??????????????????????? ?
?         ?                ?                     ?            ?
?         ????????????????????????????????????????            ?
?                          ?                                  ?
?  ????????????????????????????????????????????????????????? ?
?  ?                    Core Library                        ? ?
?  ?         (Models, Services, Business Logic)             ? ?
?  ????????????????????????????????????????????????????????? ?
?                          ?                                  ?
?  ????????????????????????????????????????????????????????? ?
?  ?                    Data Layer                          ? ?
?  ?            (Entity Framework Core, SQLite)             ? ?
?  ????????????????????????????????????????????????????????? ?
?                                                             ?
???????????????????????????????????????????????????????????????
```

---

## ?? Kurulum

### Gereksinimler
- .NET 9.0 SDK
- Visual Studio 2022 (17.8+) veya VS Code
- Node.js (opsiyonel, SCSS derleme için)

### Hýzlý Baþlangýç

```bash
# Repository'yi klonlayýn
git clone https://github.com/karamur/AydaMusavirlik.git
cd AydaMusavirlik

# Baðýmlýlýklarý yükleyin
dotnet restore

# Web uygulamasýný çalýþtýrýn
cd AydaMusavirlik.Web
dotnet run --urls "http://localhost:5050"
```

### Visual Studio ile Çalýþtýrma
1. `AydaMusavirlik.sln` dosyasýný açýn
2. `AydaMusavirlik.Web` projesini baþlangýç projesi olarak ayarlayýn
3. **F5** ile çalýþtýrýn

### Docker ile Çalýþtýrma

```bash
docker build -t ayda-musavirlik .
docker run -p 5050:80 ayda-musavirlik
```

---

## ?? Proje Yapýsý

```
AydaMusavirlik/
??? ?? AydaMusavirlik.Web/          # Blazor Web Uygulamasý
?   ??? ?? Components/
?   ?   ??? ?? Layout/              # MainLayout, EmptyLayout
?   ?   ??? ?? Pages/               # Sayfalar
?   ?       ??? Login.razor
?   ?       ??? Home.razor
?   ?       ??? Companies.razor
?   ?       ??? FinancialAnalysis.razor
?   ?       ??? Appointments.razor
?   ?       ??? ...
?   ??? ?? Models/
?   ?   ??? ?? Common/              # User, Company, Contact
?   ?   ??? ?? Accounting/          # Account, Entry, Record
?   ?   ??? ?? Financial/           # Ratios, Statements
?   ?   ??? ?? Appointment/         # Appointments, TaxCalendar
?   ??? ?? Services/                # Ýþ mantýðý servisleri
?   ??? Program.cs
?
??? ?? AydaMusavirlik.Desktop/      # WPF Desktop Uygulamasý
?   ??? ?? Views/
?   ?   ??? ?? Accounting/          # Hesap planý, fiþ giriþi
?   ?   ??? ?? Companies/           # Firma yönetimi
?   ?   ??? ?? Payroll/             # Bordro
?   ??? ?? Services/                # API istemci servisleri
?   ??? MainWindow.xaml
?
??? ?? AydaMusavirlik.Api/          # REST API
?   ??? Controllers/
?
??? ?? AydaMusavirlik.Core/         # Paylaþýlan kütüphane
?   ??? ?? Entities/
?   ??? ?? Interfaces/
?
??? ?? AydaMusavirlik.Data/         # Veritabaný katmaný
?   ??? ?? Repositories/
?
??? ?? docs/                        # Dokümantasyon
??? ?? README.md
??? ?? LICENSE
??? ?? build-setup.bat              # Kurulum oluþturma scripti
```

---

## ?? Modüller

### 1. Muhasebe Modülü
- ? Tek Düzen Hesap Planý (7 Ana Sýnýf)
- ? Fiþ Giriþi (Mahsup, Tahsilat, Ödeme)
- ?? Yevmiye Defteri
- ?? Mizan
- ?? Bilanço
- ?? Gelir Tablosu

### 2. Finansal Analiz Modülü
- ? 14 Finansal Oran Hesaplama
- ? Likidite Oranlarý
- ? Karlýlýk Oranlarý
- ? Faaliyet Oranlarý
- ? Borçluluk Oranlarý
- ? Trend Grafikleri
- ? KPI Dashboard

### 3. Randevu Modülü
- ? Randevu CRUD Ýþlemleri
- ? Vergi Takvimi
- ? Ýstatistikler
- ? Takvim Görünümü
- ?? Hatýrlatýcýlar
- ?? E-posta Bildirimleri

### 4. Firma Yönetimi
- ? Firma CRUD
- ? Firma Listesi
- ?? Firma Detaylarý
- ?? Cari Hesap Özeti

### 5. Kullanýcý Yönetimi
- ? Giriþ/Çýkýþ
- ? Rol Tabanlý Yetkilendirme
- ? Kullanýcý Listesi
- ?? Profil Yönetimi

---

## ??? Ekran Görüntüleri

### Dashboard
<img src="docs/screenshots/dashboard.png" alt="Dashboard" width="800"/>

### Finansal Analiz
<img src="docs/screenshots/financial-analysis.png" alt="Finansal Analiz" width="800"/>

### Randevu Takvimi
<img src="docs/screenshots/appointments.png" alt="Randevular" width="800"/>

---

## ?? Test Kullanýcýlarý

| Kullanýcý | Þifre | Rol | Yetki |
|-----------|-------|-----|-------|
| `admin` | `admin` | Admin | Tam eriþim |
| `muhasebe` | `muhasebe123` | Muhasebeci | Muhasebe modülü |
| `yonetici` | `yonetici123` | Yönetici | Raporlar + Yönetim |

---

## ??? Teknoloji Yýðýný

### Frontend
| Teknoloji | Versiyon | Kullaným |
|-----------|----------|----------|
| Blazor Server | .NET 9 | Web UI |
| MudBlazor | 8.5 | UI Components |
| WPF | .NET 9 | Desktop UI |

### Backend
| Teknoloji | Versiyon | Kullaným |
|-----------|----------|----------|
| ASP.NET Core | 9.0 | Web API |
| Entity Framework Core | 9.0 | ORM |
| SQLite | - | Veritabaný |
| Serilog | 9.0 | Loglama |

---

## ??? Yol Haritasý

### ? Faz 1: Temel Altyapý (Tamamlandý)
- [x] Proje yapýsý oluþturma
- [x] Kimlik doðrulama sistemi
- [x] Kullanýcý yönetimi
- [x] Firma yönetimi
- [x] Temel UI tasarýmý

### ? Faz 2: Muhasebe Temelleri (Tamamlandý)
- [x] Hesap planý yönetimi
- [x] Fiþ giriþi
- [x] Muhasebe kayýtlarý

### ? Faz 3: Finansal Analiz (Tamamlandý)
- [x] 14 finansal oran hesaplama
- [x] Grafik raporlar
- [x] KPI dashboard
- [x] Trend analizleri

### ? Faz 4: Randevu Sistemi (Tamamlandý)
- [x] Randevu yönetimi
- [x] Vergi takvimi
- [x] Takvim görünümü
- [x] Ýstatistikler

### ?? Faz 5: Raporlama (Devam Ediyor)
- [ ] Yevmiye defteri
- [ ] Mizan raporu
- [ ] Bilanço oluþturma
- [ ] Gelir tablosu
- [ ] Excel dýþa aktarým
- [ ] PDF oluþturma

### ?? Faz 6: E-Dönüþüm (Planlanan)
- [ ] E-Defter entegrasyonu
- [ ] E-Fatura entegrasyonu
- [ ] E-Arþiv desteði
- [ ] GÝB entegrasyonu

### ?? Faz 7: Bordro (Planlanan)
- [ ] Personel yönetimi
- [ ] Maaþ hesaplama
- [ ] SGK bildirgeleri
- [ ] Ýþe giriþ/çýkýþ

### ?? Faz 8: Geliþmiþ Özellikler (Planlanan)
- [ ] Cari hesaplar
- [ ] Banka entegrasyonu
- [ ] Çoklu þirket desteði
- [ ] Mobil uygulama

---

## ?? Ýlham Alýnan Özellikler

### Logo Yazýlým'dan
- ? Kullanýcý dostu arayüz tasarýmý
- ? Modüler yapý
- ? Hýzlý veri giriþi
- ? Kapsamlý raporlama

### Zirve Mali Müþavir'den
- ? Tek düzen hesap planý entegrasyonu
- ? Vergi takvimi
- ? Kapsamlý raporlama sistemi
- ? E-defter uyumluluðu

### Uyumsoft'tan
- ? Modern web tabanlý mimari
- ? Responsive tasarým
- ? Bulut desteði
- ? API tabanlý yapý

---

## ?? Derleme ve Kurulum

### Kurulum Paketi Oluþturma

```cmd
# Proje dizininde
build-setup.bat
```

Kurulum dosyalarý `C:\ARYAMusavirlik\kurulum\` dizininde oluþturulur.

### Manuel Derleme

```bash
# Release derlemesi
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish

# Portable sürüm
dotnet publish -c Release -o ./portable
```

---

## ?? Katkýda Bulunma

1. Fork yapýn
2. Feature branch oluþturun (`git checkout -b feature/YeniOzellik`)
3. Deðiþikliklerinizi commit edin (`git commit -m 'Yeni özellik eklendi'`)
4. Branch'i push edin (`git push origin feature/YeniOzellik`)
5. Pull Request açýn

### Kod Standartlarý
- C# kodlama kurallarýna uyun
- Türkçe yorum ve deðiþken adlarý kullanýn
- Unit test yazýn
- Dokümantasyon ekleyin

---

## ?? Lisans

Bu proje **MIT Lisansý** altýnda lisanslanmýþtýr. Detaylar için [LICENSE](LICENSE) dosyasýna bakýn.

---

## ?? Ýletiþim

<p align="center">
  <strong>Allbatros Software Solutions</strong><br><br>
  ?? Website: <a href="http://www.allglb.com">www.allglb.com</a><br>
  ?? E-posta: info@allglb.com<br>
  ?? Proje: AYDA Müþavirlik
</p>

---

<p align="center">
  <img src="docs/allbatros-logo.png" alt="Allbatros Logo" width="150"/><br><br>
  <strong>AYDA Müþavirlik v1.0.0</strong><br>
  <i>Profesyonel Mali Müþavirlik Çözümü</i><br><br>
  Copyright © 2024 Allbatros Software Solutions<br>
  Tüm Haklarý Saklýdýr
</p>

---

## ?? Proje Durumu

| Modül | Durum | Ýlerleme |
|-------|-------|----------|
| Kimlik Doðrulama | ? Tamamlandý | ????????????????????? 100% |
| Muhasebe | ?? Geliþtiriliyor | ????????????????????? 65% |
| Finansal Analiz | ? Tamamlandý | ????????????????????? 100% |
| Randevu | ? Tamamlandý | ????????????????????? 100% |
| Raporlama | ?? Geliþtiriliyor | ????????????????????? 40% |
| E-Dönüþüm | ?? Planlanan | ????????????????????? 0% |
| Bordro | ?? Planlanan | ????????????????????? 0% |

---

<p align="center">
  Made with ?? by <a href="http://www.allglb.com">Allbatros</a>
</p>

