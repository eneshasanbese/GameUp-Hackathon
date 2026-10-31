# Player state machine

T09 kapsamı: sağ/sol hareket, tek zıplama, yere temas ve düşüş. Yapı, paylaşılan örnekteki gibi `BaseAbility : MonoBehaviour` üzerinden ayrı ability component'leri kullanır.

## Kullanım

`Assets/Scenes/Components.unity` sahnesini açıp Play'e bas. A/D veya sol/sağ ok hareket ettirir; Space zıplatır. Yükselirken Space bırakılırsa Y hızı sıfırlanır ve yerçekimiyle düşüş başlar. Havada yön değiştirilebilir. Space basılı tutulduğunda inişte otomatik tekrar zıplanmaz; havada ikinci zıplama yoktur.

Player üzerindeki component ayarları:

- **GatherInput → Input Actions:** `Assets/PlayerInput.inputactions` bağlıdır. Asset içinde `Player/Move` (Vector2) ve `Player/Jump` (Button) bulunmalı.
- **MoveAbility → Speed / Acceleration:** yerdeki yatay hız ve hedef hıza ulaşma ivmesi.
- **IdleAbility → Deceleration:** tuş bırakıldıktan sonraki kısa duruşun yavaşlama değeri.
- **JumpAbility → Airspeed / Air Acceleration / Jump Force:** havadaki yatay hız, yatay ivme ve zıplamanın başlangıç dikey hızı. Yerçekimini Rigidbody2D yönetir.
- **PhysicsControl → Dry Ground Layer / Wet Ground Layer:** Ground-Dry ve Ground-Wet katmanlarını iki ayrı raycast ile kontrol eder.
- **PhysicsControl → Ground Check Offset / Ground Ray Distance:** ray başlangıcının Player merkezine göre konumu ve aşağı doğru uzunluğu.
- **Ability → Is Permitted:** ilgili duruma geçiş izni.

`Components.unity` sahnesindeki Player'a `Player`, `GatherInput`, `PhysicsControl`, `IdleAbility`, `MoveAbility` ve `JumpAbility` component'leri bağlıdır. Player referansları ve input asset'i sahnede ayarlanmıştır. Player Inspector'ında hareket veya zıplama ayarı bulunmaz. Su miktarı, görseller ve kaynaklar [su damlası prototipi belgesinde](water-player.md) anlatılır.

Başka bir Circle'a Player eklemek GatherInput, PhysicsControl, Rigidbody2D ve CircleCollider2D gereksinimlerini ekler. Ayrıca IdleAbility, MoveAbility ve JumpAbility component'lerini ekleyip GatherInput'a Input Actions asset'ini bağla. Rigidbody2D Dynamic ve Gravity Scale pozitif olmalı. Zeminde trigger olmayan Collider2D bulunmalı.

## Kodun düzeni

- `Player`: referansları toplar, `GetComponents<BaseAbility>()` ile aynı objenin ability'lerini bulur ve aktif durumu Update/FixedUpdate üzerinden çalıştırır. Somut ability oluşturmaz; hız, input okuma veya fizik uygulama kodu içermez.
- `StateMachine`: örnekteki `arrayOfAbilities`, `currentState` ve `previousState` yapısını kullanır. İzin verilen component'ler arasında `ExitAbility` / `EnterAbility` geçişi yapar. Aynı duruma geçiş yeniden çalıştırılmaz.
- `BaseAbility`: Player tarafından Awake içinde başlatılan ortak component tabanı; `linkedInput`, `linkedStateMachine` ve `linkedPhysicsControl` referanslarını bağlar.
- `IdleAbility`: durma, hareket girişini algılama ve boşluğa düşme geçişi.
- `MoveAbility`: yerde hareket hızı, yatay fizik uygulaması ve Idle/Jump geçişleri.
- `JumpAbility`: zıplama isteği, zıplama kuvveti, havadaki hareket ve iniş geçişi.
- `GatherInput`: ayrı component olarak Input System asset'inin oyuncuya ait kopyasını yönetir; iki fizik adımı arasındaki kısa Space basışını saklayıp JumpAbility'ye bir kez verir.
- `PhysicsControl`: ayrı component olarak Rigidbody2D ve iki katmana ayrı raycast ile zemin algılamayı yönetir. Collider boyutu değiştirilmez; gövdenin dönmesi kilitlenir.
- `PlayerStates`: `Idle`, `Move`, `Jump`. Jump hem yükselmeyi hem düşmeyi kapsar.

Durum davranışları örnekteki gibi Update içinde, fizik işlemleri FixedUpdate içinde çalışır. Fizik adımında önce PhysicsControl zemin bilgisini günceller, sonra JumpAbility zıplama isteğini işler, ardından Player aktif ability'nin fizik metodunu çağırır. Kenardan yürüyerek veya durulan zeminin kalkmasıyla düşmek, yukarı itiş uygulamadan Jump'a geçirir. İnişte yatay giriş varsa Move, yoksa Idle seçilir.

Diğer sistemler `Player.CurrentState` ve `Player.IsGrounded` üzerinden oyuncunun durumunu okuyabilir. Yeni bir durum için BaseAbility türevi bir component oluşturup enum'a ekle ve Circle'a tak. Player component'i ability'yi otomatik bulur.

## Doğrulama

Component yapısı için Unity 6000.5.0f1 üzerinde 13 Play Mode testi geçti: hareket/zıplama regresyonları, component ayarlarının davranışa etkisi, aynı duruma tekrar geçiş ve ability geçiş izinleri doğrulandı. `Components.unity` Unity Editor ile açılarak altı component, Player referansları, GatherInput'taki input asset'i, Rigidbody2D ve collider bağlantıları da doğrulandı.

Görsel oynanış hissi ve dışa aktarılan build testi yapılmadı.
