# Köz Püskürten — EmberSpitter

31 Ekim 2026 açık kullanıcı talebi: verilen EmberPack görselleriyle sabit atıcı ve ateş topu. Bu, genel düşman kapsamı yasağına yalnızca bu davranış için istisnadır. İlgili alanlar T12/T19/T25–T26, T42 ve QA06–QA09/QA16'dır; genel kabul kutularını kapatmaz.

## Kullanım ve dosyalar

- `GameUp Hackathon/Assets/Prefabs/Enemies/EmberSpitter.prefab`: sahneye sürükle; zemine veya havaya yerleştirilebilir. Yatay konumu kilitlidir; yerçekimiyle düşüp `TerrainBody` üzerinden zemine oturur. 1 Kasım 2026 son talebiyle kökte ayrı gövde trigger'ı ve `EnemyContactKnockback` de vardır; temas mevcut Player hasar/knockback sistemini kullanır.
- `GameUp Hackathon/Assets/Prefabs/Enemies/EmberFireball.prefab`: atıcının `Projectile Prefab` alanına bağlıdır; elle sahneye yerleştirilmesi gerekmez.
- `GameUp Hackathon/Assets/Scenes/EmberSpitterDemo.unity`: gerçek Player prefab'ı, mevcut Canvas/WaterBarUI, düz zemin, görüş duvarı ve 0.05 birim kalınlığında platform içeren ayrı alan. A/D veya oklar hareket, Space zıplama, R veya ekrandaki düğme başlangıca dönüş. Ana sahneler ve build sahne listesi değiştirilmedi.
- `Assets/Scripts/Enemies/EmberSpitter.cs`, `EmberFireball.cs`: çalışma zamanı davranışı. `EmberSpitterDemoControls.cs` yalnızca test alanının kontrolüdür.
- `Assets/Editor/Enemies/EmberSpitterPrefabBuilder.cs`: `GameUp → Enemies → Rebuild Ember Spitter Prefabs / Demo` menüleri. Hazır asset'leri kullanmak için çalıştırmak gerekmez. Yeniden üretim bu iki prefab/test sahnesinin ayarlarını başlangıç değerlerine döndürür.
- `Assets/Animations/Enemies/EmberSpitterIdle.anim` ve `.controller`: Visual'da 12 kare/12 FPS/1 saniye Idle döngüsü. `GameUp → Enemies → Configure Ember Spitter Idle and Gravity` mevcut prefab'ın atış ayarlarını ve nesne kimliklerini koruyarak animasyon/fizik bağlantılarını onarır.
- `Assets/Editor/Enemies/EmberSpitterVerification.cs`: ayrı batch projesinde `EmberSpitterVerification.GenerateAndRun` giriş noktası.
- Mevcut `Player.cs`: kapanışta `GameplayStopped` olayı. Mevcut `KnockbackAbility.cs`: aynı flash korumasını kullanan `TryWaterHit(float)` girişi; mermi ek itiş uygulamaz.

## Inspector ve fizik katmanları

| Atıcı alanı | Başlangıç değeri / bağlantı |
|---|---|
| Detection Radius | 5 dünya birimi; görselin ölçeğinden bağımsız |
| Windup Duration | 0.8 saniye |
| Cooldown Duration | 2.5 saniye; hazırlığın ardından başlar |
| Projectile Prefab | EmberFireball |
| Range Origin | Kök altında `(0, 0.85)` |
| Fire Point Right / Left | Kök altında `(±0.64, 0.95)`; Visual esnemesinden etkilenmez |
| Visual | SpriteRenderer + Idle Animator; flipX, hafif sıkışma ve renk uyarısı atış kodundan |
| Player Layer Mask | Player (8) |
| Obstacle Layer Mask | Ground-Dry (6), Ground-Wet (7), Ground-Extra-Dry (10) |
| Rigidbody2D | Dynamic, Gravity Scale 1, Freeze Position X + Freeze Rotation, Continuous, Interpolate |
| TerrainBody | GroundEnemy katmanında 0.9×1.2 BoxCollider2D; merkez `(0,0.6)`, ayak tabanı kök seviyesinde |

| Mermi alanı | Başlangıç değeri / bağlantı |
|---|---|
| Speed | 4 dünya birimi/saniye |
| Damage Water Points | Tek isabette sabit 10 puan; yüzde/saniyelik oran değildir |
| Max Lifetime | 4 saniye oyun süresi |
| Player / Obstacle Layer Mask | Yukarıdaki maskeler; fırlatıldığında atıcının maskeleri kullanılır |
| Visual / Rotation Offset | Visual dönüş merkezi / 0°; kaynak PNG +X yönüne bakar |
| CircleCollider2D | Merkez `(0,0)`, yarıçap 0.13; uzun görsel kuyruk hasar alanı değildir |
| Rigidbody2D | Kinematic, Gravity Scale 0, Freeze Rotation, Interpolate |

