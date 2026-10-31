# Su Damlası — Yapılacaklar Takibi

**Son kontrol:** 31 Ekim 2026, 17:20 (diskteki dosyalara ve koda göre)
**Ana belge:** [genel-yapilacaklar.md](genel-yapilacaklar.md). T numaraları oradaki görevlerle aynıdır.
**Kural:** Kurulum maddeleri diskteki dosyalarla doğrulanınca işaretlenir. Oynanış maddeleri (T/QA) ancak asıl sahnede (`Level1`) oynanarak denendikten sonra işaretlenir. Kodu yazılmış ama oyunda denenmemiş maddeler **Uygulandı; test bekliyor** olarak not edilir. Kaydedilmemiş değişiklikler sayılmaz.

## 1. Proje kurulumu

31 Ekim 2026 ek kapsam: Saw ve Slime Fire için duran/git-gel prefabları ve Slime yakınlık erime çarpanı uygulandı. Unity 6000.5.0f1 ile ayrı geçici projede 20 asset ve 31 Play Mode kontrolü geçti; ayarlar ve doğrulama kaydı [düşman belgesinde](docs/enemies.md). Asıl sahnelere yerleşim ve build testi yapılmadı.

31 Ekim 2026 Saw animasyon düzeltmesi: 10 FPS ve döngü sonundaki tekrarlı ilk kare yerine 24 FPS, eşit aralıklı sekiz kare kullanıldı. Aynı bağımsız Unity projesinde 4 asset ve 10 Play Mode kontrolü geçti; iki Saw prefabında beşer döngü örneklendi. Asıl sahnede görüntü/build kontrolü bekliyor ([kayıt](docs/enemies.md)).

31 Ekim 2026 ısı hasarı düzeltmesi (T14/QA01): kullanıcının yeni kararıyla etkin Slime Fire ısısında yerde durmak da su kaybettiriyor; ısı dışında durmak ücretsiz. Ayrı Unity projesinde gerçek prefablarla 27 Play Mode kontrolü geçti: kuru/nemli duruş ve yürüyüş tüketimi, alan çıkışı/kapatma, çarpan 1, örtüşme, farklı fizik adımları ve ısıdan ölüm/yeniden doğma. Asıl sahne/build testi bekliyor ([kayıt](docs/enemies.md)).

- [x] Düşmanlarla doğrudan temas: sonraki kullanıcı talebiyle Player KnockbackAbility ve altı gövde trigger'ı bağlandı; 13 asset ve 36 Play Mode kontrolü ayrı Unity projesinde geçti. Asıl sahnede his/denge ve build testi bekliyor ([kayıt](docs/player-state-machine.md#knockback-ve-temas-hasarı)).

31 Ekim 2026 knockback: ayrı ability/state, düşmandan uzağa itiş, kısa kontrol kilidi/kübik toparlanma, Inspector'dan ayarlanabilir temas su kaybı (10) ve 2 saniye death_01 karesiyle flash eklendi. Flash yeni gövde darbelerini engeller; ısı/duman tüketimi sürer. Player ve altı düşman prefabı bağlıdır; mevcut sahne değişiklikleri korundu. Ayrı Unity 6000.5.0f1 projesinde 49 kontrol geçti; doğrulanan asset'ler gerçek meta/GUID'lerle projeye aktarıldı. Ana sahne hissi ve build testi bekliyor ([ayarlar ve testler](docs/player-state-machine.md#knockback-ve-temas-hasarı)).

31 Ekim 2026 HapsirikKoz ek kapsamı: duran/devriye prefabları, Idle/Sneeze animasyonları ve kullanıcının istediği rastgele 1–5 saniye bekleme → tam hapşırma → rastgele 2–3 saniye cooldown döngüsü eklendi. Her tur cooldown sonrası yeni bekleme seçilir; aralıklar Inspector'dan değiştirilebilir. Ayrı Unity 6000.5.0f1 projesinde 18 asset ve 25 Play Mode kontrolü geçti; test edilmiş asset'ler gerçek meta dosyalarıyla projeye aktarıldı. Asıl sahnede yerleştirme/görüntü ve build testi bekliyor ([ayarlar ve test kaydı](docs/enemies.md)).

