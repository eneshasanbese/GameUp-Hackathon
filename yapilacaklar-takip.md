# Su Damlası — Yapılacaklar Takibi

31 Ekim 2026 Köz Ocağı (T09/T14–T16 ek kapsamı): sabit yatay çekim, bağımsız 1–6 su puanı/saniye çevresel kayıp, kullanılabilir prefab ve ayrı demo sahnesi eklendi. Mevcut küçülme/hız/zıplama korunur. Ayrı Unity 6000.5.0f1 projesinde 9 asset ve 45 Play Mode kontrolü geçti; iki yönden gerçek kaçış, zıplama, tüketim ayrımı, çoklu collider/kaynak, yaşam döngüsü ve demo görünürlüğü doğrulandı. Ana bölüm yerleşimi/görüntü ve dış build bekliyor ([ayarlar ve kayıt](docs/koz-ocagi.md)).

**Son kontrol:** 31 Ekim 2026, 02:05 (diskteki dosyalara göre)
**Ana belge:** [genel-yapilacaklar.md](genel-yapilacaklar.md). T numaraları oradaki görevlerle aynıdır.
**Kural:** Bir madde ancak Unity'de denenip çalıştığı görüldükten sonra işaretlenir. Kaydedilmemiş değişiklikler sayılmaz.

## 1. Proje kurulumu

31 Ekim 2026 ek kapsam: Saw ve Slime Fire için duran/git-gel prefabları ve Slime yakınlık erime çarpanı uygulandı. Unity 6000.5.0f1 ile ayrı geçici projede 20 asset ve 31 Play Mode kontrolü geçti; ayarlar ve doğrulama kaydı [düşman belgesinde](docs/enemies.md). Asıl sahnelere yerleşim ve build testi yapılmadı.

31 Ekim 2026 Saw animasyon düzeltmesi: 10 FPS ve döngü sonundaki tekrarlı ilk kare yerine 24 FPS, eşit aralıklı sekiz kare kullanıldı. Aynı bağımsız Unity projesinde 4 asset ve 10 Play Mode kontrolü geçti; iki Saw prefabında beşer döngü örneklendi. Asıl sahnede görüntü/build kontrolü bekliyor ([kayıt](docs/enemies.md)).

31 Ekim 2026 ısı hasarı düzeltmesi (T14/QA01): kullanıcının yeni kararıyla etkin Slime Fire ısısında yerde durmak da su kaybettiriyor; ısı dışında durmak ücretsiz. Ayrı Unity projesinde gerçek prefablarla 27 Play Mode kontrolü geçti: kuru/nemli duruş ve yürüyüş tüketimi, alan çıkışı/kapatma, çarpan 1, örtüşme, farklı fizik adımları ve ısıdan ölüm/yeniden doğma. Asıl sahne/build testi bekliyor ([kayıt](docs/enemies.md)).

- [x] Düşmanlarla doğrudan temas: sonraki kullanıcı talebiyle Player KnockbackAbility ve altı gövde trigger'ı bağlandı; 13 asset ve 36 Play Mode kontrolü ayrı Unity projesinde geçti. Asıl sahnede his/denge ve build testi bekliyor ([kayıt](docs/player-state-machine.md#knockback-ve-temas-hasarı)).

31 Ekim 2026 knockback: ayrı ability/state, düşmandan uzağa itiş, kısa kontrol kilidi/kübik toparlanma, Inspector'dan ayarlanabilir temas su kaybı (10) ve 2 saniye death_01 karesiyle flash eklendi. Flash yeni gövde darbelerini engeller; ısı/duman tüketimi sürer. Player ve altı düşman prefabı bağlıdır; mevcut sahne değişiklikleri korundu. Ayrı Unity 6000.5.0f1 projesinde 49 kontrol geçti; doğrulanan asset'ler gerçek meta/GUID'lerle projeye aktarıldı. Ana sahne hissi ve build testi bekliyor ([ayarlar ve testler](docs/player-state-machine.md#knockback-ve-temas-hasarı)).

31 Ekim 2026 HapsirikKoz ek kapsamı: duran/devriye prefabları, Idle/Sneeze animasyonları ve kullanıcının istediği rastgele 1–5 saniye bekleme → tam hapşırma → rastgele 2–3 saniye cooldown döngüsü eklendi. Her tur cooldown sonrası yeni bekleme seçilir; aralıklar Inspector'dan değiştirilebilir. Ayrı Unity 6000.5.0f1 projesinde 18 asset ve 25 Play Mode kontrolü geçti; test edilmiş asset'ler gerçek meta dosyalarıyla projeye aktarıldı. Asıl sahnede yerleştirme/görüntü ve build testi bekliyor ([ayarlar ve test kaydı](docs/enemies.md)).

