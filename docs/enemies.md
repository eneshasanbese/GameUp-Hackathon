# Saw, Slime Fire, HapsirikKoz ve Köz Ocağı prefabları

31 Ekim 2026 ek kapsamı: `KozOcagi_Stationary.prefab` ve `KozOcagiDemo.unity` sabit yatay çekim ve bağımsız puan/saniye su kaybı için eklendi. Köz Ocağı temas hasarı/knockback taşımaz. [Inspector, hareket entegrasyonu ve 54 kontrolün kaydı](koz-ocagi.md).

31 Ekim 2026 kullanıcı talebiyle iki düşmanın duran ve iki nokta arasında git-gel yapan türleri eklendi. Doğrudan temas başlangıçta knockback eklenene kadar TODO'ydu; kullanıcının sonraki talebiyle altı düşman gövdesine temas hasarı ve knockback bağlandı. Hasar gövdeleri trigger'dır; oyuncuya fiziksel engel oluşturmaz. Sonraki zemin/duvar düzeltmesinde ayrı `TerrainBody` collider'ları eklendi.

Aynı gün kullanıcı HapsirikKoz'u da istedi. Duran ve devriye türleri, mevcut `EnemyPatrol` ve `Visual`/Animator düzeniyle eklendi. Hapşırma zamanlaması kullanıcının devam mesajına göre rastgele bekleme ve ayrı cooldown kullanır.

## Yerleştirme ve ayarlar

Unity proje köküne göre `Assets/Prefabs/Enemies/` altında altı hazır prefab bulunur:

| Prefab | Davranış |
|---|---|
| `Saw_Stationary.prefab` | Yerinde kalan, sürekli dönen bıçak |
| `Saw_Patrol.prefab` | Sürekli dönen, iki nokta arasında hareket eden bıçak |
| `SlimeFire_Stationary.prefab` | Idle animasyonu ve yakınlıkta erime çarpanı |
| `SlimeFire_Patrol.prefab` | Walk animasyonu, git-gel ve yakınlıkta erime çarpanı |
| `HapsirikKoz_Stationary.prefab` | Idle, rastgele Sneeze, geçici hasarlı duman ve cooldown |
| `HapsirikKoz_Patrol.prefab` | Aynı hapşırma/duman döngüsüyle git-gel ve hareket yönüne dönüş |

Prefab'ı istenen sahneye sürüklemek yeterlidir. Saw kökü bıçağın merkezi, Slime ve HapsirikKoz kökü ayak seviyesidir. Slime/HapsirikKoz'un duran ve devriye türleri `Dynamic`, `Gravity Scale = 1` kullanır; havada bırakılınca ve kenardan çıkınca düşerler. Saw devriye türü `Dynamic`, `Gravity Scale = 0` ile uçar; sabit Saw kinematik kalır. Zemin/duvar çarpışması aşağıda açıklanmıştır.

HapsirikKoz kökü de ayak seviyesindedir. 31 Ekim 2026 görsel geçişiyle `HapsirikKoz 1/Unity_256` sheet'leri kullanılır: kareler 256×256, PPU 256 ve ortak normalize pivot `(0.5, 0.07407407)` (yaklaşık `(128, 18.963)` piksel). `Visual` yerel konumu sıfırdır. Gövde trigger'ının boyutu `(0.64, 0.7)`, offset'i `(0, 0.35)` deneme ayarıdır. Devriye sırasında Idle/Sneeze oynatılır; pakette ayrı yürüyüş animasyonu yoktur.

Hareketli türlerde **EnemyPatrol → End Offset**, yerleştirilen başlangıca göre dünya birimi cinsinden diğer uçtur; varsayılan `(3, 0)`. **Speed** varsayılan `1.5` birim/saniyedir; devriyede, takipte ve rotaya dönüşte kullanılır. Saw negatif, dikey ve çapraz offset kullanabilir. Yerdeki Slime/HapsirikKoz yalnızca X offset'ini kullanır; Y hareketi yerçekimi/çarpışmadan gelir. Sıfır hız veya sıfır etkili offset motoru ve takibi durdurur, yerçekimini durdurmaz. Seçili prefab'ın iki ucu Scene görünümünde sarı çizilir. Slime hareket yönüne döner; Saw'ın yüzü dik kalırken dişleri sheet animasyonuyla döner.

## Slime Fire ısı alanının HeatAura görseli

31 Ekim 2026 kullanıcı talebi (T42/T43 ek kapsamı): iki SlimeFire prefabında `HeatArea/HeatAura` çocuğu, mevcut `FireSlime/HeatAura/256/Heat_Sheet.png` setini oynatır. 24 kare, 12 FPS, 2 saniyelik döngü; ilk kare sonda yinelenmez. Sheet sabit 256×256 hücrelere, merkez pivot'a ve 256 PPU'ya ayarlanmıştır; Bilinear, mipmap kapalı ve sıkıştırmasızdır. Mevcut sheet GUID'i ve dilimlerin sprite kimlikleri korunur; alternatif 128/1080 dosyaları değiştirilmez.

`EnemyHeatAuraVisual`, görseli `HeatArea` collider'ının offset'ine hizalar ve yarıçap değişince ölçeği günceller; Editor ve Play Mode'da çalışır. Referans karedeki görünür çap 202/256 dünya birimidir; şeffaf kenar boşluğu hesaba katılır. Varsayılan 1.75 yarıçap için görsel ölçeği yaklaşık 4.436'dır. Karelerdeki hafif nefes hareketi yalnızca görseldir; hasar yarıçapı sabit kalır. `EnemyHeatArea`, circle collider veya trigger kapatılırsa aura gizlenir; yeniden açılınca geri gelir.

SpriteRenderer beyaz/alpha 1 ve alpha-blended unlit `Sprites/Default` materyalindedir; `Level` sorting layer, Order 0 kullanır. Slime gövdesi Order 1 ile önde kalır. Görsel düşmanla birlikte taşınır; gövdenin yön çevirmesinden bağımsızdır. Gövde/zemin collider'ları, ısı çarpanı, temas hasarı ve devriye/takip ayarları korunur. Prefablar hazır bağlıdır; ek Inspector veya sahne bağlantısı gerekmez.

Yalnızca bu efekti yeniden kurmak için **GameUp → Enemies → Rebuild Slime Fire Heat Aura** kullanılabilir (`SlimeFireHeatAuraBuilder.cs`). Bu menü iki prefabın aura çocuğunu ve ortak efekt klibi/controller'ını günceller; diğer düşmanları veya Slime hareket/hasar ayarlarını sıfırlamaz. Genel **Rebuild Enemy Prefabs** üreticisi de HeatAura'yı bağlar; genel menünün mevcut ayarları varsayılana yazma davranışı sürer.

Doğrulama: ayrı Unity `6000.5.0f1` projesinde 35 kontrol geçti (14 asset, 21 Play Mode/render). Sabit grid/sıra, iki saniyelik döngü, iki prefabda üç döngü boyunca 24 kare, gerçek zamanda bağımsız gövde/aura animasyonu, yarıçap/offset/ölçek/konum uyumu, kapatma/açma ve collider/hasar ayarlarının korunması kontrol edildi. [Sonuç](verification/heat-aura-result.txt), [kontroller](verification/heat-aura-checks.txt), [960×540 render önizlemesi](verification/heat-aura-preview.png). Ana bölümde görünüm/oynanış ve dış build testi bekliyor.

