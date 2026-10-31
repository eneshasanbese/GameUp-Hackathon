# Saw ve Slime Fire prefabları

31 Ekim 2026 kullanıcı talebiyle iki düşmanın duran ve iki nokta arasında git-gel yapan türleri eklendi. Doğrudan temas davranışı, kullanıcının sonraki kararıyla knockback eklenene kadar TODO olarak bırakıldı. Gövdeler trigger'dır; şimdilik hasar, itme, ölüm veya engelleme üretmez.

## Yerleştirme ve ayarlar

Unity proje köküne göre `Assets/Prefabs/Enemies/` altında dört hazır prefab bulunur:

| Prefab | Davranış |
|---|---|
| `Saw_Stationary.prefab` | Yerinde kalan, sürekli dönen bıçak |
| `Saw_Patrol.prefab` | Sürekli dönen, iki nokta arasında hareket eden bıçak |
| `SlimeFire_Stationary.prefab` | Idle animasyonu ve yakınlıkta erime çarpanı |
| `SlimeFire_Patrol.prefab` | Walk animasyonu, git-gel ve yakınlıkta erime çarpanı |

Prefab'ı istenen sahneye sürüklemek yeterlidir. Sahnelere otomatik düşman yerleştirilmedi. Saw kökü bıçağın merkezi, Slime kökü ayak seviyesidir. Kök Rigidbody2D kinematiktir ve yerçekimi kullanmaz; güzergâh zemin aramaz. Özellikle Slime rotasını yürünebilir yüzeye yerleştir.

Hareketli türlerde **EnemyPatrol → End Offset**, yerleştirilen başlangıca göre dünya birimi cinsinden diğer uçtur; varsayılan `(3, 0)`. **Speed** varsayılan `1.5` birim/saniyedir. Negatif, dikey ve çapraz offset kullanılabilir. Sıfır hız veya sıfır offset hareketi durdurur. Seçili prefab'ın iki ucu Scene görünümünde sarı çizilir. Slime hareket yönüne döner; Saw'ın yüzü dik kalırken dişleri sheet animasyonuyla döner.

Slime'ın `HeatArea` çocuğunda:

- **EnemyHeatArea → Drain Multiplier:** Varsayılan `2`; `1` etkiyi kapatır.
- **CircleCollider2D → Radius:** Varsayılan `1.75`; offset `(0, 0.4)`.
- **Is Trigger:** Açık tutulmalı. Kök gövde collider'ı, yakınlık alanından ayrıdır.

Yeni fizik layer'ı veya Player prefab bağlantısı gerekmez. Oyuncunun gerçek Rigidbody2D'si üzerindeki mevcut `Player` ve `WaterMeter` bulunur; düşman alanı Water layer'ına taşınmamalıdır. Özel collision matrix değişikliği yapılırsa Default ve Player arasındaki trigger etkileşimi korunmalıdır.

## Su tüketimi ve yaşam döngüsü

`WaterMeter` tek su sayacı olmaya devam eder. Çarpan yalnızca normal, zamana bağlı hareket/hava tüketimine uygulanır. Varsayılan `2x` ile kuru zeminde/havada `6`, nemli zeminde `2` birim/saniye tüketilir. Yerde hareketsizken tüketim hâlâ sıfırdır. `Consume(amount)` ile yapılan açık miktarlı işlemler çarpılmaz; kaynak dolumu değişmez.

`SetDrainMultiplier(source, multiplier)` ve `RemoveDrainMultiplier(source)` kaynak başına geçici etkiyi yönetir. Aynı alanda kalmak çarpanı tekrar tekrar çarpmaz. Örtüşen alanlarda en yüksek çarpan uygulanır; güçlü alandan çıkınca hâlâ temas edilen diğer alanın çarpanı kalır. Bir oyuncunun birden fazla collider'ı varsa son temas bitene kadar etki korunur.

