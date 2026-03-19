# AYDA Müþavirlik - Kullaným Kýlavuzu

## Ýçindekiler

1. [Giriþ](#giriþ)
2. [Kurulum](#kurulum)
3. [Ýlk Kullaným](#ilk-kullaným)
4. [Modüller](#modüller)
5. [Finansal Analiz](#finansal-analiz)
6. [SSS](#sss)

## Giriþ

AYDA Müþavirlik, mali müþavirlik ofisleri için geliþtirilmiþ profesyonel bir muhasebe yazýlýmýdýr.

### Özellikler

- Modern web tabanlý arayüz
- Tek Düzen Hesap Planý desteði
- Profesyonel finansal analiz
- Çoklu firma yönetimi
- Rol tabanlý yetkilendirme

## Kurulum

### Sistem Gereksinimleri

- Windows 10/11 (64-bit)
- .NET 9.0 Runtime
- 4 GB RAM (önerilen: 8 GB)
- 500 MB disk alaný

### Kurulum Adýmlarý

1. `AydaMusavirlik-Setup.exe` dosyasýný çalýþtýrýn
2. Kurulum sihirbazýný takip edin
3. Kurulum tamamlandýðýnda uygulamayý baþlatýn

## Ýlk Kullaným

### Giriþ Yapma

1. Tarayýcýnýzda `http://localhost:5000` adresine gidin
2. Kullanýcý adý ve þifrenizi girin
3. "Giriþ Yap" butonuna týklayýn

### Varsayýlan Kullanýcýlar

| Kullanýcý | Þifre | Yetki |
|-----------|-------|-------|
| admin | admin | Tam eriþim |
| muhasebe | muhasebe123 | Muhasebe |
| yonetici | yonetici123 | Raporlar |

## Modüller

### Muhasebe

- **Hesap Planý:** Tek düzen hesap planý yönetimi
- **Fiþ Giriþi:** Mahsup, tahsilat, ödeme fiþleri
- **Yevmiye:** Günlük kayýtlar
- **Mizan:** Bakiye kontrolü

### Firma Yönetimi

- Firma ekleme/düzenleme
- Vergi bilgileri
- Ýletiþim kiþileri

## Finansal Analiz

### Likidite Oranlarý

1. **Cari Oran:** Dönen Varlýklar / K.V. Borçlar
2. **Asit-Test Oraný:** (D.V. - Stoklar) / K.V. Borçlar
3. **Nakit Oraný:** Nakit / K.V. Borçlar

### Karlýlýk Oranlarý

1. **Brüt Kar Marjý:** Brüt Kar / Satýþlar × 100
2. **Net Kar Marjý:** Net Kar / Satýþlar × 100
3. **ROA:** Net Kar / Toplam Aktif × 100
4. **ROE:** Net Kar / Özsermaye × 100

### Faaliyet Oranlarý

1. **Alacak Devir Hýzý:** Satýþlar / Alacaklar
2. **Stok Devir Hýzý:** SMM / Stoklar
3. **Aktif Devir Hýzý:** Satýþlar / Toplam Aktif

### Borçluluk Oranlarý

1. **Borç Oraný:** Toplam Borç / Toplam Aktif × 100
2. **Borç/Özsermaye:** Toplam Borç / Özsermaye
3. **Faiz Karþýlama:** FVÖK / Faiz Gideri

## SSS

### Þifremi unuttum, ne yapmalýyým?

Admin kullanýcýsýndan þifre sýfýrlama talep edin.

### Birden fazla firma nasýl tanýmlarým?

Firmalar > Yeni Firma menüsünden yeni firma ekleyebilirsiniz.

### Verilerimi nasýl yedeklerim?

Ayarlar > Yedekleme bölümünden veritabaný yedeði alabilirsiniz.

---

© 2024 Allbatros - Tüm Haklarý Saklýdýr
