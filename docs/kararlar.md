# Kararlar

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
