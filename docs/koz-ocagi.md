# Köz Ocağı

31 Ekim 2026 kullanıcı talebi; T09/T14–T16 ve ilgili QA01–QA09/QA16 ek kapsamı. Mevcut suya bağlı küçülme, hız ve zıplama, kullanıcının netleştirmesiyle korunur.

## Prefab ve Inspector

1 Kasım 2026 nefes sesi ve mesafe (T52): `KozOcagi_Stationary.prefab` kökündeki AudioSource, kullanıcının seçtiği `Assets/Audio/Sounds/u_j7nsvy18lj-inhale-nose-408293.mp3` klibini çalar. **Play On Awake** ve **Loop** açık; **Volume 0.7**, **Spatial Blend 1 (3D)**, **Volume Rolloff Linear**, **Min Distance 5.4**, **Max Distance 13**, **Doppler Level 0**. Unity, AudioListener'a uzaklığa göre sesi azaltır ve kaynağın yönüne göre sağ/sol konumlandırır; ek script veya event bağlantısı gerekmez. Aktif kaynak sahne başlangıcından itibaren döner; menzile girince klip yeniden başlatılmaz. Ayarlar prefab kökündeki **AudioSource** component'indedir.

Level_1/2/3'te AudioListener ana kameradadır ve kamera Z = −5'tedir. Köz Ocağı Z = 0 iken 5.4–13 gerçek 3D mesafe, kameraya XY düzleminde yaklaşık 2–12 birim aralığına karşılık gelir. 5.4'e kadar tam ses, 5.4–13 arasında doğrusal azalma, 13 ve ötesinde sessizlik. Bu mesafe oyuncuya doğrudan değil kameradaki dinleyiciye göredir; kamera sınırları da işitsel konumu etkiler. Prefab'ın klip/mesafe bağlantıları kaynak dosyada kontrol edildi; uygulandı, editör/build testi bekliyor.

