# Kararlar

## HapsirikKoz — duman HeatAura ve alfa geçişi

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı duran/devriye HapsirikKoz'un ortak `HapsirikKoz_Smoke` prefabına Slime ile aynı HeatAura tasarımını, oluşurken 0→1 ve biterken 1→0 alfa geçişiyle eklemeyi istedi.

**Uygulama:** Ortak nefes animasyonu dumanın bağımsız konumunda oynar; alfa dumanın toplam ömrünü izler ve her nefes döngüsünde sıfırlanmaz. Başlangıç ayarı 3 saniye ömür, 0.25 saniye açılma ve son 0.5 saniye kapanmadır; Inspector'dan değiştirilebilir. Isı yarıçapı, çarpanı ve hapşırmanın çıkış karesi korunur. İki düşmanın mevcut ortak prefab bağlantısı yeterlidir. [Ayarlar ve doğrulama](enemies.md#hapsirikkoz-hapşırma-döngüsü).

## Slime Fire — HeatAura ile ısı alanını gösterme

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı `SlimeFire_Patrol` ve `SlimeFire_Stationary` prefablarının HeatArea'sını belirginleştirmek için mevcut `FireSlime/HeatAura/` efektinin eklenmesini istedi.

**Uygulama:** Paketin 256 piksel sheet'i, 24 kare/12 FPS/2 saniye nefes döngüsüyle Slime'ın arkasında oynatılır. Görsel merkez ve çap mevcut HeatArea collider'ını izler; hasar yarıçapı ve çarpanı değişmez. Alternatif çözünürlükler korunur. İki prefab ve yeniden üretici hazır bağlanır. [Ayarlar ve doğrulama](enemies.md#slime-fire-ısı-alanının-heataura-görseli).

## Saw — Samples 90 ve eşit döngü birleşimi

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı Saw'ın fazla hızlı döndüğünü ve döngü sonunda takıldığını bildirdi; devam yanıtıyla 15'ten 90'a çıkardığı ayarın Animation penceresindeki **Samples** olduğunu netleştirdi.

**Ölçüm ve düzeltme:** Diskteki klipte 16 sprite anahtarı 1/12 saniye aralıklı, Samples 48, son anahtar 1.25 sn ve bitiş 1.2708334 sn idi. Son sprite yaklaşık 20.833 ms görünürken diğerleri 83.333 ms görünüyordu. Samples 90 korunarak görünür değişimler 12 kare/sn ve toplam süre 16/12 sn yapılır. Native Unity bitişini sabitlemek için aynı son sprite'a bir tutma anahtarı eklenir; ilk sprite yinelenmez. Her iki Saw prefabı ve özel/genel üretici aynı klibi kullanır. Önceki 1/3 sn Saw varsayılanı bu düzeltmeyle aşılmıştır; Slime/HapsirikKoz zamanlamaları değişmez. [Ayarlar ve test kaydı](enemies.md#görseller-ve-yeniden-üretim).

## Saw ve Slime Fire — yeni 256 piksel görseller

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı HapsirikKoz'daki görsel geçişinin iki Saw ve iki SlimeFire prefabına da uygulanmasını istedi. Dosyalar hazır olana kadar inceleme yapıp bekleme talimatının ardından `Saw 1` ve `FireSlime` klasörlerini vererek “başlayabilirsin” dedi.

**Uygulama:** `Saw 1/Unity_256/Patrol.png` ile iki Saw'ın sürekli dönüşü; `FireSlime/Unity_256/Idle.png` ve `Patrol.png` ile Slime'ın duran/yürüyen görselleri. 256×256 kare, 256 PPU, Bilinear, mipmap kapalı, sıkıştırmasız import. Saw'ın yeni 16, Slime'ın yeni 12'şer karesi mevcut 1⁄3, 0.9 ve 0.7 saniyelik döngü sürelerini koruyacak FPS ile oynatılır. Saw'ın ayak pivot'u yalnızca Visual'ın -0.425 Y konumuyla merkez collider'a hizalanır. Slime'ın yeni ortak ayak pivot'uyla Visual sıfırlanır ve eski Y animasyon eğrileri kaldırılır. Hareket/takip, ısı/temas ayarları, kök ölçeği ve mevcut asset GUID'leri korunur. Yeni üretim çok satırlı sheet'lerde ad sırasını kullanır. [Ayarlar ve doğrulama](enemies.md#görseller-ve-yeniden-üretim).

