# TKBYSApp

TKBYSApp, Taşınır Kayıt ve Yönetim Sistemi (TKBYS) süreçlerini modern bir ASP.NET Core MVC uygulaması üzerinde toplamak için oluşturulmuş .NET 9 tabanlı bir çözümdür. Uygulama kimlik doğrulama için ASP.NET Core Identity kullanır ve satın alma ile zimmet akışlarını roller bazında yönetir.

## Gereksinimler

- [.NET SDK 9.0](https://dotnet.microsoft.com/)
- Microsoft SQL Server örneği

## Çözüm Yapısı

```
TKBYSApp.sln
└── TKBYSApp.Web
    ├── Controllers
    ├── Data
    ├── Migrations
    ├── Models
    ├── Services
    ├── Views
    └── wwwroot
```

## Kurulum

1. **Bağımlılıkları geri yükleyin**
   ```bash
   dotnet restore
   ```

2. **Veritabanı bağlantı dizesini yapılandırın**
   `appsettings.json` dosyasındaki `DefaultConnection` değerini ihtiyaçlarınıza göre güncelleyin. Varsayılan bağlantı dizesi lokal SQL Server için ayarlanmıştır:
   ```json
   "Server=localhost;Database=TKBYSDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
   ```

3. **Veritabanı şemasını oluşturun**
   ```bash
   dotnet ef database update --project TKBYSApp.Web
   ```

4. **Uygulamayı çalıştırın**
   ```bash
   dotnet run --project TKBYSApp.Web
   ```

## Varsayılan Roller

Uygulama başlangıcında aşağıdaki roller otomatik olarak oluşturulur:

- Admin
- Personel
- Taşınır Kayıt Yetkilisi
- Taşınır Kontrol Yetkilisi
- Harcama Yetkilisi
- Satın Alma Memuru

## Öne Çıkan Özellikler

- Identity üzerinden `Personel` kullanıcı yönetimi ve rol atamaları
- Malzeme, Depo, Zimmet ve Satın Alma Talepleri için CRUD ekranları
- Satın alma talebi akışı: Personel → Satın Alma Memuru → Harcama Yetkilisi → Satın Alma Memuru → Taşınır Kayıt Yetkilisi
- Zimmet akışı: Taşınır Kayıt Yetkilisi/Admin oluşturur, Harcama Yetkilisi onaylar, iade işlemleri yönetilir
- SQL Server üzerinde çalışacak şekilde yapılandırılmış Entity Framework Core veri modeli

## Geliştirme İpuçları

- Yeni migration oluşturmak için:
  ```bash
  dotnet ef migrations add MigName --project TKBYSApp.Web
  ```
- Varsayılan admin kullanıcısı oluşturmak istiyorsanız `Data/SeedData.cs` içerisinde ilgili kodu ekleyebilirsiniz.
- Razor görünümlerinde yerleşik `RoleConstants` değerlerini kullanarak yetki kontrollü bileşenler oluşturabilirsiniz.

## Test ve Yayın

- Çalışma sürecinde `dotnet build` ve `dotnet run` komutları ile derlemeyi kontrol edin.
- Yayın için `dotnet publish -c Release` komutunu kullanabilirsiniz.
