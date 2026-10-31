# Saw, Slime Fire ve HapsirikKoz prefabları

31 Ekim 2026 kullanıcı talebiyle iki düşmanın duran ve iki nokta arasında git-gel yapan türleri eklendi. Doğrudan temas başlangıçta knockback eklenene kadar TODO'ydu; kullanıcının sonraki talebiyle altı düşman gövdesine temas hasarı ve knockback bağlandı. Gövdeler trigger'dır; fiziksel engel oluşturmaz.

Aynı gün kullanıcı HapsirikKoz'u da istedi. Duran ve devriye türleri, mevcut `EnemyPatrol` ve `Visual`/Animator düzeniyle eklendi. Hapşırma zamanlaması kullanıcının devam mesajına göre rastgele bekleme ve ayrı cooldown kullanır.

## Yerleştirme ve ayarlar

Unity proje köküne göre `Assets/Prefabs/Enemies/` altında altı hazır prefab bulunur:

| Prefab | Davranış |
|---|---|
| `Saw_Stationary.prefab` | Yerinde kalan, sürekli dönen bıçak |
| `Saw_Patrol.prefab` | Sürekli dönen, iki nokta arasında hareket eden bıçak |
| `SlimeFire_Stationary.prefab` | Idle animasyonu ve yakınlıkta erime çarpanı |
| `SlimeFire_Patrol.prefab` | Walk animasyonu, git-gel ve yakınlıkta erime çarpanı |
| `HapsirikKoz_Stationary.prefab` | Idle, rastgele Sneeze, geçici hasarlı duman ve cooldown |
| `HapsirikKoz_Patrol.prefab` | Aynı hapşırma/duman döngüsüyle git-gel ve hareket yönüne dönüş |

Prefab'ı istenen sahneye sürüklemek yeterlidir. Sahnelere otomatik düşman yerleştirilmedi. Saw kökü bıçağın merkezi, Slime kökü ayak seviyesidir. Kök Rigidbody2D kinematiktir ve yerçekimi kullanmaz; güzergâh zemin aramaz. Özellikle Slime rotasını yürünebilir yüzeye yerleştir.

HapsirikKoz kökü de ayak seviyesindedir. Sprite'ların ortak `(64, 13)` piksel pivot'u korunur; `Visual` yerel konumu sıfırdır. Gövde trigger'ının boyutu `(0.64, 0.7)`, offset'i `(0, 0.35)` deneme ayarıdır. Devriye sırasında Idle/Sneeze oynatılır; pakette ayrı yürüyüş animasyonu yoktur.

Hareketli türlerde **EnemyPatrol → End Offset**, yerleştirilen başlangıca göre dünya birimi cinsinden diğer uçtur; varsayılan `(3, 0)`. **Speed** varsayılan `1.5` birim/saniyedir. Negatif, dikey ve çapraz offset kullanılabilir. Sıfır hız veya sıfır offset hareketi durdurur. Seçili prefab'ın iki ucu Scene görünümünde sarı çizilir. Slime hareket yönüne döner; Saw'ın yüzü dik kalırken dişleri sheet animasyonuyla döner.

Slime'ın `HeatArea` çocuğunda:

- **EnemyHeatArea → Drain Multiplier:** Varsayılan `2`; `1` etkiyi kapatır.
- **CircleCollider2D → Radius:** Varsayılan `1.75`; offset `(0, 0.4)`.
- **Is Trigger:** Açık tutulmalı. Kök gövde collider'ı, yakınlık alanından ayrıdır.

Yeni fizik layer'ı veya Player prefab bağlantısı gerekmez. Oyuncunun gerçek Rigidbody2D'si üzerindeki mevcut `Player` ve `WaterMeter` bulunur; düşman alanı Water layer'ına taşınmamalıdır. Özel collision matrix değişikliği yapılırsa Default ve Player arasındaki trigger etkileşimi korunmalıdır.

## HapsirikKoz hapşırma döngüsü

