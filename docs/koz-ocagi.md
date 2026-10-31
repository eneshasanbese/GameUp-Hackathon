# Köz Ocağı

31 Ekim 2026 kullanıcı talebi; T09/T14–T16 ve ilgili QA01–QA09/QA16 ek kapsamı. Mevcut suya bağlı küçülme, hız ve zıplama, kullanıcının netleştirmesiyle korunur.

## Prefab ve Inspector

Eklenen kaynaklar: `Assets/Scripts/Enemies/EmberSuctionEnemy.cs`, `EmberSuctionDemoControls.cs`, `Assets/Editor/Enemies/EmberSuctionPrefabBuilder.cs` ve `EmberSuctionVerification.cs`; prefab/sahne ve tüm yeni meta dosyaları. Mevcut kodda yalnızca `Player.cs` ve `PhysicsControl.cs` değişti; `WaterMeter` mevcut API'siyle kullanıldı. Ortak yönergeler, kararlar, plan/takip ve ilgili teknik belgeler güncellendi.

Unity köküne göre `Assets/Prefabs/Enemies/KozOcagi_Stationary.prefab` sahneye sürüklenebilir. Ayak hizası köktür; `Visual`, `RangeOrigin`, `PullPoint` ve `EffectArea` ayrıdır. Kök Rigidbody2D veya katı gövde taşımaz. Temas hasarı, knockback, takip, hapşırma veya mermi yoktur.

`EffectArea` üzerindeki `EmberSuctionEnemy` referansları hazır bağlıdır:

| Alan | Başlangıç ayarı |
|---|---|
| Effect Radius / Inner Full Effect Radius | 3 / 0.75 dünya birimi |
| Min / Max Water Loss Per Second | 1 / 6 su puanı/saniye |
| Min / Max Pull Strength | 0.4 / 2.5 dünya birimi/saniye yatay motor hız katkısı |
| Activation Ramp Duration | 0.35 saniye; yalnızca çekimin giriş rampası |
| Horizontal Dead Zone | 0.08 dünya birimi; ağız çevresinde sıfır çekim |
| Player Layer Mask | Player |
| RangeOrigin / PullPoint | Köke göre `(0, 0.5)` / `(0, 0.38)` |

Sayılar deneme ayarıdır. Yarıçap sırası, negatif ve sonlu olmayan değerler doğrulanır. İlgili min/max çiftini 0 yapmak o etkiyi kapatır. Dünya yarıçapı transform ölçeği değişse de korunur. Scene Gizmos dış/iç menzili ve ağzı gösterir.

Menzil `RangeOrigin` ile oyuncunun `Rigidbody2D.worldCenterOfMass` noktası arasındaki **2D mesafedir**. Merkez dışarıdaysa collider kenarı trigger'a değse bile etki yoktur. Yakınlık dış sınırdan iç yarıçapa doğrusal 0–1 artar. Her fizik adımında bir kez `WaterMeter.Consume(Lerp(min, max, yakınlık) * Time.fixedDeltaTime)` çağrılır. Bu **mutlak puandır**, kalan su yüzdesi veya normal tüketim çarpanı değildir. 6 puan/saniye yarım saniyede yaklaşık 3 puandır. Dururken ve gövde darbesi flash'ında da sürer. Çekim nedeniyle gerçek hareket oluşursa normal zemin tüketimi ayrıca devam eder.

## Hareket ve yaşam döngüsü

Unity `6000.5.0f1`; mevcut motor `PhysicsControl.ApproachHorizontalSpeed` üzerinden `linearVelocity` yazar. Kaynak başına yatay **hedef hız katkısı**, normal Idle/Move/Jump hedefiyle toplanır; mevcut ivme toplam hedefe yaklaştırır. Ayrı script'ler hızı ezmez, kuvvet/kütle birimleri karışmaz ve sınırsız ivmelenme olmaz. Çıkış yalnızca ilgili kaydı kaldırır; tüm hız sıfırlanmaz, mevcut motor yavaşlaması çalışır.

Yön yalnızca ağızla oyuncu merkezi arasındaki X farkıdır. Ölü bölgede sıfır, hemen dışında kısa yumuşama vardır. Düşman Y hızına veya yerçekimine yazmaz; mevcut eğim motoru yüzey takibini sürdürür. Mevcut Knockback kontrol kilidi önceliklidir; su kaybı o sırada da sürer, normal motor kontrolü dönünce çekim hareketle birleşir. Su azalması mevcut boyut/hız/zıplama ayarlarını doğal olarak etkileyebilir; düşman bunlara doğrudan yazmaz.