Atıcı GroundEnemy (12), mermi Default (0) katmanındadır. Yeni katman veya çarpışma matrisi değişikliği yoktur. TerrainBody, diğer yer düşmanlarıyla aynı Slip malzemesini ve yalnızca Ground-Dry/Ground-Wet/Ground-Extra-Dry/EnemyBoundary include maskesini kullanır; diğer katmanlar exclude edilir. Oyuncuyu veya başka düşmanı fiziksel olarak itmez. Engelleyici duvar/platformlar yukarıdaki zemin maskelerinden birinde ve trigger olmayan Collider2D ile bulunmalı. Yeni bir engel katmanı kullanılırsa atıcının Obstacle Layer Mask'ine eklenmelidir. Algılama ve görüş sorguları trigger'ları/dekoru dışarıda tutar; kendi kökünün altındaki collider'ları atlar.

## Atış ve su entegrasyonu

Idle → Windup → tek Fire → Cooldown → Idle. Algılama, Player component'i olan Rigidbody2D'nin gerçek konumundan yapılır; atış Rigidbody2D `worldCenterOfMass` noktasını hedefler. Sağ/sol ağız hedefe göre seçilir, gövde dönmez. Hazırlık bitiminde hedefin etkinliği, mesafesi, görüşü ve çıkış noktasının güvenliği yeniden denetlenir. İptal edilen atışın ardından 0.25 saniye beklenir. Kökle ağız arasındaki süpürme ve iki uçtaki örtüşme kontrolü, ağız noktasının ince duvarın ötesine taşarak atış yapmasını engeller.

Mermi fırlatma anındaki yönünü bir kez alır; tahmin, takip, yerçekimi, sekme veya alan hasarı yoktur. FixedUpdate'te yalnızca Rigidbody2D.MovePosition ile ilerler. Her adımda çekirdeğin başlangıç örtüşmesi ve tüm yol boyunca CircleCast yapılır. En yakın geçerli çarpışma seçilir; eşit mesafede dünya engeli önceliklidir. Trigger callback'i ayrıca hareketli oyuncunun teması için kullanılır. Tek kullanım kilidi, collider/fizik kapatılması hasardan önce uygulanır; çoklu collider ve aynı adımda gelen olaylar ikinci hasar üretemez.

`KnockbackAbility.TryWaterHit` mevcut flash/temas dokunulmazlığını denetler ve `WaterMeter.Consume(damageWaterPoints)` işlemini bir kez yapar. Koruma mermiyi reddetse de mermi tüketilir. Ayrı can/su sayacı yoktur; suyu sınırlama ve ölüm/yeniden doğma mevcut WaterMeter akışındadır. KnockbackAbility bulunmayan bir Player'da doğrudan mevcut Consume kullanılır.

Oyuncu ölüm, yeniden doğma veya mevcut LevelExit geçişi nedeniyle kapandığında `GameplayStopped` hem hazırlığı hem eski mermileri anında iptal eder. Player/WaterMeter etkinliği, fizik simülasyonu ve `SceneFader.IsBusy` ayrıca kontrol edilir. Atıcının kapanışı kendi mermilerini temizler; olay abonelikleri kaldırılır. Pause sırasında fizik zamanı ilerlemez, atış/hasar çalışmaz. Bağımsız bir final/kazanma yöneticisi mevcut değildir; ileride kazanma Player'ı kapatan mevcut sözleşmeyi kullanmalıdır.

## Görseller

1 Kasım 2026 gövde teması: kökteki **BoxCollider2D**, Size `(0.9, 1.2)`, Offset `(0, 0.6)` ve Is Trigger açık olarak yalnızca Player temasını algılar. `EnemyContactKnockback` mevcut `KnockbackAbility.TryKnockback` akışını kullanır; hasar/kuvvet/flash ayarları Player'dadır. Katı `TerrainBody` ve atış algılama/görüş katmanları korunur. Mermi ile gövde aynı flash dokunulmazlığını paylaşır. Ayrı script/event ataması gerekmez. Önceki doğrulamalardaki hasarsız gövdeden geçiş sonucu, o tarihteki kapsamın tarihsel kaydıdır.

