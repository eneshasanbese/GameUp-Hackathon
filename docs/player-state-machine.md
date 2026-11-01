# Player state machine

1 Kasım 2026 ölüm/harita sıfırlaması (T25–T26): `DeathAbility`, mevcut state machine'de `Death=4` durumudur. `WaterMeter` su sıfırlanınca veya düşme sınırında tek yeniden doğma coroutine'ini başlatır. Player ölüm animasyonu boyunca açık kalır; `StopGameplay` giriş/çekim/flash/bekleyen itişi temizler ve mevcut `GameplayStopped` abonelerine haber verir. Zıplama ve yön çevirme ölüm durumunu kesemez. Klip bitince harita yeniden yüklenir, son checkpoint yeni Player'a aktarılır. [Ayrıntı ve sahne koşulları](water-player.md#kaynak-ve-yeniden-doğuş).

31 Ekim 2026 Köz Ocağı eki: `PhysicsControl` kaynak başına yatay hedef hız katkılarını normal Idle/Move/Jump hedefiyle birleştirir. `Player.OnDisable` kayıtları temizler; mevcut suya bağlı hız/zıplama korunur. [Birimler, yaşam döngüsü ve testler](koz-ocagi.md).

T09 kapsamı: sağ/sol hareket, tek zıplama, yere temas ve düşüş. Yapı, paylaşılan örnekteki gibi `BaseAbility : MonoBehaviour` üzerinden ayrı ability component'leri kullanır.

## Kullanım

`Assets/Scenes/Components.unity` sahnesini açıp Play'e bas. A/D veya sol/sağ ok hareket ettirir; Space zıplatır. Yükselirken Space bırakılırsa Y hızı sıfırlanır ve yerçekimiyle düşüş başlar. Havada yön değiştirilebilir. Space basılı tutulduğunda inişte otomatik tekrar zıplanmaz; havada ikinci zıplama yoktur.

`Player.Flip()` yatay girdiye göre `Visual` üzerindeki `SpriteRenderer.flipX` değerini ayarlar: sola giderken çevrilir, sağa giderken düzelir. Yerde ve havada çalışır; girdi sıfırken son yön korunur, Knockback sırasında yön güncellenmez. `Player.FacingRight` güncel görsel yönünü bildirir. SpriteRenderer alt objelerden otomatik bulunur; yeni Inspector bağlantısı gerekmez. Kök dönüşü/ölçeği, collider ve WaterMeter'ın küçülme/esneme değerleri değiştirilmez.

Player üzerindeki component ayarları:

- **GatherInput → Input Actions:** `Assets/PlayerInput.inputactions` bağlıdır. Asset içinde `Player/Move` (Vector2) ve `Player/Jump` (Button) bulunmalı.
- **MoveAbility → Speed / Acceleration:** yerdeki yatay hız ve hedef hıza ulaşma ivmesi.
- **IdleAbility → Deceleration:** tuş bırakıldıktan sonraki kısa duruşun yavaşlama değeri.
- **JumpAbility → Airspeed / Air Acceleration / Jump Force:** havadaki yatay hız, yatay ivme ve zıplamanın başlangıç dikey hızı. Yerçekimini Rigidbody2D yönetir.
- **PhysicsControl → Dry Ground Layer / Wet Ground Layer / Extra Dry Ground Layer:** Ground-Dry, Ground-Wet ve Ground-Extra-Dry katmanlarındaki destek temaslarını ve aşağı collider taramasını filtreler.
- **PhysicsControl → Ground Check Distance / Maximum Ground Angle:** Güncel collider boyutuna göre tarama mesafesi ve üzerinde durulabilecek en dik yüzey açısı. Prefab değerleri 0.15 ve 60 derecedir.
- **WaterMeter → Water Size → Minimum Scale:** Su sıfıra yaklaşırken görsel ve collider'ın minimum boyutu; prefab'da 0.45. Daha küçük değer daha fazla küçültür.
- **MoveAbility → Low Water Boost → Low Water Speed Multiplier:** Düşük suda yürüme hızı çarpanı; prefab'da 1.4.
- **JumpAbility → Low Water Boost:** Low Water Air Speed Multiplier=1.4, Low Water Jump Multiplier=1.1; hava hızı ve kalkış hızı ayrı ayarlanır.
- **Ability → Is Permitted:** ilgili duruma geçiş izni.

