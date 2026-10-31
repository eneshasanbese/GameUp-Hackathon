# Su damlası

31 Ekim 2026 Köz Ocağı eki: ayrı çevresel puan/saniye kaybı tek `WaterMeter.Consume` yolunu kullanır; normal tüketim/ısı çarpanını değiştirmez. Dururken ve gövde flash'ında sürer, respawn/kontrol kapanışında durur. [Prefab ayarları ve doğrulama](koz-ocagi.md).

Su miktarı, görsel küçülme/esneme, iz, dolum ve yeniden doğuş `Assets/Scripts/Water/WaterMeter.cs` içindedir. Player.cs ability/state machine yöneticisidir.

## Ground kontrolü

PhysicsControl, güncel CircleCollider2D'nin gerçek temaslarını ve aşağı doğru şekil taramasını kullanır:

- **Dry Ground Layer:** `Ground-Dry` (layer 6).
- **Wet Ground Layer:** `Ground-Wet` (layer 7).
- **Extra Dry Ground Layer:** `Ground-Extra-Dry` (layer 10); Player prefab'ına bağlıdır. Atanmazsa Play başında uyarı verilir; bu zemin yer sayılmaz.
- **Ground Check Distance:** Tam boyda en fazla `0.15`; oyuncu küçüldükçe tarama mesafesi aynı oranda kısalır.
- **Maximum Ground Angle:** `60` derece; dik duvar ve tavan destek sayılmaz.

`dryGrounded`, `wetGrounded` ve `extraDryGrounded` destek collider'ının zemin türünü, `grounded` ise üçünün birleşimini verir. Aynı anda yalnızca biri doğrudur; nemli maske önceliklidir, hiçbir maskeye uymayan destek kuru sayılır. Yakın destek seçilir ve yüzey normali `GroundNormal` üzerinden okunabilir. Eğimde hız bu yüzeyin teğetini izler; tuş bırakıldığında eğimden kalan yukarı hız da durur. Yerdeyken yerçekimi kuvveti dengelenerek rampada boşta kayma önlenir. Gerçek zıplamada zemine yapışma devre dışıdır; kenardan çıkıldığında normal düşüş sürer. Seçili Player'ın güncel collider'ı ve tarama alanı Scene görünümünde çizilir. Eski sabit ray başlangıcı artık kullanılmaz; `groundRayDistance` verisi `groundCheckDistance` alanına taşınır.

Sahnedeki Player `Player`, ana platform `Ground-Dry`, nemli şerit `Ground-Wet` layer'ındadır. Kuruluk layer ile, malzeme ise bağımsız `GroundSurface` component'i ile belirlenir.

## Su ve küçülme

WaterMeter tek bir 0–100 su değeri tutar:

31 Ekim 2026 Köz Püskürten mermisi, mevcut `KnockbackAbility.TryWaterHit` dokunulmazlık kontrolünün ardından bu sayaca tek `Consume(damageWaterPoints)` uygular. Başlangıç hasarı 10 **sabit su puanıdır**; yüzde, saniyelik kayıp veya ısı çarpanı değildir. Ölüm/yeniden doğma yine WaterMeter içindedir; ikinci sayaç yoktur. [Atıcı ve kabul testleri](ember-spitter.md).

| Durum | İlk deneme değeri |
|---|---|
| Nemli zeminde hareketsiz | 0.25 birim/saniye |
| Kuru zeminde hareketsiz | 0.5 birim/saniye |
| Çok kuru zeminde hareketsiz | 2 birim/saniye |
| Kuru zeminde gerçek yatay ilerleme | 3 birim/saniye |
| Islak zeminde gerçek yatay ilerleme | 1 birim/saniye |
| Çok kuru zeminde gerçek yatay ilerleme | 6 birim/saniye (`Extra Dry Drain Per Second`) |
| Havada | 3 birim/saniye (kuru oran; zeminin katsayısı havaya taşınmaz) |
| Kaynağa temas | 100'e dolum |

31 Ekim 2026 son talebiyle her zeminde beklemek su tüketir. Eşit malzeme ve ısı çarpanlarıyla `0.25 < 0.5 < 1 < 2 < 3 < 6` sırası kullanıcının istediği nemli bekleme < kuru bekleme < nemli yürüme < ekstra kuru bekleme < kuru yürüme < ekstra kuru yürüme sırasıdır. Sayılar doğrulanmış bölüm dengesi değildir; Inspector'dan ayarlanabilir.

