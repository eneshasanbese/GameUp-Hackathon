# Su Damlası — Yapılacaklar Takibi

**Son kontrol:** 1 Kasım 2026, 00:37 (diskteki dosyalara ve koda göre; `Level_1`/`Level_2`/`Level_3` sahne dosyaları okunarak incelendi, oynanmadı)
**Ana belge:** [genel-yapilacaklar.md](genel-yapilacaklar.md). T numaraları oradaki görevlerle aynıdır.
**Kural:** Kurulum maddeleri diskteki dosyalarla doğrulanınca işaretlenir. Oynanış maddeleri (T/QA) ancak asıl sahnede (`Level1`) oynanarak denendikten sonra işaretlenir. Kodu yazılmış ama oyunda denenmemiş maddeler **Uygulandı; test bekliyor** olarak not edilir. Kaydedilmemiş değişiklikler sayılmaz.

## 1. Proje kurulumu

31 Ekim 2026 HapsirikKoz duman HeatAura (T42/T43 ek kapsamı): ortak `HapsirikKoz_Smoke` prefabı Slime'ın nefes görselini kullanır. Alfa duman oluşurken 0→1 (0.25 sn), ömrü biterken 1→0 (son 0.5 sn) geçer; süreler Inspector'da. Geçiş nefes döngüsünde yeniden başlamaz. İki düşmanın mevcut duman bağlantıları yeterlidir; hasar ve hapşırma zamanlaması korunur. Ayrı Unity `6000.5.0f1` projesinde **35 kontrol geçti (9 asset, 26 Play Mode/render)**. Ana bölüm görünümü ve dış build testi bekliyor ([ayarlar ve kayıt](docs/enemies.md#hapsirikkoz-hapşırma-döngüsü)).

31 Ekim 2026 Slime Fire HeatAura (T42/T43 ek kapsamı): iki prefabın `HeatArea/HeatAura` çocuğuna mevcut 256 piksel set bağlandı; 24 kare/12 FPS/2 saniyelik nefes döngüsü Slime'ın arkasında oynar. Görsel, collider yarıçapı/offset'i değişince uyarlanır ve ısı kapatılınca gizlenir; hasar, fizik ve devriye/takip ayarları korunur. Özel/genel üreticiler efekti korur. Ayrı Unity `6000.5.0f1` projesinde **35 kontrol geçti (14 asset, 21 Play Mode/render)**. Ana bölüm görünümü ve dış build testi bekliyor ([ayarlar ve kayıt](docs/enemies.md#slime-fire-ısı-alanının-heataura-görseli)).

31 Ekim 2026 Saw hız/döngü düzeltmesi (T42 ek kapsamı): kullanıcının Samples 90 tercihi korundu; 16 görünür kare 12 değişim/sn ile yaklaşık 1.3333 sn döngüde oynatılıyor. Son sprite'ın önceki 20.833 ms süresi diğer kareler gibi 83.333 ms'ye eşitlendi; Unity'nin gerçek klip bitişi aynı son sprite tutma anahtarıyla sabitlendi. Özel/genel üretici düzeltmeyi korur. Ayrı Unity `6000.5.0f1` projesinde **18 kontrol geçti (8 asset, 10 Play Mode)**; eski kusur iki prefabda yeniden üretildi, düzeltme onar kesintisiz döngüyle ölçüldü. Asıl sahne hissi ve dış build testi bekliyor ([kayıt](docs/enemies.md#doğrulama)).

31 Ekim 2026 Saw/Slime Fire görsel geçişi (T42 ek kapsamı): dört prefab ve üç mevcut klip yeni `Saw 1/Unity_256` ve `FireSlime/Unity_256` görsellerine geçirildi. 256 PPU, sıkıştırmasız/Bilinear import; kare sayıları artarken döngü süreleri korundu. Saw merkez hizası ve Slime ayak pivot'u uyarlandı, Slime'ın eski Y animasyonu kaldırıldı; collider, ısı, temas hasarı, devriye/takip ve asset kimlikleri korundu. Ayrı Unity `6000.5.0f1` projesinde **64 kontrol geçti (40 asset, 24 Play Mode/görüntü)**; test edilmiş prefab/klipler ana projeye aktarıldı. Ana bölüm görünümü ve dış build testi bekliyor ([ayarlar ve kayıt](docs/enemies.md#görseller-ve-yeniden-üretim)).

31 Ekim 2026 HapsirikKoz görsel geçişi (T42 ek kapsamı): kullanıcının talebiyle duran/devriye prefabları ve mevcut Idle/Sneeze klipleri `HapsirikKoz 1/Unity_256` sheet'lerine bağlandı; üretici yolu güncellendi. 256×256 kare, 256 PPU, Bilinear, mipmap kapalı ve Compression None; mevcut zamanlama, duman, collider, devriye ve takip ayarları korundu. Bu geçişte ayrı Unity `6000.5.0f1` projesinde **34 kontrol geçti (19 asset, 15 Play Mode/görüntü)**; iki gerçek prefabın Idle/Sneeze render'ı ve 05. karede tek duman çıkışı doğrulandı. Asıl bölüm görünümü ve dış build testi bekliyor ([ayarlar ve kayıt](docs/enemies.md#görseller-ve-yeniden-üretim)).

31 Ekim 2026 hareketli düşman takip alanı (kullanıcı ek kapsamı): Saw, Slime Fire ve HapsirikKoz devriye prefablarına ayarlanabilir dış algılama çemberi eklendi. Oyuncu içerideyken takip, çıkışta ayrıldığı noktaya dönüş ve aynı devriyeyi sürdürme uygulanır; duran türlere takip eklenmez. Ayrı Unity `6000.5.0f1` projesinde ilk uygulamada **52 kontrol geçti (9 asset, 43 Play Mode)**; giriş/çıkış, takip eksenleri, rotaya dönüş, yaşam döngüsü ve algılama/ısı/gövde hasarı ayrımı doğrulandı. Ana Level1 oynanışı ve dış build testi bekliyor ([ayarlar ve test kaydı](docs/enemies.md#hareketli-düşmanların-takip-alanı)).

31 Ekim 2026 düşman yerçekimi/duvar düzeltmesi (T09 ek kapsamı): Saw devriyesi yerçekimsiz uçuşunu koruyarak zemin/duvara çarpar. SlimeFire/HapsirikKoz duran ve devriye türleri Dynamic/yerçekimli çalışır; motor yalnızca yatay hızı değiştirir, uçurumdan düşüş ve rotaya dönüşte düşey hız korunur. Altı prefabda ayrı TerrainBody yalnızca üç zemin layer'ıyla çarpışır; hasar trigger'ları/ısı/duman korunur. Köz Ocağı ve duman sabit çevresel rollerinde kaldı. Ayrı Unity `6000.5.0f1` projesinde bu oturumda **72 fizik kontrolü (19 asset, 53 Play Mode) ve 52 takip regresyon kontrolü (9 asset, 43 Play Mode) geçti**; doğrulanmış altı prefab GUID'leri korunarak aktarıldı. Ana bölümde elle oynanış/denge ve dış build bekliyor ([fizik ayarları ve test kayıtları](docs/enemies.md#yerçekimi-ve-zeminduvar-çarpışması)).

31 Ekim 2026 görünmez düşman sınırı (T09/T30 ek kapsamı): kullanıcının Edge önerisiyle `GroundEnemyBoundary.prefab` ve **Create Ground Enemy Boundary** menüsü eklendi. Trigger olmayan, Scene'de noktaları düzenlenebilir Edge çizgisi yalnızca SlimeFire/HapsirikKoz'un GroundEnemy fizik gövdelerini durdurur; oyuncu ve Saw geçer. EnemyBoundary/GroundEnemy layer'ları boş slotlara eklendi; mevcut layer numaraları ve kamera sınırı korunur. Ayrı Unity `6000.5.0f1` projesinde bu oturumda **94 kontrol geçti (27 asset, 67 Play Mode)**: iki yönde uçurumda durma, hedef dönünce ayrılma, sınır kapanınca düşme, gerçek Player ve uçan Saw'ın engellenmemesi; önceki zemin/hasar senaryoları da tekrar geçti. Doğrulanan prefab/katmanlar ana projeye aktarıldı. Ana bölümde sınır yerleşimi, elle oynanış ve dış build bekliyor ([kullanım](docs/enemies.md#yerdeki-düşmanlar-için-görünmez-sınır), [sonuç](docs/verification/enemy-boundary-result.txt)).

31 Ekim 2026 ek kapsam: Saw ve Slime Fire için duran/git-gel prefabları ve Slime yakınlık erime çarpanı uygulandı. Unity 6000.5.0f1 ile ayrı geçici projede 20 asset ve 31 Play Mode kontrolü geçti; ayarlar ve doğrulama kaydı [düşman belgesinde](docs/enemies.md). Asıl sahnelere yerleşim ve build testi yapılmadı.

31 Ekim 2026 Saw animasyon düzeltmesi: 10 FPS ve döngü sonundaki tekrarlı ilk kare yerine 24 FPS, eşit aralıklı sekiz kare kullanıldı. Aynı bağımsız Unity projesinde 4 asset ve 10 Play Mode kontrolü geçti; iki Saw prefabında beşer döngü örneklendi. Asıl sahnede görüntü/build kontrolü bekliyor ([kayıt](docs/enemies.md)).

31 Ekim 2026 ısı hasarı düzeltmesi (T14/QA01): kullanıcının yeni kararıyla etkin Slime Fire ısısında yerde durmak da su kaybettiriyor; ısı dışında durmak ücretsiz. Ayrı Unity projesinde gerçek prefablarla 27 Play Mode kontrolü geçti: kuru/nemli duruş ve yürüyüş tüketimi, alan çıkışı/kapatma, çarpan 1, örtüşme, farklı fizik adımları ve ısıdan ölüm/yeniden doğma. Asıl sahne/build testi bekliyor ([kayıt](docs/enemies.md)).

- [x] Düşmanlarla doğrudan temas: sonraki kullanıcı talebiyle Player KnockbackAbility ve altı gövde trigger'ı bağlandı; 13 asset ve 36 Play Mode kontrolü ayrı Unity projesinde geçti. Asıl sahnede his/denge ve build testi bekliyor ([kayıt](docs/player-state-machine.md#knockback-ve-temas-hasarı)).

31 Ekim 2026 knockback: ayrı ability/state, düşmandan uzağa itiş, kısa kontrol kilidi/kübik toparlanma, Inspector'dan ayarlanabilir temas su kaybı (10) ve 2 saniye death_01 karesiyle flash eklendi. Flash yeni gövde darbelerini engeller; ısı/duman tüketimi sürer. Player ve altı düşman prefabı bağlıdır; mevcut sahne değişiklikleri korundu. Ayrı Unity 6000.5.0f1 projesinde 49 kontrol geçti; doğrulanan asset'ler gerçek meta/GUID'lerle projeye aktarıldı. Ana sahne hissi ve build testi bekliyor ([ayarlar ve testler](docs/player-state-machine.md#knockback-ve-temas-hasarı)).

31 Ekim 2026 HapsirikKoz ek kapsamı: duran/devriye prefabları, Idle/Sneeze animasyonları ve kullanıcının istediği rastgele 1–5 saniye bekleme → tam hapşırma → rastgele 2–3 saniye cooldown döngüsü eklendi. Her tur cooldown sonrası yeni bekleme seçilir; aralıklar Inspector'dan değiştirilebilir. Ayrı Unity 6000.5.0f1 projesinde 18 asset ve 25 Play Mode kontrolü geçti; test edilmiş asset'ler gerçek meta dosyalarıyla projeye aktarıldı. Asıl sahnede yerleştirme/görüntü ve build testi bekliyor ([ayarlar ve test kaydı](docs/enemies.md)).

31 Ekim 2026 HapsirikKoz duman hasarı (T14/QA01): yeni kullanıcı talebiyle hapşırmanın çıkış karesinde birkaç saniye kalan hasar alanı eklendi. Varsayılan süre 3 saniye, yarıçap 1.75, su tüketim çarpanı 2; ayarlar duman prefab'ının Inspector'ında. Duman, Slime Fire'ın `EnemyHeatArea`/tek su sayacı yolunu kullanır; dururken de hasar verir, çıktığı noktada kalır ve süresi dolunca çarpanı temizler. Ayrı Unity projesinde 10 asset ve 30 Play Mode kontrolü geçti; kuru/nemli/çok kuru/hava tüketimi, giriş/çıkış, ömür, Slime/duman örtüşmesi, kapatma/silme, çoklu collider, kontrol kapalıyken koruma, ölüm/yeniden doğma ve devriye emisyonu doğrulandı. Test edilmiş prefablar projeye aktarıldı. Duman görseli/animasyonu sonraya bırakıldı; asıl sahne ve build testi bekliyor ([kayıt](docs/enemies.md)).

31 Ekim 2026 Köz Ocağı (T09/T14–T16 ek kapsamı): sabit yatay çekim, bağımsız 1–6 su puanı/saniye çevresel kayıp, kullanılabilir prefab ve ayrı demo sahnesi eklendi. Mevcut küçülme/hız/zıplama korunur. Ayrı Unity 6000.5.0f1 projesinde 9 asset ve 45 Play Mode kontrolü geçti; iki yönden gerçek kaçış, zıplama, tüketim ayrımı, çoklu collider/kaynak, yaşam döngüsü ve demo görünürlüğü doğrulandı. Ana bölüm yerleşimi/görüntü ve dış build bekliyor ([ayarlar ve kayıt](docs/koz-ocagi.md)).

1 Kasım 2026 düşen taş tuzağı (T30/T42 ek kapsamı): `Trap_stoneSpike.prefab` oyuncu yaklaşınca tek kez düşecek, `Level / Order -1` ile zeminin arkasından geçecek ve düşüşten 3 saniye sonra silinecek şekilde bağlandı. Kullanıcının onayladığı temas hasarı mevcut Player knockback/su/flash yolunu kullanır. Ayrı Unity `6000.5.0f1` projesinde bu oturumda **41 kontrol geçti (9 asset, 32 Play Mode)**; algılama filtreleri, iki yön, üç zemin, tek darbe, dokunulmazlık ve tek ölüm/yeniden doğma dahil. Doğrulanan prefab/script/meta dosyaları ana projeye hash karşılaştırmasıyla aktarıldı. Level_2 dosyası ve mevcut kullanıcı değişiklikleri korundu; ana sahnede elle oynanış/görüntü ve dış build bekliyor. [Ayarlar ve test kayıtları](docs/stone-spike-trap.md).

31 Ekim 2026 Köz Ocağı sürekli HeatAura (T42/T43): SlimeFire_Patrol'ın mevcut 24 kare/12 FPS/2 saniyelik ısı animasyonu `EffectArea/HeatAura` çocuğunda paylaşılıyor. Oyuncu uzaktayken de döner; mevcut 6 birimlik etki alanını izler, alfa 0.35 ile rüzgârın arkasında görünür. Mevcut su/çekim değerleri ve SlimeFire prefabı korundu. Ayrı Unity 6000.5.0f1 projesinde bu oturumda **58 kontrol geçti (17 asset, 41 Play Mode/render)**; rüzgâr regresyonları ve SlimeFire kaynak kapatma/açma dahil. Ana bölüm/build testi bekliyor. [Ayarlar ve kanıtlar](docs/koz-ocagi.md#sürekli-heataura--31-ekim-2026).

31 Ekim 2026 Köz Ocağı SuctionWind (T42/T43): hazır 256 piksel set prefab'a bağlandı. Hafif Idle; gerçek çekimde normal/güçlü döngü; oyuncunun tarafından ağıza yatay akış; çıkışta 0.25 saniyelik sönme. Önceden ayarlanmış 6/1 menzil ve 1–7.5 çekim korundu. Ayrı Unity 6000.5.0f1 projesinde **37 kontrol geçti (11 asset, 26 Play Mode/render)**; iki kamera çıktısı incelendi. Ana bölüm görünümü/oynanışı ve dış build testi bekliyor. [Ayarlar ve kanıtlar](docs/koz-ocagi.md#suctionwind--31-ekim-2026).

31 Ekim 2026 Köz Püskürten Idle/yerçekimi (T42 ek kapsamı): kullanıcının son talebiyle yalnızca tek kare gösteren prefab'a hazır 12 kare/12 FPS Idle Animator bağlandı. Dynamic Rigidbody2D Gravity Scale=1; X/dönüş kilidi ve diğer yer düşmanları gibi yalnızca zemin/sınırla çarpışan TerrainBody eklendi. Atış ayarları, prefab GUID/fileID'leri ve gövdenin hasarsız olması korundu. Ayrı Unity 6000.5.0f1 projesinde **80 kontrol geçti (14 asset, 66 Play Mode)**; gerçek animasyon döngüsü, üç zemine düşme/oturma, desteğin kalkması, oyuncuyu itmeme ve atış regresyonları dahil. Ana bölüm/build testi bekliyor. [Ayarlar ve yeni sonuç](docs/ember-spitter.md#idle-ve-yerçekimi-güncellemesinin-doğrulaması).

31 Ekim 2026 Köz Püskürten (T12/T19/T25–T26, T42; QA06–QA09/QA16): açık kullanıcı talebiyle `EmberSpitter` ve `EmberFireball` prefab'ları ile ayrı `EmberSpitterDemo` sahnesi oluşturuldu. Sabit atıcı görüş/menzil ve 0.8 saniye uyarı sonrası tek düz mermi atar; mermi mevcut flash koruması üzerinden 10 sabit su puanı düşürür. Gövde hasarı yoktur. Güncel gerçek kod/prefab/input ve iki PNG ile ayrı Unity 6000.5.0f1 projesinde **65 kontrol geçti (12 asset, 53 Play Mode)**; gerçek demo HUD/R dönüşü ve 3 birimden normal zıplamayla kaçış dahil. Ana bölüm oynanışı ve build testi bekliyor; genel kutular kapatılmadı. [Ayarlar/dosyalar/kapsam](docs/ember-spitter.md), [sonuç](docs/verification/ember-spitter-result.txt), [kontroller](docs/verification/ember-spitter-checks.txt).

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
  - 31 Ekim 2026: `Ground-Extra-Dry/Extra Dry.prefab` alfabetik dizilimden `Ground-Dry`/`Ground-Wet` ile aynı yerleşime alındı (3×3 blok, dikey sütun, tek blok, yatay + yuvarlak uçlar, bulutlar, rampalar). Aynı 28 taş tile'ı, GUID'ler korundu. **Unity palet görünümü kontrol bekliyor.**
  - Not: `Ground-Extra-Dry` klasöründe 9 dekor tile'ı da var (`dry_bush`, `stone_pile` vb.; `Decor-Desert`'tekilerden ayrı dosyalar, collider tipi Sprite). `Decor` paletinde ve `Level_1/2/3` sahnelerinde kullanılıyorlar; silinmemeli. Yalnızca `Decor-Front`'a boyanmalı; istenirse Unity Project penceresinde `Decor` klasörüne taşınabilir.
- [ ] K02 kararı `docs/kararlar.md` dosyasına yazıldı: Kenney Platformer Pack, `Double` klasörü, PPU 128. Ana belgedeki T42 ve T44 maddeleri (18 PPU, Point filtre, Pixel Perfect Camera) bu paket için geçerli değil.
- [ ] Nemli zeminin görseli kesinleşti (şu an çim; "son fidan" hikâyesiyle çelişebilir, bkz. QA19)
- [ ] `Decor-Desert` paleti: Kenney stilinde 9 cansız dekor tile'ı (kum: `dry_grass`, `dry_bush`, `tumbleweed`, `dry_cactus`, `sand_pebbles`; taş: `stone_pebbles`, `stone_pile`, `stone_spike`, `stone_dry_shrub`). PPU 128, collider yok, `Decor-Front`'a boyanır. **Dosyalar eklendi; Unity içe aktarımı ve palet görünümü kontrol bekliyor.**
- [ ] QA19 — `Level1`'deki yeşil `cactus` dekorları yaşayan bitki gibi duruyor; istenirse `dry_cactus` ile değiştirilecek.
- [ ] Mağara paletleri (31 Ekim 2026): Kenney kum/çimen tile'larının yeniden boyanmasıyla aynı şekil ve çarpışma, `Ground-Dry` ile aynı yerleşim. **Dosyalar eklendi; Unity içe aktarımı ve palet görünümü kontrol bekliyor.**
  - `Cave-Rock` (28, gri-mor dalgalı kaya, collider Sprite) → `Ground-Dry`'a boyanır
  - `Cave-Wet` (28, mavi zikzak üst bant, collider Sprite) → `Ground-Wet`'e boyanır; zikzak kenar dışarıdaki çimenle aynı "nemli" işaretidir
  - `Cave-Soil` (28, kahverengi zikzak toprak, collider Sprite) → final ışığının altındaki toprak; `Ground-Wet` önerilir
  - `Cave-Decor` (9, collider yok) → `Decor-Front`: sarkıt ×2, dikit ×2, mağara taşları, zemin/küçük/tavan kristali, kuru kök. Tavandan sarkanlar tavanın hemen altındaki kareye boyanır.
- [ ] Mağara sprite'ları `platformerPack/Sprites/Cave/`: `Backgrounds/background_cave_far` ve `background_cave_near` (512×1280 = 4×10 birim, yatayda dikişsiz), `Light/glow_cyan`, `glow_warm`, `light_shaft` (3×8 birim, pivot üst-orta). Unity'de bağlanmadı.
  - Arka plan: mağara sahnesinde `Background` altındaki iki SpriteRenderer'ın sprite'ı bunlarla değiştirilir, Draw Mode Tiled, Size (34.25, 10), Y = 0; Main Camera arka plan rengi #2D2643.
  - Işık (URP 2D, tilemap'ler `Sprite-Lit-Default` kullanıyor): `Global Light 2D` yoğunluğu ≈ 0.35–0.45; kristallere camgöbeği Point Light 2D; oyuncuya küçük yumuşak Point Light 2D (yoksa damla karanlıkta seçilmiyor); finalde hüzme sprite'ı `Sprite-Unlit-Default` ile + sıcak Point/Spot Light 2D. Değerler önizleme hesabıdır, oyunda denenmedi.
- [ ] Karar kaydı: `docs/kararlar.md` K04 iki bölüm diyor; diskte `Level_3` (mağara) var ve kullanıcı final fidanı mağaranın sonuna koymayı planlıyor. Bölüm sayısı ve finalin hangi sahnede olduğu K04'e yazılmalı.

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

- [x] `Level2` sahnesi oluşturuldu: diskte `Scenes/Level_2.unity` (sahne `Level1` de `Level_1` olarak yeniden adlandırıldı). 31 Ekim 2026, 18:49 kaydında bölüm x≈53'e kadar çizili: başlangıç ve üç kum bulutu (iki `HapsirikKoz`), üç taş bulut (`Ground-Extra-Dry`, üç `Saw_Patrol`), kum blok + `HapsirikKoz`, `KozOcagi_Stationary`, alt mağarada birikinti ve sağda 5 birimlik duvar. Dekor `Decor-Desert` ile yapıldı. Oynanarak denenmedi.
- [x] `Level_2` build listesinde `Level_1`'in hemen altında (sıra: 0 `Level_1`, 1 `Level_2`); diskte doğrulandı.
- [ ] `Level2`'de Player başlangıç konumu, su kaynakları ve `CameraBounds` noktaları yeni bölüme göre ayarlandı (sonra **Invalidate Bounding Shape Cache**). Player ayarlı (−10.8, −0.3). Tek `WaterSource` (50, −3) mağarada; başlangıçtan oraya kadar ≈60 birim boyunca checkpoint yok. `CameraBounds` hâlâ `Level1` değerlerinde (x −13…55); sol duvar (x −14) görüş dışında.
- [ ] **Mağaradaki birikinti = bölüm sonu** (20:03 kullanıcı kararı: bölüm sonu su birikintisidir). Mağaradan çıkış olmaması bu durumda sorun değil; birikintiye ulaşınca bölüm biter. 1 Kasım: `Level_2` artık son bölüm değil; bu birikintiye `LevelExit` (Next Scene Name `Level_3`) eklenecek, `Level_3` build listesine 3. sırada girecek. **Kullanıcı kuruyor; diskte henüz yok.**
- [ ] `jumpForce` 7 artık iki sahnede de sahne override'ı (prefab'da hâlâ 10). Alana sağ tıklayıp **Apply to Prefab** ile prefab'a taşınmalı; `Level_1` bu değerle oynanarak denenmeli.
- [ ] Bulut düşmanları: 4 karelik bulutlarda 3 birim devriye + 1,75 duman; iniş yeri neredeyse hep dolu, itilince çukura düşülüyor. Taş bulutlarda da aynı durum: x 22'deki testere 3 karelik bulutun dışına taşıyor, x 28–34'teki iki testere iç içe geçiyor. Devriye mesafesini kısalt, düşmanı sabitle ya da bulutu genişlet.
- [ ] Final: x≈53'te 5 birimlik duvar var; bölüm sonu mağaradaki birikinti (`Level_3`'e geçiş). Fidan artık `Level_3`'te.
- [ ] Küçük: x 34–35 arasında 1 karelik delik (küçük damla düşebilir). Birikintinin altında (49–50, −4) hem kum hem çimen tile'ı üst üste duruyor; kum olanları sil.
- [ ] Zorluk geçişi: `Level_2`'de 9 düşman var (4 `HapsirikKoz`, 3 `Saw_Patrol`, 1 `KozOcagi`). 1 Kasım: arkadaş `Level_1`'e 6 düşman yerleştirdi (`KozOcagi_Stationary`, `HapsirikKoz_Stationary`, `EmberSpitter`, `Saw_Stationary`, `SlimeFire_Patrol`, `SlimeFire_Stationary`); düşmanlar artık Level_1'de tanıtılıyor. Zorluk eğrisi oynanarak denenmeli.
- [ ] Nemli zemin `Level_2`'de yalnızca 2 tile (mağaradaki birikintinin altında); bölümün geri kalanı kuru ve çok kuru. Uzun yürüme yerlerine (başlangıç zemini, x 35–41 blok) birkaç nemli parça dinlenme alanı olabilir.
- [ ] Küçük: (53, −1)'deki `ground_dry_floor_up_short_base` eğim tile'ı duvarın içinde kalmış; köşesinde ufak bir çentik görünür. Normal `block_center` ile değiştir.
- [ ] `Level_2`'nin üç zemin tilemap'ine `GroundSurface` ekle (`Level_1`'deki gibi: `Ground-Dry` → Sand, `Ground-Wet` → Grass, `Ground-Extra-Dry` → BlackStone). Şu an malzeme çarpanlarının hepsi 1 olduğu için fark yok; çarpanlar değişirse `Level_2` zemini çarpansız kalır.
- [ ] `Level_1` sonunda `Level_2`'ye geçiş noktası. Script yazıldı: `Assets/Scripts/Level/LevelExit.cs` (su varken tek seferlik geçiş, yeni bölüm dolu suyla başlar). Bölüm sonu su birikintisi olduğu için bileşen `Level_1`'in son `WaterSource` örneğine (x≈44) eklenir; prefab'a uygulanmaz. Kullanıcı sahneye ekleyip geçişi denedi ve geçişte donma fark etti. Diskte doğrulandı: `Level_1`'de birikintiye `LevelExit` (Next Scene Name `Level_2`) eklenmiş, build listesi `Level_1`, `Level_2` (kaydedilmiş, commit edilmemiş). 20:18: kararma/açılma geçişi eklendi (`SceneFader.cs`, varsayılan 0.5 sn siyah; sahne kurulumu gerekmez). **Fade geçişi uygulandı; Unity'de test bekliyor.** 1 Kasım merge: arkadaşın `1f49bbc` commit'i bu `LevelExit`'i yanlışlıkla silmişti; merge'de (58, 8)'deki birikintiye geri eklendi (Next Scene Name `Level_2`, 0.5 sn siyah). Unity'de birikintide bileşenin göründüğü kontrol edilmeli.
- [ ] Karar: `Level2`'ye su dolu mu başlıyor, yoksa `Level1`'den kalan su mu taşınıyor? Şu an `LevelExit` ile dolu başlıyor (öneri buydu); taşınması istenirse ek kod gerekir.

**Level 3 — mağara (`Scenes/Level_3.unity` yapım aşamasında, commit edilmedi, build listesinde yok)**

- [ ] Lav: anında ölüm (kullanıcı kararı, [kararlar](docs/kararlar.md)). Script yazıldı: `Assets/Scripts/Level/KillZone.cs`, değince su sıfırlanır ve mevcut ölüm/yeniden doğma akışı başlar. 1 Kasım 2026 diskte doğrulandı: `Hazard` tilemap (layer Default, Tilemap Collider 2D Is Trigger, Composite yok, `KillZone` açık, `Sprite-Unlit-Default`), sol altta lav havuzu (x −12…7, y −29…−26), yüzeyde `LavaAnimation` Animated Tile (Collider Type Sprite), kaya ile üst üste binme yok. **Kurulum doğru; oynanarak test bekliyor.**
- [ ] **Düşme sınırı:** `Level_3` Player'ında `WaterMeter → Fall Limit` hâlâ −12, bölüm −30'a kadar iniyor. Damla y −12'nin altına indiği anda ölüyor; alt mağara, `WaterSource_3` (52, −23) ve lav ulaşılamaz. Bu sahnede Fall Limit −35 yapılmalı (prefab'a uygulanmamalı; Level_1/2 çukurları −12'ye göre).
- [ ] `Level_3`'te Player → `Visual` üzerinde sahnede eklenmiş ikinci bir Animator ve `PlayerAnimation` var (prefab'da zaten var; `Level_1` kopyasından gelmiş). `+` işaretli olanları **Remove Component** ile sil; arkadaş `Level_1`'de aynısını temizledi.
- [ ] `Level_3` mağarasının tamamı `Ground-Extra-Dry` tilemap'inde (taş): her yerde çok kuru tüketim (yürürken 6, dururken 2 birim/sn). Bilerek mi, karar bekliyor. `Ground-Dry`/`Ground-Wet` boş. `GroundSurface` bileşenleri de yok (`Level_2`'den kopyalandığı için).
- [ ] Plan (kod yok): karanlık mağara (Global Light 2D kısık), damlaya ve birikintilere ışık, ısı alanlı meşaleler (`torch_on_a/b` + Spot Light 2D + mevcut `EnemyHeatArea`), mağara ortasında ışık hüzmesi altında susuz fidan (final). 1 Kasım: oyun üç bölüm oldu ([K04](docs/kararlar.md)); final fidanı `Level_3`'te.
## 4. Oyuncu

- [ ] T08 — Player prefab'ı: kökte Rigidbody 2D + Circle Collider 2D (su azaldıkça küçülür, bkz. [K03](docs/kararlar.md)), animasyonlu sprite `Visual` alt objesinde, layer `Player`. **Uygulandı; Level1'de test bekliyor.**
- [ ] T09 — Yürüme, zıplama ve yere basma kontrolü (`Ground-Dry`, `Ground-Wet`, `Ground-Extra-Dry`). **Uygulandı; Level1'de test bekliyor.**
- [ ] T09 — 31 Ekim yön çevirme: yatay girdiye göre `Player.Flip()` ile `SpriteRenderer.flipX` güncellenir; yerde/havada çalışır, durunca son yönü korur, Knockback kilidinde yön değişmez. Yeni Inspector bağlantısı yoktur. Ayrı Unity 6000.5.0f1 projesinde gerçek Player prefab'ıyla 19 Play Mode kontrolü geçti; asıl sahne görüntüsü/build testi bekliyor ([kayıt](docs/player-state-machine.md#doğrulama)).
- [ ] T09 — Rampada dururken aşağı kaymıyor (kodda eğim telafisi var). **Uygulandı; Level1'deki rampalarda test bekliyor.**
- [ ] T11 — Boşluğa düşünce yeniden deneme başlıyor (Fall Limit Y = -12). **Uygulandı; test bekliyor.**
- [ ] Prefab'a taşınacak ayarlar: Interpolate ve `Slip` malzemesi şu an sadece `Level1`'de sahne override'ı. `Extra Dry Ground Layer`, son bekleme tüketimi güncellemesinde prefab'a bağlandı (`1024`); çok kuru desteğin sahne override'ına bağımlılığı kaldırıldı.

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
- [ ] T14 — Durum önceliği: oyun aktif değilken tüketim yok, kaynakta dolum, havada normal; yerde bekleme ve yürüme için ayrı pozitif kuruluk oranları × malzeme × ısı çarpanı. **Uygulandı; Level1 oynanış testi bekliyor.**
- [ ] T15 — Kuru, nemli ve çok kuru zemin ayırt ediliyor; kum/çimen/siyah taş malzemesi bağımsız `GroundSurface` ile seçiliyor. **Uygulandı; Level1 oynanış testi bekliyor.**
- [ ] T16 — Tüketim kare hızından bağımsız (geçen süreye göre hesaplanıyor). **Uygulandı; test bekliyor.**
- [ ] T17 — Fizik titreşimi ve duvara doğru yürümek hareket sayılmıyor. **Uygulandı; test bekliyor.**
- [ ] T18 — Zıplama ve düşmede normal tüketim; zeminin katsayısı havaya taşınmıyor. **Uygulandı; test bekliyor.**
- [ ] T19 — Su çubuğu: `WaterBarUI` + `Assets/Prefabs/Canvas.prefab` (portre, siyah çerçeve, mavi dolgu) `Level1`'e yerleştirildi, Fill ve Portrait bağlı. Low Water Portrait henüz atanmadı (isteğe bağlı). **Uygulandı; test bekliyor.**
- [ ] T20 — Bekleme/yürüme oranları (nemli 0.25/1, kuru 0.5/3, çok kuru 2/6) ve üç bağımsız malzeme çarpanı (başlangıçta 1×) Inspector'da. **Eksik:** kapasite hâlâ kodda sabit (`Capacity = 100`).
- [ ] Çok kuru zemin ve Slime Fire ısı çarpanı birlikte çalışıyor (nötr malzemede 2× ısı ile bekleme 4, yürüme 12 birim/sn). **Uygulandı; Level1 oynanış testi bekliyor.**

31 Ekim 2026 son bekleme tüketimi talebi (T14–T18/T20, QA01–QA05/QA07–QA10/QA16): eski ücretsiz duruş kuralı kaldırıldı. Gerçek Player prefab'ı ve güncel kodla ayrı Unity 6000.5.0f1 projesinde **79 kontrol geçti**; istenen altılı sıra, 9 malzeme/kuruluk birleşiminin bekleme-yürümesi, duvar/eğim, dikey zıplama/düşüş, ısı örtüşmesi/temizliği, kontrol/fizik kapanışı, iki fizik adımı, kaynakta tek Heal ve sıradan beklemeden ölüm/yeniden doğma doğrulandı. Level1'in üç zemin tilemap'ine mevcut görsellerine göre Sand/Grass/BlackStone işaretleri eklendi; kuruluk layer'ları bağımsız kaldı. [Sonuç](docs/verification/water-drain-result.txt), [kontroller](docs/verification/water-drain-checks.txt), [ayarlar ve kapsam](docs/water-player.md). Asıl Level1 oynanışı ve build testi bekliyor; önceki ısı dışı sıfır tüketim kayıtları tarihsel sonuçlardır.

## 7. Su birikintisi, yeniden doğma, final

- [ ] T21 — Su birikintisi prefab'ı (`WaterSource`: görsel + Water layer'ında trigger + `RespawnPoint`); `Level1`'de 2 adet. **Uygulandı; test bekliyor.**
- [ ] T22 — Birikintiye girince su tamamen doluyor. **Uygulandı; test bekliyor.**
- [ ] T23 — Birikinti checkpoint oluyor; ilk birikintiden önce bölüm başına dönülüyor. **Uygulandı; test bekliyor.**
- [ ] T24 — Birikintiye art arda girip çıkmak hata veya kilitlenme yaratmıyor. **Uygulandı; test bekliyor.**
- [ ] T25 — Su bitince tek seferlik ölüm başlıyor. **Uygulandı; test bekliyor.**
- [ ] T26 — Yeniden doğmada konum, hız, su, kontrol ve UI sıfırlanıyor. **Uygulandı (UI bar dahil); test bekliyor.**
- [ ] T27 — Final fidanı: kazanma mesajı çıkıyor, tüketim ve kontrol kapanıyor. **Başlanmadı.** Görseller hazır: `platformerPack/Sprites/Sapling/sapling_thirsty.png` (oyun boyunca) ve `sapling_green.png` (kazanınca, T54). 128×192 px, PPU 128, alt-orta pivot; nesne zemin yüzeyine konur. Fidan yalnızca son bölümde (`Level_3`, mağara) olacak; `Level_1` ve `Level_2` su birikintisiyle (`LevelExit`) biter.
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
  - Ses durumu (31 Ekim 2026): projede `platformerPack/Sounds` altında 10 Kenney efekti var (CC0, `platformerPack/License.txt`); hiçbir sahne/prefab'da `AudioSource` yok. Efekt bağlanacak hazır olaylar: `WaterMeter.RefilledFromSource`, `WaterMeter.Depleted`.
  - Müzik adayı (T53 kaydı için): "Chill Chiptunes (Collection)", Holizna, CC0, https://opengameart.org/content/chill-chiptunes-collection, `chill_chiptunes.zip` 75,8 MB. **Henüz indirilmedi/projeye eklenmedi.** Projeye yalnızca seçilen parçalar konacak; indirme tarihi ve parça adları eklenince yazılmalı.
- [ ] QA01–QA20 — Son test listesi
- [ ] D01–D09 — Teslim listesi