31 Ekim 2026 HapsirikKoz duman hasarı (T14/QA01): yeni kullanıcı talebiyle hapşırmanın çıkış karesinde birkaç saniye kalan hasar alanı eklendi. Varsayılan süre 3 saniye, yarıçap 1.75, su tüketim çarpanı 2; ayarlar duman prefab'ının Inspector'ında. Duman, Slime Fire'ın `EnemyHeatArea`/tek su sayacı yolunu kullanır; dururken de hasar verir, çıktığı noktada kalır ve süresi dolunca çarpanı temizler. Ayrı Unity projesinde 10 asset ve 30 Play Mode kontrolü geçti; kuru/nemli/çok kuru/hava tüketimi, giriş/çıkış, ömür, Slime/duman örtüşmesi, kapatma/silme, çoklu collider, kontrol kapalıyken koruma, ölüm/yeniden doğma ve devriye emisyonu doğrulandı. Test edilmiş prefablar projeye aktarıldı. Duman görseli/animasyonu sonraya bırakıldı; asıl sahne ve build testi bekliyor ([kayıt](docs/enemies.md)).

31 Ekim 2026 Köz Ocağı (T09/T14–T16 ek kapsamı): sabit yatay çekim, bağımsız 1–6 su puanı/saniye çevresel kayıp, kullanılabilir prefab ve ayrı demo sahnesi eklendi. Mevcut küçülme/hız/zıplama korunur. Ayrı Unity 6000.5.0f1 projesinde 9 asset ve 45 Play Mode kontrolü geçti; iki yönden gerçek kaçış, zıplama, tüketim ayrımı, çoklu collider/kaynak, yaşam döngüsü ve demo görünürlüğü doğrulandı. Ana bölüm yerleşimi/görüntü ve dış build bekliyor ([ayarlar ve kayıt](docs/koz-ocagi.md)).

- [x] Unity 6000.5.0f1 projesi oluşturuldu (URP 2D, yeni Input System)
- [x] `.gitignore` eklendi; `Library`, `Temp`, `Logs`, `UserSettings` depo dışında (T05)
- [x] Unity git ayarları doğru: Force Text, Visible Meta Files
- [x] GitHub deposu bağlandı, ilk commit gönderildi
- [x] Paket, paletler (`Ground-Dry`, `Ground-Wet`, `Decor`, `Ground-Extra-Dry`), sahne ve layer'lar commit edildi
- [ ] T01 — Etkinlik kuralları `docs/kararlar.md` dosyasına yazıldı (dosya var, etkinlik kuralları henüz yok)
- [ ] T04 — İki kişi de projeyi aynı Unity sürümüyle açtı
- [ ] T06 — Dosya sahipliği belirlendi (ana sahneyi kim düzenliyor?)
- [ ] T07 — Boş/test build alındı ve başka bilgisayarda açıldı

## 2. Görseller ve import ayarları

- [x] Kenney Platformer Pack eklendi (`Assets/platformerPack`)
- [x] `Tiles/Double` import ayarları: Single, PPU 128, Full Rect, Bilinear, mip map kapalı, sıkıştırma yok (314 dosya)
- [x] `Backgrounds/Double` import ayarları aynı şekilde yapıldı (14 dosya)
- [x] Tile paletleri oluşturuldu: `Ground-Dry` (kum), `Ground-Wet` (çim), `Decor` (314 tile), `Ground-Extra-Dry`
- [ ] K02 kararı `docs/kararlar.md` dosyasına yazıldı: Kenney Platformer Pack, `Double` klasörü, PPU 128. Ana belgedeki T42 ve T44 maddeleri (18 PPU, Point filtre, Pixel Perfect Camera) bu paket için geçerli değil.
- [ ] Nemli zeminin görseli kesinleşti (şu an çim; "son fidan" hikâyesiyle çelişebilir, bkz. QA19)
- [ ] `Decor-Desert` paleti: Kenney stilinde 9 cansız dekor tile'ı (kum: `dry_grass`, `dry_bush`, `tumbleweed`, `dry_cactus`, `sand_pebbles`; taş: `stone_pebbles`, `stone_pile`, `stone_spike`, `stone_dry_shrub`). PPU 128, collider yok, `Decor-Front`'a boyanır. **Dosyalar eklendi; Unity içe aktarımı ve palet görünümü kontrol bekliyor.**
- [ ] QA19 — `Level1`'deki yeşil `cactus` dekorları yaşayan bitki gibi duruyor; istenirse `dry_cactus` ile değiştirilecek.