**Bekleme ayarları:** Player → WaterMeter → **Ground Idle Drain**: Wet Idle Drain Per Second=`0.25`, Dry Idle Drain Per Second=`0.5`, Extra Dry Idle Drain Per Second=`2`. Mevcut Dry/Wet/Extra Dry Drain Per Second alanları yürüme içindir; Dry Drain Per Second ayrıca havada kullanılır. Duvara basma ve tolerans altındaki titreşim düşük bekleme oranını kullanır; hareket gerçek yatay ilerlemeden ölçülür.

**Bağımsız malzeme:** Zemin collider'ı/tilemap'i veya onun üst nesnesine `GroundSurface` ekleyip Surface Material alanında `Sand`, `Grass` veya `BlackStone` seç. Kuruluk için aynı collider'ın Ground-Wet/Ground-Dry/Ground-Extra-Dry layer'ını seç; malzeme seçimi layer'ı değiştirmez. Tek tilemap'te farklı malzemeler gerekiyorsa collider/tilemap'leri malzemeye göre ayır; component destek collider'ı içindir, tek tek tile okumaz. WaterMeter → **Ground Material Multipliers** altındaki üç ayrı çarpan başlangıçta `1`'dir. Malzeme belirtilmeyen veya component'i kapalı destek `1×` kullanır. Çarpanlar pozitif tutulur. Hesap: `bekleme/yürüme kuruluk oranı × malzeme çarpanı × etkin ısı çarpanı`. Verilen altılı sıra aynı malzeme/ısı çarpanı için geçerlidir; farklı malzemelerin birbirine oranı ayrıca ayarlanabilir.

Level1'de mevcut görsellere göre Ground-Dry tilemap'inde `Sand`, Ground-Wet'te `Grass`, Ground-Extra-Dry'da `BlackStone` bağlıdır. Bu başlangıç yerleşimi bir kural değildir; Surface Material ve layer ayrı değiştirilebilir. Ground-Wet tilemap'indeki alt dolgu kum görseli aynı destek malzemesini kullanır; ayrı bir kum destek yolu gerekiyorsa ayrı tilemap gerekir.

