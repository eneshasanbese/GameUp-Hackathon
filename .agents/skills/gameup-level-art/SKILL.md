---
name: gameup-level-art
description: GameUp projesinde bölüm rotası ve su dengesi, tilemap, kamera/arka plan, görsel import, damla görünümü ve su izi entegrasyonu yap. T10 ve T30–T48 için kullan; onaysız nihai görsel tarz veya yeni mekanik seçme.
---

# GameUp bölüm ve görsel entegrasyonu

[Ortak yönergeleri](../../../AGENT.md), [genel plandaki](../../../genel-yapilacaklar.md) ilgili T10/T30–T48 maddelerini ve [günlük takibi](../../../yapilacaklar-takip.md) karşılaştır. Unity yolları depo altındaki `GameUp Hackathon/` klasörüne göredir.

## Önce gerçek sahneyi ve kararı belirle

`Assets/Scenes/SampleScene.unity` ile `Components.unity` farklıdır. Hangi sahnenin düzenleneceğini, ana sahne sahipliğini ve açık diff'i belirle. Tilemap veya görseli bir sahnede görüp diğerinde de var sayma. `ProjectSettings/TagManager.asset`, ilgili renderer ve sprite `.meta` ayarlarını oku.

K02 tarz, K03 küçülme, K04 bölüm içeriğidir. Mevcut paketin veya ölçek kodunun bulunması bu kapıları kapatmaz. Yanıt gerektiren kararda küçük, karşılaştırılabilir örnek hazırla; etkilenmeyen gri kutu düzenini ilerlet. Kullanıcı zaten ilgili seçimi yaptıysa tekrar onay isteme.

## Bölüm ve denge

- Rota önce hareket/zıplamayı, sonra su kaybını, zemin farkını ve birikintiyi öğretsin. Son yaşayan fidan yalnızca finalde olsun. Nemli zemini anlatmak için yaşayan ara bitki checkpoint'i ekleme; mevcut çim görselinin hikâyeyle uyumunu da kontrol et.
- Güvenli durma ve okunabilir iniş alanları bırak. İlk kaynak kusursuz refleks gerektirmesin; kaynak sonrası güvenli RespawnPoint ve boşluğa düşme dönüşü denensin.
- Kaynaklar arası süre/mesafe ile tüketimi birlikte ölç; her adımda tek büyük denge değişkenini değiştir. Sayıları “deneme ayarı” ve “oynanışla doğrulandı” diye ayır.
- Sürekli zıplama kuru yüzeyi avantajlı geçiyorsa gözlemi kaydet. Bu nedenle havada tüketim veya dururken ücretsiz bekleme kararını kendiliğinden değiştirme.
- K04 netleşmeden ek bölüm, alternatif rota, çok kademeli son veya yeni hareket mekaniği ekleme. İlk kısa rota çalışmadan dekor ve parallax'a öncelik verme.

## Tilemap, import ve kamera

- Mevcut `Assets/platformerPack/Sprites/Tiles/Double` ve `Backgrounds/Double` için takip belgesi PPU 128, Bilinear, mip map kapalı ve sıkıştırmasız ayar bildirir; ilgili `.meta` dosyalarında doğrula. Genel belgedeki Kenney Pixel Platformer 18 PPU/Point önerisini bu farklı pakete otomatik uygulama.
- Tile paletleri `Tiles/Double/Palettes` altındadır. Layer, sorting layer, grid ölçeği, tile collider ve statik Rigidbody2D bağlantılarını gerçek sahnede kontrol et. Yeni tile/tilemap taşınırsa `.meta` eşleşmesini koru.
- Ground-Dry ve Ground-Wet fizik katmanlarıyla görsel ayrım uyuşsun. Nemli zemini yalnızca renkle ayırma; doku/şekil gibi ikinci ipucu kullan.
- Cinemachine şu an manifestte bulunmaz. Yeni paket veya kamera altyapısı gerekiyorsa gerekçeyi ve mevcut yetkilendirmeyi kontrol et; basit mevcut kamera görevi karşılıyorsa onu kullan. Kurulu olmayan API üzerinden kod yazma.
- Kamera iniş yerlerini göstersin. Confiner için ayrı trigger/layer kullanılıyorsa oyuncunun fizik/trigger etkileşimine karışmadığını doğrula. Parallax eklenirse gerçek kamera güncellemesinden sonra çalışmasını ve farklı oranlarda boşluk oluşmamasını test et.

## Damla, iz ve dış asset

`Assets/Scripts/Water/WaterMeter.cs` bugün küçülme/esnemeyi birlikte `Visual.localScale` üzerine yazar; ikinci script aynı değeri ezmemeli. Collider ve kök ölçek değişmez; sprite küçüldüğünde ayak hizası, kaynakta dolum ve respawn görünümü tutarlı kalır. Dört boy/30 kare otomatik gereksinim değildir.

İz bugün TrailRenderer ile kısa ömürlüdür; kalıcı obje biriktirmez ve gameplay zemini yaratmaz. Yeni efekt okunabilirliği bozmamalı; en küçük damla da zeminden seçilebilmeli.

Asset üretimi veya indirmesi gerçekten isteniyorsa önce eldeki dosyaları ve onaylı tarzı kontrol et. `damla_sprite.zip` plan metninde geçtiği için diskte varmış gibi davranma. Kullanılan dış dosyanın kaynak URL'sini, tam paket/dosya adını, lisansını, indirme tarihini ve gerekli atfını kaydet; platform adından lisans çıkarma.

## Doğrula

İlgili T30–T48 kabul ölçütlerini ve QA11, QA12, QA18, QA19'u seç. Rotayı dolu suyla ve makul hatalarla dene; durma, iniş, kaynak, düşme dönüşü ve final okunabilirliğini gözle. Görsel/kamera değişikliğini hedef çözünürlüklerde ve hareket sırasında kontrol et. Gerçek build görüntüsünü Editor görünümüyle karıştırma; test edilmediyse raporda ayır.