## Hareketli düşmanların takip alanı

31 Ekim 2026 kullanıcı talebiyle `Saw_Patrol`, `SlimeFire_Patrol` ve `HapsirikKoz_Patrol`, oyuncu dış algılama çemberine girdiğinde devriyeyi kesip onu takip eder. Oyuncu tamamen çemberden çıkınca düşman ayrıldığı rota noktasına döner, önceki devriye yönüyle aynı iki uç arasında devam eder. Dönüş sırasında oyuncu yeniden alana girerse takip hemen başlar; özgün dönüş noktası korunur.

- **EnemyPatrol → Player Chase → Detection Radius:** Varsayılan `4` dünya birimi. Çemberin merkezi düşmanın kök konumudur ve düşmanla birlikte hareket eder. `0` takibi kapatır. Scene görünümünde seçili düşmanın alanı camgöbeği, takipte kırmızı görünür; **Gizmos** açık olmalıdır.
- **Flying:** Saw'da açık; motor iki eksende çalışır. Slime/HapsirikKoz'da kapalı; motor yalnızca yatay hızı belirler, düşüş hızını korur. Prefab üreticisi buna uygun Rigidbody ayarlarını kaydeder.
- **Horizontal Chase Only:** Saw'da kapalı; iki eksende oyuncuya yaklaşır. Uçan türde açılırsa yalnızca yatay takip eder. Yerdeki türler her zaman yalnızca yatay takip eder; oyuncu zıplarken yukarı taşınmaz, desteksiz kalınca düşer.

Algılama, her fizik adımında gerçek oyuncu collider'larının çemberle örtüşmesini sorgular; yeni bir hasar/trigger collider'ı eklenmez. Oyuncunun trigger sensörleri ve Player component'i taşımayan nesneler takip başlatmaz. Oyuncu kapanınca, ölürken/yeniden doğarken, collider'ı kapanınca/silinince veya uzağa ışınlanınca takip bırakılır. Çoklu oyuncu collider'larında kalan fiziksel örtüşme yeterlidir. Takip alanı Slime ısısından ve gövde temasından bağımsızdır; kendisi su kaybı veya knockback üretmez.

Üç prefab hazır ayarlıdır; sahnede oyuncu referansı, ek layer veya collider bağlantısı gerekmez. Prefab üreticisi Saw'ın iki eksenli ayarını da korur. Duran türlerde ve Köz Ocağı'nda takip yoktur. Duvarın arkasındaki oyuncu algılanabilir; düşman fiziksel olarak duvarda durur. Yol bulma/zıplama yoktur; engelin etrafından dolaşmayı denemez.

## Yerçekimi ve zemin/duvar çarpışması

31 Ekim 2026 kullanıcı bildirimi: takipte duvar/zemin içinden geçme ve yerdeki düşmanların havada kalması. Üç devriye türünde `MovePosition` yerine `linearVelocity` kullanılır. Yerdeki türlerin Y hızı her fizik adımında korunur; takip bitince eski yüksekliğe uçmak yerine yalnızca rota X konumuna dönerler. Motor kapanınca yatay hız kesilir, düşüş sürer. [Unity MovePosition belgesi](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Rigidbody2D.MovePosition.html), bu metodun taşıma adımında yerçekimini uygulamadığını açıklar.

Altı gövde prefabında `TerrainBody` çocuğu vardır. **Is Trigger kapalıdır**; `Ground-Dry`, `Ground-Wet`, `Ground-Extra-Dry` katmanlarıyla çarpışır. Slime/HapsirikKoz'un fizik çocuğu `GroundEnemy` katmanındadır ve ayrıca `EnemyBoundary` ile çarpışır; Saw bu görünmez sınırdan geçer. `Include Layers` izin verilen katmanları, `Exclude Layers` diğerlerini seçer; `Layer Override Priority = 1`. Oyuncu, diğer düşmanlar, ısı/duman alanları bu fizik kutusuyla engellenmez. Mevcut kök hasar trigger'ı ve bütün ısı/duman bağlantıları korunur. Yerdeki kutu ayak hizasına kadar uzanır; hasar kutusunun kısaltılması görseli zemine gömmez. Mevcut sürtünmesiz `Slip` materyali duvarda asılı kalmayı önler; hareketli gövdelerde Continuous çarpışma kullanılır.

Slime/HapsirikKoz'un duran türlerinde yalnızca yatay konum ve dönüş kilitlidir; düşebilirler. Köz Ocağı sabit çevresel kaynak, duman ise çıktığı yerde kalan geçici alan olarak korunur. Uçurumdan düşen yürüyen düşman eski platforma tırmanmaz; güvenli devriye uçlarını sahnede ayarla. Ayrı düşman ölüm/respawn veya yol bulma sistemi eklenmedi.

Mevcut prefab ayarlarını, animasyonları ve GUID'leri koruyarak yeniden uygulamak için **GameUp → Enemies → Configure Terrain Physics** kullanılır. Normal prefab üreticileri de aynı fizik ayarlarını uygular. Sahnedeki zemin/duvar collider'ları trigger olmamalı ve üç zemin katmanından birinde bulunmalıdır. Prefab instance'larında eski Rigidbody/collider override'ı varsa Inspector'dan kontrol et; sahneler bu düzeltmede yeniden yazılmadı.

## Yerdeki düşmanlar için görünmez sınır

31 Ekim 2026 kullanıcı talebiyle, takip sırasında uçurumdan düşebilen SlimeFire/HapsirikKoz için `Assets/Prefabs/Enemies/GroundEnemyBoundary.prefab` eklendi. Görseli ve runtime script'i yoktur; düzenlenebilir `EdgeCollider2D` fiziksel bir duvardır. Kamera sınırı mevcut sahnelerde PolygonCollider2D/Cinemachine ile çalışır; bu yeni nesne ayrı bir fizik sınırıdır.

Sahnede kullanım:

1. **GameUp → Enemies → Create Ground Enemy Boundary** seç veya prefab'ı Hierarchy'ye sürükle.
2. Nesneyi uçurumun güvenli tarafına, düşmanın duracağı kenara yerleştir. Sınırı bölüm hiyerarşisinde sabit bir nesne olarak tut.
3. **Edge Collider 2D → Edit Collider** ile çizgiyi kenarda dikey çiz. Alt ucu zeminin altında, üst ucu düşmanın başından yukarıda kalsın. Varsayılan yerel noktalar `(0, -1)` ve `(0, 2)`; kalınlık yarıçapı `0.02` dünya birimidir. Birden fazla noktayla çizgi şekillendirilebilir.
4. Platformun diğer ucunu da kapatmak için nesneyi çoğaltıp taşı. **Is Trigger kapalı** kalmalıdır. Nesne Game görünümünde görünmez; çizgiyi Scene görünümünde düzenlersin.

