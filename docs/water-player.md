# Su damlası

Su miktarı, görsel küçülme/esneme, iz, dolum ve yeniden doğuş `Assets/Scripts/Water/WaterMeter.cs` içindedir. Player.cs ability/state machine yöneticisidir.

## Ground kontrolü

PhysicsControl, güncel CircleCollider2D'nin gerçek temaslarını ve aşağı doğru şekil taramasını kullanır:

- **Dry Ground Layer:** `Ground-Dry` (layer 6).
- **Wet Ground Layer:** `Ground-Wet` (layer 7).
- **Extra Dry Ground Layer:** `Ground-Extra-Dry` (layer 10). Atanmazsa Play başında uyarı verilir; bu zemin yer sayılmaz.
- **Ground Check Distance:** Tam boyda en fazla `0.15`; oyuncu küçüldükçe tarama mesafesi aynı oranda kısalır.
- **Maximum Ground Angle:** `60` derece; dik duvar ve tavan destek sayılmaz.

`dryGrounded`, `wetGrounded` ve `extraDryGrounded` destek collider'ının zemin türünü, `grounded` ise üçünün birleşimini verir. Aynı anda yalnızca biri doğrudur; nemli maske önceliklidir, hiçbir maskeye uymayan destek kuru sayılır. Yakın destek seçilir ve yüzey normali `GroundNormal` üzerinden okunabilir. Eğimde hız bu yüzeyin teğetini izler; tuş bırakıldığında eğimden kalan yukarı hız da durur. Yerdeyken yerçekimi kuvveti dengelenerek rampada boşta kayma önlenir. Gerçek zıplamada zemine yapışma devre dışıdır; kenardan çıkıldığında normal düşüş sürer. Seçili Player'ın güncel collider'ı ve tarama alanı Scene görünümünde çizilir. Eski sabit ray başlangıcı artık kullanılmaz; `groundRayDistance` verisi `groundCheckDistance` alanına taşınır.

Sahnedeki Player `Player`, ana platform `Ground-Dry`, nemli şerit `Ground-Wet` layer'ındadır. Zeminlerde ayrıca script bulunmaz.

## Su ve küçülme

WaterMeter tek bir 0–100 su değeri tutar:

| Durum | İlk deneme değeri |
|---|---|
| Isı dışında yerde hareketsiz | 0 |
| Slime Fire ısısında yerde hareketsiz | Kuru zeminde 6, nemli zeminde 2, çok kuru zeminde 12 birim/saniye (varsayılan 2× ısı) |
| Kuru zeminde gerçek yatay ilerleme | 3 birim/saniye |
| Islak zeminde gerçek yatay ilerleme | 1 birim/saniye |
| Çok kuru zeminde gerçek yatay ilerleme | 6 birim/saniye (`Extra Dry Drain Per Second`) |
| Havada | 3 birim/saniye (kuru oran; zeminin katsayısı havaya taşınmaz) |
| Kaynağa temas | 100'e dolum |

Kuru zeminde su daha hızlı azalır ve damla daha hızlı küçülür; ıslak zeminde ikisi de yavaştır. 31 Ekim kullanıcı kararlarına göre görsel ve collider kalan suya göre birlikte 1–0.45 aralığında küçülür; yürüme/hava hızı ve zıplama kalkış hızı mevcut ability'lerin Inspector çarpanlarıyla artar. Kök obje ölçeği değişmez. Duvara basılı tutmak yerine gerçek yatay ilerleme ölçülür. Önceki sabit collider/hız/zıplama kurallarının yerini [güncel kararlar](kararlar.md) almıştır.

**Boyut ayarı:** Hierarchy'de kök **Player** objesini seç → **WaterMeter → Water Size → Minimum Scale**. Varsayılan `0.45`, su sıfıra yaklaşırken tam boyun %45'ine yaklaşılmasıdır; `0.35` daha küçük, `0.60` daha büyük bir minimum verir. Alan aralığı 0.25–1.00'dır ve Play Mode'da değiştirildiğinde de uygulanır. Sıfır su, küçük boyda yaşamayı sürdürmek yerine mevcut yeniden doğmayı başlatır.

**Hız/zıplama ayarları:** Aynı Player üzerinde MoveAbility ve JumpAbility'nin **Low Water Boost** alanlarındadır; [oyuncu belgesine](player-state-machine.md#düşük-suda-hız-ve-zıplama) bak. Katsayılar mevcut `NormalizedWater` değerini okur; WaterMeter ikinci bir hareket hesabı veya kaynak sayacı tutmaz. Minimum Scale, boyut sınırını; ability çarpanları hareket artışının sınırını ayrı ayrı belirler.

Tek CircleCollider2D'nin başlangıç yarıçapı ve offset'i tam boy referansıdır. `PhysicsControl.SetBodyScale` yarıçapı değiştirip alt hizayı koruyacak offset'i hesaplar; `BodyScale` gerçekleşen boyutu verir. Eğimde destek noktası için dikey konum telafisi yapılır. Dolumda dar tavan/duvar tam boyu engelliyorsa su hemen 100 olur; görsel ve collider sığan boyutta kalır, alan açılınca büyüme tamamlanır. Animasyonun esnemesi collider'a aktarılmaz.

Dry Drain Per Second, Wet Drain Per Second, Minimum Scale ve hareket toleransı WaterMeter Inspector'ındadır. `CurrentWater`, `NormalizedWater`, `Consume(amount)`, `Refill()` ve `WaterChanged` ortak su arayüzüdür. `RefilledFromSource`, eksik suyla geçerli bir kaynağa girişte dolum tamamlanınca bir kez bildirilir; kaynakta kalma ve yeniden doğuş bu olayı üretmez.