31 Ekim 2026 HapsirikKoz duman hasarı (T14/QA01): yeni kullanıcı talebiyle hapşırmanın çıkış karesinde birkaç saniye kalan hasar alanı eklendi. Varsayılan süre 3 saniye, yarıçap 1.75, su tüketim çarpanı 2; ayarlar duman prefab'ının Inspector'ında. Duman, Slime Fire'ın `EnemyHeatArea`/tek su sayacı yolunu kullanır; dururken de hasar verir, çıktığı noktada kalır ve süresi dolunca çarpanı temizler. Ayrı Unity projesinde 10 asset ve 30 Play Mode kontrolü geçti; kuru/nemli/çok kuru/hava tüketimi, giriş/çıkış, ömür, Slime/duman örtüşmesi, kapatma/silme, çoklu collider, kontrol kapalıyken koruma, ölüm/yeniden doğma ve devriye emisyonu doğrulandı. Test edilmiş prefablar projeye aktarıldı. Duman görseli/animasyonu sonraya bırakıldı; asıl sahne ve build testi bekliyor ([kayıt](docs/enemies.md)).

- [x] Unity 6000.5.0f1 projesi oluşturuldu (URP 2D, yeni Input System)
- [x] `.gitignore` eklendi; `Library`, `Temp`, `Logs`, `UserSettings` depo dışında (T05)
- [x] Unity git ayarları doğru: Force Text, Visible Meta Files
- [x] GitHub deposu bağlandı, ilk commit gönderildi
- [ ] Son değişiklikler commit edildi (paket, paletler, sahne, layer'lar, `genel-yapilacaklar.md`)
- [ ] T01 — Etkinlik kuralları `docs/kararlar.md` dosyasına yazıldı
- [ ] T04 — İki kişi de projeyi aynı Unity sürümüyle açtı
- [ ] T06 — Dosya sahipliği belirlendi (ana sahneyi kim düzenliyor?)
- [ ] T07 — Boş/test build alındı ve başka bilgisayarda açıldı

## 2. Görseller ve import ayarları

- [x] Kenney Platformer Pack eklendi (`Assets/platformerPack`)
- [x] `Tiles/Double` import ayarları: Single, PPU 128, Full Rect, Bilinear, mip map kapalı, sıkıştırma yok (314 dosya)
- [x] `Backgrounds/Double` import ayarları aynı şekilde yapıldı (14 dosya)
- [x] Tile paletleri oluşturuldu: `Ground-Dry` (kum), `Ground-Wet` (çim)
- [ ] K02 kararı `docs/kararlar.md` dosyasına yazıldı: Kenney Platformer Pack, `Double` klasörü, PPU 128. Ana belgedeki T42 ve T44 maddeleri (18 PPU, Point filtre, Pixel Perfect Camera) bu paket için geçerli değil.
- [ ] Nemli zeminin görseli kesinleşti (şu an çim; "son fidan" hikâyesiyle çelişebilir, bkz. QA19)

## 3. Sahne ve test zemini

- [x] `Grid` ve altında `Ground-Dry`, `Ground-Wet` tilemap'leri var
- [x] Grid tam sayı koordinatta, tile'lar hizalı
- [x] Fizik layer'ları: `Ground-Dry`, `Ground-Wet`, `Player`; tilemap'ler kendi layer'larında
- [x] Sorting layer'lar: Background, Level, Default, Player; tilemap'ler `Level` katmanında
- [x] Test zemini çizildi (rampa dahil)
- [x] Collider'lar: Tilemap Collider 2D (Merge) + Composite Collider 2D (Polygons) + Static Rigidbody 2D

> Collision matrix'te şimdilik değişiklik gerekmiyor. Tek istisna 5. bölümdeki `CameraBounds` layer'ı.

## 4. Oyuncu

- [ ] T08 — Oyuncu prefab'ı: kökte Rigidbody 2D + Capsule Collider 2D, `Visual` alt objesinde geçici sprite; layer `Player`, sorting layer `Player`
- [ ] T09 — Yürüme, zıplama ve yere basma kontrolü (`Ground-Dry` ve `Ground-Wet` layer'ları birlikte)
- [ ] T09 — Rampada dururken aşağı kaymıyor
- [ ] T11 — Boşluğa düşünce yeniden deneme başlıyor

## 5. Kamera ve arka plan (Cinemachine + parallax)

Cinemachine kamerayı oyuncunun peşinden götürür. Arka plan kameranın altında olduğu için ekran hep dolu kalır. Parallax da katmanları farklı hızlarda kaydırarak derinlik verir.

**Arka plan kurulumu**

- [ ] `Background` objesi Main Camera'nın altında, Position `0, 0, 10`
- [ ] `background_solid_sky` ve `background_fade_hills` silindi (kamera rengi gökyüzü dolgusunun yerini tutuyor)
- [ ] `background_fade_desert` X 0, Y -1 ve `background_clouds` X 0, Y 3; ikisinde de Draw Mode Tiled, Width 28, Height 4 (parallax kaydırmasına pay kalsın diye 24 yerine 28)
- [ ] Main Camera'nın arka plan rengi gökyüzü rengi yapıldı (`C3E3FF`)

**Cinemachine (T10)**

- [ ] Cinemachine paketi kuruldu (Package Manager → Unity Registry → Cinemachine); karar `docs/kararlar.md` dosyasına yazıldı
- [ ] Main Camera'da Cinemachine Brain var
- [ ] Cinemachine Camera oyuncuyu takip ediyor (Position Composer); Lens → Orthographic Size 5
- [ ] Kamera bölümün dışını göstermiyor: Confiner 2D + bölümü çevreleyen bir Polygon Collider 2D
- [ ] Sınır collider'ı Is Trigger ve ayrı bir `CameraBounds` layer'ında; collision matrix'te bu layer hiçbir şeyle çarpışmıyor (oyuncuya trigger olayı göndermesin)

**Parallax**

- [ ] Parallax script'i: tepeler kameradan biraz yavaş, bulutlar daha da yavaş kayıyor; katmanlar her 4 birimde başa sarılıp sonsuz tekrar ediyor
- [ ] Parallax, Cinemachine kamerayı hareket ettirdikten sonra çalışıyor (arka planda titreme yok)
- [ ] Game görünümünde kamera sağa-sola ve yukarı-aşağı giderken arka planda boşluk ya da kesik yok

> Zaman sıkışırsa ilk ertelenecek iş parallax olur (ana belgedeki kapsam kesme sırası). Kamera takibi ve arka plan kurulumu korunur.

## 6. Su sistemi

- [ ] T12 — Su değerini tek bir bileşen (`WaterMeter`) yönetiyor
- [ ] T13 — Su 0–100 arasında kalıyor
- [ ] T14 — Durum önceliği: oyun aktif değilken tüketim yok, kaynakta dolum, havada normal, ısı/duman dışında yerde dururken sıfır, yerde hareket ederken veya etkin Slime Fire ısısı/HapsirikKoz dumanında dururken zemin ve ısı katsayılı
- [ ] T15 — Kuru ve nemli zemin ayırt ediliyor; bilinmeyen zemin normal sayılıyor
- [ ] T16 — Tüketim kare hızından bağımsız (geçen süreye göre hesaplanıyor)
- [ ] T17 — Fizik titreşimi ve duvara doğru yürümek hareket sayılmıyor
- [ ] T18 — Zıplama ve düşmede normal tüketim; nemli zeminin katsayısı havaya taşınmıyor
- [ ] T19 — Su çubuğu (UI) suyun azalıp dolmasını gösteriyor
- [ ] T20 — Normal tüketim, nemli zemin katsayısı ve kapasite Inspector'dan değiştirilebiliyor

## 7. Su birikintisi, yeniden doğma, final

- [ ] T21 — Su birikintisi prefab'ı
- [ ] T22 — Birikintiye girince su tamamen doluyor
- [ ] T23 — Birikinti checkpoint oluyor; ilk birikintiden önce bölüm başına dönülüyor
- [ ] T24 — Birikintiye art arda girip çıkmak hata veya kilitlenme yaratmıyor
- [ ] T25 — Su bitince tek seferlik ölüm başlıyor
- [ ] T26 — Yeniden doğmada konum, hız, su, kontrol ve UI sıfırlanıyor
- [ ] T27 — Final fidanı: kazanma mesajı çıkıyor, tüketim ve kontrol kapanıyor
- [ ] T28 — Tekrar oyna bölümü baştan başlatıyor
- [ ] T29 — Baştan sona ilk build alındı ve denendi

**Bugünün hedefi:** 4.–7. bölümler bitince oyun çirkin olsa da başlanıp kaybedilip kazanılabiliyor olmalı.

## 8. Sonraki aşamalar

Ayrıntılar [genel-yapilacaklar.md](genel-yapilacaklar.md) içinde.

- [ ] T30–T38 — Bölüm tasarımı ve oynanış dengesi
- [ ] T39–T48 — Görsel karar, damla görseli, küçülme, su izi

31 Ekim 2026, T09/T45–T48 güncellemesi: kullanıcı suya bağlı collider küçülmesini onayladı ([K03](docs/kararlar.md)). Tek CircleCollider2D görselle birlikte küçülüyor; `Player → WaterMeter → Water Size → Minimum Scale` deneme değeri 0.45. Temas/şekil taraması ve eğime göre hareket uygulandı. Gerçek Player prefab'ının kopyasıyla ayrı Unity projesinde 47 Play Mode doğrulaması geçti; asıl Components/Level 5 sahnelerinde oynanış ve build kontrolü bekliyor. Bu sonuç tüm T39–T48 veya genel QA listesinin tamamlandığı anlamına gelmez.

31 Ekim 2026, düşük su hareket eki: kullanıcı talebiyle MoveAbility/JumpAbility Inspector'ına bağımsız yürüme, hava hızı ve kalkış hızı çarpanları eklendi (1.4/1.4/1.1). Aynı su değeri kullanılıyor; 1 katsayısı artışı kapatıyor. Dolu/yarım/az su ölçümleri, Inspector ayarı, havada dolum, yeniden doğma ve hızlı küçük oyuncuyla rampa regresyonu dahil 48 Play Mode doğrulaması ayrı Unity projesinde geçti. Asıl sahne/build oynanışı bekliyor; [kapsam](docs/player-state-machine.md#doğrulama).

- [ ] T49–T55 — Arayüz, ses, kaynak/lisans listesi, final sunumu
- [ ] QA01–QA20 — Son test listesi
- [ ] D01–D09 — Teslim listesi