Prefab `EnemyBoundary` layer'ını kullanır. Collider yalnızca `GroundEnemy` katmanını dahil eder, diğer tüm katmanları dışlar ve önceliği `2`'dir. Bu nedenle oyuncu ve Saw içinden geçer; SlimeFire/HapsirikKoz'un `TerrainBody` collider'ı durur. Hasar/ısı alanları sınırdan etkilenmez. Yeni iki layer boş kullanıcı slotlarına eklenir; mevcut layer numaraları ve global collision matrix korunur. [Unity layer override önceliği](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Collider2D-layerOverridePriority.html).

Takip hedefi sınırın ötesindeyken düşman çizgide kalır; oyuncu geri gelince ondan uzaklaşabilir, algılama alanından çıkınca mevcut rotaya dönüşü sürdürür. Normal devriye uçlarını sınırların içinde tut. Sınır kapatılırsa veya yerleştirilmezse yerçekimi ve uçurumdan düşüş devam eder. Yeni sınırları sahnede hangi uçuruma koyacağın bölüm tasarımına bağlıdır; bu değişiklik ana sahnelere otomatik yerleşim yapmaz.

**Configure Ground Enemy Boundary** menüsü prefab ve dört yerdeki gövde türünün çarpışma ayarlarını yeniler; var olan sınır prefabının çizgi noktalarını korur. **Rebuild Enemy Prefabs** de yeni layer/mask ayarlarını korur. Kamera sınırının layer'ını bu amaçla değiştirmek gerekmez.

## Slime Fire ısı alanı

Slime'ın `HeatArea` çocuğunda:

- **EnemyHeatArea → Drain Multiplier:** Varsayılan `2`; `1` etkiyi kapatır.
- **CircleCollider2D → Radius:** Varsayılan `1.75`; offset `(0, 0.4)`.
- **Is Trigger:** Açık tutulmalı. Kök gövde collider'ı, yakınlık alanından ayrıdır.

Isı alanı ve Player prefab bağlantısı korunur. Oyuncunun gerçek Rigidbody2D'si üzerindeki mevcut `Player` ve `WaterMeter` bulunur; düşman alanı Water layer'ına taşınmamalıdır. Özel collision matrix değişikliği yapılırsa Default ve Player arasındaki trigger etkileşimi korunmalıdır.

## HapsirikKoz hapşırma döngüsü

Kökteki `HapsirikKozSneeze`, `Visual` Animator'ına bağlıdır; prefab'larda bu bağlantı hazırdır.

1 Kasım 2026 ses eki (T52): `Assets/Scripts/Audio/SoundOnAnimationEvent.cs`, Animator ile aynı nesneye eklenen genel bir Animation Event alıcısıdır. İki HapsirikKoz prefab'ının `Visual` nesnesine **Sound On Animation Event** eklenir; ortak `HapsirikKozSneeze.anim` klibinde istenen kareye event konur, **Function → PlaySound**, **Object/AudioClip → freesound_community-sneeze-98347.mp3** seçilir. Klip event üzerinden verilir; **Volume** ve **Hearing Distance** component'ten ayarlanır. Mevcut `Sfx.PlayAt` yolunu kullanır ve yalnızca Play Mode'da, component etkinken ses çalar. Prefab ve event bağlantıları kullanıcıya bırakılmıştır; ses dosyasının başlangıcındaki olası hazırlık/sessizlik için event zamanı dinlenerek ayarlanmalıdır. Component ve `Sfx.cs`, kurulu Unity projesinin referanslarına karşı C# derleme kontrolünden geçti. Uygulandı; editör/build testi bekliyor.

- **Sneeze Delay Range:** Varsayılan `(1, 5)` saniye. Her tur için bu aralıktan yeni rastgele süre seçilir.
- **Cooldown Range:** Varsayılan `(2, 3)` saniye. Tam hapşırma animasyonu bittikten sonra bu aralıktan yeni rastgele cooldown seçilir. Cooldown bitince yeni 1–5 saniyelik bekleme başlar; süreler birbirine eklenir.
- **Automatic Sneezing:** Otomatik döngüyü kapatır/açar. `Sneeze()` metodu veya component'in **Sneeze** bağlam menüsü elle tetikleyebilir; animasyon sürerken ve cooldown içinde gelen istekler reddedilir.

Varsayılanlarla sıra **1–5 saniye bekleme → 1 saniye hapşırma → 2–3 saniye cooldown → yeni bekleme** şeklindedir. İki hapşırmanın başlangıcı arasında en az yaklaşık 4 saniye vardır. Süreler oyun zamanını izler; oyun duraklatılınca ilerlemez. Obje/component yeniden etkinleşince yeni bekleme seçilir.

Kullanıcının sonraki talebiyle hapşırmaya geçici duman hasarı eklendi. Hazırlık kareleri duman üretmez; `Sneeze` animasyonunun 05 numaralı çıkış karesinde (sıfır tabanlı, yaklaşık 0.417 saniye) tek bir `HapsirikKoz_Smoke.prefab` oluşturulur. Dumanın bağımsız Rigidbody2D'si vardır; devriye düşmanı uzaklaşsa veya kapatılsa da çıktığı noktada ömrünü tamamlar. Alan oluşmadan veya alanın dışındayken duman hasarı uygulanmaz. Sonraki HeatAura talebiyle aşağıdaki alfa geçişli görsel bağlandı. Düşmanın gövdesi animasyonundan bağımsız olarak aşağıdaki temas darbesini verebilir.

`Assets/Prefabs/Enemies/HapsirikKoz_Smoke.prefab` üzerindeki Inspector ayarları:

- **EnemySmokeCloud → Duration:** Varsayılan `3` saniye; süre sonunda obje ve ısı etkisi temizlenir.
- **EnemySmokeCloud → Smoke Visual:** Prefabın `HeatAura` çocuğundaki SpriteRenderer; hazır bağlıdır.
- **EnemySmokeCloud → Fade In Duration / Fade Out Duration:** Varsayılan `0.25 / 0.5` saniye. Oluşurken alfa 0→1, ömrünün son yarım saniyesinde 1→0 ilerler. `0` ilgili geçişi anlık yapar. Toplam geçiş süresi duman ömrünü aşarsa iki süre aynı oranda kısalır.
- **EnemyHeatArea → Drain Multiplier:** Varsayılan `2`; Slime Fire ile aynı tüketim sistemini kullanır. `1` etkiyi kapatır.
- **CircleCollider2D → Radius:** Varsayılan `1.75`; offset `(0, 0.4)`, **Is Trigger** açık.

İki düşman prefabının **HapsirikKozSneeze → Smoke Cloud Prefab** bağlantısı hazırdır. Sahneye ayrıca duman yerleştirmek gerekmez. Duman içinde hareketsizken de zemin tüketimi çarpılır; Slime/duman alanları örtüşürse en yüksek çarpan uygulanır. Alandan çıkınca, duman kapanınca veya süresi dolunca o kaynağın çarpanı kalkar. Duman fiziksel engel değildir ve Water layer'ında değildir.

