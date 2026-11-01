# Ses dosyalarını bağlama

1 Kasım 2026, T52: Olay çağrıları ve prefab/component bağlantıları hazırlanmıştır; aşağıdaki yerlere **AudioClip sürüklemek yeterlidir**. Ember'ın atışına `sfx_throw.ogg`, WaterSource temasına kullanıcının Level_1'de seçtiği `freesound_gamestudio-find-408438.mp3` bağlandı. Diğer yeni efekt alanları boş bırakıldı. Boş alan sessiz kalır ve hata üretmez. Mevcut zıplama, hapşırma, Köz Ocağı, testere ve müzik seçimleri korunur.

Unity Project penceresinden ilgili prefab'ı açın, kök nesneyi seçin ve tabloda belirtilen component'teki alana ses dosyasını sürükleyin. Prefab'a kaydedilen seçim, o prefab'ın sahne örneklerine geçer. Bölüm geçişi ve final alanları doğrudan ilgili sahne nesnesindedir.

## Oyuncu ve su dolumu

Su kaynağına **temas sesi**: `Assets/Prefabs/WaterSource.prefab` → **AudioSource → Audio Resource**. Kullanıcının Level_1'de seçtiği kayıt prefab'ın varsayılanına da bağlandı; sahnedeki mevcut seçim korunur. **Play On Awake** ve **Loop** kapalı, **Volume** 0.8'dir. Kökteki `WaterSourceSound`, oyuncu girdiğinde bu klibi bir kez çalar. Su tamamen doluyken de temas sesi duyulur; kaynakta bekleme sesi yeniden başlatmaz, çıkıp tekrar giriş yeniden çalar. Oyuncu olmayan temaslar, boş klip, kapalı/mute AudioSource ve yeniden doğan oyuncu sessiz kalır.

Temas sesi `Sfx.PlayAt` üzerinden çalınır; böylece son kaynak bölüm değiştirirken klip yarıda kesilmez. Klip ve seviye AudioSource'tan, kameraya XY mesafesinde kısılma sınırı **Water Source Sound → Hearing Distance** alanından (12 birim) alınır. Buradaki duyulma mesafesi AudioSource'un 3D eğrisinden bağımsızdır. Animasyon veya Animation Event gerekmez.

Konum: `Assets/Prefabs/Player.prefab` → kökteki **Player Sounds**.

| Alan | Ne zaman çalar? |
|---|---|
| **Hurt** | Kabul edilen düşman/tuzağa temas veya ateş topu darbesinde |
| **Refill** | İsteğe bağlı ek oyuncu tepkisi; yalnızca eksik suyla su kaynağına girişte |
| **Death** | Su bitişi, lav veya bölümden düşmeyle ölüm başladığında |
| **Land** | En az 0.25 saniye havada kaldıktan sonraki inişte |
| **Low Water Warning** | Su ilk kez %25 ve altına indiğinde |
| **Jump** | İsteğe bağlı koddan zıplama sesi; mevcut Jump Animation Event'i kullanıldığı için boş bırakıldı |

**Volume** genel efekt seviyesi (0.8), **Land Volume** iniş seviyesi (0.4), **Min Air Time For Land** kısa adımların sessiz kalma süresidir (0.25 sn). **Low Water Threshold** 0–1 oranıdır; 0.25 = %25. Uyarı kalan su azaldığı her karede tekrarlanmaz; su eşikten 10 yüzde puanı yukarı toparlandığında yeniden hazırlanır (varsayılanla en az %35; yüksek eşiklerde tam su yeterlidir).

Player Sounds içindeki Refill sesi kaynağın üzerinde beklerken, dolu suyla girerken veya yeniden doğarken tekrarlanmaz. WaterSource AudioSource alanına dolum/mutlu olma sesi koyduysanız aynı kaydı buraya tekrar atamayın; Refill boş kalabilir. Flash dokunulmazlığının reddettiği darbeler Hurt çalmaz. Öldüren darbe yalnızca Death çalar; Hurt/Death sesleri üst üste bindirilmez. Sürekli zemin/ısı tüketimi darbe sesi üretmez.

