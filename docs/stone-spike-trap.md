# Düşen taş tuzağı

1 Kasım 2026 kullanıcı talebi: `Level_2.unity` içindeki `Trap_stoneSpike.prefab`, oyuncu yaklaşınca bir kez düşer. Kırılma animasyonu yerine zeminin arkasından aşağı geçip silinir. Kullanıcı temasın mevcut su kaybı/geri itme sistemini kullanmasını da onayladı. T30/T42 ve QA06–QA09 ile ilişkili ek kapsamdır; genel kabul kutularını kapatmaz.

## Prefab ve ayarlar

`Assets/Prefabs/Traps/Trap_stoneSpike.prefab` doğrudan bağlıdır; sahnede oyuncu referansı veya ek sensör gerektirmez. Özgün prefab GUID'i, kök/Transform/SpriteRenderer fileID'leri, sprite, materyal, konum ve aşağı bakan dönüş korunur. `Level_2` dosyasına yazılmaz; mevcut instance davranışı prefab'dan alır.

`FallingStoneTrap` Inspector ayarları:

| Alan | İlk deneme değeri | İşlev |
|---|---|---|
| Detection Size | `(3, 7)` | Dünya biriminde algılama kutusunun genişliği/yüksekliği |
| Detection Offset | `(0, -3.5)` | Kökten dünya eksenlerinde kutu merkezi; kutu taşın altına uzanır |
| Player Layer Mask | `Player` | Yalnızca etkin Player'ın fizik collider'ını algılar |
| Gravity Scale | `3` | Serbest bırakıldıktan sonraki yerçekimi çarpanı |
| Maximum Fall Speed | `14` | Aşağı hızın fizik adımlarında sınırlandırıldığı değer |
| Despawn Delay | `3` saniye | Düşüşün başlamasından sonra silinme süresi |

Algılama kutusu taşın 180° dönüşünden bağımsız olarak aşağı uzanır; seçili nesnede **Gizmos** açıkken turuncu çizilir. Başlangıçta Rigidbody2D Kinematic, yerçekimsiz ve hasar collider'ı kapalıdır. Algılama, ek bir trigger oluşturmadan her fizik adımında sorgulanır; oyuncu olmayan nesneler, trigger sensörleri, kapalı collider/fizik/kontrol ve yeniden doğan oyuncu düşüş başlatmaz.

Tetiklenince gövde Dynamic olur. Yatay konum ve dönüş kilitlidir; oyuncu uzaklaşsa da düşüş sürer. Sprite biçimindeki PolygonCollider2D bir trigger'dır ve yalnızca Player ile etkileşir; üç zemin türünden, diğer tuzaklardan ve düşmanlardan geçer. SpriteRenderer **Level / Order -1** kullanır; `Level_2` zeminlerinin **Level / Order 0** çiziminin gerisinde, Background'ın önündedir. Yeni sorting/physics layer veya collision matrix değişikliği yoktur.

İlk oyuncu teması `KnockbackAbility.TryKnockback` çağırır; hasar Player'ın **Contact Water Loss** alanından gelir (mevcut varsayılan `10` su puanı). Geri itme ve flash dokunulmazlığı da aynı bileşenden gelir. Taşın hasar collider'ı ilk temasta kapanır; flash korumasının reddettiği teması sonradan tekrar denemez. Yeni su sayacı, ölüm veya yeniden doğma akışı yoktur. Taş hasar verdikten sonra da aşağı gidip süresi dolunca silinir.

Tuzak tek kullanımlıktır; sahne yeniden yüklenince geri gelir. Bileşeni kapatmak düşüşü/hasarı durdurur; yeniden açmak kalan süreyle sürdürür. Süre oyun zamanıyladır, dolayısıyla pause sırasında ilerlemez. Değerler deneme ayarıdır; bölümde oynanış dengesiyle doğrulanmalıdır.

## Doğrulama

Doğrulama kodu `Assets/Editor/Traps/FallingStoneTrapVerification.cs` içindedir. Açık ana projeyi etkilememek için aynı Unity sürümüyle ayrı batch projede `-executeMethod FallingStoneTrapVerification.Run` kullanılır. `ConfigureAndRun` yalnızca ayrı kopyada ilk prefab kurulumunu da yapar.

Bu oturumda ayrı Unity `6000.5.0f1` projesinde **41 kontrol geçti (9 asset, 32 Play Mode)**. Güncel Player prefab'ı, input, runtime script'leri ve özgün taş prefab'ı kullanıldı. İki yönden yaklaşma, alan dışı/üstteki oyuncu, oyuncu olmayan nesne ve trigger/kapalı collider/kontrol/fizik filtreleri; üç zemin türünden geçiş; yön/hız/ömür; bileşen kapatma/açma; tek su darbesi, ortak flash koruması ve tek ölüm/yeniden doğma doğrulandı. Test düzeneğinde standart su tüketimi sıfırlandı ve Player konumu sabitlendi; darbe, flash ve respawn gerçek oyuncu bileşenleridir.

Doğrulanan prefab/script/meta dosyaları ana projeye aktarıldı ve dosya hash'leri karşılaştırıldı. [Sonuç](verification/stone-spike-result.txt), [geçen kontroller](verification/stone-spike-checks.txt). Ayrı proje `GameUp Hackathon/Temp/StoneSpikeTask/Verification`, log `GameUp Hackathon/Temp/StoneSpikeTask/verification.log` konumundadır; bu üretilen dosyalar Git kapsamı dışındadır. Unity Search başlangıç indeksleyicisinde `ArgumentOutOfRangeException` görüldü; oyun kodu derlendi ve son koşuda oyun kaynaklı çalışma istisnası olmadı. Önceki iki fixture koşusunda eksik alan adı ve interpolasyonlu görsel konum ölçümü düzeltildi; son sonuç yalnızca düzeltilmiş koşuya aittir.

Ana `Level_2` sahnesinde elle oynanış/görüntü ve dış build testi bekliyor. Başlangıç değerleri bölüm dengesi onayı değildir.