31 Ekim 2026 HeatAura eki (T42/T43): `HapsirikKoz_Smoke/HeatAura`, Slime ile aynı 256 piksel sheet'i, 24 kare/12 FPS/2 saniyelik ortak nefes klibini, unlit materyali ve collider hizalama bileşenini kullanır. `Level`, Order 0 ile düşman gövdesinin arkasındadır. `EnemySmokeCloud` yalnızca bu dumanın renderer alfa değerini kendi yaşam süresinden hesaplar; ortak klip alfa yazmaz. Böylece iki saniyelik nefes döngüsü yeni bir 0→1 geçişi başlatmaz ve Slime'ın sürekli aurası etkilenmez. Başlangıç sprite'ı alfa 0 olarak kaydedilir; ilk karede opak parıltı oluşmaz. Yeniden etkinleşme hem süreyi hem açılma geçişini sıfırlar; duraklatma geçişi de durdurur. Isı yarıçapı ve hasar çarpanı alfa ile değişmez; ısı ömür boyunca etkin kalır.

İki HapsirikKoz mevcut ortak duman prefabını kullandığından düşman prefablarına yeni bağlantı gerekmez. Yalnızca duman görselini yeniden bağlamak için **GameUp → Enemies → Rebuild HapsirikKoz Smoke Visual** kullanılabilir; dumanın mevcut ömür/hasar ayarlarını korur. **Rebuild HapsirikKoz Prefabs** ve genel üretici de görseli bağlar; bu genel menülerin varsayılan ayarlara yazma davranışı sürer.

Bu ek için ayrı Unity `6000.5.0f1` projesinde **35 kontrol geçti (9 asset, 26 Play Mode/render)**: başlangıç alfa 0, açılma/tam görünürlük/kapanma, nefes döngüsünde alfa sıfırlanmaması, Inspector süreleri, kısa ömür, yeniden etkinleşme, duraklatma, geçişler boyunca ısı ve süre sonunda temizliği; iki gerçek prefabda tek emisyon, devriyeden bağımsız konum ve düşman kapanınca ömrü tamamlama. [Sonuç](verification/smoke-aura-result.txt), [kontroller](verification/smoke-aura-checks.txt), [önizleme](verification/smoke-aura-preview.png). Ana bölüm görünümü ve dış build testi bekliyor.

## Gövde teması ve knockback

Altı düşman prefabının kökünde `EnemyContactKnockback` vardır. Yalnızca kökteki gerçek gövde collider'ına değen oyuncunun `KnockbackAbility.TryKnockback` metodu çağrılır. Rigidbody'ye çocuk ısı alanından gelen trigger olayları, gövde gerçekten temas etmiyorsa reddedilir. Slime'ın geniş HeatArea'sı ve HapsirikKoz dumanı knockback/flash veya tek seferlik temas hasarı üretmez; kendi süreli tüketimlerini sürdürür.