Trigger giriş/kalma/çıkış olayları kullanılır. Devre dışı alan, kapatılan/yok edilen collider veya düşman ve teleport sonrası eski temaslar temizlenir. WaterMeter kapatıldığında veya yeniden doğma başladığında geçici çarpanları temizler; oyuncu yeniden etkinleşip aynı alanda kalırsa trigger stay etkiyi tekrar kurar. Kontrol kapalıyken ve yeniden doğmada mevcut tüketim korumaları sürer.

## Görseller ve yeniden üretim

`Assets/Animations/Enemies/` altındaki klip/controller'lar doğrudan ilgili `Sheets` sprite alt asset'lerini kullanır. İki Saw türü de `saw_walk.png` üzerindeki sekiz dönüş karesini oynatır. Slime duran türde `slime_fire_idle.png`, hareketli türde `slime_fire_walk.png` kullanır. Animasyonlar 10 kare/saniye döngülüdür; Slime'ın kırpılmış kareleri ayak seviyesini koruyacak görsel Y konumuyla oynatılır. Kök ve collider animasyondan ölçek/konum almaz.

Saw idle/walk sheet'lerinin otomatik dilimlenmesinde ilk iki kare birleşmişti. Unity Sprite Editor veri sağlayıcısıyla sekiz adet 128×128 kareye düzeltildi; var olan GUID, spriteID ve fileID'ler korundu, eksik ikinci kare için `_01` adlı yeni dilim eklendi. Kullanıcının PPU 128 ve Point filtre tercihleri korundu. Slime dilimlerine dokunulmadı.

`Assets/Editor/Enemies/EnemyPrefabBuilder.cs`, **GameUp → Enemies → Rebuild Enemy Prefabs** menüsünden aynı asset'leri yeniden üretebilir. Menü, bu dört prefab'ın ve üç klip/controller'ın ayarlarını varsayılanlara yeniden yazar; hazır prefab'ları kullanmak için menüyü çalıştırmak gerekmez. Projeye yeni paket eklenmedi.

## Bekleyen iş

- [ ] Doğrudan temas için Player knockback ekle ve düşman gövde trigger'larına bağla. Slime'ın geniş HeatArea trigger'ı knockback tetiklememeli.
- [ ] Asıl bölümde yerleşim, rota güvenliği, ısı yarıçapı ve çarpan dengesini elle oynayarak doğrula; dış build'i test et.

## Doğrulama

Unity 6000.5.0f1 ile `C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073` altındaki bağımsız projede 51 doğrulama geçti: 20 asset/bağlantı kontrolü ve 31 Play Mode davranış kontrolü. Asıl projede açık editör bulunduğundan ikinci editör bu projeye başlatılmadı. Güncel Player prefab'ı/kodu, Input System, oyuncu animasyonları ve gerçek düşman sheet'leri kopyalandı; düşman prefabları Unity tarafından üretildi. Geçen kontrollerin ardından aynı prefab/animasyon dosyaları ve meta'ları asıl projeye aktarıldı.

Kapsam: dört prefab'ın component ve sheet referansları, sekiz tek bıçaklı Saw karesi, Slime ayak hizası, duran türlerin konumu, iki uçta dönüş ve yönlenme, negatif/çapraz offset, sıfır hız, gerçek trigger giriş/kalma/çıkış, örtüşmede en güçlü etki, çoklu collider, alan/collider/WaterMeter kapatma-açma, düşmanın/collider'ın silinmesi, teleport, yeniden doğma ve sıfır su temizliği. Doğrudan temasın etkisiz kaldığı kontrol edildi. 2x ısıda kuru/hava tüketimi yaklaşık 6.0001, nemli tüketim 2.0000 birim/saniye ölçüldü; yerde durma ve kapalı kontrolde tüketim sıfır kaldı.

Test düzeneği: `Assets/Editor/EnemyVerification.cs`; kayıtlar: `verification.log` ve `verification-result.txt` (geçici test projesinde). Saw onarımında var olan tüm spriteID/fileID değerlerinin, texture GUID'lerinin ve kullanıcı import ayarlarının korunduğu ayrıca karşılaştırıldı. Bu doğrulama asıl Components/Level 5 sahnelerinin oynanışı, render kontrolü veya dış build testi değildir; bunlar bekliyor.