Önceki ses adayının kaynağı/lisansı (T53): [Inhale.wav — green3y3guy](https://freesound.org/people/green3y3guy/sounds/420855/), **CC0 1.0**, kaynak sayfasında 1 Kasım 2026 doğrulandı. Herkese açık [HQ MP3 önizlemesi](https://cdn.freesound.org/previews/420/420855_3665858-hq.mp3) aynı tarihte indirilerek döngü hazırlanmıştı; özgün WAV indirilmiş gibi sunulmaz. Kaynak sayfası kaydı erkek içe nefes çekişi olarak etiketler. Bu aday artık prefab'a bağlı değildir; bu kayıt yeni seçilen MP3'ün lisans doğrulaması sayılmaz.

Önceki `koz_ocagi_inhale_loop.wav` adayının düzenlemesi: giriş/çıkış sessizlikleri yerine 1.32–2.42 saniye arasındaki nefes bölümünden 1 saniyelik parçalar, 0.5 saniye aralık ve sinüs penceresiyle dairesel olarak örtüştürülerek 8 saniyelik sürekli doku oluşturuldu. 160 Hz high-pass, 350 Hz −4 dB ve 2600 Hz +3 dB EQ, 10 kHz low-pass uygulandı; ortalama seviye −27 dBFS, tepe −13.58 dBFS. Baş/son sıfır geçişine alındı; PCM dosyada sınır sıçraması 0, sınır komşusu adımı en çok 71/32768. Bunlar önceki adayın dosya ölçümleridir; mevcut MP3 veya mesafe davranışının oynanış testi değildir.

Eklenen kaynaklar: `Assets/Scripts/Enemies/EmberSuctionEnemy.cs`, `EmberSuctionDemoControls.cs`, `Assets/Editor/Enemies/EmberSuctionPrefabBuilder.cs` ve `EmberSuctionVerification.cs`; prefab/sahne ve tüm yeni meta dosyaları. Mevcut kodda yalnızca `Player.cs` ve `PhysicsControl.cs` değişti; `WaterMeter` mevcut API'siyle kullanıldı. Ortak yönergeler, kararlar, plan/takip ve ilgili teknik belgeler güncellendi.

Unity köküne göre `Assets/Prefabs/Enemies/KozOcagi_Stationary.prefab` sahneye sürüklenebilir. Ayak hizası köktür; `Visual`, `RangeOrigin`, `PullPoint` ve `EffectArea` ayrıdır. Kök Rigidbody2D veya katı gövde taşımaz. 1 Kasım 2026 son kullanıcı talebiyle köke Player'a özel **BoxCollider2D trigger** ve mevcut **EnemyContactKnockback** bağlandı: Size `(1.1, 0.95)`, Offset `(0, 0.475)`. Bu küçük gövde alanına temas, Player'daki Contact Water Loss (10), Force `(6, 5)` ve Flash Duration (2 saniye) ayarlarını kullanır. Geniş EffectArea yalnızca önceki emiş ve sürekli su kaybını yürütür; flash sırasında da bu tüketim sürer. Ek Inspector bağlantısı gerekmez.

`EffectArea` üzerindeki `EmberSuctionEnemy` referansları hazır bağlıdır:

| Alan | Güncel prefab ayarı |
|---|---|
| Effect Radius / Inner Full Effect Radius | 6 / 1 dünya birimi |
| Min / Max Water Loss Per Second | 1 / 6 su puanı/saniye |
| Min / Max Pull Strength | 1 / 7.5 dünya birimi/saniye yatay motor hız katkısı |
| Activation Ramp Duration | 0.35 saniye; yalnızca çekimin giriş rampası |
| Horizontal Dead Zone | 0.08 dünya birimi; ağız çevresinde sıfır çekim |
| Player Layer Mask | Player |
| RangeOrigin / PullPoint | Köke göre `(0, 0.5)` / `(0, 0.38)` |

Sayılar deneme ayarıdır. Rüzgâr eklenmeden önce değiştirilmiş menzil/çekim değerleri korunmuştur. Tam prefab üreticisi ilk prototipin 3/0.75 menzil ve 0.4–2.5 çekim varsayılanlarını kullanır; yalnızca rüzgârı yenilemek için aşağıdaki ayrı menüyü kullanın. Yarıçap sırası, negatif ve sonlu olmayan değerler doğrulanır. İlgili min/max çiftini 0 yapmak o etkiyi kapatır. Dünya yarıçapı transform ölçeği değişse de korunur. Scene Gizmos dış/iç menzili ve ağzı gösterir.

Menzil `RangeOrigin` ile oyuncunun `Rigidbody2D.worldCenterOfMass` noktası arasındaki **2D mesafedir**. Merkez dışarıdaysa collider kenarı trigger'a değse bile etki yoktur. Yakınlık dış sınırdan iç yarıçapa doğrusal 0–1 artar. Her fizik adımında bir kez `WaterMeter.Consume(Lerp(min, max, yakınlık) * Time.fixedDeltaTime)` çağrılır. Bu **mutlak puandır**, kalan su yüzdesi veya normal tüketim çarpanı değildir. 6 puan/saniye yarım saniyede yaklaşık 3 puandır. Dururken ve gövde darbesi flash'ında da sürer. Çekim nedeniyle gerçek hareket oluşursa normal zemin tüketimi ayrıca devam eder.

## Hareket ve yaşam döngüsü

Unity `6000.5.0f1`; mevcut motor `PhysicsControl.ApproachHorizontalSpeed` üzerinden `linearVelocity` yazar. Kaynak başına yatay **hedef hız katkısı**, normal Idle/Move/Jump hedefiyle toplanır; mevcut ivme toplam hedefe yaklaştırır. Ayrı script'ler hızı ezmez, kuvvet/kütle birimleri karışmaz ve sınırsız ivmelenme olmaz. Çıkış yalnızca ilgili kaydı kaldırır; tüm hız sıfırlanmaz, mevcut motor yavaşlaması çalışır.

Yön yalnızca ağızla oyuncu merkezi arasındaki X farkıdır. Ölü bölgede sıfır, hemen dışında kısa yumuşama vardır. Düşman Y hızına veya yerçekimine yazmaz; mevcut eğim motoru yüzey takibini sürdürür. Mevcut Knockback kontrol kilidi önceliklidir; su kaybı o sırada da sürer, normal motor kontrolü dönünce çekim hareketle birleşir. Su azalması mevcut boyut/hız/zıplama ayarlarını doğal olarak etkileyebilir; düşman bunlara doğrudan yazmaz.

Trigger'ın gerçek örtüşmeleri fizik adımlarında sorgulanır. Player layer'ı ve Rigidbody üzerindeki `Player`/`PhysicsControl`/`WaterMeter` birlikte aranır. Trigger oyuncu collider'ları filtrelenir. Rigidbody başına tek kayıt etkiyi çoğaltmaz; son collider çıkana kadar etki korunur. Merkez mesafesi/etkinlik kontrolleri teleport, collider/kontrol/WaterMeter/fizik kapanışı ve respawn'ı izler. Düşman kapanışı/silinmesi ve sahne boşaltılması da kaydı temizler. Event aboneliği yoktur.

Birden çok Köz Ocağı su kayıplarını ve yatay katkılarını toplar; biri kalkınca diğeri kalır. Demo tek düşmanlıdır; çoklu yerleşimde toplam çekim ayrıca dengelenmelidir. Kazanma sistemi henüz yoktur; mevcut sözleşmeye göre Player kapatılınca tüketim durur ve motor kayıtları temizlenir. `Time.timeScale = 0` fizik adımlarını/tüketimi durdurur.

## Görsel ve demo

Gövde için verilen `KozOcagi/Unity_128/Idle.png` sheet'inin `Idle_00` sprite'ı kullanılır. Tek kare 128×128; alfa >10 içerik sınırı üstten ölçümle `(11,24)–(116,108)`, 9.443 piksel şeffaf/çok saydamdır. Mevcut PPU 128 ve ayak pivotu `(0.5, 0.15833333)` korunur. Yaklaşık 0.83 birim görünür genişlik, 1 birim çaplı Player ile karşılaştırılıp yalnızca `Visual` 1.5 ölçeklendi. Gövdenin kaynak importları/GUID'leri değiştirilmedi.

### Sürekli HeatAura — 31 Ekim 2026

Kullanıcı SlimeFire_Patrol'daki ısı animasyonunu Köz Ocağı'nda da istedi (T42/T43). `EffectArea/HeatAura`, SlimeFire'ın mevcut sprite'ını ve `SlimeFireHeatAura` controller/klibini paylaşır: 24 kare, 12 FPS, 2 saniyelik sürekli döngü. Oyuncu menzil dışında da oynar. Geniş 6 birimlik alanda çevre/rüzgâr okunabilsin diye SpriteRenderer alfası 0.35'tir; `Level` sorting order -1 ile rüzgârın (0) ve gövdenin (1) arkasındadır.

Ortak `EnemyHeatAuraVisual`, `EnemyHeatArea` yanında mevcut `EmberSuctionEnemy` kaynağını da destekler. Köz Ocağı'ndaki kaynak/collider referansları hazırdır; görsel merkez ve çapını mevcut çekim collider'ından alır. Kaynak veya trigger kapanınca gizlenir. Yeni ısı çarpanı, collider veya hasar yolu eklenmedi; ocağın bağımsız su kaybı ve çekimi korunur. SlimeFire'ın eski `EnemyHeatArea` yolu aynı davranışı sürdürür.

Ek Inspector/sahne bağlantısı gerekmez. Yalnızca bu görseli yenilemek için **GameUp → Enemies → Rebuild Koz Ocagi Heat Aura** menüsü kullanılabilir; mevcut menzil/çekim/su/rüzgâr ayarlarını korur. Tam prefab üreticisi de artık ısı görselini bağlar. SlimeFire prefabı ve paylaşılan klip/controller/sheet dosyaları değiştirilmez.

Bu ekleme bu oturumda, açık ana editörden ayrı `C:/Users/yhcan/AppData/Local/Temp/gameup-suction-heat-0jiiaf01` projesinde Unity **6000.5.0f1 ile 58 kontrolden geçti (17 asset, 41 Play Mode/render)**. Gerçek güncel Player/kod/prefab/input ve paylaşılan SlimeFire asset'leri kullanıldı; rüzgâr regresyonları da yeniden koşuldu. Isının oyuncu uzaktayken döngüsü, collider merkez/çap/ölçek uyumu, kaynak/trigger kapatma-açma, SlimeFire'ın eski yolunun korunması, ısı görünümü kapalıyken aynı su kaybı ve ısı+rüzgârın birlikte görünümü doğrulandı. Test edilen prefab aktarıldı; SlimeFire kaynak prefabının değişmediği hash ile kontrol edildi.

Test girişi: ayrı batch projede `-executeMethod EmberSuctionWindVerification.GenerateHeatAuraAndRun`. [Sonuç](verification/koz-ocagi-heat-aura-result.txt), [kontroller](verification/koz-ocagi-heat-aura-checks.txt), [sürekli ısı alanı](verification/koz-ocagi-heat-aura.png), [ısı ve rüzgâr birlikte](verification/koz-ocagi-heat-and-wind.png). Kamera çıktıları 1280×720; testte Player materyali yalnızca test nesnesinde ışıksız yapıldı. Logda Unity SearchDatabase başlangıç indeksleme hatası vardı; C# derleme hatası veya başarısız son kontrol yoktu. Ana bölüm okunabilirliği/oynanışı ve dış build testi bekliyor; genel QA kutuları kapatılmadı.

### SuctionWind — 31 Ekim 2026

Kullanıcının hazır verdiği `KozOcagi/SuctionWind/Unity_256` seti, `OKU.txt` doğrultusunda eklendi (T42/T43). Üç sheet'in 24'er karesi 256 PPU, ortak `(0.14, 0.5)` ağız pivotu, Full Rect, Bilinear, mipmap kapalı ve sıkıştırmasız kullanılır. Sprite kimlikleri korundu; diğer boyutlar ve master görseller değiştirilmedi.

`SuctionWind` çocuğunun SpriteRenderer/Animator ve `EmberSuctionWindVisual` bağlantıları hazırdır. Kaynak mevcut `EffectArea/EmberSuctionEnemy` bileşenidir; yeni algılama veya hasar yolu yoktur. Efekt `Level` katmanında gövdenin bir sıra arkasındadır.

- Menzil dışında küçük `IdleWind`: 24 kare / 12 FPS / 2 saniye, ölçek 1.5, alfa 0.35.
- Gerçek çekim hedefinde `PullWind`: 24 kare / 16 FPS / 1.5 saniye. Yakınlık 0.7'de `PullWindStrong` başlar (24 kare / 24 FPS / 1 saniye); dönüş eşiği 0.6'dır. Klip geçişi fazı korur. Yakınlık hızı 0.85–1.15, alfayı 0.5–0.9 aralığında değiştirir; giriş mevcut çekim rampasını izler.
- Rüzgâr oyuncunun tarafından ağıza doğru akar. Ortak pivotta 0°/180° dönüş ve `.758` kaynak-hedef referansıyla yatay uzunluk ayarlanır. Ağızdaki yatay ölü bölgede Idle görünür.
- Çıkışta mevcut döngü/faz 0.25 saniyede alfa ile söner, ardından küçük Idle'a döner. `FadeOut` sheet'i faz sıçramasını önlemek için kullanılmaz. Kaynak/trigger kapanışı efekti gizler; Player/fizik kapanışı, teleport ve respawn eski aktif hedefi bırakır. Pause animasyon/geçişi dondurur.

Inspector: `Idle Scale`, `Idle Alpha`, `Wind Width`, `Fade Duration`. Ek sahne bağlantısı gerekmez. **GameUp → Enemies → Rebuild Koz Ocagi Suction Wind** görseli, üç klibi/controller'ı ve importlarını yenilerken mevcut su/çekim/menzil ayarlarını korur. Tam prefab üreticisi de efekti ekler.

Bu oturumda açık editörden ayrı `C:/Users/yhcan/AppData/Local/Temp/gameup-suction-wind-44c58db9` projesinde Unity **6000.5.0f1 ile 37 kontrol geçti (11 asset, 26 Play Mode/render)**. Güncel gerçek Player/kod/prefab/input ve üç sheet kullanıldı; test edilen asset'ler aynı GUID'lerle aktarıldı. Yeniden koşma: ayrı batch projede `-executeMethod EmberSuctionWindVerification.GenerateAndRun`. [Sonuç](verification/koz-ocagi-suction-wind-result.txt), [kontroller](verification/koz-ocagi-suction-wind-checks.txt), [sağ normal akış](verification/koz-ocagi-suction-wind-right.png), [sol güçlü akış](verification/koz-ocagi-suction-wind-left-strong.png).

Kapsam: mevcut ayar/GUID'lerin korunması, tam klip süreleri ve döngü, sağ/sol/ağız/ölçek hizası, yakınlık, sönme/geri giriş, ölü bölge, kapatma-açma, pause ve görsel kapalıyken aynı su kaybı. İki 1280×720 çıktı incelendi; yalnızca testte Player'ın materyali ışıksız yapıldı. Logda Unity SearchDatabase başlangıç indeksleme hatası vardır; C# derleme hatası veya başarısız davranış kontrolü yoktur. Ana bölüm görünümü/oynanışı ve dış build testi bekliyor; genel QA kutuları kapatılmadı.

`Assets/Scenes/KozOcagiDemo.unity`: düz kuru zemin, menzil dışında başlayan Player, tek düşman ve mevcut `Canvas`/`WaterBarUI`. A/D veya oklar yürütür, Space zıplatır; **R** veya ekrandaki düğme başlangıca tam suyla döndürür. Ana bölüm sahneleri, Player prefab ayarları ve build sahne listesi korunur.

Yeniden üretme menüleri: **GameUp → Enemies → Rebuild Koz Ocagi Prefab** ve **Rebuild Koz Ocagi Demo**. Yalnızca bu iki asset'i varsayılanlara yazar; hazır prefab/sahneyi kullanmak için çalıştırmak gerekmez.

## Doğrulama

1 Kasım 2026 gövde teması güncellemesi: güncel Player/Köz Ocağı/Ember prefab'ları ve runtime dosyaları ayrı Unity 6000.5.0f1 projesinde test edildi; **47 kontrol geçti (9 asset, 38 Play Mode)**. Gövdeye sağ/sol temas ve öldüren darbe, tek su kaybı/itiş, flash ve toparlanma, sürekli su kaybının flash sırasında devam etmesi ve geniş etki alanının gövde darbesi üretmemesi doğrulandı. Prefab hash'i test kopyasıyla aynıdır. [Kapsam ve sonuç](player-state-machine.md#doğrulama). Önceki sonuçlar kendi tarihlerindeki temas hasarı olmayan kapsamı gösterir; ana bölümde elle oynanış ve dış build bekler.

Unity 6000.5.0f1 ile açık ana editörden ayrı `C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073` projesinde **54 kontrol geçti: 9 asset/bağlantı ve 45 Play Mode**. Güncel kod, gerçek Player/Canvas prefabları ve kaynak sprite kullanıldı. Kontroller `Assets/Editor/Enemies/EmberSuctionVerification.cs` içindedir; yalnızca ayrı batch projede `-executeMethod EmberSuctionVerification.Run` ile çalışır. Üretimle birlikte `.GenerateAndRun` kullanılabilir. Sonuç test projesindeki `ember-suction-result.txt`, ayrıntılar `ember-suction-verification.log` dosyasındadır. Kalıcı kopyalar: [sonuç](verification/koz-ocagi-result.txt), [kontrol listesi](verification/koz-ocagi-checks.txt).

Kapsam: dışarıda sıfır etki; mesafeyle artan durağan tüketim; farklı başlangıç suyunda eşit puan kaybı; ısı/flash ayrımı; çekim rampası; **en güçlü bölgeden iki yönde gerçek input ile kaçış**; merkezde titreşimsizlik; çoklu collider/kaynak; kapatma, teleport, sahne boşaltma, tek ölüm/respawn temizliği; kuru/nemli normal tüketim; zıplama yüksekliğinin korunması; 30/120 hedef render FPS ve 0.01/0.04 fizik adımlarında aynı 6 puan/saniye; dünya yarıçapı/parametre doğrulaması; gerçek demo sahnesinin güvenli başlangıcı, hasarı ve R ile dönüşü.

Asıl bölümde elle yerleştirme, görsel okunabilirlik/oynanış dengesi ve dış build doğrulaması bekliyor. Genel QA kutuları bu alt sistem testiyle kapatılmaz.

Logda önceki test kayıtlarında da bulunan bağımsız `UnityEditor.Search.SearchDatabase` başlangıç indeksleme hatası vardır. C# derleme hatası veya başarısız davranış kontrolü yoktur. Test edilmiş prefab ve sahne, Unity'nin ürettiği meta/GUID'lerle ana projeye aktarılmıştır.

Demo Player'ına yalnızca sahnede ışıksız sprite materyali bağlandı; Player prefab'ı ve projenin URP ayarları değiştirilmedi. Ayrı test projesinin [960×540 kamera çıktısı](verification/koz-ocagi-demo.png) incelendi: Player, ocak ve zemin görünürdür. Bu kamera çıktısı ekran üstü Canvas/OnGUI arayüzünü içermez; ana projenin URP görüntüsü veya dış build testi değildir.