1 Kasım 2026 atış sesi (T52): `Assets/Prefabs/Enemies/EmberSpitter.prefab` kökündeki **Ember Spitter → Sounds → Fire Sound** alanına `Assets/platformerPack/Sounds/sfx_throw.ogg` bağlandı. Mermi gerçekten oluşturulup fırlatıldığı anda koddan bir kez çalar; atış animasyonu veya Animation Event gerekmez. Hazırlık ve iptal edilen atış bu sesi üretmez. Sound Volume 0.7, Hearing Distance 12 birimdir. Su kaynağı temas düzeltmesiyle birlikte ayrı Unity projesinde 26 kontrol geçti; [alanlar ve sonuç](ses-kurulumu.md#çalışma-ve-kontrol).

31 Ekim 2026 sonraki kullanıcı talebiyle ilk sürümdeki tek kareli Idle yerine hazır Idle animasyonu bağlandı. İncelemede önceki prefab'da Animator bulunmadığı ve yalnızca `Frames/Idle/Idle_00.png` gösterildiği doğrulandı.

- `EmberPack/EmberSpitter/Unity_128/Idle.png`: 512×384 sheet, 12 adet 128×128 hücre ve mevcut `(0.5,0.16)` ortak pivot. `Idle_00`–`Idle_11` 12 FPS ile bir saniyede döner. Karelerin dış sınırı sabittir; verilen çizimin Idle etkisi gövdedeki hafif ışıma değişimidir. Visual ölçeği 1.75; şeffaf boşluk/pivot nedeniyle yerel Y yaklaşık 0.04758 ile ayak hizası korunur.
- `EmberPack/EmberFireball/Unity_128/Frames/Fly/Fly_00.png`: 128×128 tuvalde mevcut 91×63 kırpım; 0.65× görsel ölçeği. İç Sprite, parlak yuvarlak çekirdeği Visual dönüş merkezine getirecek şekilde kaydırılmıştır.

Kullanılan Idle sheet ve ateş topu 128 PPU, Bilinear, mipmap kapalı ve sıkıştırmasızdır. Sheet'in mevcut hücreleri, spriteID/fileID/GUID ve pivot'ları korunur. Animasyon yalnızca SpriteRenderer.sprite değiştirir; Transform eğrisi/root motion olmadığı için atış uyarısının ölçeğini, ağız noktalarını ve fizik gövdesini etkilemez. Yeni çizim veya paket yoktur; görseller kullanıcı tarafından sağlandı.

## Doğrulama

1 Kasım 2026 gövde teması güncellemesi: gerçek Player, Ember ve Köz Ocağı prefab'larıyla ayrı Unity 6000.5.0f1 projesinde **47 kontrol geçti (9 asset, 38 Play Mode)**. Sağ/sol gövde darbesi, tek su kaybı ve fizik itişi, ortak mermi/gövde flash koruması, toparlanma, koruma sonrası yeni darbe ve öldüren temas doğrulandı. Ember'ın atış hazırlığı/mermi üretimi çalışır; algılama alanı gövde knockback'i başlatmaz. İki prefab'ın hash'leri test kopyalarıyla eşleşti. [Kapsam ve sonuç](player-state-machine.md#doğrulama). Ana bölümde elle oynanış ve dış build testi bekler.

Aşağıdaki 65 kontrol ilk tek kareli, yerçekimsiz sürümün tarihsel kaydıdır. Idle/yerçekimi güncellemesinin yeni sonuçları bu belgenin sonundadır.

31 Ekim 2026, Unity **6000.5.0f1** ile `C:/Users/yhcan/AppData/Local/Temp/gameup-ember-spitter-d7hi3iu9` ayrı projesinde **65 kontrol geçti: 12 asset + 53 Play Mode**. C# derlemesi başarılıdır. Güncel Player/Canvas prefab'ları, gerçek oyuncu/su/ability kodu, Input System, animasyonlar ve verilen iki PNG kullanıldı. Mevcut kurulu Input System/UGUI paketleri yerel cache'den kullanıldı; ana projeye paket eklenmedi. Asset'ler gerçek Unity PrefabUtility/SceneManager ile üretildi; test edilmiş iki prefab/sahne ve yalnızca iki karenin import ayarı ana projeye aktarıldı. Script GUID'leri ana editörle karşılaştırıldı.

[Sonuç](verification/ember-spitter-result.txt), [65 kontrolün listesi](verification/ember-spitter-checks.txt), [1280×720 kamera görüntüsü](verification/ember-spitter-demo.png). Tam log geçici projenin `ember-spitter-verification.log` dosyasındadır. Tekrarlama girişi `-batchmode -projectPath "<ayrı proje>" -executeMethod EmberSpitterVerification.GenerateAndRun -logFile "<log yolu>"`; kaynaklar ve gerekli prefab/sprite/input bağımlılıkları kopyalanmış, aynı sürümde ayrı bir proje kullanılmalıdır. Açık ana projede ikinci batch editör çalıştırılmamalıdır.

Doğrulananlar: menzil/görüş, 0.8 saniye hazırlık ve tek atış/cooldown, iki iptal durumu, kök/ağız/duvar örtüşmesi, sağ/sol/yukarı/aşağı yönler, collider merkezindeki offset'e doğru nişan ve takip etmeme, sabit hasar, çoklu collider/aynı anda iki mermi, mermi ve gövdenin ortak dokunulmazlığı, üç fizik adımında (0.01/0.02/0.033333) 300 birim/s merminin 0.02 kalınlığında engelde durması, trigger'ları geçme, lifetime, tek ölüm/yeniden doğma, aynı kare disable/enable ve olay temizliği. Gerçek klavye input'uyla normal hareket/zıplama ve pozitif bekleme tüketimi de geçti.

Başlangıç değerleriyle 3 birimden gelen atıştan, uyarı sonrası normal zıplamayı 0.6 saniye basılı tutarak su kaybetmeden kaçıldı. Çok kısa Space bırakışı mevcut JumpAbility'de yukarı hızı sıfırlar; erken iniş mermiyle çakışabilir. Bu nedenle deneme değerleri bütün mesafe ve kısa basışlarda kaçış garantisi veya nihai denge sonucu değildir.

Kaydedilmiş **EmberSpitterDemo** ayrıca Play Mode'da yüklendi: güvenli başlangıç, gerçek atıcıdan hasar, WaterBarUI dolgusunun anında güncellenmesi ve R ile tam suyla başlangıca dönüş/eski mermilerin temizlenmesi geçti. Kamera görüntüsü görsel olarak incelendi; sprite ölçekleri ve zemin hizası uygundur. Kamera RenderTexture görüntüsü ekran üstü HUD/OnGUI katmanını içermez; HUD bağlantısı ve değeri ayrıca gerçek bileşen üzerinden doğrulandı.

Logda Unity'nin editör başlangıcındaki `UnityEditor.Search.SearchDatabase.EnumerateAll` kaynaklı bağımsız `ArgumentOutOfRangeException` kaydı vardır; oyun/test kodu kaynaklı hata yoktur ve 65 kontrol tamamlanmıştır. Ana Level_1/Level_2 sahnelerinde elle oynanış, tam final/kazanma akışı ve dışa aktarılan build testi yapılmadı. Mevcut final yöneticisi olmadığı için kazanma koruması, LevelExit'in kullandığı Player/fizik kapanış sözleşmesi üzerinden sınandı. Genel T/QA kutuları kapatılmadı.

### Idle ve yerçekimi güncellemesinin doğrulaması

31 Ekim 2026 bu güncellemede aynı ayrı Unity **6000.5.0f1** projesine güncel kaynak/prefab'lar kopyalandı ve `EmberSpitterVerification.UpgradeAndRun` çalıştırıldı: **80 kontrol geçti (14 asset, 66 Play Mode)**. Bu sonuç eski 65 kontrolün tekrar aktarımı değildir; tüm atış/hasar/yaşam döngüsü regresyonları yeni prefab ile yeniden koşuldu.

Yeni kontroller: gerçek Animator'ın hedef olmadan 12 farklı Idle sprite'ını birden fazla döngü boyunca göstermesi; klibin 12 FPS/1 saniye olması; Transform eğrisi olmaması; üç kuruluk zemininde havadan düşme/zemine oturma ve zemin kaldırılınca tekrar düşme; X/dönüş kilidi; oyuncunun gövdeden itilmeden ve hasar almadan geçmesi; merminin sahibinin yeni collider'ından geçip oyuncuyu vurması. Mevcut prefab'a iki kez uygulanan yükseltmede prefab GUID'i ile kök, EmberSpitter component'i ve ağız fileID'leri korundu; atış ayarları değişmedi.

[Yeni sonuç](verification/ember-spitter-idle-gravity-result.txt), [80 kontrol](verification/ember-spitter-idle-gravity-checks.txt), [güncel kamera görüntüsü](verification/ember-spitter-idle-gravity-demo.png). Tam log geçici projenin `ember-spitter-idle-gravity.log` dosyasında. Test edilmiş prefab, yeni animasyon/controller ve sheet'in yalnızca Compression None değişikliği ana projeye aktarıldı; demo sahnesini yeniden üretmek gerekmedi. Ana bölümde elle oynanış ve dış build testi bekliyor.
