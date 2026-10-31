# Kararlar

## Saw, Slime Fire ve yakınlıkta erime

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı, mevcut Saw/Slime Fire sheet'lerinden duran ve belirli iki nokta arasında git-gel yapan prefablar istedi. Slime'ın yakınlıktaki oyuncunun erime çarpanını artırması açıkça istendi. Önceki düşmanları kapsam dışında tutan karar, yalnızca bu iki düşman ve davranışları için güncellendi.

**Temas kararı:** Kullanıcının devam mesajı: “doğrudan temas şu anlık /to-do olarak kalsın player a knockback ekleyeceğiz.” Bu nedenle doğrudan temas şimdilik etkisizdir; knockback, temas hasarı veya temasla yeniden doğma uygulanmadı. Isı alanı ayrı trigger'dır ve mevcut tüketimi çarpar. Çarpan 2, alan yarıçapı 1.75, git-gel offset'i (3, 0) ve hızı 1.5 deneme değerleridir. Üst üste alanlarda en güçlü etki kullanılır. Ayrıntılar [düşman belgesindedir](enemies.md).

**31 Ekim 2026 ısı düzeltmesi:** Kullanıcı “fire slime a yakınken sabit durunca canımız gitmiyor. sabit dursak da hasar yemeliyiz” dedi. Slime Fire'ın etkin ısı alanında hareketsizken de su tüketilir; önceki sıfır tüketim uygulaması bu alan için geçersizdir. Çarpan >1 olduğunda zeminin normal tüketimi çarpılır; varsayılan 2× ile kuru zeminde 6, nemli zeminde 2 birim/saniye. Alan dışında veya çarpan 1 iken yerde durma ücretsizdir. Ayrı bir can sayacı eklenmez; hasar mevcut su azalması ve sıfırda yeniden doğma yolunu izler. Doğrudan temas/knockback TODO kararı sürer.

## HapsirikKoz — Rastgele hapşırma ve cooldown

**Tarih ve dayanak:** 31 Ekim 2026. Kullanıcı mevcut `Assets/platformerPack/Sprites/Enemies/HapsirikKoz/` görsellerinden düşmanı eklemeyi ve Saw/Slime Fire prefablarını örnek almayı istedi. Devam mesajında her hapşırma öncesinde rastgele 1–5 saniye bekleme, ardından art arda sık hapşırmayı önlemek için 2–3 saniye cooldown istedi.

**Uygulama:** Duran ve mevcut `EnemyPatrol` ile iki uç arasında hareket eden iki prefab; 8 FPS Idle ve tek seferlik 12 FPS Sneeze; hapşırma tamamlandıktan sonra rastgele 2–3 saniye cooldown ve ardından yeni rastgele 1–5 saniyelik bekleme. Her iki aralık Inspector'da ayarlanabilir. Varsayılan bir saniyelik animasyonla iki başlangıç arasında en az yaklaşık dört saniye vardır. İlk eklemede hapşırma görseldi; sonraki kullanıcı talebiyle aşağıdaki duman hasarı kapsama alındı. Knockback TODO kararı korunur. Asıl sahneye yerleşim yapılmadı. Ayrıntılar [düşman belgesinde](enemies.md).

**31 Ekim 2026 duman hasarı:** Kullanıcı, hapşırınca birkaç saniye kalan ateş dumanının içindeki oyuncunun Slime Fire yakınlığıyla aynı mantıkta daha hızlı su tüketmesini istedi; duman animasyonunu sonraya bıraktı. Hapşırmanın çıkış karesinde bağımsız bir duman trigger'ı oluşur, çıktığı dünya konumunda kalır ve süresi dolunca yok olur. `EnemyHeatArea` ve mevcut tek `WaterMeter` kullanılır; yerde hareketsizken de tüketim vardır. Örtüşen Slime/duman kaynaklarında en güçlü çarpan geçerlidir; çıkış/kapanma/süre bitişi çarpanı temizler. Süre `3` saniye, yarıçap `1.75` ve çarpan `2` Inspector'dan ayarlanabilir deneme değerleridir. Dumanın görünümü/animasyonu ve doğrudan temas/knockback bekliyor.

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