Mevcut zıplama sesi `Assets/Animations/Jump.anim` içindeki **PlaySound** event'indedir. Orada dosya seçiliyken **Player Sounds → Jump** alanını boş tutun.

Kullanıcının `Knockback.anim` içindeki **PlaySound** event'ine bağladığı hasar sesi de korunur. Bu tek karelik klipte **Loop Time kapalıdır**; son hasar pozu flash boyunca tutulurken ses bir kez çalar. Bu yolu kullanıyorsanız **Player Sounds → Hurt** alanını boş bırakın. Hurt alanını kullanmak için klipteki aynı ses event'ini kaldırıp dosyayı bu alana taşıyın; iki yol birlikte doluysa sesler üst üste biner. [Döngü düzeltmesi ve 35 kontrol](player-state-machine.md#knockback-ve-temas-hasarı).

## Ateş eden düşman ve mermi

| Konum | Alan | Olay |
|---|---|---|
| `Assets/Prefabs/Enemies/EmberSpitter.prefab` → **Ember Spitter / Sounds** | **Windup Sound** | Gerçek atış hazırlığı başladığında bir kez |
| Aynı component | **Fire Sound** | Ateş topu oluşturulup fırlatıldığında bir kez |
| `Assets/Prefabs/Enemies/EmberFireball.prefab` → **Ember Fireball / Sounds** | **Impact Sound** | Mermi oyuncuya veya katı engele değdiğinde, silinmeden önce |

Atıcıda **Sound Volume** 0.7; mermide 0.6; **Hearing Distance** 12 birimdir. Hazırlık her fizik adımında tekrarlanmaz. Hazırlık sonrası menzil/görüş uygun değilse Fire Sound çalmaz. Merminin ömrünün bitmesi veya oyun durduğu için temizlenmesi Impact Sound üretmez.

Ember'ın `Fire Sound` alanına `Assets/platformerPack/Sounds/sfx_throw.ogg` bağlandı. Ses doğrudan mermi fırlatma kodunda tetiklenir; ayrı atış animasyonu veya Animation Event gerekmez. Değiştirmek için prefab kökündeki **Ember Spitter → Sounds → Fire Sound** alanını kullanın.

## Düşen platformlar ve sarkıt

| Konum | Alan | Olay |
|---|---|---|
| `Assets/Prefabs/Traps/FallingGround_Sand.prefab` → **Falling Ground Trap / Sounds** | **Warning Sound** | İlk geçerli üst yüzey basışında; çatlama/titreşim için |
| Aynı component | **Fall Sound** | Destek kalkıp platform düşmeye başladığında |
| `Assets/Prefabs/Traps/FallingGround_Rock.prefab` → **Falling Ground Trap / Sounds** | Aynı iki alan | Kaya platformunun aynı olayları |
| `Assets/Prefabs/Traps/Trap_stoneSpike.prefab` → **Falling Stone Trap / Sounds** | **Fall Sound** | Oyuncu algılanıp sarkıt bırakıldığında |
| Aynı component | **Hit Sound** | İlk oyuncu temasında; taş darbesi için |

Her component'te **Sound Volume** 0.6 ve **Hearing Distance** 12'dir. Bu alanlara kısa düşme/hava/çatlama efektleri seçin. Sesler olay başlangıcında bir kez çalar; düşüş sürdükçe yeniden başlatılmaz.

1 Kasım 2026 kullanıcı seçimi: `Assets/Audio/Sounds/537868__xantherock__fall-and-impact.wav`, sarkıtın **Fall Sound** ve hem kum hem kaya platformunun **Warning Sound** alanına bağlandı. Sarkıt düşmeye başladığında, platform ise ilk geçerli basışla çatlama uyarısına girdiğinde çalar. Klip yaklaşık 0.568 saniye, mono/48 kHz'dir. Platformun ayrıca Fall Sound alanı boş bırakıldı; aynı kaydı 0.25 saniye sonra ikinci kez başlatmaz. Prefab GUID/alan bağlantıları ve kaydın okunabildiği kontrol edildi; bu dosyayla Unity'de dinleme/build testi bekler.

Bu tuzaklar mevcut tasarımda zeminin içinden geçer; platformun destek collider'ı düşüşte kapanır. Bu nedenle olmayan bir zemin çarpışmasına kırılma/çarpma sesi bağlanmadı. Sarkıtın Hit Sound'u oyuncuyla temasa aittir. Ses hazırlığı fizik veya hasar kurallarını değiştirmez.

## İsteğe bağlı ortam ve duman sesleri

| Konum | Dosyanın atanacağı alan | Önerilen kullanım |
|---|---|---|
| `Assets/Prefabs/Enemies/SlimeFire_Stationary.prefab` → **AudioSource** | **Audio Resource** | Hafif ateş çıtırtısı |
| `Assets/Prefabs/Enemies/SlimeFire_Patrol.prefab` → **AudioSource** | **Audio Resource** | Aynı ateş ambiyansı |
| `Assets/Prefabs/Enemies/HapsirikKoz_Smoke.prefab` → **Sound On Enable** | **Clip** | Duman oluşma/puf sesi |

İki Slime ortam kaynağında **Play On Awake** ve **Loop** açık, ses 0.2, **Spatial Blend 1 (3D)**, **Linear Rolloff**, **Min Distance 5.4 / Max Distance 13**, **Doppler 0** hazırdır. Dosya boşken çalmazlar. WaterSource'un önceki ortam kaynağı kullanıcının isteğiyle yukarıdaki temas sesine çevrildi. Dumanın tek seferlik sesi 0.4 seviyesinde ve 12 birim duyma mesafesiyle hazırlanmıştır. Duman alanına ikinci bir hapşırma sesi atamak, mevcut hapşırık event'iyle iki ses çalmasına yol açar; burası ayrı bir puf efekti içindir.

Mevcut Saw rüzgâr kaydı ve Köz Ocağı nefesi değiştirilmedi. Saw'ın uzun kaydı için **Loop kapalı** kalır. Kamera Z = −5 ve ses kaynağı Z = 0 iken 5.4–13 3D mesafe, kameraya XY düzleminde yaklaşık 2–12 birime karşılık gelir.

## Bölüm sonu ve final

| Konum | Alan | Olay |
|---|---|---|
| `Level_1` ve `Level_2` sahnelerindeki çıkış su kaynağı → **Level Exit** | **Exit Sound** | Geçerli bölüm geçişi başladığında |
| Aynı component | **Exit Sound Volume** | Geçiş efekti seviyesi (0.8) |
| `Level_3` → `Fidan` → **Final Sapling / Sound Effects** | **Absorb Sound** | Damla fidana geçerli temas yaptığında |
| Aynı component | **Bloom Sound** | Damla emildikten sonra fidan yeşerme aşamasına girerken |
| Aynı component | **Victory Sound** | Final sekansı ve bekleme tamamlandığında |
| Aynı component | **Restart Sound** | Mevcut `RestartGame()` çağrısı yeniden başlatma geçişini kabul ettiğinde |

Final efektleri **Sound Volume** (0.8) kullanır. **Ending Music** mevcut ayrı müzik alanıdır ve korundu. Çıkış sesini ortak WaterSource prefab'ına değil, sahnedeki **Level Exit** component'ine atayın; yalnızca bölümün son birikintisinde bulunur.

## Çalışma ve kontrol

Ember atış/WaterSource temas düzeltmesi, 1 Kasım 2026: aynı ayrı Unity 6000.5.0f1 projesinde **26 asset/Play Mode kontrolü geçti**. Gerçek seçili seslerle Ember atışı ve su kaynağı teması; tam suyla giriş, yeniden giriş, beklerken tekrar etmeme, tek su dolumu/checkpoint, boş/mute/kapalı alanlar, oyuncu olmayan temas ve iptal edilen atış doğrulandı. LevelExit ile gerçek bir test sahnesine geçildi; yalnızca bu uzunluk kontrolünde dört saniyelik sentetik klip kullanılarak eski kaynak silindikten sonra sesin sürdüğü ölçüldü. LevelExit'in Player'ı önce kapatması da ayrıca sınandı. Güncel script ve prefab'lar test kopyalarıyla hash üzerinden eşleşti. [26 kontrolün sonucu](verification/water-ember-sound-result.txt), [doğrulama kodu](verification/water-ember-sound-verification.cs); ayrı projede giriş `-executeMethod WaterEmberSoundVerification.Run`. Bilinen Search indeksleme hatası ayrı kaydedildi; oyun/ses hatası yoktu. Dinleyici susturularak otomatik kontrol yapıldı; ana bölümde işitsel seviye kontrolü ve dış build testi bekler.

Tek seferlik efektler mevcut `Sfx` kaynağını kullanır. Sahne değişimi veya nesnenin silinmesi efektin sonunu kesmez. Oyuncu/final efektleri 2D; atış, mermi, tuzak ve duman efektleri kameraya XY mesafesinde kısılır. Ortam kaynakları yerleşik 3D AudioSource kullanır. Ek AudioSource veya Animation Event eklemek gerekmez.

Atamaları Play Mode dışında kaydedin. Ses dosyası eklendikten sonra ilgili olayı oyunda tetikleyip ses seviyesi, kayıt başlangıcındaki sessizlik ve seçilen klibin uzunluğunu dinleyerek ayarlayın. Yeni dosyaların kaynak/lisans bilgilerini T53 için kaydedin.

İlk boş-alan hazırlık doğrulamasında açık ana editörden ayrı `GameUp Hackathon/Temp/SoundSlotsTask/Verification` projesinde **Unity 6000.5.0f1 ile 67 kontrol geçti: 24 asset/bağlantı, 43 Play Mode**. O aşamadaki 9 runtime dosyasının hash'leri test edilen kopyayla eşleşti. Gerçek Player, kaynak, atıcı/mermi ve tuzak prefab'ları; testte üretilmiş geçici bir AudioClip ve son fidan bileşeni kullanıldı. Gerçek kaynak temasında dolum, gövde/mermi hasarı, flash reddi, öldüren darbe, pozitif suyla ölüm, iniş, düşük su uyarısı ve en yüksek eşikte yeniden dolum, hazırlık/atış, mermi çarpması, tuzakların tek seferlik sesleri, final emilme/yeşerme/kazanma ve boş alanların sessizliği doğrulandı. Test sesi dinleyici seviyesinden susturuldu; bu ilk aşamada kullanıcı için efekt dosyası seçilmedi.

[Sonuç ve geçen kontroller](verification/sound-slots-result.txt), [bu boş-alan hazırlık aşamasında kullanılan doğrulama kodu](verification/sound-slots-verification.cs). Ayrı proje girişi: `-executeMethod SoundSlotsVerification.Run`. Çalıştırma ana projeye uygulanmaz. Yeni efekt alanları boş olan bu hazırlık kopyası, seçilmiş ses dosyalarıyla yapılan işitsel QA yerine geçmez.

Logdaki `UnityEditor.Search.SearchDatabase` başlangıç indeksleme `ArgumentOutOfRangeException` hatası raporda `EDITOR_INDEXER_ISSUE` olarak ayrıca saklandı; oyun kaynaklı hata veya C# derleme hatası yoktu. Asıl bölümlerde gerçek dosyalar atandıktan sonraki dinleme/seviye kontrolü ve dış build testi bekler; genel T52/QA kutuları kapatılmadı.
