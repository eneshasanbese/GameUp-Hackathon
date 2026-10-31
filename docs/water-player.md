# Su damlası

Su miktarı, görsel küçülme/esneme, iz, dolum ve yeniden doğuş `Assets/Scripts/Water/WaterMeter.cs` içindedir. Player.cs ability/state machine yöneticisidir.

## Ground kontrolü

PhysicsControl aynı noktadan aşağı doğru iki Physics2D.Raycast atar:

- **Dry Ground Layer:** `Ground-Dry` (layer 6).
- **Wet Ground Layer:** `Ground-Wet` (layer 7).
- **Ground Check Offset:** Player merkezine göre `(0, -0.45)`.
- **Ground Ray Distance:** `0.15`.

`dryGrounded` ve `wetGrounded` zemin türünü, `grounded` ise ikisinin birleşimini verir. Ray iki katmana da çarparsa daha yakın yüzey seçilir. Zıplama yükselirken önceki zemin tekrar grounded sayılmaz. Seçili Player'ın rayı Scene görünümünde çizilir.

Sahnedeki Player `Player`, ana platform `Ground-Dry`, nemli şerit `Ground-Wet` layer'ındadır. Zeminlerde ayrıca script bulunmaz.

## Su ve küçülme

WaterMeter tek bir 0–100 su değeri tutar:

| Durum | İlk deneme değeri |
|---|---|
| Yerde hareketsiz | 0 |
| Kuru zeminde gerçek yatay ilerleme | 3 birim/saniye |
| Islak zeminde gerçek yatay ilerleme | 1 birim/saniye |
| Havada | 3 birim/saniye |
| Kaynağa temas | 100'e dolum |

Kuru zeminde su daha hızlı azalır ve damla daha hızlı küçülür; ıslak zeminde ikisi de yavaştır. Görsel boyut kalan suya göre 1–0.70 aralığındadır. Collider, kök obje ölçeği, hareket hızı ve zıplama kuvveti bu değerle değiştirilmez. Duvara basılı tutmak yerine gerçek yatay ilerleme ölçülür.

Dry Drain Per Second, Wet Drain Per Second, Minimum Scale ve hareket toleransı WaterMeter Inspector'ındadır. `CurrentWater`, `NormalizedWater`, `Consume(amount)`, `Refill()` ve `WaterChanged` ortak su arayüzüdür. `RefilledFromSource`, eksik suyla geçerli bir kaynağa girişte dolum tamamlanınca bir kez bildirilir; kaynakta kalma ve yeniden doğuş bu olayı üretmez.

## Görsel hiyerarşi

```text
Player                         Rigidbody2D, collider, ability'ler ve WaterMeter
├── Visual                     SpriteRenderer; küçülme ve esneme burada birleşir
│   └── Highlight              Beyaz parlama
└── TrailAnchor                TrailRenderer
```

Önceki SizeRoot → DeformRoot → Body düzeni, küçülme ile esnemenin aynı localScale değerini ayrı scriptlerden ezmesini önlemek içindi. Tek script artık `başlangıç ölçeği × su boyutu × esneme` sonucunu Visual'a bir kez yazar; bu üç ayrı objeye ihtiyaç kalmaz. Fizikten ayrı bir Visual çocuğu, collider'ın görselle beraber küçülmesini/esnemesini önler. Görsel konumu da ayak hizasını sabit tutacak şekilde ayarlanır.

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

Unity 6000.5.0f1 ile ayrı geçici projede 17 Play Mode testi geçti: iki rayın zemin ayrımı, üst üste zeminlerde yakın yüzey seçimi, zıplama, yürüyüş/duruş, kuru ve ıslak tüketim/küçülme, duvara dayanma, havada tüketim, ayak/collider sabitliği, farklı kare/fizik adımları, scriptsiz kaynak ve yeniden doğuş doğrulandı. Sahne Unity Editor'de açılarak layer, tek Visual ve script referansları ayrıca kontrol edildi. Build testi yapılmadı.

31 Ekim 2026 idle düzeltmesi: Unity 6000.5.0f1 ile ayrı, küçük bir geçici projede güncel klip, controller, sprite sheet ve başlangıç sprite'ı içe aktarıldı. Editor'de Animator üç döngü boyunca örneklendi; 0.8 saniyelik döngü, dört eşit kare süresi ve 1.0/0.7 su ölçeklerinde taban sabitliği doğrulandı. GUID/fileID referansları ve başlangıç taban hizası korundu; dilimlerin görünür pikselleri kesmediği ayrıca kontrol edildi. Bu kontrol sahnenin Play Mode veya build testi değildir; sahne içi görsel doğrulama bekliyor.

31 Ekim 2026 Move/Jump/Heal bağlantısı: aynı Unity sürümünde ayrı bir test sahnesinde güncel kod ve asset'lerle 19 Play Mode doğrulaması geçti. Kaynak dolumunun anlık kalması, Heal'ın kaynakta tekrarlanmaması, yeniden girişte oynayabilmesi, oynarken hareket edilebilmesi ve yeniden doğmada temizlenmesi doğrulandı. Kök ölçek ve collider korundu. Asıl Components sahnesi veya dış build bu kontrolde çalıştırılmadı; kapsam [oyuncu belgesinde](player-state-machine.md#doğrulama) kayıtlıdır.