Kökteki `HapsirikKozSneeze`, `Visual` Animator'ına bağlıdır; prefab'larda bu bağlantı hazırdır.

- **Sneeze Delay Range:** Varsayılan `(1, 5)` saniye. Her tur için bu aralıktan yeni rastgele süre seçilir.
- **Cooldown Range:** Varsayılan `(2, 3)` saniye. Tam hapşırma animasyonu bittikten sonra bu aralıktan yeni rastgele cooldown seçilir. Cooldown bitince yeni 1–5 saniyelik bekleme başlar; süreler birbirine eklenir.
- **Automatic Sneezing:** Otomatik döngüyü kapatır/açar. `Sneeze()` metodu veya component'in **Sneeze** bağlam menüsü elle tetikleyebilir; animasyon sürerken ve cooldown içinde gelen istekler reddedilir.

Varsayılanlarla sıra **1–5 saniye bekleme → 1 saniye hapşırma → 2–3 saniye cooldown → yeni bekleme** şeklindedir. İki hapşırmanın başlangıcı arasında en az yaklaşık 4 saniye vardır. Süreler oyun zamanını izler; oyun duraklatılınca ilerlemez. Obje/component yeniden etkinleşince yeni bekleme seçilir.

Kullanıcının sonraki talebiyle hapşırmaya geçici duman hasarı eklendi. Hazırlık kareleri duman üretmez; `Sneeze` animasyonunun 05 numaralı çıkış karesinde (sıfır tabanlı, yaklaşık 0.417 saniye) tek bir `HapsirikKoz_Smoke.prefab` oluşturulur. Dumanın bağımsız Rigidbody2D'si vardır; devriye düşmanı uzaklaşsa veya kapatılsa da çıktığı noktada ömrünü tamamlar. Alan oluşmadan veya alanın dışındayken duman hasarı uygulanmaz. Duman görseli/animasyonu sonraya bırakıldı. Düşmanın gövdesi animasyonundan bağımsız olarak aşağıdaki temas darbesini verebilir.

`Assets/Prefabs/Enemies/HapsirikKoz_Smoke.prefab` üzerindeki Inspector ayarları:

- **EnemySmokeCloud → Duration:** Varsayılan `3` saniye; süre sonunda obje ve ısı etkisi temizlenir.
- **EnemyHeatArea → Drain Multiplier:** Varsayılan `2`; Slime Fire ile aynı tüketim sistemini kullanır. `1` etkiyi kapatır.
- **CircleCollider2D → Radius:** Varsayılan `1.75`; offset `(0, 0.4)`, **Is Trigger** açık.

İki düşman prefabının **HapsirikKozSneeze → Smoke Cloud Prefab** bağlantısı hazırdır. Sahneye ayrıca duman yerleştirmek gerekmez. Duman içinde hareketsizken de zemin tüketimi çarpılır; Slime/duman alanları örtüşürse en yüksek çarpan uygulanır. Alandan çıkınca, duman kapanınca veya süresi dolunca o kaynağın çarpanı kalkar. Duman fiziksel engel değildir ve Water layer'ında değildir.

## Gövde teması ve knockback

Altı düşman prefabının kökünde `EnemyContactKnockback` vardır. Yalnızca kökteki gerçek gövde collider'ına değen oyuncunun `KnockbackAbility.TryKnockback` metodu çağrılır. Rigidbody'ye çocuk ısı alanından gelen trigger olayları, gövde gerçekten temas etmiyorsa reddedilir. Slime'ın geniş HeatArea'sı ve HapsirikKoz dumanı knockback/flash veya tek seferlik temas hasarı üretmez; kendi süreli tüketimlerini sürdürür.