Trigger'ın gerçek örtüşmeleri fizik adımlarında sorgulanır. Player layer'ı ve Rigidbody üzerindeki `Player`/`PhysicsControl`/`WaterMeter` birlikte aranır. Trigger oyuncu collider'ları filtrelenir. Rigidbody başına tek kayıt etkiyi çoğaltmaz; son collider çıkana kadar etki korunur. Merkez mesafesi/etkinlik kontrolleri teleport, collider/kontrol/WaterMeter/fizik kapanışı ve respawn'ı izler. Düşman kapanışı/silinmesi ve sahne boşaltılması da kaydı temizler. Event aboneliği yoktur.

Birden çok Köz Ocağı su kayıplarını ve yatay katkılarını toplar; biri kalkınca diğeri kalır. Demo tek düşmanlıdır; çoklu yerleşimde toplam çekim ayrıca dengelenmelidir. Kazanma sistemi henüz yoktur; mevcut sözleşmeye göre Player kapatılınca tüketim durur ve motor kayıtları temizlenir. `Time.timeScale = 0` fizik adımlarını/tüketimi durdurur.

## Görsel ve demo

Verilen `KozOcagi/Unity_128/Idle.png` sheet'inin `Idle_00` sprite'ı kullanılır. Tek kare 128×128; alfa >10 içerik sınırı üstten ölçümle `(11,24)–(116,108)`, 9.443 piksel şeffaf/çok saydamdır. Mevcut PPU 128 ve ayak pivotu `(0.5, 0.15833333)` korunur. Yaklaşık 0.83 birim görünür genişlik, 1 birim çaplı Player ile karşılaştırılıp yalnızca `Visual` 1.5 ölçeklendi. Kaynak importları/GUID'ler değiştirilmedi. Tek idle sprite kullanılır; animasyon/FX eklenmedi.

`Assets/Scenes/KozOcagiDemo.unity`: düz kuru zemin, menzil dışında başlayan Player, tek düşman ve mevcut `Canvas`/`WaterBarUI`. A/D veya oklar yürütür, Space zıplatır; **R** veya ekrandaki düğme başlangıca tam suyla döndürür. Ana bölüm sahneleri, Player prefab ayarları ve build sahne listesi korunur.

Yeniden üretme menüleri: **GameUp → Enemies → Rebuild Koz Ocagi Prefab** ve **Rebuild Koz Ocagi Demo**. Yalnızca bu iki asset'i varsayılanlara yazar; hazır prefab/sahneyi kullanmak için çalıştırmak gerekmez.

## Doğrulama

Unity 6000.5.0f1 ile açık ana editörden ayrı `C:/Users/yhcan/AppData/Local/Temp/gameup-enemies-324e5073` projesinde **54 kontrol geçti: 9 asset/bağlantı ve 45 Play Mode**. Güncel kod, gerçek Player/Canvas prefabları ve kaynak sprite kullanıldı. Kontroller `Assets/Editor/Enemies/EmberSuctionVerification.cs` içindedir; yalnızca ayrı batch projede `-executeMethod EmberSuctionVerification.Run` ile çalışır. Üretimle birlikte `.GenerateAndRun` kullanılabilir. Sonuç test projesindeki `ember-suction-result.txt`, ayrıntılar `ember-suction-verification.log` dosyasındadır. Kalıcı kopyalar: [sonuç](verification/koz-ocagi-result.txt), [kontrol listesi](verification/koz-ocagi-checks.txt).

Kapsam: dışarıda sıfır etki; mesafeyle artan durağan tüketim; farklı başlangıç suyunda eşit puan kaybı; ısı/flash ayrımı; çekim rampası; **en güçlü bölgeden iki yönde gerçek input ile kaçış**; merkezde titreşimsizlik; çoklu collider/kaynak; kapatma, teleport, sahne boşaltma, tek ölüm/respawn temizliği; kuru/nemli normal tüketim; zıplama yüksekliğinin korunması; 30/120 hedef render FPS ve 0.01/0.04 fizik adımlarında aynı 6 puan/saniye; dünya yarıçapı/parametre doğrulaması; gerçek demo sahnesinin güvenli başlangıcı, hasarı ve R ile dönüşü.

Asıl bölümde elle yerleştirme, görsel okunabilirlik/oynanış dengesi ve dış build doğrulaması bekliyor. Genel QA kutuları bu alt sistem testiyle kapatılmaz.

Logda önceki test kayıtlarında da bulunan bağımsız `UnityEditor.Search.SearchDatabase` başlangıç indeksleme hatası vardır. C# derleme hatası veya başarısız davranış kontrolü yoktur. Test edilmiş prefab ve sahne, Unity'nin ürettiği meta/GUID'lerle ana projeye aktarılmıştır.

Demo Player'ına yalnızca sahnede ışıksız sprite materyali bağlandı; Player prefab'ı ve projenin URP ayarları değiştirilmedi. Ayrı test projesinin [960×540 kamera çıktısı](verification/koz-ocagi-demo.png) incelendi: Player, ocak ve zemin görünürdür. Bu kamera çıktısı ekran üstü Canvas/OnGUI arayüzünü içermez; ana projenin URP görüntüsü veya dış build testi değildir.