## Her zeminde bekleme tüketimi ve bağımsız malzeme

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı Player ve Player prefab'ı için dururken de su/can kaybı istedi. Açık sıra: çimde bekleme < kuruda bekleme < çimde yürüme < ekstra kuruda bekleme < kuruda yürüme < ekstra kuruda yürüme. Devam sorusuna “Evet, malzeme ve kuruluk bağımsız olsun” yanıtını verdi; kum, çimen ve siyah taş tek bir kuruluk durumuna sabitlenmez.

**Uygulama:** Mevcut `Ground-Wet` nemli tabandır; bekleme/yürüme oranları nemli `0.25/1`, kuru `0.5/3`, ekstra kuru `2/6` su/saniyedir. Bunlar oynanışla ayarlanacak başlangıç değerleridir. Malzeme `GroundSurface` üzerinden, kuruluk mevcut collider layer'ından okunur. Malzeme çarpanları WaterMeter Inspector'ında ayrı ayrı ayarlanır; aralarında bir sıralama istenmediğinden başlangıçta hepsi `1×` değerindedir. Böylece örneğin nemli kum veya ekstra kuru çimen mümkündür. Malzeme belirtilmeyen destek `1×` kullanır.

Isı/duman, artık seçilen bekleme veya yürüme tabanını malzeme çarpanından sonra çarpar. Eski “ısı dışında durmak ücretsiz” kuralı ve önceki duruş hasarı sayıları bu kararla geçersizdir. Havada önceki zeminin kuruluğu/malzemesi taşınmaz; normal `3` su/saniye ve varsa ısı çarpanı uygulanır. Köz Ocağı'nın bağımsız kaybı, tek su sayacı, kontrol kapalı/respawn tüketim koruması ve kaynak dolumu korunur. [Ayarlar ve test kaydı](water-player.md).

## HapsirikKoz — 256 piksel görsel setine geçiş

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı HapsirikKoz'un iki prefabını mevcut `HapsirikKoz` setinden daha yüksek çözünürlüklü `HapsirikKoz 1` setine geçirmek istedi; `Unity_256` üzerinden geçiş önerisinin ardından “Bu değişimi senin yapmanı istiyorum” talimatını verdi.

**Uygulama:** `Unity_256/Idle.png` ve `Sneeze.png`; 256×256 kareler, 256 PPU, ortak pivot `(0.5, 0.07407407)`, Bilinear, mipmap kapalı ve sıkıştırmasız import. Mevcut 8 kare/8 FPS Idle döngüsü ve 12 kare/12 FPS tek hapşırma korunur. Duman yine 05 numaralı karede çıkar. İki prefabın başlangıç sprite'ı, mevcut kliplerin sprite referansları ve üreticinin kaynak yolu değişir; hareket/takip, collider ve duman ayarları korunur. Bu, diğer görsellerin K02 kararını değiştirmez. [Bağlantılar ve doğrulama](enemies.md#görseller-ve-yeniden-üretim).

## Hareketli düşmanlar — algılama alanında oyuncuyu takip

