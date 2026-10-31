# Player state machine

T09 kapsamı: sağ/sol hareket, tek zıplama, yere temas ve düşüş. Yapı, paylaşılan örnekteki gibi `BaseAbility : MonoBehaviour` üzerinden ayrı ability component'leri kullanır.

## Kullanım

`Assets/Scenes/Components.unity` sahnesini açıp Play'e bas. A/D veya sol/sağ ok hareket ettirir; Space zıplatır. Yükselirken Space bırakılırsa Y hızı sıfırlanır ve yerçekimiyle düşüş başlar. Havada yön değiştirilebilir. Space basılı tutulduğunda inişte otomatik tekrar zıplanmaz; havada ikinci zıplama yoktur.

Player üzerindeki component ayarları:

- **GatherInput → Input Actions:** `Assets/PlayerInput.inputactions` bağlıdır. Asset içinde `Player/Move` (Vector2) ve `Player/Jump` (Button) bulunmalı.
- **MoveAbility → Speed / Acceleration:** yerdeki yatay hız ve hedef hıza ulaşma ivmesi.
- **IdleAbility → Deceleration:** tuş bırakıldıktan sonraki kısa duruşun yavaşlama değeri.
- **JumpAbility → Airspeed / Air Acceleration / Jump Force:** havadaki yatay hız, yatay ivme ve zıplamanın başlangıç dikey hızı. Yerçekimini Rigidbody2D yönetir.
- **PhysicsControl → Dry Ground Layer / Wet Ground Layer:** Ground-Dry ve Ground-Wet katmanlarındaki destek temaslarını ve aşağı collider taramasını filtreler.
- **PhysicsControl → Ground Check Distance / Maximum Ground Angle:** Güncel collider boyutuna göre tarama mesafesi ve üzerinde durulabilecek en dik yüzey açısı. Prefab değerleri 0.15 ve 60 derecedir.
- **WaterMeter → Water Size → Minimum Scale:** Su sıfıra yaklaşırken görsel ve collider'ın minimum boyutu; prefab'da 0.45. Daha küçük değer daha fazla küçültür.
- **MoveAbility → Low Water Boost → Low Water Speed Multiplier:** Düşük suda yürüme hızı çarpanı; prefab'da 1.4.
- **JumpAbility → Low Water Boost:** Low Water Air Speed Multiplier=1.4, Low Water Jump Multiplier=1.1; hava hızı ve kalkış hızı ayrı ayarlanır.
- **Ability → Is Permitted:** ilgili duruma geçiş izni.

`Components.unity` sahnesindeki Player'a `Player`, `GatherInput`, `PhysicsControl`, `IdleAbility`, `MoveAbility` ve `JumpAbility` component'leri bağlıdır. Player referansları ve input asset'i sahnede ayarlanmıştır. Player Inspector'ında hareket veya zıplama ayarı bulunmaz. Su miktarı, görseller ve kaynaklar [su damlası prototipi belgesinde](water-player.md) anlatılır.

Başka bir Circle'a Player eklemek GatherInput, PhysicsControl, Rigidbody2D ve CircleCollider2D gereksinimlerini ekler. Ayrıca IdleAbility, MoveAbility ve JumpAbility component'lerini ekleyip GatherInput'a Input Actions asset'ini bağla. Rigidbody2D Dynamic ve Gravity Scale pozitif olmalı. Zeminde trigger olmayan Collider2D bulunmalı.

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

`State` int parametresi mevcut `PlayerStates` değerlerini izler: Idle=0, Move=1, Jump=2. Hareket durumları arasında geçiş beklemesizdir; zıplama/düşüşten inişte güncel Idle veya Move seçilir. Jump sheet'inin altıncı, yere basarken sıkışma karesi havada tutulmaz; iniş esnemesi mevcut WaterMeter'da kalır.

`Heal` trigger'ı yalnızca eksik suyla kaynağa girişte gönderilir. Kaynakta kalmak klibi yeniden başlatmaz. Heal bitince o andaki Idle, Move veya Jump'a geçilir; oynarken input ve fizik devam eder. Yeniden doğma Heal'ı temizler. Yeni bir gameplay state'i eklenmez ve mevcut ability/input akışı değiştirilmez.

Klipler yalnızca sprite değiştirir; Visual ölçeği/konumu WaterMeter'da kalır. Yeni sprite dilimleri 128 PPU, 128×128 hücre ve `(0.5, 0.47265625)` pivot kullanarak Idle'ın mevcut taban hizasını korur. Heal, `player_refill` sheet'indeki dört tam hücreyi kullanır; ayrı dilimlenmiş parıltılar tek başına kare olarak oynatılmaz.

## Kodun düzeni