## Knockback ve temas hasarı

1 Kasım 2026 son kullanıcı talebiyle `EmberSpitter.prefab` ve `KozOcagi_Stationary.prefab` köklerine ayrı gövde trigger'ları ve mevcut `EnemyContactKnockback` bağlandı. İkisi de bu belgedeki aynı Contact Water Loss, Force, kontrol kilidi/toparlanma ve flash ayarlarını kullanır. Geniş algılama/emiş alanları gövde darbesi üretmez. Flash sırasında tekrar eden gövde/mermi darbeleri reddedilir; Köz Ocağı'nın sürekli tüketimi sürer. Alan boyutları [Ember](ember-spitter.md#görseller) ve [Köz Ocağı](koz-ocagi.md#prefab-ve-inspector) belgelerindedir.

31 Ekim 2026 Köz Püskürten eki: `KnockbackAbility.TryWaterHit(float waterPoints)` aynı flash dokunulmazlığını kullanarak sabit mermi hasarını `WaterMeter.Consume` ile bir kez uygular; itiş veya yeni fizik state'i başlatmaz. Mermi/gövde isabetleri aynı korumayı paylaşır. `Player.GameplayStopped`, Player kapanışında hazırlık hedeflerini ve eski mermileri anında iptal etmek için yayımlanır. [Prefab ve test ayrıntıları](ember-spitter.md).

31 Ekim 2026 kullanıcı talebiyle `KnockbackAbility : BaseAbility` ve `PlayerStates.State.Knockback = 3` eklendi. Referans alınan diğer proje dosyasından ayrı knockback durumu, kontrol kilidi ve kübik toparlanma yaklaşımı uyarlandı; bu projede yeni input veya can sistemi kurulmadı. Hasar, kullanıcının devam onayıyla mevcut `WaterMeter.Consume` üzerinden tek seferlik su kaybıdır.

`Assets/Prefabs/Player.prefab` kökündeki **KnockbackAbility** ayarları:

| Alan | Varsayılan | Etki |
|---|---|---|
| Force | `(6, 5)` | Düşmandan uzağa yatay ve yukarı başlangıç hızı |
| Control Lock Duration | `0.2` s | Input'un darbeyi kesemediği başlangıç süresi |
| Recovery Duration | `0.3` s | Hava kontrolünün kübik olarak geri geldiği süre |
| Recovery Airspeed | `4` | Toparlanma sırasındaki yatay hedef hız |
| Contact Water Loss | `10` | Kabul edilen her gövde darbesindeki tek su kaybı |
| Flash Duration | `2` s | Yanıp sönme ve yeni gövde darbelerine karşı koruma |
| Flash Interval | `0.1` s | Görünür/gizli faz aralığı |
| Body Visual | `Visual` SpriteRenderer | Hazır prefab bağlantısı |

Darbe yönü oyuncu ve kaynak konumlarından bulunur; merkezler çakışırsa input'un tersine veya giriş yokken sağa itilir. Kuvvet mevcut state machine üzerinden FixedUpdate'te bir kez uygulanır; yukarı hız gerçek zıplama gibi zemin yapışmasını keser. Space basma/bırakma olayları knockback sırasında tüketilir ama yeni zıplama veya yükseliş kesme üretmez. Kilit ve toparlanma bitince yerde Idle/Move, havada Jump'a dönülür; flash kontrol geri geldikten sonra da sürebilir.

Flash boyunca `Knockback.anim`, kullanıcının istediği `Frames/death/player_death_01.png` karesini tutar. `Visual.controller` ve `Player.controller` aynı Knockback state'ini kullanır. Tek karenin eski otomatik kırpımı/100 PPU ayarı, mevcut spriteID/fileID/GUID korunarak 128×128 hücre, 128 PPU ve normal oyuncu klipleriyle ortak `(0.5, 0.47265625)` pivot'a eşlendi. Kök, collider ve WaterMeter'ın yönettiği Visual ölçeği/konumu animasyondan değiştirilmez.

1 Kasım 2026 döngü düzeltmesi (T09/T52): Hasar klibine eklenen `PlaySound` event'i, 0.1 saniyelik klip döngüsü yüzünden iki saniyelik flash içinde 20 kez çalışıyordu. Fiziksel ability ise tek kez girip kilit + toparlanma sonunda (varsayılan 0.5 sn) çıkıyor, tek su darbesi veriyor ve hareket kontrolünü geri bırakıyordu. `Knockback.anim` artık **Loop Time kapalı** olarak son karesini tutar; seçilmiş ses/event ve fizik/flash süreleri korunur. `KnockbackSetup.Configure` da tek oynatma ayarını üretir. Event'ten hasar sesi kullanılırken **Player Sounds → Hurt** boş bırakılır; iki ses yolu birlikte doldurulmaz.

Yanıp sönme yalnızca SpriteRenderer görünürlüğünü değiştirir; renk/alpha yazmaz, böylece yeniden doğma sönmesiyle yarışmaz. Flash sırasında Heal hasar görünümünü kesmez. Flash bitince normal state animasyonu ve başlangıç görünürlüğü geri gelir. Tekrarlı temaslar ve birden fazla düşman/collider flash'ı yeniden başlatmaz veya fazladan temas suyu düşürmez; koruma bittikten sonra gövdede kalınmışsa trigger stay yeni darbeyi kabul edebilir. Isı/duman tüketimi bu koruma sırasında devam eder.

Player veya ability kapatılınca ve ölüm/yeniden doğma başlayınca flash ve bekleyen itiş temizlenir; tekrar etkinleşme eski Knockback durumunu devam ettirmez. Ölüm için ikinci coroutine veya sayaç yoktur. `GameUp → Player → Configure Knockback` menüsü mevcut Player/düşman prefab ayarlarını koruyarak component bağlantılarını, iki controller'ı ve hasar karesini yeniden kurar; hazır asset'leri kullanırken menüyü çalıştırmak gerekmez.

`Components.unity` sahnesindeki Player prefab'ında `Player`, `GatherInput`, `PhysicsControl`, `IdleAbility`, `MoveAbility`, `JumpAbility`, `KnockbackAbility` ve `DeathAbility` component'leri vardır. Player referansları ve input asset'i sahnede ayarlanmıştır. Player Inspector'ında hareket veya zıplama ayarı bulunmaz. Su miktarı, görseller ve kaynaklar [su damlası prototipi belgesinde](water-player.md) anlatılır.

Başka bir Circle'a Player eklemek GatherInput, PhysicsControl, Rigidbody2D ve CircleCollider2D gereksinimlerini ekler. Ayrıca IdleAbility, MoveAbility, JumpAbility, temas hasarı için KnockbackAbility ve ölüm animasyonu için DeathAbility component'lerini ekleyip GatherInput'a Input Actions asset'ini bağla. Rigidbody2D Dynamic ve Gravity Scale pozitif olmalı. Zeminde trigger olmayan Collider2D bulunmalı.

## Düşük suda hız ve zıplama

`Speed`, `Airspeed` ve `Jump Force` tam sudaki temel değerlerdir. Bonus, `1 - WaterMeter.NormalizedWater` üzerinden 1.0 ile seçilen düşük su çarpanı arasında doğrusal hesaplanır; temel serialized alanlar her karede değiştirilmez. Örneğin prefab'ın 6 yürüme/hava hızı ve 10 kalkış hızı, %50 suda 7.2/7.2/10.5 olur; su sıfıra yaklaşırken 8.4/8.4/11'e yaklaşır. Components sahnesindeki mevcut Jump Force=5 override'ı korunur; o oyuncuda katsayı 5 üzerinden hesaplanır.

Inspector'daki her çarpanı 1 yapmak ilgili artışı kapatır. Sadece daha uzağa gitmek için hava hızı artırılıp Jump çarpanı 1 bırakılabilir. Yürüme ve hava hızı güncel suyu izler; zıplama katsayısı yalnızca Space ile kalkışta alınır. Havada su kaybetmek veya doldurmak yeni dikey itiş üretmez. İvme/fren ayarları, kısa Space bırakma, tek zıplama ve durum geçişleri aynı kalır. WaterMeter olmayan oyuncu temel değerlerle çalışır; yeni Inspector referansı bağlamak gerekmez.

## Animasyonlar

`Components.unity` sahnesinde `Visual` üzerinde Animator ve `PlayerAnimation` vardır. `PlayerAnimation` aynı objenin Animator'ını, üstteki Player ve WaterMeter'ı otomatik bulur. Animator'a `Assets/Animations/Visual.controller` bağlıdır; Inspector'da başka referans gerekmez. Başka bir oyuncuya taşınırsa bu iki component'i SpriteRenderer'ın olduğu Visual'a ekle. `Player.controller` da aynı klip ve geçişleri içerir; sahnede kullanılan controller Visual'dır.

| Klip | Kare / hız | Süre | Davranış |
|---|---|---|---|
| Idle | 4 / 5 FPS | 0.8 s | Döngü |
| Move | 6 / 10 FPS | 0.6 s | Döngü |
| Jump | 5 / 10 FPS | 0.5 s | Bir kez; havada son düşüş pozu tutulur |
| Heal | 4 / 5 FPS | 0.8 s | Kaynaktan dolumda bir kez |
| Knockback | 1 / 10 FPS | 0.1 s | Hasar karesi flash/knockback süresince tutulur |
| Death | 8 / 10 FPS | 0.8 s | Bir kez; tamamlanınca sahne sıfırlanır ve son checkpoint'e dönülür |

`State` int parametresi mevcut `PlayerStates` değerlerini izler: Idle=0, Move=1, Jump=2, Knockback=3, Death=4. Ölüm en yüksek görsel önceliktir; Heal tetikleyicisi ve flash temizlenir. Ölüm dışında flash sürerken animasyon parametresi Knockback'te tutulabilir; fiziksel state toparlanma sonunda normal harekete döner. Hareket durumları arasında geçiş beklemesizdir; zıplama/düşüşten inişte güncel Idle veya Move seçilir. Jump sheet'inin altıncı, yere basarken sıkışma karesi havada tutulmaz; iniş esnemesi mevcut WaterMeter'da kalır.

`Heal` trigger'ı yalnızca eksik suyla kaynağa girişte gönderilir. Kaynakta kalmak klibi yeniden başlatmaz. Heal bitince o andaki Idle, Move veya Jump'a geçilir; oynarken input ve fizik devam eder. Yeniden doğma Heal'ı temizler. Yeni bir gameplay state'i eklenmez ve mevcut ability/input akışı değiştirilmez.

Klipler yalnızca sprite değiştirir; Visual ölçeği/konumu WaterMeter'da kalır. Yeni sprite dilimleri 128 PPU, 128×128 hücre ve `(0.5, 0.47265625)` pivot kullanarak Idle'ın mevcut taban hizasını korur. Heal, `player_refill` sheet'indeki dört tam hücreyi kullanır; ayrı dilimlenmiş parıltılar tek başına kare olarak oynatılmaz.

## Kodun düzeni

- `Player`: referansları toplar, `GetComponents<BaseAbility>()` ile aynı objenin ability'lerini bulur ve aktif durumu Update/FixedUpdate üzerinden çalıştırır. GatherInput'ın yatay girdisine göre sprite yönünü günceller. Somut ability oluşturmaz; hız, doğrudan input okuma veya fizik uygulama kodu içermez.
- `StateMachine`: örnekteki `arrayOfAbilities`, `currentState` ve `previousState` yapısını kullanır. İzin verilen component'ler arasında `ExitAbility` / `EnterAbility` geçişi yapar. Aynı duruma geçiş yeniden çalıştırılmaz.
- `BaseAbility`: Player tarafından Awake içinde başlatılan ortak component tabanı; input/state/physics referanslarını ve varsa aynı objenin WaterMeter'ını bulur. Ortak düşük su katsayısını tek su değerinden hesaplar.
- `IdleAbility`: durma, hareket girişini algılama ve boşluğa düşme geçişi.
- `MoveAbility`: yerde hareket hızı, yatay fizik uygulaması ve Idle/Jump geçişleri.
- `JumpAbility`: zıplama isteği, zıplama kuvveti, havadaki hareket ve iniş geçişi.
- `GatherInput`: ayrı component olarak Input System asset'inin oyuncuya ait kopyasını yönetir; iki fizik adımı arasındaki kısa Space basışını saklayıp JumpAbility'ye bir kez verir.
- `PhysicsControl`: Rigidbody2D, gerçek collider temasları ve aşağı şekil taramasıyla zemin desteğini yönetir. Eğimde hareket yüzey teğetini izler; duruşta yokuştan kalan dikey hız ve kayma engellenir. Suya bağlı boyut isteğinde mevcut CircleCollider2D'nin yarıçapı/offset'i birlikte değişir, destek hizası korunur ve büyümede boş alan kontrol edilir. Kök ölçek ve zıplama kuvveti değiştirilmez; gövdenin dönmesi kilitlenir.
- `KnockbackAbility`: gövde temasıyla tek su kaybı, FixedUpdate itişi, kontrol kilidi/toparlanma, flash ve tekrar darbe koruması. `OnPlayerDisabled` ile bekleyen etki temizlenir.
- `DeathAbility`: kontrol/hasar görselini temizleyip Death klibini başlatır; klip tamamlanmasını WaterMeter okur. İkinci ölüm coroutine'i veya su sayacı tutmaz.
- `PlayerStates`: `Idle`, `Move`, `Jump`, `Knockback`, `Death`. Jump hem yükselmeyi hem düşmeyi kapsar.

Durum davranışları örnekteki gibi Update içinde, fizik işlemleri FixedUpdate içinde çalışır. Fizik adımında önce PhysicsControl zemin bilgisini günceller, sonra JumpAbility zıplama isteğini işler, ardından Player aktif ability'nin fizik metodunu çağırır. Kenardan yürüyerek veya durulan zeminin kalkmasıyla düşmek, yukarı itiş uygulamadan Jump'a geçirir. İnişte yatay giriş varsa Move, yoksa Idle seçilir.

Diğer sistemler `Player.CurrentState` ve `Player.IsGrounded` üzerinden oyuncunun durumunu okuyabilir. Yeni bir durum için BaseAbility türevi bir component oluşturup enum'a ekle ve Circle'a tak. Player component'i ability'yi otomatik bulur.

## Doğrulama

1 Kasım 2026 Ember/Köz Ocağı gövde teması: açık ana editörden ayrı `GameUp Hackathon/Temp/SoundSlotsTask/Verification` projesinde Unity **6000.5.0f1** ile **47 kontrol geçti (9 asset, 38 Play Mode)**. Gerçek Player ve iki düşman prefab'ıyla sağ/sol gövde teması, tam 10 puan tek su kaybı, dışarı/yukarı gerçek fizik itişi, Knockback/flash, mermiyle ortak koruma, flash boyunca ek darbe olmaması, toparlanmada ability'den çıkış ve koruma bitince yeni temas darbesi doğrulandı. Kapalı Player, kapalı collider/component, oyuncunun trigger sensörü ve öldüren temas kontrol edildi. Köz Ocağı'nın geniş alanının yalnızca sürekli kayıp/çekim üretmesi ve flash sırasında tüketimi sürdürmesi; Ember'ın gövde hasarıyla birlikte atış yapabilmesi de geçti.

Bu testte su darbesini izole eden senaryolarda normal tüketim ve düşmanın özel atış/emiş bileşeni yalnızca test örneklerinde kapatıldı; diğer senaryolarda gerçek emiş/atış açık kaldı. Sürekli örtüşme testi için fizik gövdeleri NeverSleep tutuldu; iki yöndeki itiş serbest Rigidbody2D ile ölçüldü. Güncel iki prefab, Player prefab'ı ve dört runtime dosyasının hash'leri test edilen kopyalarla eşleşti. Oyun kaynaklı hata yoktu; bilinen Unity Search başlangıç indeksleme istisnası raporda ayrı kaydedildi. [47 kontrolün sonucu](verification/ember-body-contact-result.txt), [doğrulama kodu](verification/ember-body-contact-verification.cs); ayrı projede giriş `-executeMethod EmberBodyContactVerification.Run`. Ana bölümde elle oynanış ve dış build testi bekler.

Bu döngü düzeltmesinin 1 Kasım 2026 kontrolü: Açık ana editörden ayrı `GameUp Hackathon/Temp/SoundSlotsTask/Verification` projesinde Unity **6000.5.0f1** ile bozuk davranış **29 kontrolle yeniden üretildi**; düzeltme **35 kontrolden geçti**. Gerçek Player prefab'ı ve her iki controller'da fiziksel darbe 1, ses event'i önce 20, sonra 1 ölçüldü. State'ten çıkış, flash sürerken gerçek Input System D girdisiyle hareket, tek 10 puan su kaybı, koruma sırasında darbenin reddi, sonraki yeni darbede bir yeni ses ve kapatma/açma doğrulandı. Test edilen klip gerçek dosyayla hash üzerinden eşleşti. Asıl sahnede elle oynanış ve dış build bekler.

[Önceki ölçüm](verification/knockback-loop-before.txt), [düzeltme sonrası sonuç](verification/knockback-loop-after.txt), [Editor doğrulama kodu](verification/knockback-loop-verification.cs), [event sayacı](verification/knockback-sound-probe.cs). Kodlar ayrı kopyada `Assets/Editor/KnockbackLoopVerification.cs` ve `Assets/Scripts/Audio/KnockbackSoundProbe.cs` olarak kullanılır; girişler `KnockbackLoopVerification.Reproduce` ve `.Verify` olur. Test klavyesi mevcut doğrulamalardaki IgnoreFocus / AllDeviceInputAlwaysGoesToGameView ayarını kullanır. İlk denemelerde testin Game View yönlendirmesi ve alternatif controller'ın test kopyasına alınması düzeltildi; sayılar son koşulara aittir. Oyun kaynaklı istisna yoktu; bilinen editör Search indeksleme başlangıç hatası logda ayrıdır.

31 Ekim 2026 yön çevirme düzeltmesi (T09): güncel Player prefab'ı ve oyuncu/su koduyla ayrı Unity 6000.5.0f1 projesinde 19 Play Mode kontrolü geçti. Gerçek Input System klavye olaylarıyla sağ/sol yürüyüş, Move klibinin çevrilmiş hâlde ilerlemesi, Idle ve havada son yönün korunması, havada yön değiştirme/iniş, düşük suda görsel/collider küçülmesi, kök ölçek/dönüşünün korunması, Knockback kilidi/toparlanması, Player kapatıp açma ve yeniden doğma doğrulandı. Asıl sahnede görüntü/oynanış ve build testi yapılmadı.

Component yapısı için Unity 6000.5.0f1 üzerinde 13 Play Mode testi geçti: hareket/zıplama regresyonları, component ayarlarının davranışa etkisi, aynı duruma tekrar geçiş ve ability geçiş izinleri doğrulandı. `Components.unity` Unity Editor ile açılarak altı component, Player referansları, GatherInput'taki input asset'i, Rigidbody2D ve collider bağlantıları da doğrulandı.

Görsel oynanış hissi ve dışa aktarılan build testi yapılmadı.

31 Ekim 2026 animasyon bağlantısı: Unity 6000.5.0f1 ile ayrı, küçük bir projede güncel oyuncu/su kodu, Input System, klipler ve controller kullanılarak 19 Play Mode doğrulaması geçti. Sanal klavyeyle Move/Idle, zıplama/iniş, kaynağa giriş/kalma/tekrar giriş, Heal sırasında hareket ve bitişte Move'a dönüş, yeniden doğmada temizlenme ve dolu suyla giriş kontrol edildi. Sprite referansları, klip süreleri ve taban hizası ayrıca Unity importuyla doğrulandı. Test sahnesi geçici olarak oluşturuldu; asıl Components sahnesinde oynanış hissi ve build testi yapılmadı.

31 Ekim 2026 eğim/boyut güncellemesi: güncel Player prefab'ı ve gerçek kodla ayrı Unity 6000.5.0f1 projesinde 47 Play Mode doğrulaması geçti. ±45° kuru/nemli rampalarda tam ve küçük boyda duruş/hareket/tuş bırakma artık gereksiz Jump geçişi üretmiyor. Gerçek zıplama, kısa Space bırakışı, basılı tutarak iniş, duvarın zemin sayılmaması ve kenardan normal düşüş korundu. Boyut, dolum, düşük tavan ve yeniden doğma kapsamı [su belgesindedir](water-player.md#doğrulama). Asıl Components/Level 5 sahnelerinin oynanış hissi ve build testi yapılmadı.

31 Ekim 2026 düşük su hareket artışı: güncel Player prefab'ı, ability/su/fizik kodu, Input System ve animasyonlarla ayrı Unity 6000.5.0f1 projesinde 48 Play Mode doğrulaması geçti. Tüketimi kapalı tutulan karşılaştırma testlerinde 100/50/1 su için yürüme ve hava hızı 6/7.2/8.376, kalkış hızı 10/10.5/10.99 ölçüldü. Tam basışta yatay zıplama mesafesi tam suda 12.24, 1 suda 18.93 test birimiydi; bunlar düz test geometrisindeki ölçümlerdir. Serialized Inspector alanlarının bağımsız ayarı, tüm çarpanların 1 ile kapanması, yalnızca yatay mesafe artışı, Space basılı inişte tek kalkış, erken bırakma, havada dolum/tüketimde ek dikey itiş oluşmaması, dolum/yeniden doğmada temel değerlere dönüş, hızlı küçük oyuncuyla 45° rampada durma ve WaterMeter yokken temel hareket doğrulandı. Asıl Components/Level 5 sahnelerinin oynanış/build testi yapılmadı.

31 Ekim 2026 knockback/flash: bu uygulamada Unity 6000.5.0f1 ile `C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073` projesinde **49 kontrol geçti (13 asset, 36 Play Mode)**. Güncel gerçek Player ve altı düşman prefab'ı, orijinal input/oyuncu/su kodu ve iki Animator Controller kullanıldı. Doğrulananlar: GUID ve hasar sprite fileID'sinin korunması, kurulumun tekrarlanabilmesi, tam bir temas su kesintisi, iki yönde itiş, merkez çakışmasında belirli yön, kontrol kilidi/kübik toparlanma, Space basma/bırakmayla darbeyi kesememe, gerçek Animator'da death_01 pozu, görünür/gizli flash ve bitişte normale dönüş, Heal önceliği, tekrar darbe koruması, birden çok düşman/collider ve trigger stay ile koruma sonrası yeni darbe.

Ayrıca altı gerçek düşmanın gövde teması, geniş Slime ısı alanı ve dumanın knockback üretmemesi, flash sırasında ısı/duman tüketiminin sürmesi (yaklaşık 6 su/s), Player/ability kapatma-açma temizliği, ölümcül darbede tek yeniden doğma ve tam görünür/normal kontrole dönüş, izin kapatma ve Inspector hasar/kuvvet/süre değişimleri geçti. Normal Move/Idle, tek zıplama, erken bırakma ve iniş regresyonları da geçti. Test geometrisinde Player başlangıcı sıfıra alındı ve çok kuru zemin maskesi test örneğine bağlandı. Ana sahnelerin mevcut kullanıcı değişikliklerine yazılmadı.

Düzenek `Assets/Editor/KnockbackVerification.cs`, sonuç `knockback-result.txt`, log `knockback-verification.log` dosyalarındadır. Test edilmiş prefab/controller/klip ve hedef sprite meta'sı ana projeye aktarıldı; yeni component'lerin gerçek Unity meta dosyaları eşleşti. Logda önceki geçici proje kayıtlarındaki bağımsız `UnityEditor.Search.SearchDatabase` başlangıç hatası vardır; C# derlemesi ve 49 kontrol başarılıdır. Ana sahnede oynanış hissi/görüntü ve dış build testi bekliyor.
