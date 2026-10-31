---
name: gameup-water-loop
description: GameUp oyununda tek su değeri, tüketim, su UI bağlantısı, birikinti, checkpoint, ölüm, yeniden doğma, final ve yeniden oynama döngüsünü geliştir veya düzelt. T12–T29 ve ilgili QA senaryolarında kullan.
---

# GameUp su ve oyun döngüsü

[Ortak yönergeleri](../../../AGENT.md), [su belgesini](../../../docs/water-player.md) ve [genel plandaki](../../../genel-yapilacaklar.md) ilgili T12–T29 maddelerini oku. Kod yolları Unity proje kökü `GameUp Hackathon/` klasörüne göredir.

## Mevcut uygulamadan ilerle

`Assets/Scripts/Water/WaterMeter.cs` tek su değerinin sahibidir; bugün görsel, iz, kaynak ve respawn da buradadır. Önce bu kodu, `PhysicsControl` bilgisini ve `Assets/Scenes/Components.unity` referanslarını incele. Ayrı `WaterSource`, `GameFlow` veya `RespawnController` varmış gibi kod yazma; ihtiyaç doğmadıkça çalışan davranışı yeniden bölme.

- Kapasite bugün `Capacity = 100f` sabitidir. Kuru/hava tüketimi başlangıçta 3, nemli tüketim 1 birim/saniyedir; bunlar değiştirilebilir prototip ayarlarıdır, onaylı denge sonucu değildir. T20 kapasiteyi Inspector'a açmayı hedefler; mevcut sabiti Inspector alanı diye raporlama.
- UI için `CurrentWater`, `NormalizedWater` ve `WaterChanged` kullanılır. `WaterChanged` mutlak su miktarını taşır; 0–1 bekleyen göstergeye normalize ederek bağla. İlk görünümü mevcut değerden kur; abonelik yaşam döngüsünü yönet.
- Mevcut ortak işlemler: `Consume`, `Refill`, `SetCheckpoint`, `ResetPositionTracking`, `ClearTrail`, `RespawnAtCheckpoint`; durumlar `IsEmpty`, `IsRespawning`, `Checkpoint`. Yeni sayaç veya paralel ölüm coroutine'i oluşturma.

## Durum önceliği ve kabul ölçütleri

| Durum | Hedef davranış |
|---|---|
| Kontrol kapalı / yeniden doğma / oyun sonucu | Ek tüketim yok |
| Kaynak işlemi | Su dolumu; tekrar temas güvenli |
| Havada, yatay giriş olsun veya olmasın | Normal tüketim; eski nemli katsayı taşınmaz |
| Yerde, gerçek yatay ilerleme tolerans altında | Tüketim yok |
| Yerde hareket | Algılanan zemine göre tüketim; bilinmeyen yüzey için açık normal varsayılan |
| Su sıfır / düşme sınırı | Tek yeniden doğma akışı |
| Finale pozitif suyla temas | Tek kazanma, input ve tüketim kapanır; su dolmaz |
| Yeniden oyna | Bölüm başlangıcı, başlangıç checkpoint'i ve oyun durumunun tamamı sıfırlanır |

Tablo tasarım/kabul ölçütüdür; bütün satırların mevcut kodda tamamlandığını varsayma. Input tuşu yerine gerçek ilerlemeyi ölç; titreşim veya duvara basmak bedel üretmemeli. Su miktarını sınırlandır, zamana bağlı azalt. Su bitişi ile kaynak/final temasının aynı fizik adımındaki sonucunu bilinçli tanımla; sıfır suyla kazanma olmasın.

## Sahne sözleşmesi

- Layer'lar bugün Water=4, Ground-Dry=6, Ground-Wet=7, Player=8'dir. Gerçek `ProjectSettings/TagManager.asset` ve Inspector maskesini doğrula; kodda yeni sihirli layer numaraları yayma.
- Kaynak, Water layer'ında trigger Collider2D'dir. `WaterMeter.waterSourceLayer` bunu kapsar. Checkpoint için collider'ın transform'u altında tam adı `RespawnPoint` olan çocuk aranır; çocuğu güvenli doğuş merkezine koy. Kaynak collider'ı farklı bir çocukta ise aramanın nereden başladığını dikkate al.
- İlk kaynak öncesi dönüş, Player'ın başlangıç konumudur. Respawn konum/hız, su, input izni, fizik simülasyonu, görsel saydamlık/ölçek, UI ve izi birlikte toparlamalıdır. Teleport sonrası konum takibini ve izi temizle.
- Final fidanını Water layer'ında kaynak gibi kurma. Kazanma/yeniden oyna sistemi ekleniyorsa Player'ın ve WaterMeter'ın etkinlik kontrolleriyle tutarlı tek akış kur; sahnede gerçekten bağla.
- Görsel `Visual`, iz `TrailAnchor` üzerindedir. Suya bağlı değişim fizik boyutuna ve ability hızlarına dokunmaz. Nihai küçülme yöntemi için K03 kararını kontrol et.

## Doğrulama

İlgili QA01–QA16 içinden etkilenenleri seç: yerde durma, duvar/rampa titreşimi, kuru-nemli tüketim, nemli zeminden dikey zıplama/düşüş, su sınırları, tekrarlı kaynak teması, ilk/son checkpoint, su bitişi/düşme, yeniden doğmada input ve iz temizliği, bir kez kazanma ve bölüm başından yeniden oynama. Kaynak sınırındaki dolum/tüketim sırasını ve farklı kare/fizik adımlarını değişiklik etkiliyorsa ölç.

Yeni UI veya final kodunun yazılması tek başına T19/T27/T28'i kapatmaz. Inspector bağlantıları ile sahne davranışını doğrula; çalıştıramadıysan bekleyen testi belirt. Önceki belgede geçen 17 test, bu oturumun testi değildir.