- `Player`: referansları toplar, `GetComponents<BaseAbility>()` ile aynı objenin ability'lerini bulur ve aktif durumu Update/FixedUpdate üzerinden çalıştırır. Somut ability oluşturmaz; hız, input okuma veya fizik uygulama kodu içermez.
- `StateMachine`: örnekteki `arrayOfAbilities`, `currentState` ve `previousState` yapısını kullanır. İzin verilen component'ler arasında `ExitAbility` / `EnterAbility` geçişi yapar. Aynı duruma geçiş yeniden çalıştırılmaz.
- `BaseAbility`: Player tarafından Awake içinde başlatılan ortak component tabanı; input/state/physics referanslarını ve varsa aynı objenin WaterMeter'ını bulur. Ortak düşük su katsayısını tek su değerinden hesaplar.
- `IdleAbility`: durma, hareket girişini algılama ve boşluğa düşme geçişi.
- `MoveAbility`: yerde hareket hızı, yatay fizik uygulaması ve Idle/Jump geçişleri.
- `JumpAbility`: zıplama isteği, zıplama kuvveti, havadaki hareket ve iniş geçişi.
- `GatherInput`: ayrı component olarak Input System asset'inin oyuncuya ait kopyasını yönetir; iki fizik adımı arasındaki kısa Space basışını saklayıp JumpAbility'ye bir kez verir.
- `PhysicsControl`: Rigidbody2D, gerçek collider temasları ve aşağı şekil taramasıyla zemin desteğini yönetir. Eğimde hareket yüzey teğetini izler; duruşta yokuştan kalan dikey hız ve kayma engellenir. Suya bağlı boyut isteğinde mevcut CircleCollider2D'nin yarıçapı/offset'i birlikte değişir, destek hizası korunur ve büyümede boş alan kontrol edilir. Kök ölçek ve zıplama kuvveti değiştirilmez; gövdenin dönmesi kilitlenir.
- `PlayerStates`: `Idle`, `Move`, `Jump`. Jump hem yükselmeyi hem düşmeyi kapsar.

Durum davranışları örnekteki gibi Update içinde, fizik işlemleri FixedUpdate içinde çalışır. Fizik adımında önce PhysicsControl zemin bilgisini günceller, sonra JumpAbility zıplama isteğini işler, ardından Player aktif ability'nin fizik metodunu çağırır. Kenardan yürüyerek veya durulan zeminin kalkmasıyla düşmek, yukarı itiş uygulamadan Jump'a geçirir. İnişte yatay giriş varsa Move, yoksa Idle seçilir.

Diğer sistemler `Player.CurrentState` ve `Player.IsGrounded` üzerinden oyuncunun durumunu okuyabilir. Yeni bir durum için BaseAbility türevi bir component oluşturup enum'a ekle ve Circle'a tak. Player component'i ability'yi otomatik bulur.

## Doğrulama

Component yapısı için Unity 6000.5.0f1 üzerinde 13 Play Mode testi geçti: hareket/zıplama regresyonları, component ayarlarının davranışa etkisi, aynı duruma tekrar geçiş ve ability geçiş izinleri doğrulandı. `Components.unity` Unity Editor ile açılarak altı component, Player referansları, GatherInput'taki input asset'i, Rigidbody2D ve collider bağlantıları da doğrulandı.

Görsel oynanış hissi ve dışa aktarılan build testi yapılmadı.

31 Ekim 2026 animasyon bağlantısı: Unity 6000.5.0f1 ile ayrı, küçük bir projede güncel oyuncu/su kodu, Input System, klipler ve controller kullanılarak 19 Play Mode doğrulaması geçti. Sanal klavyeyle Move/Idle, zıplama/iniş, kaynağa giriş/kalma/tekrar giriş, Heal sırasında hareket ve bitişte Move'a dönüş, yeniden doğmada temizlenme ve dolu suyla giriş kontrol edildi. Sprite referansları, klip süreleri ve taban hizası ayrıca Unity importuyla doğrulandı. Test sahnesi geçici olarak oluşturuldu; asıl Components sahnesinde oynanış hissi ve build testi yapılmadı.

31 Ekim 2026 eğim/boyut güncellemesi: güncel Player prefab'ı ve gerçek kodla ayrı Unity 6000.5.0f1 projesinde 47 Play Mode doğrulaması geçti. ±45° kuru/nemli rampalarda tam ve küçük boyda duruş/hareket/tuş bırakma artık gereksiz Jump geçişi üretmiyor. Gerçek zıplama, kısa Space bırakışı, basılı tutarak iniş, duvarın zemin sayılmaması ve kenardan normal düşüş korundu. Boyut, dolum, düşük tavan ve yeniden doğma kapsamı [su belgesindedir](water-player.md#doğrulama). Asıl Components/Level 5 sahnelerinin oynanış hissi ve build testi yapılmadı.

31 Ekim 2026 düşük su hareket artışı: güncel Player prefab'ı, ability/su/fizik kodu, Input System ve animasyonlarla ayrı Unity 6000.5.0f1 projesinde 48 Play Mode doğrulaması geçti. Tüketimi kapalı tutulan karşılaştırma testlerinde 100/50/1 su için yürüme ve hava hızı 6/7.2/8.376, kalkış hızı 10/10.5/10.99 ölçüldü. Tam basışta yatay zıplama mesafesi tam suda 12.24, 1 suda 18.93 test birimiydi; bunlar düz test geometrisindeki ölçümlerdir. Serialized Inspector alanlarının bağımsız ayarı, tüm çarpanların 1 ile kapanması, yalnızca yatay mesafe artışı, Space basılı inişte tek kalkış, erken bırakma, havada dolum/tüketimde ek dikey itiş oluşmaması, dolum/yeniden doğmada temel değerlere dönüş, hızlı küçük oyuncuyla 45° rampada durma ve WaterMeter yokken temel hareket doğrulandı. Asıl Components/Level 5 sahnelerinin oynanış/build testi yapılmadı.