Varsayılan kabul edilen darbe: oyuncudan 10 su, düşmandan uzağa `(6, 5)` hız, 0.2 saniye kontrol kilidi + 0.3 saniye toparlanma ve 2 saniye hasar karesiyle flash. Flash yeni gövde darbelerini engeller; ısı/duman su tüketimini durdurmaz. İtiş, su kaybı ve flash ayarları Player prefabındaki `KnockbackAbility` üzerindedir; ayrıntılar [oyuncu belgesinde](player-state-machine.md#knockback-ve-temas-hasarı). Düşmanlarda ek Inspector referansı gerekmez. Su biterse mevcut tek ölüm/yeniden doğma akışı çalışır.

Mevcut prefabları ayarlarını koruyarak güncellemek için `GameUp → Player → Configure Knockback` kullanılabilir. Düşman üreticileri de yeni ürettikleri altı gövdeye temas component'ini ekler; duman prefabına eklemez.

## Su tüketimi ve yaşam döngüsü

`WaterMeter` tek su sayacı olmaya devam eder. 31 Ekim son bekleme tüketimi kararıyla bütün zeminlerde ısı dışında da pozitif kayıp vardır; ısı/duman seçili bekleme veya yürüme oranını malzeme çarpanından sonra çarpar. Nötr malzemede varsayılan `2x` ısıyla nemli/kuru/çok kuru bekleme `0.5/1/4`, yürüme `2/6/12` birim/saniyedir; havada `6` uygulanır. Isıdan çıkınca veya çarpan `1` olduğunda normal bekleme kaybı sürer. Önceki doğrulama kayıtlarındaki ücretsiz duruş ve eski duruş hasarı sayıları tarihsel sonuçlardır. `Consume(amount)` ile yapılan açık miktarlı işlemler çarpılmaz; kaynak dolumu değişmez. [Güncel oranlar ve bağımsız malzeme ayarları](water-player.md).

`SetDrainMultiplier(source, multiplier)` ve `RemoveDrainMultiplier(source)` kaynak başına geçici etkiyi yönetir. Aynı alanda kalmak çarpanı tekrar tekrar çarpmaz. Örtüşen alanlarda en yüksek çarpan uygulanır; güçlü alandan çıkınca hâlâ temas edilen diğer alanın çarpanı kalır. Bir oyuncunun birden fazla collider'ı varsa son temas bitene kadar etki korunur.

Trigger giriş/kalma/çıkış olayları kullanılır. Devre dışı alan, kapatılan/yok edilen collider veya düşman ve teleport sonrası eski temaslar temizlenir. WaterMeter kapatıldığında veya yeniden doğma başladığında geçici çarpanları temizler; oyuncu yeniden etkinleşip aynı alanda kalırsa trigger stay etkiyi tekrar kurar. Kontrol kapalıyken ve yeniden doğmada mevcut tüketim korumaları sürer.

## Görseller ve yeniden üretim

Mevcut klip/controller'lar 31 Ekim 2026 kullanıcı talebiyle yeni 256 piksel Saw ve FireSlime sheet'lerine bağlandı. Kareler 256×256, PPU 256, Bilinear, mipmap kapalı ve Compression None kullanır. FireSlime'ın döngü süreleri korunur; Saw'ın sonraki hız/tekleme düzeltmesi aşağıdaki güncel zamanlamayı kullanır.

| Prefab | Sheet (`Assets/platformerPack/Sprites/Enemies/` altında) | Klip | Kare / görsel değişimi-sn / döngü |
|---|---|---|---|
| İki Saw türü | `Saw 1/Unity_256/Patrol.png` | `SawSpin` | 16 / 12 / yaklaşık 1.3333 sn |
| `SlimeFire_Stationary` | `FireSlime/Unity_256/Idle.png` | `SlimeFireIdle` | 12 / yaklaşık 13.3333 / 0.9 sn |
| `SlimeFire_Patrol` | `FireSlime/Unity_256/Patrol.png` | `SlimeFireWalk` | 12 / yaklaşık 17.1429 / 0.7 sn |

Saw'ın **Samples** değeri kullanıcının seçimiyle `90`'dır; gerçek sprite değişimleri `1/12` saniye aralıklıdır. Son sprite diğerleri gibi yaklaşık 83.333 ms görünür. Unity gerçek klip sonunu son anahtar + bir Samples adımıyla hesapladığından, klipte 16 benzersiz sprite'a ek olarak aynı son sprite'ı tutan bir bitiş anahtarı vardır. Bu anahtar ilk kareyi tekrar etmez veya görüntüyü değiştirmez; Samples 90 ile son kare süresinin kısalmasını önler. Özel Saw yeniden üretimi de aynı zamanlamayı korur. Önceki 1⁄3 saniyelik hızlı Saw döngüsü artık güncel varsayılan değildir.

Saw'ın duran türü de önceki davranış gibi sürekli döner; iki tür ortak `SawSpin` controller/klibini kullanır. Yeni Saw pivot'u `(0.5, 0.075)` ayak hizalı geldiği için yalnızca `Visual.localPosition.y = -0.425` ile bıçağın merkezi kökteki collider'a hizalanır. FireSlime'ın ortak pivot'u `(0.5, 0.103)` ayak seviyesindedir; eski kırpılmış sprite'lara ait Y konumu eğrileri kaldırıldı ve iki Slime `Visual` konumu sıfırlandı. Kök ölçeği, collider'lar, temas hasarı, ısı alanı, devriye/takip ayarları ve mevcut asset GUID'leri korundu. Prefablar hazır bağlıdır; ek Inspector bağlantısı veya Rebuild işlemi gerekmez.

Önceki 128 piksel sette Saw idle/walk sheet'lerinin otomatik dilimlenmesinde ilk iki kare birleşmişti. İlk entegrasyonda Unity Sprite Editor veri sağlayıcısıyla sekiz adet 128×128 kareye düzeltildi; var olan GUID, spriteID ve fileID'ler korundu, eksik ikinci kare için `_01` adlı yeni dilim eklendi. O sette PPU 128 ve Point filtre tercihleri korunmuştu. Yeni görsellere geçişte eski sheet'ler değiştirilmedi; eski dilim onarımı yeni üreticiden kaldırıldı.

`Assets/Editor/Enemies/EnemyPrefabBuilder.cs`, **GameUp → Enemies → Rebuild Enemy Prefabs** menüsünden aynı asset'leri yeniden üretebilir. Menü, altı düşman prefab'ının, duman prefab'ının, HeatAura dahil altı klibin ve beş controller'ın ayarlarını varsayılanlara yeniden yazar; hazır prefab'ları kullanmak için menüyü çalıştırmak gerekmez. HapsirikKoz menüsü de iki düşman prefabıyla duman prefab'ını birlikte üretir. Projeye yeni paket eklenmedi.

Yalnızca Saw klibini güncellemek için **GameUp → Enemies → Rebuild Saw Animation** kullanılabilir; bu menü prefabları ve Slime kliplerini yeniden yazmaz. Ortak klip üreticisi artık çok satırlı sheet'lerde `Idle_00`/`Patrol_00` adlarına göre sayısal sırayla çalışır, beklenen kare sayısını doğrular ve ilk kareyi sonda tekrar eklemez. Slime'ın eski konum eğrilerini yeniden üretmez.

Yalnızca HapsirikKoz'u üretmek için **GameUp → Enemies → Rebuild HapsirikKoz Prefabs** kullanılabilir (`HapsirikKozPrefabBuilder.cs`). Bu menü mevcut Saw/Slime asset'lerine yazmaz. `HapsirikKozIdle.anim`, `Idle.png` içindeki 8 kareyi 8 FPS ile döngüde; `HapsirikKozSneeze.anim`, `Sneeze.png` içindeki 12 kareyi 12 FPS ile bir kez oynatır. Çok satırlı sheet'ler `_00`, `_01` sırasıyla okunur; ilk kare döngü sonuna tekrar eklenmez. Ortak `HapsirikKoz.controller`, `Sneeze` trigger'ıyla Idle'dan Sneeze'e geçer ve animasyonun tamamından sonra Idle'a döner. Klipler yalnızca `Visual` SpriteRenderer'ının sprite'ını değiştirir. Üreticinin güncel kaynak yolu `Assets/platformerPack/Sprites/Enemies/HapsirikKoz 1/Unity_256/` klasörüdür.

31 Ekim 2026 yüksek çözünürlük geçişi: kullanıcının onayıyla iki klibin sprite referansları ve iki prefabın başlangıç `Idle_00` sprite'ı yeni `Unity_256` sheet'lerine geçirildi. Kare adları/fileID eşleşmeleri doğrulanarak yalnızca sheet GUID referansları değiştirildi; mevcut klip/prefab GUID'leri, zamanlamalar, controller, duman, collider, devriye ve takip ayarları korundu. Yeni sheet'ler 256 PPU, Bilinear, mipmap kapalı ve Compression None kullanır; eski set ve yeni setin `Unity_128`/`Masters_1080` dosyaları bu geçişte değiştirilmedi. Kare tuvali yine 1×1 dünya birimidir; kök veya `Visual` ölçeğini değiştirmen gerekmez. İki prefab hazır bağlıdır; geçişi almak için Rebuild menüsünü çalıştırmak gerekmez. Rebuild menüsü prefab/duman ayarlarını varsayılanlara yazar.

## Bekleyen iş

- [ ] Asıl sahnede knockback kuvveti, temas su kaybı ve flash süresinin hissini oynayarak dengele. Altı prefab bağlantısı ayrı Unity projesinde doğrulandı.
- [ ] Asıl bölümde yerleşim, rota güvenliği, ısı yarıçapı ve çarpan dengesini elle oynayarak doğrula; dış build'i test et.

## Doğrulama

31 Ekim 2026 görünmez Edge sınırı, Unity `6000.5.0f1`, ayrı `C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073` projesi: güncel kod/prefab/katman ayarlarıyla **94 kontrol geçti (27 asset, 67 Play Mode)**. Önceki düşme, gerçek zemin/duvar çarpışması ve hasar kontrolleri tekrar geçti. Yeni senaryolarda SlimeFire/HapsirikKoz iki yöndeki uçurumdan önce Edge çizgisinde durdu, hedef taraf değiştirince çizgiden ayrılabildi, Edge kapatılınca normal düşüş sürdü. Uçan Saw takip sırasında çizgiden geçti; gerçek Player gövdesi fiziksel olarak çizgiyi engellenmeden ve hasarsız geçti. [Sonuç](verification/enemy-boundary-result.txt), [kontroller](verification/enemy-boundary-checks.txt).

Doğrulanan dört yerdeki prefab mevcut GUID'leri korunarak, yeni sınır prefabı/meta'sı ve iki katmanın TagManager ayarlarıyla ana projeye aktarıldı. Aynı `EnemyTerrainVerification.Run` düzeneği kullanılır; test projesindeki çıktı adları `enemy-terrain-result.txt` / `enemy-terrain-checks.txt`, bu oturumun depodaki kopyaları yukarıdaki `enemy-boundary-*` dosyalarıdır. Ayrıntılı `enemy-boundary-verification.log` geçici projededir. Mevcut Player'ın her karedeki debug kayıtları yalnızca test logunda filtrelenir; üretim kodu değişmez. Başlangıç SearchDatabase indeksleme istisnası kaydedildi; C# derleme hatası veya başarısız davranış kontrolü yoktur. **Ana bölümde sınır yerleşimi, elle oynanış/denge ve dış build testi bekliyor.** Aşağıdaki sayılar önceki oturumların kayıtlarıdır.

31 Ekim 2026 yerçekimi/duvar düzeltmesi, Unity `6000.5.0f1`, ayrı `C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073` projesi: güncel oyuncu/düşman kodu, gerçek prefablar, animasyonlar, zemin layer'ları ve Physics2D ayarlarıyla **72 kontrol geçti (19 asset, 53 Play Mode)**. Üç zemin katmanında dört yerdeki türün düşüşü/inişi, motor kapalıyken yerçekimi, takipte duvarda kalma ve geri ayrılma, Saw'ın havada kalması ve taban/tavan çarpışması, kenardan devriye/takipte düşüş, düşüşte rotaya dönüşün eski yüksekliğe taşımaması, motor kapatma, yerdeki dikey offset'in yok sayılması ve ısı/gövde hasarı ayrımı doğrulandı. Doğrulanan altı prefab mevcut meta/GUID'leri korunarak ana projeye aktarıldı. [Sonuç](verification/enemy-terrain-result.txt), [kontroller](verification/enemy-terrain-checks.txt).

Aynı oturumda güncellenen takip regresyonu da **52 kontrol geçti (9 asset, 43 Play Mode)**. Yerdeki türlerin düzeneğine zemin eklendi; çapraz uçuş rotası Saw'da sınandı; hasarı tek başına ölçmek için normal bekleme tüketimi test içinde sıfırlandı. Üretim su oranları değiştirilmedi. [Regresyon sonucu](verification/enemy-chase-physics-result.txt), [kontroller](verification/enemy-chase-physics-checks.txt). Önceki aşağıdaki kayıtlar tarihsel sonuçlardır.

Yeni yeniden çalıştırılabilir düzenek `Assets/Editor/Enemies/EnemyTerrainVerification.cs`: ayrı kopya projede `-batchmode -nographics -projectPath "<test projesi>" -executeMethod EnemyTerrainVerification.Run -logFile "<log yolu>"`; `-quit` ekleme. Bu düzenek test kopyasındaki prefab fiziğini menü komutuyla günceller. Detaylı `enemy-terrain-verification.log` ve `enemy-chase-physics-verification.log` geçici projede bulunur. Her iki çalıştırmada Unity'nin başlangıç SearchDatabase indeksleme istisnası kaydedildi; C# derleme hatası ve başarısız davranış kontrolü yoktur. **Ana bölümde elle oynanış/denge ve dış build testi bekliyor.**

31 Ekim 2026 Saw döngü sonu düzeltmesi: ayrı Unity `6000.5.0f1` projesinde **18 kontrol geçti (8 asset, 10 Play Mode)**. Konum: `C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073`. Oturum başındaki gerçek klip, önceki bozuk zamanlamanın karşılaştırma örneği olarak saklandı: görünür sprite değişimleri yaklaşık 83.333 ms, son sprite gösterimi yaklaşık 20.833 ms idi. Hata iki gerçek Saw prefabının Animator'ında yeniden üretildi. Samples 90 korunarak aynı son sprite ile bitiş sabitlenince native klip süresi 1.333333 sn ve son görünür sprite süresi de 83.333 ms oldu.

İki prefabda onar kesintisiz döngü 960 örnek/sn ile ölçüldü. Ölçüm penceresi kenarındaki kısmi gösterimler dışarıda bırakıldığında her tam sprite gösterimi 80–81 ölçüm adımı içinde kaldı (hedef 80 adım = 83.333 ms; yaklaşık 1 ms ölçüm toleransı). Sprite sırası, döngü birleşimi, değişmeyen kök/collider/görsel konumu ve yeniden üretimin aynı düzeltmeyi koruması geçti. Slime/Hapsirik klipleri, prefablar, controller ve GUID'ler korundu; test edilmiş Saw klibi ana projeye aktarıldı. Kaynak PNG'deki son-ilk kare farkı diğer ardışık kare farklarıyla benzerdi; PNG'ye değişiklik yapılmadı.

[Sonuç](verification/saw-loop-result.txt), [geçen kontroller](verification/saw-loop-checks.txt). Düzenek `Assets/Editor/SawLoopVerification.cs`, log `saw-loop-verification.log` aynı geçici projededir; düzenek ve önceki klip ana projede yok sayılan `Temp/SawLoopFix/` altında da saklandı. Bu, önceki 64 kontrollük geçişteki hızlı Saw zamanlamasını güncelleyen ayrı doğrulamadır. Asıl sahnede algılanan dönüş hissi ve dış build testi bekliyor.

31 Ekim 2026 Saw/FireSlime 256 piksel geçişi (T42 ek kapsamı): ayrı Unity `6000.5.0f1` projesinde **64 kontrol geçti (40 asset, 24 Play Mode/görüntü)**. Konum: `C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073`. Güncel üretim prefab/kodları, üç mevcut klip ve yeni sheet'ler kullanıldı. Üreticinin yalnızca ilgili klip oluşturma metodu çalıştırıldı; dört mevcut prefabda yalnızca başlangıç sprite'ı ve `Visual` hizalama konumu değiştirildi. Genel Rebuild çalıştırılmadı. Test edilmiş dört prefab ve üç klip aynı `.meta`/GUID'ler korunarak ana projeye birebir aktarıldı.

Kontroller: 256 PPU, tüm dilimler/pivotlar ve tam texture çözünürlüğü; sıkıştırmasız/Bilinear/mipmap kapalı import; 1⁄3/0.9/0.7 saniyelik döngülerin korunması; çok satırlı sayısal kare sırası; kaldırılan eski Y eğrileri; değişmeyen collider/ısı/devriye/takip ayarları. Dört gerçek Animator üçer döngü boyunca örneklendi; görsel hizası, kök ve fizik sabit kaldı. İki hareketli tür gerçek Player'ı takip edip çıkışta özgün rotasına döndü; duran türler hareket etmedi. Slime'ın bağımsız ısı alanı, 10 su kaybı/flash üreten gerçek gövde teması ve çıkışta ısı temizliği geçti. Aktarım anındaki hash karşılaştırması runtime düşman kodunun, controller'ların, klip kimliklerinin ve HapsirikKoz asset'lerinin korunduğunu gösterdi. Ana sahnelere bu geçişte yazılmadı; oturum sırasında görülen ayrı Level1 değişiklikleri korundu.

[Sonuç](verification/saw-slime-artwork-result.txt), [geçen kontroller](verification/saw-slime-artwork-checks.txt), [Unity görüntüsü](verification/saw-slime-artwork-preview.png). Görüntüde soldan sağa Saw Stationary, Saw Patrol, SlimeFire Stationary, SlimeFire Patrol bulunur. Düzenek `Assets/Editor/SawSlimeArtworkVerification.cs`, ayrıntılı log `saw-slime-artwork-verification.log` geçici projededir; düzenek kaynağı ana projede yok sayılan `Temp/SawSlimeArtworkMigration/` altında da saklandı. Logda önceki oturumlarda da görülen bağımsız `UnityEditor.Search.SearchDatabase` başlangıç indeksleme hatası vardır; C# derleme hatası veya başarısız davranış kontrolü yoktur. Görüntü ayrı projenin unlit Sprite renderer'ında alındı; asıl Level1/Level2 kamera/ışık görünümü ve dış build testi bekliyor.

31 Ekim 2026 yüksek çözünürlüklü HapsirikKoz geçişi (T42 ek kapsamı): Unity `6000.5.0f1`, `C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073` ayrı projesinde bu geçiş için **34 kontrol geçti (19 asset, 15 Play Mode/görüntü)**. Güncel üretim prefabları/klipleri doğrudan kopyalandı; Rebuild çalıştırılmadı. Yeni 256 piksel sheet'lerinin tüm dilimleri ve pivotu, 256 PPU, sıkıştırmasız/Bilinear/mipmap kapalı import, tam texture çözünürlüğü, klip süresi/sırası/loop'u, ortak controller, duman referansı ve mevcut collider/devriye/takip ayarları doğrulandı. Her iki gerçek prefabın Animator'ında iki Idle döngüsü ve tam Sneeze örneklendi; 05 öncesinde duman çıkmadı, 05'te tek duman çıktı, bitişte Idle'a dönüldü ve cooldown korundu. Gerçek devriye hareketi ve iki prefabın Idle/Sneeze görüntüleri de doğrulandı. Kaynak karşılaştırması iki prefab/klipte yalnızca sheet GUID referanslarının değiştiğini, sahnelerin, runtime düşman kodunun, controller'ın ve duman prefabının korunduğunu gösterdi.

[Sonuç](verification/hapsirik-artwork-result.txt), [geçen kontroller](verification/hapsirik-artwork-checks.txt), [Idle görüntüsü](verification/hapsirik-artwork-idle.png) ve [Sneeze görüntüsü](verification/hapsirik-artwork-sneeze.png). Test düzeneği `Assets/Editor/HapsirikKozArtworkVerification.cs`, ayrıntılı log `hapsirik-artwork-verification.log` aynı geçici projededir; düzenek kaynağı ana projede yok sayılan `Temp/HapsirikArtworkMigration/` altında da bulunur. Komut: Unity `-batchmode -projectPath "<ayrı test projesi>" -executeMethod HapsirikKozArtworkVerification.Run -logFile "<log yolu>"`; render doğrulaması için `-nographics` ve asenkron bitiş nedeniyle `-quit` kullanılmaz. Logda önceden kaydedilen bağımsız Unity Search başlangıç indeksleme hatası vardır; C# derleme hatası veya başarısız davranış kontrolü yoktur. Görüntüler ayrı projenin unlit Sprite renderer'ıyla alındı; ana Level1/Level2 kamera/ışık görünümü ve dış build testi bekliyor.

31 Ekim 2026 takip alanı: Unity `6000.5.0f1` ile `C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073` altındaki ayrı projede **52 kontrol geçti (9 asset, 43 Play Mode)**. Bu oturumdaki kaynak kod ve gerçek Player/düşman prefabları kullanıldı. Üç hareketli prefabın varsayılanları, animasyon/gövde referansları ve duran türlerin değişmemesi doğrulandı. Oyuncu alana girince ters yöndeki devriyenin kesilmesi, yatay/iki eksenli takip, yüz yönü, çıkışta özgün rotaya dönüş, negatif/çapraz rota, sınırdaki fiziksel örtüşme, çoklu/çocuk collider, yalnızca trigger sensörünün reddi, sıfır hız/offset/yarıçap ve kapatma/yeniden açma kontrolleri geçti. Oyuncu/collider kapanışı, simülasyonun durması, teleport, silinme ve ölüm/yeniden doğmada eski takip kalmadı. Algılama tek başına hasar/flash üretmedi; takipte Slime ısısı ve gerçek gövde darbesi ayrı çalıştı, çıkışta ısı temizlendi.

Yeniden çalıştırılabilir düzenek: `Assets/Editor/Enemies/EnemyChaseVerification.cs`. Ayrı kopya projeyi Unity ile `-batchmode -nographics -projectPath "<test projesi>" -executeMethod EnemyChaseVerification.Run -logFile "<log yolu>"` kullanarak çalıştır; asenkron Play Mode bitişini düzenek yönettiği için `-quit` ekleme. [Sonuç](verification/enemy-chase-result.txt) ve [geçen kontroller](verification/enemy-chase-checks.txt) depoda saklandı; ayrıntılı `enemy-chase-verification.log` geçici projede bulunur. Logdaki bağımsız `UnityEditor.Search.SearchDatabase` başlangıç indeksleme hatası ve mevcut Player'ın boş çok kuru zemin maskesi uyarısı kaydedildi; C# derleme hatası veya başarısız davranış kontrolü yoktur. Ana Level1 oynanışı/rota güvenliği ve dış build testi bekliyor.

İlk uygulamada Unity 6000.5.0f1 ile `C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073` altındaki bağımsız projede 51 doğrulama geçti: 20 asset/bağlantı kontrolü ve 31 Play Mode davranış kontrolü. Bu ilk kayıt, kullanıcı hareketsiz ısı hasarını istemeden önceki davranışa aittir. Asıl projede açık editör bulunduğundan ikinci editör bu projeye başlatılmadı. Güncel Player prefab'ı/kodu, Input System, oyuncu animasyonları ve gerçek düşman sheet'leri kopyalandı; düşman prefabları Unity tarafından üretildi. Geçen kontrollerin ardından aynı prefab/animasyon dosyaları ve meta'ları asıl projeye aktarıldı.

Kapsam: dört prefab'ın component ve sheet referansları, sekiz tek bıçaklı Saw karesi, Slime ayak hizası, duran türlerin konumu, iki uçta dönüş ve yönlenme, negatif/çapraz offset, sıfır hız, gerçek trigger giriş/kalma/çıkış, örtüşmede en güçlü etki, çoklu collider, alan/collider/WaterMeter kapatma-açma, düşmanın/collider'ın silinmesi, teleport, yeniden doğma ve sıfır su temizliği. Doğrudan temasın etkisiz kaldığı kontrol edildi. 2x ısıda kuru/hava tüketimi yaklaşık 6.0001, nemli tüketim 2.0000 birim/saniye ölçüldü; yerde durma ve kapalı kontrolde tüketim sıfır kaldı.

Test düzeneği: `Assets/Editor/EnemyVerification.cs`; kayıtlar: `verification.log` ve `verification-result.txt` (geçici test projesinde). Saw onarımında var olan tüm spriteID/fileID değerlerinin, texture GUID'lerinin ve kullanıcı import ayarlarının korunduğu ayrıca karşılaştırıldı. Bu doğrulama asıl Components/Level 5 sahnelerinin oynanışı, render kontrolü veya dış build testi değildir; bunlar bekliyor.

31 Ekim 2026 Saw geçiş düzeltmesi: önceki 10 FPS klip, sekiz kareye ek olarak ilk kareyi sonda tekrarlıyor ve 0.9 saniyelik döngünün birleşiminde ilk kareyi toplam 0.2 saniye tutuyordu. Yeni 24 FPS klipte sekiz benzersiz kare, her biri 1/24 saniye gösterilir; toplam döngü 1/3 saniyedir. Yukarıdaki bağımsız Unity 6000.5.0f1 projesinde 4 asset ve 10 Play Mode kontrolü geçti. İki gerçek Saw prefabının Animator'ı beşer döngüde toplam 800 kez örneklendi; sıra, eşit kare süreleri, döngü birleşimi ve kök/collider korunması doğrulandı. Düzenek `Assets/Editor/SawAnimationVerification.cs`, sonuç `saw-animation-result.txt`, log `saw-animation-verification.log` dosyalarındadır. Aynı test edilmiş klip mevcut GUID ile projeye aktarıldı. Asıl sahnede görüntü ve dış build testi yapılmadı.

31 Ekim 2026 hareketsiz ısı hasarı: kullanıcının yeni kararına göre `WaterMeter.FixedUpdate`, çarpan >1 iken yerde hareketsiz tüketimi de uygular. Aynı bağımsız Unity projesinde güncel kod, gerçek Player ve Slime prefablarıyla 27 Play Mode kontrolü geçti. Kuru/nemli zeminde alan dışında sıfır, 2× ısı içinde dururken ve yürürken yaklaşık 6/2 birim/saniye; alan çıkışında ve ısı/oyuncu kontrolü kapatıldığında kaybın durması; çarpan 1 ile etkiyi kapatma; örtüşmede en güçlü etki ve 0.01/0.04 saniyelik fizik adımlarında aynı tüketim doğrulandı. Hareketsiz ısı tek başına suyu bitirip yalnızca bir yeniden doğma başlattı; güvenli checkpoint'te tam su ve kontrol geri geldi. Düzenek `Assets/Editor/HeatIdleVerification.cs`, sonuç `heat-idle-result.txt`, log `heat-idle-verification.log` dosyalarındadır. Asıl sahneler ve build çalıştırılmadı. İlk 51 kontroldeki “ısı içinde yerde dururken tüketim sıfır” sonucu artık geçerli tasarım değildir.

31 Ekim 2026 HapsirikKoz ilk entegrasyonu (duman hasarı eklenmeden önce): aynı bağımsız Unity 6000.5.0f1 projesinde **43 kontrol geçti (18 asset, 25 Play Mode)**. Ana editörün ürettiği yeni script GUID'leri test projesinde kullanıldı; iki prefab, iki klip ve controller Unity tarafından üretildi ve test edilmiş dosyalar meta'larıyla birlikte birebir ana projeye aktarıldı. Yeniden üretimde GUID'lerin korunması, çift state/geçiş oluşmaması ve mevcut Saw/Slime asset'lerinin değişmemesi doğrulandı.

Her iki gerçek prefab'ın Animator'ında 8 Idle karesi iki döngü boyunca, 12 Sneeze karesi bir tam oynatım boyunca örneklendi: sıra ve eşit kare süreleri, Idle'a dönüş, kök/collider/ayak pivot'unun korunması geçti. On iki düşmanda ilk rastgele beklemeler 1–5 saniye, animasyon sonrası cooldown + yeni bekleme toplamı 3–8 saniye aralığında kaldı. Yeni bekleme sıfırlanarak ayrı ölçülen altı rastgele cooldown 2.030–2.876 saniye sürdü; animasyonun bitiminden sonra başladıkları ve cooldown'da elle tetiklemenin reddedildiği kontrol edildi. Devriye/yönlenme, negatif-çapraz offset, sıfır hız, otomatiği kapatma ve yeniden etkinleştirme de geçti.

Düzenek `Assets/Editor/HapsirikKozVerification.cs`, sonuç `hapsirik-koz-result.txt`, log `hapsirik-koz-verification.log` dosyalarındadır (`C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073`). Logda testlerden bağımsız bir `UnityEditor.Search.SearchDatabase` başlangıç indeksleme hatası da vardır; C# derleme hatası veya başarısız davranış kontrolü yoktur. Asıl sahnede yerleştirme/görüntü ve dış build testi bekliyor.

31 Ekim 2026 HapsirikKoz duman hasarı: bu ekin doğrulamasında aynı bağımsız Unity 6000.5.0f1 projesinde **40 kontrol geçti (10 asset, 30 Play Mode)**. Güncel Player/su/düşman kodu, gerçek Player ve iki HapsirikKoz prefab'ı kullanıldı; mevcut Idle/Sneeze klipleri ve controller birebir korundu. Oyuncu test zemininin üzerine yerleştirildi; çok kuru zemin maskesi test örneğine bağlandı. Duman prefab'ı Unity'de üretildi; iki mevcut düşman prefab'ına yalnızca duman prefab referansı eklendi. Test edilmiş üç prefab ve yeni duman meta'sı ana projeye aktarıldı; mevcut GUID'ler ve diğer ayarlar korundu.

Doğrulananlar: Idle/gövde teması ve hazırlık karelerinde sıfır hasar; çıkış karesinde tek alan; 3 saniyelik ömür (ölçüm 2.998 saniye); kuru/nemli/çok kuru zeminde hareketsiz tüketim yaklaşık 6/2/12, havada 6 birim/saniye; alan çıkışı ve tekrar giriş; çarpan 1 ile kapatma; Slime ve birden çok duman örtüşmesinde en güçlü kaynağın kullanılması; güçlü kaynak bitince zayıfa dönüş; erken kapatma/silme, çoklu oyuncu collider'ı, kapalı kontrol ve su bitmesiyle tek yeniden doğma sonrası temizlik. Devriye düşmanı ilerlerken dumanın çıktığı konumda kaldığı, düşman hazırlık sırasında kapatılırsa duman çıkmadığı ve oluşmuş dumanın düşman silinse de kendi ömrünü tamamladığı da geçti.

Düzenek `Assets/Editor/HapsirikSmokeVerification.cs`, sonuç `hapsirik-smoke-result.txt`, log `hapsirik-smoke-verification.log` dosyalarındadır (aynı geçici proje). Logda önceki kayıtta belirtilen bağımsız Unity Search başlangıç hatası vardır; C# derlemesi ve 40 kontrol başarılıdır. Ana sahne yerleşimi/oynanışı ve dış build testi bekliyor; duman görseli kullanıcı kararıyla sonraya bırakıldı.

31 Ekim 2026 gövde knockback'i: güncel Player ve altı düşman prefab'ıyla aynı ayrı Unity projesinde **49 kontrol geçti (13 asset, 36 Play Mode)**. Altı gövde gerçek trigger temasında bir kez su düşürüp oyuncuyu itti. Flash sırasında tekrar darbeler ve çoklu collider/düşman yığılması engellendi; koruma bitince trigger stay yeni darbeyi verebildi. Slime geniş alanı ve duman itme üretmeden yaklaşık 6 su/s tüketimini sürdürdü; bu tüketim flash sırasında da sürdü. İtiş, hasar görünümü/yanıp sönme, Inspector ayarları, temizleme/ölüm/yeniden doğma ve hareket regresyonları [oyuncu belgesinde](player-state-machine.md#doğrulama) kayıtlıdır. Önceki 51/40 kontrol kayıtlarındaki “gövde teması zararsız” davranışı bu yeni kullanıcı kararıyla değişmiştir. Sahne yerleşimi/his ve build kontrolü bekliyor.