Varsayılan kabul edilen darbe: oyuncudan 10 su, düşmandan uzağa `(6, 5)` hız, 0.2 saniye kontrol kilidi + 0.3 saniye toparlanma ve 2 saniye hasar karesiyle flash. Flash yeni gövde darbelerini engeller; ısı/duman su tüketimini durdurmaz. İtiş, su kaybı ve flash ayarları Player prefabındaki `KnockbackAbility` üzerindedir; ayrıntılar [oyuncu belgesinde](player-state-machine.md#knockback-ve-temas-hasarı). Düşmanlarda ek Inspector referansı gerekmez. Su biterse mevcut tek ölüm/yeniden doğma akışı çalışır.

Mevcut prefabları ayarlarını koruyarak güncellemek için `GameUp → Player → Configure Knockback` kullanılabilir. Düşman üreticileri de yeni ürettikleri altı gövdeye temas component'ini ekler; duman prefabına eklemez.

## Su tüketimi ve yaşam döngüsü

`WaterMeter` tek su sayacı olmaya devam eder. 31 Ekim kullanıcı kararlarıyla etkin Slime ısısı veya HapsirikKoz dumanı çarpanı (>1) içinde yerde hareketsiz durmak da su kaybettirir. Dururken ve hareket ederken varsayılan `2x` ile kuru zeminde/havada `6`, nemli zeminde `2`, çok kuru zeminde `12` birim/saniye tüketilir. Isı/duman dışında veya çarpan `1` iken yerde durmak ücretsizdir. `Consume(amount)` ile yapılan açık miktarlı işlemler çarpılmaz; kaynak dolumu değişmez.

`SetDrainMultiplier(source, multiplier)` ve `RemoveDrainMultiplier(source)` kaynak başına geçici etkiyi yönetir. Aynı alanda kalmak çarpanı tekrar tekrar çarpmaz. Örtüşen alanlarda en yüksek çarpan uygulanır; güçlü alandan çıkınca hâlâ temas edilen diğer alanın çarpanı kalır. Bir oyuncunun birden fazla collider'ı varsa son temas bitene kadar etki korunur.

Trigger giriş/kalma/çıkış olayları kullanılır. Devre dışı alan, kapatılan/yok edilen collider veya düşman ve teleport sonrası eski temaslar temizlenir. WaterMeter kapatıldığında veya yeniden doğma başladığında geçici çarpanları temizler; oyuncu yeniden etkinleşip aynı alanda kalırsa trigger stay etkiyi tekrar kurar. Kontrol kapalıyken ve yeniden doğmada mevcut tüketim korumaları sürer.

## Görseller ve yeniden üretim

`Assets/Animations/Enemies/` altındaki klip/controller'lar doğrudan ilgili `Sheets` sprite alt asset'lerini kullanır. İki Saw türü de `saw_walk.png` üzerindeki sekiz dönüş karesini 24 kare/saniyede, 1/3 saniyelik eşit aralıklı döngüde oynatır. Döngü sonuna ilk kare tekrar eklenmez; böylece birleşimde fazladan bekleme oluşmaz. Slime duran türde `slime_fire_idle.png`, hareketli türde `slime_fire_walk.png` kullanır ve mevcut 10 kare/saniye klipleri korunmuştur. Slime'ın kırpılmış kareleri ayak seviyesini koruyacak görsel Y konumuyla oynatılır. Kök ve collider animasyondan ölçek/konum almaz.

Saw idle/walk sheet'lerinin otomatik dilimlenmesinde ilk iki kare birleşmişti. Unity Sprite Editor veri sağlayıcısıyla sekiz adet 128×128 kareye düzeltildi; var olan GUID, spriteID ve fileID'ler korundu, eksik ikinci kare için `_01` adlı yeni dilim eklendi. Kullanıcının PPU 128 ve Point filtre tercihleri korundu. Slime dilimlerine dokunulmadı.

`Assets/Editor/Enemies/EnemyPrefabBuilder.cs`, **GameUp → Enemies → Rebuild Enemy Prefabs** menüsünden aynı asset'leri yeniden üretebilir. Menü, altı düşman prefab'ının, duman prefab'ının, beş klibin ve dört controller'ın ayarlarını varsayılanlara yeniden yazar; hazır prefab'ları kullanmak için menüyü çalıştırmak gerekmez. HapsirikKoz menüsü de iki düşman prefabıyla duman prefab'ını birlikte üretir. Projeye yeni paket eklenmedi.

Yalnızca Saw klibini güncellemek için **GameUp → Enemies → Rebuild Saw Animation** kullanılabilir; bu menü prefabları ve Slime kliplerini yeniden yazmaz. Ortak klip üreticisi, yeni üretimlerde döngü sonunu kare sayısı/kare hızı ile belirler ve ilk kareyi sonda tekrar eklemez.

Yalnızca HapsirikKoz'u üretmek için **GameUp → Enemies → Rebuild HapsirikKoz Prefabs** kullanılabilir (`HapsirikKozPrefabBuilder.cs`). Bu menü mevcut Saw/Slime asset'lerine yazmaz. `HapsirikKozIdle.anim`, `Idle.png` içindeki 8 kareyi 8 FPS ile döngüde; `HapsirikKozSneeze.anim`, `Sneeze.png` içindeki 12 kareyi 12 FPS ile bir kez oynatır. Çok satırlı sheet'ler `_00`, `_01` sırasıyla okunur; ilk kare döngü sonuna tekrar eklenmez. Ortak `HapsirikKoz.controller`, `Sneeze` trigger'ıyla Idle'dan Sneeze'e geçer ve animasyonun tamamından sonra Idle'a döner. Klipler yalnızca `Visual` SpriteRenderer'ının sprite'ını değiştirir. Kullanıcının mevcut PPU 128, Point ve sıkıştırma ayarları değiştirilmedi.

## Bekleyen iş

- [ ] Asıl sahnede knockback kuvveti, temas su kaybı ve flash süresinin hissini oynayarak dengele. Altı prefab bağlantısı ayrı Unity projesinde doğrulandı.
- [ ] Asıl bölümde yerleşim, rota güvenliği, ısı yarıçapı ve çarpan dengesini elle oynayarak doğrula; dış build'i test et.

## Doğrulama

İlk uygulamada Unity 6000.5.0f1 ile `C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073` altındaki bağımsız projede 51 doğrulama geçti: 20 asset/bağlantı kontrolü ve 31 Play Mode davranış kontrolü. Bu ilk kayıt, kullanıcı hareketsiz ısı hasarını istemeden önceki davranışa aittir. Asıl projede açık editör bulunduğundan ikinci editör bu projeye başlatılmadı. Güncel Player prefab'ı/kodu, Input System, oyuncu animasyonları ve gerçek düşman sheet'leri kopyalandı; düşman prefabları Unity tarafından üretildi. Geçen kontrollerin ardından aynı prefab/animasyon dosyaları ve meta'ları asıl projeye aktarıldı.

Kapsam: dört prefab'ın component ve sheet referansları, sekiz tek bıçaklı Saw karesi, Slime ayak hizası, duran türlerin konumu, iki uçta dönüş ve yönlenme, negatif/çapraz offset, sıfır hız, gerçek trigger giriş/kalma/çıkış, örtüşmede en güçlü etki, çoklu collider, alan/collider/WaterMeter kapatma-açma, düşmanın/collider'ın silinmesi, teleport, yeniden doğma ve sıfır su temizliği. Doğrudan temasın etkisiz kaldığı kontrol edildi. 2x ısıda kuru/hava tüketimi yaklaşık 6.0001, nemli tüketim 2.0000 birim/saniye ölçüldü; yerde durma ve kapalı kontrolde tüketim sıfır kaldı.

Test düzeneği: `Assets/Editor/EnemyVerification.cs`; kayıtlar: `verification.log` ve `verification-result.txt` (geçici test projesinde). Saw onarımında var olan tüm spriteID/fileID değerlerinin, texture GUID'lerinin ve kullanıcı import ayarlarının korunduğu ayrıca karşılaştırıldı. Bu doğrulama asıl Components/Level 5 sahnelerinin oynanışı, render kontrolü veya dış build testi değildir; bunlar bekliyor.

31 Ekim 2026 Saw geçiş düzeltmesi: önceki 10 FPS klip, sekiz kareye ek olarak ilk kareyi sonda tekrarlıyor ve 0.9 saniyelik döngünün birleşiminde ilk kareyi toplam 0.2 saniye tutuyordu. Yeni 24 FPS klipte sekiz benzersiz kare, her biri 1/24 saniye gösterilir; toplam döngü 1/3 saniyedir. Yukarıdaki bağımsız Unity 6000.5.0f1 projesinde 4 asset ve 10 Play Mode kontrolü geçti. İki gerçek Saw prefabının Animator'ı beşer döngüde toplam 800 kez örneklendi; sıra, eşit kare süreleri, döngü birleşimi ve kök/collider korunması doğrulandı. Düzenek `Assets/Editor/SawAnimationVerification.cs`, sonuç `saw-animation-result.txt`, log `saw-animation-verification.log` dosyalarındadır. Aynı test edilmiş klip mevcut GUID ile projeye aktarıldı. Asıl sahnede görüntü ve dış build testi yapılmadı.

31 Ekim 2026 hareketsiz ısı hasarı: kullanıcının yeni kararına göre `WaterMeter.FixedUpdate`, çarpan >1 iken yerde hareketsiz tüketimi de uygular. Aynı bağımsız Unity projesinde güncel kod, gerçek Player ve Slime prefablarıyla 27 Play Mode kontrolü geçti. Kuru/nemli zeminde alan dışında sıfır, 2× ısı içinde dururken ve yürürken yaklaşık 6/2 birim/saniye; alan çıkışında ve ısı/oyuncu kontrolü kapatıldığında kaybın durması; çarpan 1 ile etkiyi kapatma; örtüşmede en güçlü etki ve 0.01/0.04 saniyelik fizik adımlarında aynı tüketim doğrulandı. Hareketsiz ısı tek başına suyu bitirip yalnızca bir yeniden doğma başlattı; güvenli checkpoint'te tam su ve kontrol geri geldi. Düzenek `Assets/Editor/HeatIdleVerification.cs`, sonuç `heat-idle-result.txt`, log `heat-idle-verification.log` dosyalarındadır. Asıl sahneler ve build çalıştırılmadı. İlk 51 kontroldeki “ısı içinde yerde dururken tüketim sıfır” sonucu artık geçerli tasarım değildir.

31 Ekim 2026 HapsirikKoz ilk entegrasyonu (duman hasarı eklenmeden önce): aynı bağımsız Unity 6000.5.0f1 projesinde **43 kontrol geçti (18 asset, 25 Play Mode)**. Ana editörün ürettiği yeni script GUID'leri test projesinde kullanıldı; iki prefab, iki klip ve controller Unity tarafından üretildi ve test edilmiş dosyalar meta'larıyla birlikte birebir ana projeye aktarıldı. Yeniden üretimde GUID'lerin korunması, çift state/geçiş oluşmaması ve mevcut Saw/Slime asset'lerinin değişmemesi doğrulandı.

Her iki gerçek prefab'ın Animator'ında 8 Idle karesi iki döngü boyunca, 12 Sneeze karesi bir tam oynatım boyunca örneklendi: sıra ve eşit kare süreleri, Idle'a dönüş, kök/collider/ayak pivot'unun korunması geçti. On iki düşmanda ilk rastgele beklemeler 1–5 saniye, animasyon sonrası cooldown + yeni bekleme toplamı 3–8 saniye aralığında kaldı. Yeni bekleme sıfırlanarak ayrı ölçülen altı rastgele cooldown 2.030–2.876 saniye sürdü; animasyonun bitiminden sonra başladıkları ve cooldown'da elle tetiklemenin reddedildiği kontrol edildi. Devriye/yönlenme, negatif-çapraz offset, sıfır hız, otomatiği kapatma ve yeniden etkinleştirme de geçti.

Düzenek `Assets/Editor/HapsirikKozVerification.cs`, sonuç `hapsirik-koz-result.txt`, log `hapsirik-koz-verification.log` dosyalarındadır (`C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073`). Logda testlerden bağımsız bir `UnityEditor.Search.SearchDatabase` başlangıç indeksleme hatası da vardır; C# derleme hatası veya başarısız davranış kontrolü yoktur. Asıl sahnede yerleştirme/görüntü ve dış build testi bekliyor.

31 Ekim 2026 HapsirikKoz duman hasarı: bu ekin doğrulamasında aynı bağımsız Unity 6000.5.0f1 projesinde **40 kontrol geçti (10 asset, 30 Play Mode)**. Güncel Player/su/düşman kodu, gerçek Player ve iki HapsirikKoz prefab'ı kullanıldı; mevcut Idle/Sneeze klipleri ve controller birebir korundu. Oyuncu test zemininin üzerine yerleştirildi; çok kuru zemin maskesi test örneğine bağlandı. Duman prefab'ı Unity'de üretildi; iki mevcut düşman prefab'ına yalnızca duman prefab referansı eklendi. Test edilmiş üç prefab ve yeni duman meta'sı ana projeye aktarıldı; mevcut GUID'ler ve diğer ayarlar korundu.

Doğrulananlar: Idle/gövde teması ve hazırlık karelerinde sıfır hasar; çıkış karesinde tek alan; 3 saniyelik ömür (ölçüm 2.998 saniye); kuru/nemli/çok kuru zeminde hareketsiz tüketim yaklaşık 6/2/12, havada 6 birim/saniye; alan çıkışı ve tekrar giriş; çarpan 1 ile kapatma; Slime ve birden çok duman örtüşmesinde en güçlü kaynağın kullanılması; güçlü kaynak bitince zayıfa dönüş; erken kapatma/silme, çoklu oyuncu collider'ı, kapalı kontrol ve su bitmesiyle tek yeniden doğma sonrası temizlik. Devriye düşmanı ilerlerken dumanın çıktığı konumda kaldığı, düşman hazırlık sırasında kapatılırsa duman çıkmadığı ve oluşmuş dumanın düşman silinse de kendi ömrünü tamamladığı da geçti.

Düzenek `Assets/Editor/HapsirikSmokeVerification.cs`, sonuç `hapsirik-smoke-result.txt`, log `hapsirik-smoke-verification.log` dosyalarındadır (aynı geçici proje). Logda önceki kayıtta belirtilen bağımsız Unity Search başlangıç hatası vardır; C# derlemesi ve 40 kontrol başarılıdır. Ana sahne yerleşimi/oynanışı ve dış build testi bekliyor; duman görseli kullanıcı kararıyla sonraya bırakıldı.

31 Ekim 2026 gövde knockback'i: güncel Player ve altı düşman prefab'ıyla aynı ayrı Unity projesinde **49 kontrol geçti (13 asset, 36 Play Mode)**. Altı gövde gerçek trigger temasında bir kez su düşürüp oyuncuyu itti. Flash sırasında tekrar darbeler ve çoklu collider/düşman yığılması engellendi; koruma bitince trigger stay yeni darbeyi verebildi. Slime geniş alanı ve duman itme üretmeden yaklaşık 6 su/s tüketimini sürdürdü; bu tüketim flash sırasında da sürdü. İtiş, hasar görünümü/yanıp sönme, Inspector ayarları, temizleme/ölüm/yeniden doğma ve hareket regresyonları [oyuncu belgesinde](player-state-machine.md#doğrulama) kayıtlıdır. Önceki 51/40 kontrol kayıtlarındaki “gövde teması zararsız” davranışı bu yeni kullanıcı kararıyla değişmiştir. Sahne yerleşimi/his ve build kontrolü bekliyor.