Kullanıcının knockback talebiyle kabul edilen düşman gövde darbesi ayrıca tek seferlik `WaterMeter.Consume` çağrısıyla su düşürür; varsayılan miktar Player'ın `KnockbackAbility → Contact Water Loss` alanında `10`'dur. Flash süresince yeni gövde darbeleri reddedilir; ısı/dumanın süreli tüketimi devam eder. Ayrıntılar [knockback belgesinde](player-state-machine.md#knockback-ve-temas-hasarı).

HapsirikKoz'un hapşırma dumanı, Slime Fire ile aynı `EnemyHeatArea`/su çarpanı sistemini kullanır. Geçici alan varsayılan 3 saniyede yok olur; içinden çıkınca veya süre dolunca çarpan kalkar. Örtüşen alanlar çarpılmaz; en yüksek çarpan geçerlidir. Ayarlar ve zamanlama [düşman belgesindedir](enemies.md#hapsirikkoz-hapşırma-döngüsü).

Kuru zeminde su daha hızlı azalır ve damla daha hızlı küçülür; ıslak zeminde ikisi de yavaştır. 31 Ekim kullanıcı kararlarına göre görsel ve collider kalan suya göre birlikte 1–0.45 aralığında küçülür; yürüme/hava hızı ve zıplama kalkış hızı mevcut ability'lerin Inspector çarpanlarıyla artar. Kök obje ölçeği değişmez. Duvara basılı tutmak yerine gerçek yatay ilerleme ölçülür. Önceki sabit collider/hız/zıplama kurallarının yerini [güncel kararlar](kararlar.md) almıştır.

**Boyut ayarı:** Hierarchy'de kök **Player** objesini seç → **WaterMeter → Water Size → Minimum Scale**. Varsayılan `0.45`, su sıfıra yaklaşırken tam boyun %45'ine yaklaşılmasıdır; `0.35` daha küçük, `0.60` daha büyük bir minimum verir. Alan aralığı 0.25–1.00'dır ve Play Mode'da değiştirildiğinde de uygulanır. Sıfır su, küçük boyda yaşamayı sürdürmek yerine mevcut yeniden doğmayı başlatır.

**Hız/zıplama ayarları:** Aynı Player üzerinde MoveAbility ve JumpAbility'nin **Low Water Boost** alanlarındadır; [oyuncu belgesine](player-state-machine.md#düşük-suda-hız-ve-zıplama) bak. Katsayılar mevcut `NormalizedWater` değerini okur; WaterMeter ikinci bir hareket hesabı veya kaynak sayacı tutmaz. Minimum Scale, boyut sınırını; ability çarpanları hareket artışının sınırını ayrı ayrı belirler.

Tek CircleCollider2D'nin başlangıç yarıçapı ve offset'i tam boy referansıdır. `PhysicsControl.SetBodyScale` yarıçapı değiştirip alt hizayı koruyacak offset'i hesaplar; `BodyScale` gerçekleşen boyutu verir. Eğimde destek noktası için dikey konum telafisi yapılır. Dolumda dar tavan/duvar tam boyu engelliyorsa su hemen 100 olur; görsel ve collider sığan boyutta kalır, alan açılınca büyüme tamamlanır. Animasyonun esnemesi collider'a aktarılmaz.

Yürüme ve bekleme oranları, malzeme çarpanları, Minimum Scale ve hareket toleransı WaterMeter Inspector'ındadır. `CurrentWater`, `NormalizedWater`, `Consume(amount)`, `Refill()` ve `WaterChanged` ortak su arayüzüdür. `RefilledFromSource`, eksik suyla geçerli bir kaynağa girişte dolum tamamlanınca bir kez bildirilir; kaynakta kalma ve yeniden doğuş bu olayı üretmez.

## Görsel hiyerarşi

Slime Fire yakınlık alanları, aynı sayaç üzerindeki `DrainMultiplier` değerini geçici olarak artırabilir. Kaynaklar `SetDrainMultiplier(source, multiplier)` / `RemoveDrainMultiplier(source)` kullanır; örtüşmede en güçlü etki geçerlidir. Isı, seçili bekleme/yürüme oranını ve malzeme etkisini çarpar. Örneğin nötr malzemede `2×` ısı ile bekleme nemli/kuru/çok kuru zeminde `0.5/1/4`, yürüme `2/6/12` su/saniyedir. Isıdan çıkınca pozitif normal bekleme tüketimi sürer. Dolum/yeniden doğma ve kapalı kontrolde tüketmeme kuralları korunur. Ayarlar ve yaşam döngüsü [düşman belgesindedir](enemies.md).

```text
Player                         Rigidbody2D, collider, ability'ler ve WaterMeter
├── Visual                     SpriteRenderer; küçülme ve esneme burada birleşir
└── TrailAnchor                TrailRenderer
```

Önceki SizeRoot → DeformRoot → Body düzeni, küçülme ile esnemenin aynı localScale değerini ayrı scriptlerden ezmesini önlemek içindi. WaterMeter artık `başlangıç ölçeği × gerçekleşen gövde boyutu × esneme` sonucunu Visual'a bir kez yazar; bu üç ayrı objeye ihtiyaç kalmaz. Fizikten ayrı Visual çocuğu, animasyon esnemesinin collider'ı bozmasını önler; suya bağlı fizik boyutu ayrıca PhysicsControl'de ayarlanır. Görsel konumu da ayak hizasını sabit tutacak şekilde ayarlanır.

WaterMeter'da **Body Visual** alanına Visual'ın SpriteRenderer'ı, **Trail** alanına TrailAnchor'ın TrailRenderer'ı bağlıdır. Duruş, hareket, zıplama ve iniş şekilleri aynı scriptte hesaplanır. İz 0.6 saniye yaşar; `Assets/Materials/WaterTrail.mat` kullanılır. Kalıcı zemin lekesi üretmez.

`Components.unity` sahnesinde Visual'ın Animator'ı `Assets/Animations/Visual.controller` üzerinden Idle, Move, Jump ve Heal kliplerini oynatır; PlayerAnimation durum ve dolum olayını Animator'a aktarır. Klipler yalnızca sprite değiştirir; ölçek ve konuma yazmaz. Idle'ın dört karesi `0 → 1 → 2 → 3` sırasıyla 0.2'şer saniye gösterilir; döngü 0.8 saniyedir (5 Samples). `Sheets/player_idle.png` dilimleri 128×121 ve merkez pivotludur; yeni kliplerin 128×128 dilimleri aynı taban pivotuna hizalıdır. Başlangıçtaki `Frames/idle/player_idle_00.png` sprite'ının taban yüksekliği korunur. Klip süreleri ve bağlantılar [oyuncu belgesindedir](player-state-machine.md#animasyonlar).

## Kaynak ve yeniden doğuş

Su kaynağı için ayrıca script gerekmez:

1. Kaynak objesinin layer'ını **Water** yap.
2. Collider2D üzerinde **Is Trigger** açık olsun.
3. Checkpoint kullanılacaksa kaynağın altına **RespawnPoint** adlı çocuk obje ekle ve güvenli doğuş merkezine yerleştir.
4. WaterMeter'ın **Water Source Layer** alanında Water seçili olsun.

WaterMeter trigger temasında suyu doldurur; RespawnPoint varsa checkpoint'i kaydeder. Su bitince kısa sönme sonrası hızı, suyu, görseli ve izi sıfırlayıp bu noktaya döner. Başlangıçta Player'ın ilk konumu kullanılır. Evaporation Duration varsayılan 0.3 saniye, Fall Limit Y=-12'dir.

Eksik suyla kaynağa giriş, dolumdan sonra Heal animasyonunu tetikler. Dolum anlıktır; animasyon su kazanımını geciktirmez veya kontrolü kilitlemez. Kaynakta beklerken OnTriggerStay dolumu sürdürür, Heal tekrarlanmaz. Dolu suyla giriş ve yeniden doğuştaki Refill Heal'ı tetiklemez.

## Su göstergesi (T19)

`Assets/Scripts/UI/WaterBarUI.cs`, sol üstteki portreli yatay su barını yönetir. Kendi su sayacı yoktur: açılışta `NormalizedWater` değerini gösterir, sonra `WaterChanged` olayındaki mutlak miktarı `Capacity` ile 0–1'e çevirir. Mavi `Fill` objesinin anchor'ları bu orana göre ayarlanır, bu yüzden sprite gerekmez. Yeniden doğuştaki dolum da aynı olayla bara yansır.

Inspector: **Fill** mavi dolgu RectTransform'u, **Portrait** portre Image'ı, **Water Meter** boş bırakılırsa sahnedeki WaterMeter otomatik bulunur. **Low Water Portrait** isteğe bağlıdır; su **Low Water Threshold** (varsayılan 0.25) altına inince portre bu sprite'a geçer, böylece düşük su yalnızca renkle anlatılmaz (T50).

Önerilen hiyerarşi (`Level1`): `Canvas` (Scale With Screen Size, 1920×1080) → `WaterHUD` (WaterBarUI) → `Portrait` ve `BarFrame` (siyah) → `BarBack` (koyu, 4 px içeride) → `Fill` (mavi).

## Bölüm geçişi (LevelExit)

`Assets/Scripts/Level/LevelExit.cs`, bölüm sonundaki trigger alanıdır. Bölüm sonu bir su birikintisidir: bileşen, bölümün son `WaterSource` örneğinin kök nesnesine (Water layer'daki trigger'ın olduğu yere) eklenir. Yalnızca sahnedeki o örneğe eklenir, prefab'a uygulanmaz; yoksa her birikinti bölümü bitirir. Player suyu varken girince sıradaki sahne bir kez yüklenir; su sıfırken veya yeniden doğarken geçiş olmaz. Yüklemeden önce Player bileşeni kapatılır, böylece input ve tüketim durur. Yeni sahne kendi Player'ıyla başladığı için su dolu başlar; önceki bölümden su taşınmaz ([K04](kararlar.md)).

Inspector: **Next Scene Name** boşsa build listesindeki bir sonraki sahne, doluysa o isimdeki sahne (ör. `Level_2`) yüklenir. Sahne build listesinde değilse geçiş olmaz ve Console'da bir kez uyarı çıkar. Nesne bir Collider2D (Is Trigger açık) ister; WaterSource'un kendi trigger'ı bunu karşılar. Birikinti aynı anda suyu doldurur ve checkpoint kaydeder, ama sahne hemen değiştiği için bunun etkisi olmaz.

Geçiş ekranı: `Assets/Scripts/Level/SceneFader.cs` kodla oluşturulur, sahneye bir şey eklenmez. Damla birikintiye girince yerinde durdurulur (Player kapalı, `Rigidbody2D.simulated` kapalı; input ve tüketim yok). Ekran **Fade Duration** (varsayılan 0.5 sn) içinde **Fade Color** rengine (varsayılan siyah) kararır. Sonraki sahne arka planda `LoadSceneAsync` ile yüklenir ve iki kare daha beklenir, sonra yeni bölüm aynı sürede açılır. Yükleme takılması karartının arkasında kalır. Fade Duration 0 olursa geçiş anında olur.

## Anında ölüm alanı (KillZone, lav)

`Assets/Scripts/Level/KillZone.cs` trigger'a giren Player'ın suyunu `Consume(CurrentWater)` ile sıfırlar. Böylece ölüm, su bitişindeki aynı tek akışla olur: buharlaşma, son checkpoint'e dönüş ve dolum. Yeniden doğarken ya da su zaten sıfırken etkisi yoktur. Bileşen, aynı nesnede Is Trigger açık bir Collider2D ister; Tilemap Collider 2D de olur. Collider yoksa Console'da uyarı çıkar.

Lav için önerilen kurulum: `Grid` altında ayrı bir `Hazard-Lava` tilemap, layer `Default` (Player ile çarpışır, zemin maskelerinde yok). Bu tilemap'te Tilemap Collider 2D (Is Trigger açık, Composite yok) ve `KillZone` bulunur. Collider katı olmadığı için damla lavın üstünde durmaz, içine girer ve ölür. Final fidanı (T27) bu bileşen değildir; son bölümde kazanma ayrı kurulacak.

## Doğrulama

31 Ekim 2026 son bekleme/malzeme güncellemesi: ayrı `C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073` projesinde Unity 6000.5.0f1 ile güncel Player prefab'ı ve oyuncu/su kodu derlendi; **79 asset/Play Mode kontrolü geçti**. Altı oran `0.25/0.5/1/2/3/6` olarak ölçüldü. Üç malzemenin üç kuruluk layer'ında bekleme/yürüme tüketimi, işaretsiz/kapalı component varsayılanı, üst nesnedeki malzeme, gerçek yakın destek seçimi, duvara basma, ±30° eğimde bekleme, dikey zıplama/düşüşte malzemenin sıfırlanması, gerçek Slime ısı alanı ve örtüşmesi/temizliği, kapalı kontrol/fizik/su component'i, 0.01/0.04 fizik adımları, kaynakta tek Heal ve sıradan beklemeden tek ölüm/yeniden doğma doğrulandı. Denge testi için malzeme çarpanları yalnızca test nesnelerinde 1.25/0.8/1.5 yapıldı; gerçek prefab'da hepsi 1'dir.

Düzenek `Assets/Editor/WaterDrainVerification.cs`; ayrı proje kopyasında `-executeMethod WaterDrainVerification.Run` ile çalışır. [Sonuç](verification/water-drain-result.txt), [geçen kontroller](verification/water-drain-checks.txt); tam log geçici projedeki `water-drain-verification.log` dosyasındadır. Testin ilk ısı örneğinde Slime gövdesinin teması knockback üretti; düzenekte gövde uzaklaştırılıp gerçek ısı trigger'ı korunarak son koşu geçti. Logda UnityEditor.Search başlangıç indeksleme istisnası vardır; oyun kodu derleme/çalışma istisnası yoktur. Level1'in yeni üç malzeme bağlantısı kaynak incelemesiyle kontrol edildi; asıl sahnede Play Mode/görüntü ve dış build testi yapılmadı. Aşağıdaki ücretsiz duruş sonuçları önceki tasarımın tarihsel kayıtlarıdır.

Aşağıdaki ilk kayıtlar önceki sabit collider uygulamasına aittir; yeni fiziksel küçülme için bu bölümün sonundaki güncel sonuç geçerlidir.

Unity 6000.5.0f1 ile ayrı geçici projede 17 Play Mode testi geçti: iki rayın zemin ayrımı, üst üste zeminlerde yakın yüzey seçimi, zıplama, yürüyüş/duruş, kuru ve ıslak tüketim/küçülme, duvara dayanma, havada tüketim, ayak/collider sabitliği, farklı kare/fizik adımları, scriptsiz kaynak ve yeniden doğuş doğrulandı. Sahne Unity Editor'de açılarak layer, tek Visual ve script referansları ayrıca kontrol edildi. Build testi yapılmadı.

31 Ekim 2026 idle düzeltmesi: Unity 6000.5.0f1 ile ayrı, küçük bir geçici projede güncel klip, controller, sprite sheet ve başlangıç sprite'ı içe aktarıldı. Editor'de Animator üç döngü boyunca örneklendi; 0.8 saniyelik döngü, dört eşit kare süresi ve 1.0/0.7 su ölçeklerinde taban sabitliği doğrulandı. GUID/fileID referansları ve başlangıç taban hizası korundu; dilimlerin görünür pikselleri kesmediği ayrıca kontrol edildi. Bu kontrol sahnenin Play Mode veya build testi değildir; sahne içi görsel doğrulama bekliyor.

31 Ekim 2026 Move/Jump/Heal bağlantısı: aynı Unity sürümünde ayrı bir test sahnesinde güncel kod ve asset'lerle 19 Play Mode doğrulaması geçti. Kaynak dolumunun anlık kalması, Heal'ın kaynakta tekrarlanmaması, yeniden girişte oynayabilmesi, oynarken hareket edilebilmesi ve yeniden doğmada temizlenmesi doğrulandı. Kök ölçek ve collider korundu. Asıl Components sahnesi veya dış build bu kontrolde çalıştırılmadı; kapsam [oyuncu belgesinde](player-state-machine.md#doğrulama) kayıtlıdır.

31 Ekim 2026 fiziksel küçülme/eğim güncellemesi: Unity 6000.5.0f1 ile güncel Player prefab'ı, oyuncu/su kodu, Input System ve animasyon asset'leri ayrı geçici projeye kopyalanarak 47 Play Mode doğrulaması geçti. Tam/yarım/az su ve Inspector'da minimum boyut değişimi, ayak hizası, gerçek zıplama/kısa basış/basılı tutma, ±45° kuru/nemli eğimde hareket ve tuş bırakınca durma, küçük boyda eğim desteği, duvar/kenar ayrımı, kaynakta tek Heal, dolum ve yeniden doğmada tam boyuta dönüş kontrol edildi. Dar tavan altında büyümenin ertelenmesi ve alan açılınca tamamlanması da geçti. Yerde dururken sıfır, nemli yürüyüşte düşük ve havada normal tüketim korundu. Bu test gerçek prefab ve kodla geçici test geometrisinde yapıldı; asıl Components/Level 5 sahneleri veya build çalıştırılmadı.

31 Ekim 2026 hız/zıplama çarpanları: ayrı Unity projesinde gerçek prefab ve güncel kodla 48 Play Mode doğrulaması geçti. Bonuslar aynı WaterMeter'ın su oranından okundu; dolum/yeniden doğma temel harekete döndü ve havada dolum/tüketim yeni zıplama itişi üretmedi. Inspector ayarı, ölçülen hız/mesafe ve regresyon kapsamı [oyuncu belgesindedir](player-state-machine.md#doğrulama). Asıl sahneler ve build bu kontrolde çalıştırılmadı.

31 Ekim 2026 WaterMeter merge çözümü: yereldeki hareketsiz Slime ısı hasarı ile gelen Ground-Extra-Dry tüketimi aynı FixedUpdate satırlarında çakıştı; iki davranış birlikte korundu. Unity 6000.5.0f1 ile `C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073` projesinde güncel WaterMeter/PhysicsControl, gerçek Player ve Slime prefablarıyla 33 Play Mode kontrolü geçti. Isı dışı duruş, kuru/nemli ısı hasarı, alan çıkışı/kapatma, örtüşme, fizik adımı farkları ve yeniden doğma regresyonlarına ek olarak gerçek çok kuru zemin algısı, çok kuru zeminde hareket tüketimi 6 ve 2× ısıda duruş/hareket tüketimi yaklaşık 12 birim/saniye doğrulandı. Havaya çıkınca çok kuru oran taşınmadı; ısıda normal hava tüketimi yaklaşık 6 birim/saniye oldu. Sonuç `water-merge-result.txt`, log `water-merge-verification.log`, düzenek `Assets/Editor/HeatIdleVerification.cs` dosyalarındadır. Asıl Level1 sahnesi ve build bu kontrolde çalıştırılmadı.