## Görsel hiyerarşi

Slime Fire yakınlık alanları, aynı sayaç üzerindeki `DrainMultiplier` değerini geçici olarak artırabilir. Kaynaklar `SetDrainMultiplier(source, multiplier)` / `RemoveDrainMultiplier(source)` kullanır; örtüşmede en güçlü etki geçerlidir. 31 Ekim kullanıcı düzeltmesiyle etkin çarpan >1 iken yerde hareketsiz beklemek de zemin tüketimi × çarpan kadar su kaybettirir. Isı dışında dururken tüketim sıfırdır; dolum/yeniden doğma ve kapalı kontrolde tüketmeme kuralları korunur. Ayarlar ve yaşam döngüsü [düşman belgesindedir](enemies.md).

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

## Doğrulama

Aşağıdaki ilk kayıtlar önceki sabit collider uygulamasına aittir; yeni fiziksel küçülme için bu bölümün sonundaki güncel sonuç geçerlidir.

Unity 6000.5.0f1 ile ayrı geçici projede 17 Play Mode testi geçti: iki rayın zemin ayrımı, üst üste zeminlerde yakın yüzey seçimi, zıplama, yürüyüş/duruş, kuru ve ıslak tüketim/küçülme, duvara dayanma, havada tüketim, ayak/collider sabitliği, farklı kare/fizik adımları, scriptsiz kaynak ve yeniden doğuş doğrulandı. Sahne Unity Editor'de açılarak layer, tek Visual ve script referansları ayrıca kontrol edildi. Build testi yapılmadı.

31 Ekim 2026 idle düzeltmesi: Unity 6000.5.0f1 ile ayrı, küçük bir geçici projede güncel klip, controller, sprite sheet ve başlangıç sprite'ı içe aktarıldı. Editor'de Animator üç döngü boyunca örneklendi; 0.8 saniyelik döngü, dört eşit kare süresi ve 1.0/0.7 su ölçeklerinde taban sabitliği doğrulandı. GUID/fileID referansları ve başlangıç taban hizası korundu; dilimlerin görünür pikselleri kesmediği ayrıca kontrol edildi. Bu kontrol sahnenin Play Mode veya build testi değildir; sahne içi görsel doğrulama bekliyor.

31 Ekim 2026 Move/Jump/Heal bağlantısı: aynı Unity sürümünde ayrı bir test sahnesinde güncel kod ve asset'lerle 19 Play Mode doğrulaması geçti. Kaynak dolumunun anlık kalması, Heal'ın kaynakta tekrarlanmaması, yeniden girişte oynayabilmesi, oynarken hareket edilebilmesi ve yeniden doğmada temizlenmesi doğrulandı. Kök ölçek ve collider korundu. Asıl Components sahnesi veya dış build bu kontrolde çalıştırılmadı; kapsam [oyuncu belgesinde](player-state-machine.md#doğrulama) kayıtlıdır.

31 Ekim 2026 fiziksel küçülme/eğim güncellemesi: Unity 6000.5.0f1 ile güncel Player prefab'ı, oyuncu/su kodu, Input System ve animasyon asset'leri ayrı geçici projeye kopyalanarak 47 Play Mode doğrulaması geçti. Tam/yarım/az su ve Inspector'da minimum boyut değişimi, ayak hizası, gerçek zıplama/kısa basış/basılı tutma, ±45° kuru/nemli eğimde hareket ve tuş bırakınca durma, küçük boyda eğim desteği, duvar/kenar ayrımı, kaynakta tek Heal, dolum ve yeniden doğmada tam boyuta dönüş kontrol edildi. Dar tavan altında büyümenin ertelenmesi ve alan açılınca tamamlanması da geçti. Yerde dururken sıfır, nemli yürüyüşte düşük ve havada normal tüketim korundu. Bu test gerçek prefab ve kodla geçici test geometrisinde yapıldı; asıl Components/Level 5 sahneleri veya build çalıştırılmadı.

31 Ekim 2026 hız/zıplama çarpanları: ayrı Unity projesinde gerçek prefab ve güncel kodla 48 Play Mode doğrulaması geçti. Bonuslar aynı WaterMeter'ın su oranından okundu; dolum/yeniden doğma temel harekete döndü ve havada dolum/tüketim yeni zıplama itişi üretmedi. Inspector ayarı, ölçülen hız/mesafe ve regresyon kapsamı [oyuncu belgesindedir](player-state-machine.md#doğrulama). Asıl sahneler ve build bu kontrolde çalıştırılmadı.

31 Ekim 2026 WaterMeter merge çözümü: yereldeki hareketsiz Slime ısı hasarı ile gelen Ground-Extra-Dry tüketimi aynı FixedUpdate satırlarında çakıştı; iki davranış birlikte korundu. Unity 6000.5.0f1 ile `C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073` projesinde güncel WaterMeter/PhysicsControl, gerçek Player ve Slime prefablarıyla 33 Play Mode kontrolü geçti. Isı dışı duruş, kuru/nemli ısı hasarı, alan çıkışı/kapatma, örtüşme, fizik adımı farkları ve yeniden doğma regresyonlarına ek olarak gerçek çok kuru zemin algısı, çok kuru zeminde hareket tüketimi 6 ve 2× ısıda duruş/hareket tüketimi yaklaşık 12 birim/saniye doğrulandı. Havaya çıkınca çok kuru oran taşınmadı; ısıda normal hava tüketimi yaklaşık 6 birim/saniye oldu. Sonuç `water-merge-result.txt`, log `water-merge-verification.log`, düzenek `Assets/Editor/HeatIdleVerification.cs` dosyalarındadır. Asıl Level1 sahnesi ve build bu kontrolde çalıştırılmadı.