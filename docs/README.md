# Dokümantasyon

`tkbys_kavramsal_tasarim.xlsx` dosyası, kavramsal tasarımdaki verileri dört ayrı çalışma sayfasında toplamaktadır. Aşağıdaki tablolar çalışma kitabındaki içerik ile bire bir aynı olacak şekilde referans olarak eklenmiştir.

## Varlıklar

| Varlık | Alan | Açıklama / Örnek |
| --- | --- | --- |
| Malzeme | Id | Benzersiz numara (Primary Key) |
| Malzeme | TKBYSNo | TKBYS'deki referans numarası (Opsiyonel, benzersiz olabilir) |
| Malzeme | Ad | Malzemenin adı (örn: "HP ProBook 450 G9 Dizüstü Bilgisayar") |
| Malzeme | Ozellikler | Malzemenin teknik özellikleri (örn: "16GB RAM, 512GB SSD, i7 12. Nesil") |
| Malzeme | Adet | Stoktaki miktar |
| Malzeme | Aciklama | Ek bilgiler |
| Malzeme | DepoId | Hangi depoda olduğunu belirten anahtar (Foreign Key) |
| Malzeme | Durum | Enum: Depoda, Zimmetli, Hurda, BirimDisi |
| Malzeme | KayitTarihi | Sisteme eklendiği tarih |
| Personel | Id | ASP.NET Core Identity tarafından yönetilen benzersiz Guid |
| Personel | AdSoyad | Personelin adı ve soyadı |
| Personel | Email | E-posta adresi (kullanıcı adı) |
| Personel | SicilNo | Kurum sicil numarası |
| Depo | Id | Benzersiz numara |
| Depo | Ad | Deponun adı (örn: "Ana Bina - Zemin Kat Depo") |
| Depo | Aciklama | Depo ile ilgili detaylar |
| Zimmet | Id | Benzersiz numara |
| Zimmet | MalzemeId | Zimmetlenen malzemenin ID'si (Foreign Key) |
| Zimmet | PersonelId | Zimmetlenen personelin ID'si (Foreign Key) |
| Zimmet | ZimmetTarihi | Zimmetin yapıldığı tarih |
| Zimmet | IadeTarihi | Zimmetin geri alındığı tarih (nullable) |
| Zimmet | OnaylayanYetkiliId | Onaylayan Harcama Yetkilisi'nin ID'si |
| Zimmet | Durum | Enum: OnayBekliyor, Aktif, IadeEdildi |
| SatınAlmaTalep | Id | Benzersiz numara |
| SatınAlmaTalep | TalepEdenPersonelId | Talebi oluşturan personelin ID'si |
| SatınAlmaTalep | MalzemeAdi | İstenen malzemenin adı |
| SatınAlmaTalep | Gerekce | Talebin gerekçesi |
| SatınAlmaTalep | Durum | Enum: TalepEdildi, SatinAlmaOnayinda, HarcamaYetkilisiOnayinda, Onaylandi, Reddedildi, Tamamlandi |
| SatınAlmaTalep | TalepTarihi | Talebin oluşturulduğu tarih |
| SatınAlmaTalep | BelgeYolu | Satın alma sonrası yüklenecek belgenin dosya yolu |

## Roller ve Yetkiler

| Rol | Yetkiler |
| --- | --- |
| Admin | Tüm sistem üzerinde tam yetki; kullanıcı/malzeme ekleme, rol atama, tüm kayıtları görme ve düzenleme |
| Personel | Sadece kendi zimmetlerini görme, yeni malzeme talebi oluşturma |
| Taşınır Kayıt Yetkilisi | Malzeme ve personel kaydı yapma, rol atama, malzemeleri depoya ekleme, zimmet işlemlerini başlatma |
| Taşınır Kontrol Yetkilisi | Tüm malzeme, depo ve zimmet kayıtlarını görüntüleme |
| Harcama Yetkilisi | Zimmet ve satın alma taleplerini onaylama/reddetme, tüm kayıtları görüntüleme |
| Satın Alma Memuru | Satın alma taleplerini değerlendirme, bütçe kontrolü yapma, Harcama Yetkilisi'ne yönlendirme, onaylanan alımları gerçekleştirip belgesi yükleme |

## İş Akışları

| Akış | Adım | Sorumlu | Açıklama | Durum Değişimi |
| --- | --- | --- | --- | --- |
| Yeni Malzeme Satın Alma | 1 | Personel | Satın alma talebi oluşturur | TalepEdildi |
| Yeni Malzeme Satın Alma | 2 | Satın Alma Memuru | Talebi görüntüler ve bütçe uygunluğunu kontrol eder | TalepEdildi |
| Yeni Malzeme Satın Alma | 3 | Satın Alma Memuru | Uygunsa Harcama Yetkilisi'ne yönlendirir | HarcamaYetkilisiOnayinda |
| Yeni Malzeme Satın Alma | 4 | Harcama Yetkilisi | Talebi inceler ve onaylar veya reddeder | Onaylandi / Reddedildi |
| Yeni Malzeme Satın Alma | 5 | Satın Alma Memuru | Onaylanan talepleri satın alır ve belgeyi yükler | Tamamlandi |
| Yeni Malzeme Satın Alma | 6 | Taşınır Kayıt Yetkilisi | Gelen malzemeyi sisteme kaydeder ve depoya ekler | Malzeme kaydedildi |
| Malzeme Zimmetleme | 1 | Personel / Taşınır Kayıt Yetkilisi | Malzeme talebi oluşturur veya zimmet işlemi başlatır | Talep oluşturuldu |
| Malzeme Zimmetleme | 2 | Taşınır Kayıt Yetkilisi | Malzemenin depoda mevcut olup olmadığını kontrol eder | Uygunluk doğrulandı |
| Malzeme Zimmetleme | 3 | Taşınır Kayıt Yetkilisi | Zimmet kaydı oluşturur | OnayBekliyor |
| Malzeme Zimmetleme | 4 | Harcama Yetkilisi | Zimmet talebini inceler | Aktif / Reddedildi |
| Malzeme Zimmetleme | 5 | Sistem | Onaylanırsa malzeme durumunu günceller ve personele bildirim gönderir | Malzeme: Zimmetli |

## Durum Listeleri

| Bağlam | Durum Değerleri | Açıklama |
| --- | --- | --- |
| Malzeme.Durum | Depoda, Zimmetli, Hurda, BirimDisi | Malzemenin stok konumunu veya kullanım durumunu belirtir |
| Zimmet.Durum | OnayBekliyor, Aktif, IadeEdildi | Zimmet sürecinin aşamasını temsil eder |
| SatınAlmaTalep.Durum | TalepEdildi, SatinAlmaOnayinda, HarcamaYetkilisiOnayinda, Onaylandi, Reddedildi, Tamamlandi | Satın alma sürecindeki iş adımlarını izler |