**Görünmez sınır (31 Ekim 2026):** Kullanıcı yerçekimli takip eden düşmanların uçurumdan düşmesini önlemek için kameraya benzer, Edge ile çizilen gizli duvar istedi. `GroundEnemyBoundary` prefabında düzenlenebilir, trigger olmayan `EdgeCollider2D` kullanılır. Yalnızca yerdeki SlimeFire/HapsirikKoz fizik gövdeleriyle çarpışır; oyuncu ve uçan Saw geçebilir. Katmanlar `EnemyBoundary` ve `GroundEnemy`; kamera sınırı ayrı kalır. Düşman sınırın ötesindeki hedefi takip etmeyi denerken duvarda durur; çizgi kapalıysa düşüş normal sürer. [Yerleştirme ve ayarlar](enemies.md#yerdeki-düşmanlar-için-görünmez-sınır).

**Sonraki fizik düzeltmesi (31 Ekim 2026):** Kullanıcı takipte duvar/yer içinden geçmeyi bildirdi; uçan `Saw_Patrol` ile diğer gövde düşmanlarını ayrı değerlendirmeyi ve yerdekilerin düşmesini istedi. Saw yerçekimsiz uçar, zemin/duvara çarpar. SlimeFire ve HapsirikKoz'un duran/devriye türleri yerçekimiyle düşer; devriye/takip/dönüş motoru yalnızca X hızını belirler. Hasar trigger'ı korunur, ayrı `TerrainBody` yalnızca mevcut üç zemin katmanıyla çarpışır. Önceki yerdeki çapraz rota/yüksekliğe doğrudan dönüş davranışı bu düzeltmeyle geçersizdir. Köz Ocağı ve dumanın sabit çevresel rolleri korunur. [Fizik ve Inspector ayarları](enemies.md#yerçekimi-ve-zeminduvar-çarpışması).

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı `Assets/Prefabs/Enemies/` altındaki hareket edebilen düşmanlar için dış bölge istedi: karakter bölgeye girince normal hareket şemasını bırakıp, bölgeden çıkana kadar karakteri takip edecekler.

**Uygulama tercihleri:** Mevcut `EnemyPatrol` kullanan Saw, Slime Fire ve HapsirikKoz prefabları; düşmanla hareket eden, Inspector'dan ayarlı `4` dünya birimi yarıçaplı algılama çemberi; mevcut `Speed` ile takip; çıkışta ayrıldığı noktaya dönüp aynı rota/yönde devriyeyi sürdürme. Slime/HapsirikKoz yatayda, Saw iki eksende takip eder; uçan türde `Horizontal Chase Only` değiştirilebilir. Duran türlere takip eklenmez. Algılama hasar üretmez; mevcut ısı, duman ve gövde darbesi kendi alanlarında çalışır. Ölüm/kapanma/teleport eski takip bırakmaz. Bu değerler oynanışta ayarlanabilir başlangıç tercihleridir; yol bulma eklenmedi. [Inspector ve doğrulama](enemies.md#hareketli-düşmanların-takip-alanı).

## Köz Ocağı — sabit yatay çekim ve bağımsız su kaybı

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı `KozOcagi/` görselleri ve ayrıntılı istek metniyle sabit çevresel düşman, prefab ve test sahnesi istedi. Yalnızca yatay çekim; yaklaştıkça artan **mutlak su puanı/saniye** kaybı. Temas hasarı, knockback, takip, hapşırma ve mermi kapsam dışıdır. Tüketim ısı çarpanına dönüşmez; dururken ve gövde darbesi flash'ında da sürer.

**Netleştirme:** İstek metnindeki sabit oyuncu boyutu/hızı/zıplaması mevcut K03 uygulamasıyla çeliştiğinden soruldu. Kullanıcı “Mevcut davranışı koru; Köz Ocağı'nı ekle” dedi. Suya bağlı küçülme ve ability çarpanları korunur.

**Uygulama tercihi:** Kaynak başına yatay motor hedef hızı katkısı; su için tek `WaterMeter.Consume(rate * fixedDeltaTime)` yolu. Örtüşen Köz Ocağı katkıları toplanır; çıkış yalnızca ilgili kaynağı temizler. 3/0.75 menzil, 1–6 su puanı/saniye, 0.4–2.5 dünya birimi/saniye çekim ve 0.35 saniye rampa deneme değerleridir. [Prefab, sahne, birimler ve testler](koz-ocagi.md).

## Saw, Slime Fire ve yakınlıkta erime

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı, mevcut Saw/Slime Fire sheet'lerinden duran ve belirli iki nokta arasında git-gel yapan prefablar istedi. Slime'ın yakınlıktaki oyuncunun erime çarpanını artırması açıkça istendi. Önceki düşmanları kapsam dışında tutan karar, yalnızca bu iki düşman ve davranışları için güncellendi.

**İlk temas kararı:** Kullanıcının devam mesajı: “doğrudan temas şu anlık /to-do olarak kalsın player a knockback ekleyeceğiz.” İlk uygulamada doğrudan temas bu nedenle etkisizdi. Sonraki knockback talebiyle bu TODO uygulamaya alındı; güncel karar aşağıdadır. Isı alanı ayrı trigger'dır ve mevcut tüketimi çarpar. Çarpan 2, alan yarıçapı 1.75, git-gel offset'i (3, 0) ve hızı 1.5 deneme değerleridir. Üst üste alanlarda en güçlü etki kullanılır. Ayrıntılar [düşman belgesindedir](enemies.md).

**31 Ekim 2026 ısı düzeltmesi:** Kullanıcı “fire slime a yakınken sabit durunca canımız gitmiyor. sabit dursak da hasar yemeliyiz” dedi. Slime Fire'ın etkin ısı alanında hareketsizken de su tüketilir; önceki sıfır tüketim uygulaması bu alan için geçersizdir. Çarpan >1 olduğunda zeminin normal tüketimi çarpılır; varsayılan 2× ile kuru zeminde 6, nemli zeminde 2 birim/saniye. Alan dışında veya çarpan 1 iken yerde durma ücretsizdir. Ayrı bir can sayacı eklenmez; hasar mevcut su azalması ve sıfırda yeniden doğma yolunu izler. Bu aşamada temas/knockback TODO'ydu; sonraki karar aşağıdadır.

## Gövde teması — Knockback, flash ve tek su kaybı

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı artık knockback'i eklemeyi, ayrı bir ability olmasını, diğer projesindeki `KnockbackAbility.cs` dosyasından esinlenilmesini ve birkaç saniyelik flash sırasında `player_death_01.png` görünümünü istedi. Devam sorusuna “Evet, su kaybı da olsun; miktarı Inspector’dan ayarlayalım” yanıtını verdi. Bu karar önceki doğrudan temas TODO'sunun yerini alır.

**Uygulama:** Altı düşmanın yalnızca gövdeleri tek darbe verir; Player'ın mevcut state machine'ine `Knockback` ability/state'i eklendi. Tek su kaybı `WaterMeter.Consume` üzerinden olur. Darbe FixedUpdate'te düşmandan uzağa ve yukarı uygulanır; kısa kontrol kilidinden sonra kübik toparlanmayla normal harekete dönülür. Yeni input, çift zıplama veya can sayacı eklenmedi. Flash boyunca yeni gövde darbeleri engellenir; Slime ısısı ve dumanın süreli tüketimi devam eder. Isı/duman alanları knockback tetiklemez. Ölüm/yeniden doğma, Player veya ability kapanışı bekleyen itişi ve flash'ı temizler.

**Inspector deneme değerleri:** Force `(6, 5)`, Contact Water Loss `10`, Control Lock Duration `0.2` saniye, Recovery Duration `0.3` saniye, Recovery Airspeed `4`, Flash Duration `2` saniye ve Flash Interval `0.1` saniye. Denge değerleri oynanışla ayarlanabilir. Hasar karesinin sprite kimliği korunarak PPU/pivot normal oyuncuyla eşlendi. Bağlantılar ve test kapsamı [oyuncu](player-state-machine.md#knockback-ve-temas-hasarı) ve [düşman](enemies.md) belgelerindedir.

## HapsirikKoz — Rastgele hapşırma ve cooldown

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı mevcut `Assets/platformerPack/Sprites/Enemies/HapsirikKoz/` görsellerinden düşmanı eklemeyi ve Saw/Slime Fire prefablarını örnek almayı istedi. Devam mesajında her hapşırma öncesinde rastgele 1–5 saniye bekleme, ardından art arda sık hapşırmayı önlemek için 2–3 saniye cooldown istedi.

**Uygulama:** Duran ve mevcut `EnemyPatrol` ile iki uç arasında hareket eden iki prefab; 8 FPS Idle ve tek seferlik 12 FPS Sneeze; hapşırma tamamlandıktan sonra rastgele 2–3 saniye cooldown ve ardından yeni rastgele 1–5 saniyelik bekleme. Her iki aralık Inspector'da ayarlanabilir. Varsayılan bir saniyelik animasyonla iki başlangıç arasında en az yaklaşık dört saniye vardır. İlk eklemede hapşırma görseldi; sonraki kullanıcı talebiyle aşağıdaki duman hasarı kapsama alındı. İlk eklemede asıl sahneye yerleşim yapılmadı. Knockback'in sonraki kapsamı yukarıdadır; ayrıntılar [düşman belgesinde](enemies.md).

**31 Ekim 2026 duman hasarı:** Kullanıcı, hapşırınca birkaç saniye kalan ateş dumanının içindeki oyuncunun Slime Fire yakınlığıyla aynı mantıkta daha hızlı su tüketmesini istedi; duman animasyonunu sonraya bıraktı. Hapşırmanın çıkış karesinde bağımsız bir duman trigger'ı oluşur, çıktığı dünya konumunda kalır ve süresi dolunca yok olur. `EnemyHeatArea` ve mevcut tek `WaterMeter` kullanılır; yerde hareketsizken de tüketim vardır. Örtüşen Slime/duman kaynaklarında en güçlü çarpan geçerlidir; çıkış/kapanma/süre bitişi çarpanı temizler. Süre `3` saniye, yarıçap `1.75` ve çarpan `2` Inspector'dan ayarlanabilir deneme değerleridir. Dumanın görünümü/animasyonu bekliyor; sonradan eklenen gövde knockback'i duman alanına uygulanmaz.

## K03 — Suya bağlı görsel ve fiziksel küçülme

**Tarih:** 31 Ekim 2026.

**Dayanak:** Kullanıcı, eğimde hareket bırakılınca Jump'a geçme sorununun giderilmesiyle birlikte Player küçüldükçe collider'ın da küçülmesini ve görselin daha fazla küçülebilmesini açıkça istedi. Birden fazla collider kullanılması zorunlu tutulmadı.

**Geçerli karar:** Su azaldıkça görsel ile çarpışma boyutu birlikte küçülür. Önceki sabit collider kısıtı kaldırılmıştır. Kök ölçek değişmez. Kullanıcının sonraki hız/zıplama talebiyle hareket ve zıplama artışı da kapsam içindedir; ayrıntı aşağıdadır. Sıfır su, canlı küçük damla durumu yerine mevcut ölüm/yeniden doğma akışını başlatır. Bu karar K02 görsel tarzını veya K04 bölüm kapsamını kesinleştirmez.

**Uygulama seçimi:** Mevcut tek `CircleCollider2D` sürekli ölçeklenir; `PhysicsControl` yarıçap ve offset'i yönetir, `WaterMeter` tek su değerinden boyutu hesaplar. Animasyon esnemesi collider'a uygulanmaz. Ayak hizası düz zeminde korunur; eğimde destek noktasına göre konum telafisi yapılır. Dolumda tavan/duvar büyümeye izin vermiyorsa su hemen dolar, görsel ve collider sığan boyutta kalır ve alan açılınca büyümeyi tamamlar.

**Deneme ayarı:** Player prefab'ında `WaterMeter → Water Size → Minimum Scale = 0.45`. Önceki 0.70 yerine su sıfıra yaklaşırken %45 boyuta yaklaşılır; Inspector aralığı 0.25–1.00'dır. Bu sayı ayarlanabilir bir başlangıç değeridir.

## Su azaldıkça hız ve zıplama artışı

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı küçüldükçe hız ve zıplama mesafesinin artırılması yöntemini sordu; su oranından ayrı çarpanlar hesaplama önerisinin ardından “Koda uygula ve inspectorda ayarlanabilir yap” talimatını verdi. Bu talimat önceki sabit hız/zıplama kuralının yerini alır.

**Uygulama:** `1 - WaterMeter.NormalizedWater` değeriyle tam sudaki 1.0 katsayısından düşük su için seçilen en büyük katsayıya doğrusal geçilir. MoveAbility yürüme, JumpAbility hava hızı ve zıplama için ayrı ayarlar taşır. Yürüme/hava katsayıları güncel suyu izler; zıplama katsayısı yalnızca kalkışta alınır, havada yeniden itiş uygulanmaz. Temel hız/kuvvet, ivme ve fren değerleri ezilmez. WaterMeter bulunmayan oyuncu temel değerlerle çalışır.

**Inspector deneme değerleri:** MoveAbility Low Water Speed Multiplier=1.4; JumpAbility Low Water Air Speed Multiplier=1.4 ve Low Water Jump Multiplier=1.1. Her ayarda 1.0 artışı kapatır. Jump çarpanını 1.0 tutup hava hızını artırmak, tam zıplama yüksekliğini artırmadan yatay mesafeyi uzatır. Su dolunca sonraki hareket ve zıplama temel değerlere döner; halen devam eden zıplamanın dikey hızı yeniden yazılmaz. Bu ayarlar oynanışla dengelenecek başlangıç değerleridir.

## Üçüncü zemin: çok kuru (Ground-Extra-Dry)

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı bölüme "extra dry" zemin ekledi, `Ground-Extra-Dry` fizik layer'ını (10) oluşturdu ve kodun bu layer'ı algılamasını istedi. Genel plandaki "sıcak bölge ilk sürüm için zorunlu değil" notu ve P2 "özel sıcak/kuru alan" fikri bu talimatla kapsama alınmıştır.

**Uygulama:** `PhysicsControl` üçüncü bir LayerMask (`Extra Dry Ground Layer`) taşır ve `extraDryGrounded` bilgisini verir; bu zemin de `grounded` sayılır, böylece yürüme ve zıplama normal çalışır. `WaterMeter`, çok kuru zeminde gerçek yatay ilerlemede `Extra Dry Drain Per Second` oranını kullanır. Isı dışında yerde dururken tüketim yoktur; etkin Slime ısısında dururken de çok kuru oran ısı çarpanıyla uygulanır. Havada kuru oran geçerlidir, çok kuru katsayı havaya taşınmaz.

**Isı alanıyla birlikte:** Slime Fire ısı alanının `DrainMultiplier` değeri, o an geçerli zemin oranını çarpar. Çok kuru zeminde etkin ısı alanında dururken ve hareket ederken deneme değerleriyle 6 × 2 = 12 birim/saniye tüketilir; bu birleşim dengelenirken birlikte test edilmelidir.

**Deneme ayarı:** `Extra Dry Drain Per Second = 6` (kuru oranın iki katı). Oynanışla dengelenecek başlangıç değeridir. Görsel olarak yalnızca renkle değil doku/şekil ile de ayrılması gerekir (T43).

## K04 (kısmi) — İkinci bölüm

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı: “Ben şimdi Level 2yi yapmak istiyorum.” Genel plandaki “ek bölüm eklenmez” varsayılanı ve T38'deki “ikinci bölüm şart değil” notu bu talimatla aşılmıştır.

**Karar:** Oyun `Level1` ve `Level2` olmak üzere iki bölümden oluşur. Her bölüm ayrı sahnedir. Yaşayan tek fidan yalnızca son bölümdedir; `Level1` bir geçiş noktasıyla biter, ara fidan eklenmez.

**Bölüm sonu (31 Ekim 2026):** Kullanıcı: “benim sonraki seviyeye geçmem için o su birikintileri var ya, oralara gelmem gerekiyor, yani aslında su birikintisi level sonuna ulaştın demek.” Bölümün sonu bir su birikintisidir; o birikintiye `LevelExit` eklenir ve sıradaki bölüm yüklenir. Son bölümün sonunun (fidan) nasıl kurulacağı ayrıca belirlenecek.

**Açık kalanlar:** Bir sonraki bölüme su taşınıp taşınmayacağı (öneri: her bölüm dolu suyla başlar; sahne yüklenince bu kendiliğinden olur), bölüm uzunlukları ve kaynak sayıları (T38, ilk oynanış testinden sonra), “Tekrar oyna”nın `Level1`'den mi son bölümün başından mı başlayacağı.