## 3. Sahne ve bölüm (`Level1`)

- [x] `SampleScene` → `Level1` olarak yeniden adlandırıldı (sahne GUID'i korundu); build listesindeki tek sahne `Level1`
- [x] `Grid` ve altında `Ground-Dry`, `Ground-Wet`, `Ground-Extra-Dry`, `Decor-Front` tilemap'leri var; tile'lar tam sayı koordinatlara oturuyor
- [x] Fizik layer'ları: `Ground-Dry` (6), `Ground-Wet` (7), `Player` (8), `CameraBounds` (9), `Ground-Extra-Dry` (10); zemin tilemap'leri kendi layer'larında
- [x] Sorting layer'lar: Background, Level, Default, Player; tilemap'ler `Level` katmanında
- [x] Zemin collider'ları: Tilemap Collider 2D (Merge) + Composite Collider 2D (Polygons) + Static Rigidbody 2D
- [x] `Decor-Front`: layer `Default`, sorting `Level` / Order 1, collider yok (zeminin önünde, damlanın arkasında)
- [x] Bölüm çizildi: başlangıç, atlama taşları, ilk kaynak (X≈11), yukarıda kuru / aşağıda nemli iki yol, ikinci kaynak (X≈44)

> Collision matrix: `CameraBounds` hiçbir şeyle çarpışmıyor. `CameraBounds` × `Ground-Extra-Dry` kutusu hâlâ işaretli; ikisi de hareketsiz olduğu için etkisi yok.

**Level2** (31 Ekim 2026'da kullanıcı ikinci bölümü yapmaya karar verdi, bkz. [kararlar](docs/kararlar.md))

- [ ] `Level2` sahnesi `Level1` kopyalanarak oluşturuldu, tile'lar temizlendi. **Başlanmadı.**
- [ ] `Level2` build listesinde `Level1`'in hemen altında (sıra: 0 `Level1`, 1 `Level2`)
- [ ] `Level2`'de Player başlangıç konumu, su kaynakları ve `CameraBounds` noktaları yeni bölüme göre ayarlandı (sonra **Invalidate Bounding Shape Cache**)
- [ ] `Level1` sonunda `Level2`'ye geçiş noktası. **Script yazılmadı.**
- [ ] Karar: `Level2`'ye su dolu mu başlıyor, yoksa `Level1`'den kalan su mu taşınıyor? (öneri: dolu başlasın)

## 4. Oyuncu

- [ ] T08 — Player prefab'ı: kökte Rigidbody 2D + Circle Collider 2D (su azaldıkça küçülür, bkz. [K03](docs/kararlar.md)), animasyonlu sprite `Visual` alt objesinde, layer `Player`. **Uygulandı; Level1'de test bekliyor.**
- [ ] T09 — Yürüme, zıplama ve yere basma kontrolü (`Ground-Dry`, `Ground-Wet`, `Ground-Extra-Dry`). **Uygulandı; Level1'de test bekliyor.**
- [ ] T09 — Rampada dururken aşağı kaymıyor (kodda eğim telafisi var). **Uygulandı; Level1'deki rampalarda test bekliyor.**
- [ ] T11 — Boşluğa düşünce yeniden deneme başlıyor (Fall Limit Y = -12). **Uygulandı; test bekliyor.**
- [ ] Prefab'a taşınacak ayarlar: `Extra Dry Ground Layer`, Interpolate ve `Slip` malzemesi şu an sadece `Level1`'de sahne override'ı. Prefab'a uygulanmazsa `Components` / `Level 5` sahnelerinde çok kuru zemin "yer" sayılmaz (Console'da uyarı çıkar).

## 5. Kamera ve arka plan (Cinemachine + parallax)

Cinemachine kamerayı oyuncunun peşinden götürür. Arka plan kameranın altında olduğu için ekran hep dolu kalır. Parallax da katmanları farklı hızlarda kaydırarak derinlik verir.

**Arka plan kurulumu**

- [x] `Background` objesi Main Camera'nın altında, Position `0, 0, 10`
- [x] `background_solid_sky` ve `background_fade_hills` silindi (kamera rengi gökyüzü dolgusunun yerini tutuyor)
- [x] `background_fade_desert` (0, -1, 0) ve `background_clouds` (0, 3, 0); Draw Mode Tiled, Width 34.25, Height 4 (en az 28 olması yeterli)
- [x] Main Camera'nın arka plan rengi gökyüzü rengi yapıldı (`C3E3FF`)

**Cinemachine (T10)**

- [x] Cinemachine 3.1.7 kuruldu
- [ ] Cinemachine kararı `docs/kararlar.md` dosyasına yazıldı
- [x] Main Camera'da Cinemachine Brain var (Update Method: Late Update)
- [x] `PlayerCamera` Player'ı takip ediyor: Orthographic Size 5, Damping 0.5 / 0.5, Lookahead 0.2 sn (Ignore Y)
- [x] Confiner 2D + `CameraBounds` Polygon Collider 2D: (-13, -4) – (55, 8). Bölüm büyüyünce noktaları güncelle ve **Invalidate Bounding Shape Cache**'e bas.
- [x] Sınır collider'ı Is Trigger ve `CameraBounds` layer'ında; collision matrix'te oyuncu ve zeminlerle çarpışmıyor
- [x] Player → Rigidbody 2D → Interpolate açık (`Level1` override'ı)
- [ ] T10 — Kamera oyuncuyu takip ediyor, iniş yerleri görünüyor, bölüm dışı görünmüyor. **Uygulandı; Game görünümünde test bekliyor.**

**Parallax**

- [x] `ParallaxLayer` script'i (`Assets/Scripts/Camera`) derlendi ve iki katmana eklendi: tepeler 0.7, bulutlar 0.8 (öneri 0.9)
- [ ] Arka planda titreme yok (konum, çizimden hemen önce hesaplanıyor). **Uygulandı; test bekliyor.**
- [ ] Game görünümünde kamera sağa-sola ve yukarı-aşağı giderken arka planda boşluk ya da kesik yok. **Test bekliyor.**

> Zaman sıkışırsa ilk ertelenecek iş parallax olur (ana belgedeki kapsam kesme sırası). Kamera takibi ve arka plan kurulumu korunur.

## 6. Su sistemi

Su değerinin tamamı `WaterMeter`'da; ayrıntılar [su belgesinde](docs/water-player.md).

- [ ] T12 — Su değerini tek bir bileşen (`WaterMeter`) yönetiyor; UI kendi sayacını tutmuyor. **Uygulandı; test bekliyor.**
- [ ] T13 — Su 0–100 arasında kalıyor (kodda sınırlandırılıyor). **Uygulandı; test bekliyor.**
- [ ] T14 — Durum önceliği: oyun aktif değilken tüketim yok, kaynakta dolum, havada normal, ısı/duman dışında yerde dururken sıfır, yerde hareket ederken veya etkin Slime Fire ısısı/HapsirikKoz dumanında dururken zemin ve ısı katsayılı. **Uygulandı; test bekliyor.**
- [ ] T15 — Kuru, nemli ve çok kuru zemin ayırt ediliyor; bilinmeyen zemin kuru (normal) sayılıyor. **Uygulandı; test bekliyor.**
- [ ] T16 — Tüketim kare hızından bağımsız (geçen süreye göre hesaplanıyor). **Uygulandı; test bekliyor.**
- [ ] T17 — Fizik titreşimi ve duvara doğru yürümek hareket sayılmıyor. **Uygulandı; test bekliyor.**
- [ ] T18 — Zıplama ve düşmede normal tüketim; zeminin katsayısı havaya taşınmıyor. **Uygulandı; test bekliyor.**
- [ ] T19 — Su çubuğu: `WaterBarUI` + `Assets/Prefabs/Canvas.prefab` (portre, siyah çerçeve, mavi dolgu) `Level1`'e yerleştirildi, Fill ve Portrait bağlı. Low Water Portrait henüz atanmadı (isteğe bağlı). **Uygulandı; test bekliyor.**
- [ ] T20 — Tüketim oranları (kuru 3, nemli 1, çok kuru 6) Inspector'da. **Eksik:** kapasite hâlâ kodda sabit (`Capacity = 100`).
- [ ] Çok kuru zemin (`Ground-Extra-Dry`, 6 birim/sn) ve Slime Fire ısı çarpanı birlikte çalışıyor (ör. 6 × 2 = 12 birim/sn). **Uygulandı; test bekliyor.**

## 7. Su birikintisi, yeniden doğma, final

- [ ] T21 — Su birikintisi prefab'ı (`WaterSource`: görsel + Water layer'ında trigger + `RespawnPoint`); `Level1`'de 2 adet. **Uygulandı; test bekliyor.**
- [ ] T22 — Birikintiye girince su tamamen doluyor. **Uygulandı; test bekliyor.**
- [ ] T23 — Birikinti checkpoint oluyor; ilk birikintiden önce bölüm başına dönülüyor. **Uygulandı; test bekliyor.**
- [ ] T24 — Birikintiye art arda girip çıkmak hata veya kilitlenme yaratmıyor. **Uygulandı; test bekliyor.**
- [ ] T25 — Su bitince tek seferlik ölüm başlıyor. **Uygulandı; test bekliyor.**
- [ ] T26 — Yeniden doğmada konum, hız, su, kontrol ve UI sıfırlanıyor. **Uygulandı (UI bar dahil); test bekliyor.**
- [ ] T27 — Final fidanı: kazanma mesajı çıkıyor, tüketim ve kontrol kapanıyor. **Başlanmadı.** Görseller hazır: `platformerPack/Sprites/Sapling/sapling_thirsty.png` (oyun boyunca) ve `sapling_green.png` (kazanınca, T54). 128×192 px, PPU 128, alt-orta pivot; nesne zemin yüzeyine konur. Fidan yalnızca son bölümde (`Level2`) olacak; `Level1` geçiş noktasıyla biter.
- [ ] T28 — Tekrar oyna bölümü baştan başlatıyor. **Başlanmadı.**
- [ ] T29 — Baştan sona ilk build alındı ve denendi. **Başlanmadı.**

**Bugünün hedefi:** 4.–7. bölümler bitince oyun çirkin olsa da başlanıp kaybedilip kazanılabiliyor olmalı. En büyük eksik: T27–T28 (oyun henüz kazanılamıyor).

## 8. Sonraki aşamalar

Ayrıntılar [genel-yapilacaklar.md](genel-yapilacaklar.md) içinde.

- [ ] T30–T38 — Bölüm tasarımı ve oynanış dengesi. Level1 ön incelemesi (hesap, test edilmedi): bölüm suya göre kısa, başlangıç suyuyla kaynaklara uğramadan bitirilebiliyor (T35); zeminin çoğu nemli olduğu için kuru/nemli farkı hissedilmiyor; iniş yerlerinin çoğu 1 tile genişliğinde (T32–T33).
- [ ] T39–T48 — Görsel karar, damla görseli, küçülme, su izi

31 Ekim 2026, T09/T45–T48 güncellemesi: kullanıcı suya bağlı collider küçülmesini onayladı ([K03](docs/kararlar.md)). Tek CircleCollider2D görselle birlikte küçülüyor; `Player → WaterMeter → Water Size → Minimum Scale` deneme değeri 0.45. Temas/şekil taraması ve eğime göre hareket uygulandı. Gerçek Player prefab'ının kopyasıyla ayrı Unity projesinde 47 Play Mode doğrulaması geçti; asıl Components/Level 5 sahnelerinde oynanış ve build kontrolü bekliyor. Bu sonuç tüm T39–T48 veya genel QA listesinin tamamlandığı anlamına gelmez.

31 Ekim 2026, düşük su hareket eki: kullanıcı talebiyle MoveAbility/JumpAbility Inspector'ına bağımsız yürüme, hava hızı ve kalkış hızı çarpanları eklendi (1.4/1.4/1.1). Aynı su değeri kullanılıyor; 1 katsayısı artışı kapatıyor. Dolu/yarım/az su ölçümleri, Inspector ayarı, havada dolum, yeniden doğma ve hızlı küçük oyuncuyla rampa regresyonu dahil 48 Play Mode doğrulaması ayrı Unity projesinde geçti. Asıl sahne/build oynanışı bekliyor; [kapsam](docs/player-state-machine.md#doğrulama).

- [ ] T49–T55 — Arayüz, ses, kaynak/lisans listesi, final sunumu
- [ ] QA01–QA20 — Son test listesi
- [ ] D01–D09 — Teslim listesi
