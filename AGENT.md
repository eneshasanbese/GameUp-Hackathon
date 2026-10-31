# GameUp Hackathon — Ortak agent yönergeleri

Bu depo, son yaşayan fidanı yeşertmeye çalışan bir su damlasının Unity 2D platform oyunudur. Takım iki kişiliktir; öncelik baştan sona oynanabilir, kısa ve teslim edilebilir bir sürümdür. Kullanıcıyla Türkçe iletişim kur; kodda mevcut İngilizce adlandırmayı sürdür.

## Başlarken ve kaynakları seçerken

1. [Genel yapılacaklar](genel-yapilacaklar.md) belgesindeki ilgili T/QA/D maddelerini ve karar kapılarını oku. Kullanıcının istediği iş paketini belirt; belgeyi hazırlamak veya incelemek oyun mekaniğini uygulama yetkisi sayılmaz.
2. `git status --short` ve ilgili diff ile mevcut çalışmayı belirle. Kullanıcının veya takım arkadaşının değişikliklerini koru.
3. [Günlük takip](yapilacaklar-takip.md), ilgili `docs/` belgesi ve gerçek kaynak dosyalarını karşılaştır. Yalnızca görevin gerektirdiği skill'i oku.
4. Küçük, tamamlanabilir bir iş üzerinde ilerle. P0 tamamlanmadan P2 içerik ekleme; çalışan sistemi sırf önerilen sınıf/dizin adlarına uydurmak için yeniden kurma.

**Karar ile uygulama farklı kaynaklardır:** Son açık kullanıcı kararı geçerlidir; ardından genel belgedeki onaylı tasarım gelir. Diskteki kod ve sahne, mevcut uygulamanın kanıtıdır; bir özelliğin bulunması tasarım onayı veya başarılı test anlamına gelmez. Takip dosyası eski kalabilir. `docs/` içindeki önceki test sonuçlarını geçmiş kayıt olarak aktar; bu oturumda yeniden koşulmuş gibi sunma. Çelişkiyi görev kapsamında açıkla, sessizce yeni bir oyun kuralına dönüştürme.

## Depo ve Unity proje kökü

Komut örnekleri depo kökünden çalışır. Unity Hub'da açılacak klasör **`GameUp Hackathon/`** klasörüdür; depo kökü değildir. Aşağıdaki Unity bilgileri 31 Ekim 2026 tarihinde diskten okunmuştur; değişiklikten önce ilgili dosyaları tekrar kontrol et.

| Konum | Kullanım |
|---|---|
| `GameUp Hackathon/ProjectSettings/ProjectVersion.txt` | Unity `6000.5.0f1`; kendiliğinden sürüm yükseltme |
| `GameUp Hackathon/Packages/manifest.json` | URP `17.5.0`, Input System `1.19.0`, Test Framework `1.7.0`; gerçek bağımlılık listesi |
| `GameUp Hackathon/Assets/Scripts/Player/` | Input, fizik, state machine ve ability component'leri |
| `GameUp Hackathon/Assets/Scripts/Water/WaterMeter.cs` | Tek su değeri, tüketim, görsel, iz, dolum ve yeniden doğma |
| `GameUp Hackathon/Assets/Scenes/Components.unity` | Mevcut oyuncu ve su prototipinin test sahnesi |
| `GameUp Hackathon/Assets/Scenes/SampleScene.unity` | Ayrı sahne; mevcut build listesinde etkin olan sahne |
| `GameUp Hackathon/Assets/PlayerInput.inputactions` | Oyuncunun `Player/Move` ve `Player/Jump` action'ları |
| `GameUp Hackathon/Assets/Settings/` | URP 2D renderer ve render ayarları |
| `GameUp Hackathon/Assets/platformerPack/` | İçe aktarılmış paket, tile'lar ve paletler |
| `docs/player-state-machine.md`, `docs/water-player.md` | Mevcut component ve Inspector bağlantılarının açıklaması |

Build alırken `ProjectSettings/EditorBuildSettings.asset` ve varsa seçili Build Profile içindeki sahneleri doğrula. `Components.unity` üzerinde test yapmak, dışa aktarılan oyunun o sahneyi açtığını kanıtlamaz. Cinemachine manifestte şu anda yoktur; takip belgesindeki kurulum maddesi kurulu paket kanıtı değildir.

## Korunacak oyun kuralları

- Slime Fire ısısı, HapsirikKoz'un geçici dumanı ve kabul edilen düşman gövde darbeleri dışında yerde gerçekten hareketsizken su kaybı yoktur. 31 Ekim 2026 kullanıcı kararlarıyla etkin ısı/duman çarpanı (>1) içinde durmak da su tüketir; hareket şartı aranmaz. İki alan aynı çarpan sistemini kullanır. Tüketim yüzeye bağlıdır; nemli zemin normal/kuru zeminden az tüketir. Gövde darbesi ise Inspector'dan ayarlı tek `WaterMeter.Consume` işlemiyle su düşürür; flash yeni gövde darbelerini engeller.
- Zıplama ve düşüş, yatay input olmasa da normal tüketim üretir. Önceki nemli zemin katsayısı havaya taşınmaz. Tüketim geçen süreye bağlıdır.
- Su tek bir bileşende tutulur ve sınırlandırılır. UI ve görsel başka bir su sayacı oluşturmaz. Su sıfırken damla kazanamaz.
- Ara duraklar su birikintileridir. Yaşayan ara fidan/ağaç checkpoint'i ekleme; yaşayan tek fidan finaldedir.
- 31 Ekim 2026 kullanıcı kararlarıyla su azalınca görsel ve `CircleCollider2D` birlikte küçülür; yürüme/hava hızı ve zıplama kalkış hızı Inspector çarpanlarıyla artar. Kök ölçek değişmez. Esneme yalnızca `Visual` üzerindedir; fiziksel boyutu `PhysicsControl`, suya bağlı boyut isteğini `WaterMeter`, hareket çarpanlarını mevcut ability'ler yönetir. Önceki sabit collider/hız/zıplama yönergeleri bu kararlar için geçersizdir; [karar kaydını](docs/kararlar.md) esas al. Zıplama artışı yalnızca kalkışta uygulanır.
- Su izi geçici görseldir; yeni zemin, bitki, geri toplanabilir su veya yeni kaynak oluşturmaz.
- Ölüm/yeniden doğma ve kazanma tek kez başlatılır. Kazanınca kontrol ve tüketim durur. Yeniden oyna, son checkpoint yerine bölüm başlangıcını ve tüm oyun durumunu sıfırlar.
- Düşman, silah, ek bölüm ve diğer kapsam dışı sistemleri göreve ekleme. Suya bağlı collider küçülmesi son kullanıcı kararıyla kapsam içindedir.

## Mevcut kodla çalışma

- `Player`, aynı GameObject üzerindeki `BaseAbility` component'lerini toplar ve `StateMachine` üzerinden çalıştırır. Mevcut durumlar `Idle`, `Move`, `Jump`, `Knockback` olup `Jump` düşüşü de kapsar. Knockback sırasında zıplama darbeyi kesmez; flash yeni gövde darbelerini engellerken ısı/duman tüketimi sürer. Player kapatılırken ability'lerin `OnPlayerDisabled` temizliği çalışır. [Oyuncu belgesini](docs/player-state-machine.md) oku.
- `GatherInput`, InputActionAsset'in oyuncuya ait kopyasını yönetir; kısa zıplama basma/bırakma olaylarını fizik adımları arasında saklar. İkinci input sistemi ekleme.
- `PhysicsControl`, `Ground-Dry` ve `Ground-Wet` desteğini gerçek collider temasları ve aşağı şekil taramasıyla algılar; `Rigidbody2D` ve `CircleCollider2D` ister. Tarama güncel collider boyutunu izler, duvar normalini zemin saymaz. Eğimde hareket yüzey teğetini izler; gerçek zıplamada zemine yapışma uygulanmaz. Takip dosyasındaki Capsule önerisi nedeniyle collider türünü otomatik değiştirme.
- `WaterMeter` bugün birkaç sorumluluğu bir arada taşır. Genel belgedeki `WaterSource`, `RespawnController`, `PlayerSizeVisual`, `GameFlow` gibi isimler öneridir; bu sınıfların mevcut olduğunu varsayma veya yalnızca isimleri tamamlamak için sistemi bölme. [Su belgesini](docs/water-player.md) oku.
- Fizik işlemlerini mevcut `FixedUpdate` düzeninde tut. Unity 6 kodundaki `linearVelocity` kullanımını eski API örneklerine göre değiştirme. Input → zemin algılama → zıplama → aktif ability fiziği → su tüketimi sırasını etkileyen değişiklikleri davranışla doğrula.
- Yerel C# düzenini sürdür: dosya adıyla eşleşen component sınıfı, mevcut global namespace, Inspector ayarları için `[SerializeField]`, mevcut stil ve API adları. Sırf stil için toplu yeniden adlandırma/formatlama yapma. Serialized alan adı değişirse Inspector verilerinin korunmasını sağla.

## Unity dosyaları ve birlikte çalışma

- Yeni oyun kodunu mevcut `Assets/Scripts/Player` ve `Assets/Scripts/Water` düzenine ekle; başka sorumluluk varsa uygun dar bir dizin seç. Sırf öneri olduğu için `_Game/` yapısına toplu taşıma yapma.
- Asset ile `.meta` dosyasını birlikte koru; mevcut GUID'leri değiştirme. Taşıma/yeniden adlandırmada Unity Editor'ü tercih et. Yeni asset'lerin `.meta` üretimini ve sürüm kontrolüne dahil edildiğini doğrula; rastgele GUID ile eksik referans gizleme.
- Sahne/prefab üzerinde kapsamı dar tut, fileID/GUID ve serialized referansları koru. Metin düzenlemesi gerekirse ilgili nesne bloğunu anlayarak değiştir ve Unity'de bağlantıları doğrula; sahneyi baştan üretme.
- Ana sahne veya ortak prefab'a yazmadan mevcut sahipliği ve açık değişiklikleri kontrol et. Başka kişinin sahip olduğu ortak dosyada koordinasyon gerekiyorsa önce değişiklik kapsamını bildir; bağımsız kod/test işini ilerlet. Sahiplik atanmadıysa atanmış gibi yazma.
- `GameUp Hackathon/.gitignore` kapsamındaki `Library`, `Temp`, `Obj`, `Logs`, `UserSettings`, IDE/proje üretimleri ve build çıktıları kaynak dosyası değildir. Normal aramalarda bu klasörleri dışarıda bırak; `Library/PackageCache` yalnızca kurulu bir API'yi araştırmak gerektiğinde okunabilir.
- Yeni paket, Unity yükseltmesi veya render/input dönüşümü gerekiyorsa önce gerekçeyi somutlaştır. Kullanıcı zaten yetkilendirmediyse ilgili değişiklik için onay al; aynı onayı tekrar isteme.

## Açık kararlar

Genel belgedeki K01–K05 kapılarını koru. Görsel paket, nihai tarz/küçülme, bölüm miktarı ve teslim platformunu dosya varlığından çıkarsama. `docs/kararlar.md` henüz yoksa varmış gibi alıntılama; karar gerçekten alındığında tarih ve dayanakla oluştur/güncelle.

Mevcut paketin `Double` görselleri için takip belgesi PPU 128 ve Bilinear bildirir. Ana belgedeki 18 PPU/Point ayarları başka bir piksel paketine koşulludur; mevcut importları topluca dönüştürme. Benzer şekilde kodda görsel küçülmenin bulunması K03'ün kesinleştiğini göstermez. Onaysız karara bağlı uygulama yerine etkilenmeyen prototip işlerini ilerlet.

## Doğrulama ve görev kapanışı

Değişikliğe uygun kontrolü yap: yalnızca belge değiştiyse bağlantı/yol ve tutarlılık kontrolü; oyun davranışı değiştiyse Unity derleme ve ilgili Play Mode/oynanış senaryoları; teslim işi ise dışa aktarılan build. Bu depoda kalıcı bir test paketi veya build otomasyon komutu varmış gibi davranma. `dotnet build`, Unity sahne ve oynanış doğrulamasının yerine geçmez.

T/QA/D kutularını yalnızca ilgili kabul ölçütü kanıtlandığında işaretle. Genel planı, günlük takibi ve teknik belgeyi yalnızca değişen davranış kadar güncelle. Unity testi yapılamadıysa **“uygulandı; editör/build testi bekliyor”** yaz; önceki bir test sayısını bu oturuma mal etme.

Kapanışta şu bilgileri ver: ilgili görev kimliği/kapsamı, değişen dosyalar ve davranış, gereken Inspector/sahne bağlantıları, gerçekten yapılan test ve sonucu, kalan risk veya bekleyen karar. Commit/push/yükleme durumunu yalnızca gerçekleştiyse bildir; bu dosyalar dış yayın için kendi başına yetki vermez.

## Projeye özel skills

Asıl talimatlar `.agents/skills/` altındadır. Claude Code için `.claude/skills/` içinde aynı talimatlara yönlendiren girişler vardır; davranış değişikliğini asıl skill'de tut, adı/açıklaması değişirse iki girişin metadata'sını birlikte güncelle. Görevle ilgili skill'i seç; hepsini her oturumda yükleme.

| Skill | Ne zaman? |
|---|---|
| [gameup-player-controller](.agents/skills/gameup-player-controller/SKILL.md) | Input, hareket, zıplama, zemin ve ability/state machine işleri; T08–T11 |
| [gameup-water-loop](.agents/skills/gameup-water-loop/SKILL.md) | Su, UI bağlantısı, kaynak, yeniden doğma, kazanma ve yeniden oynama; T12–T29 |
| [gameup-level-art](.agents/skills/gameup-level-art/SKILL.md) | Bölüm dengesi, tilemap, kamera, görsel import, küçülme ve iz; T10, T30–T48 |
| [gameup-verify-delivery](.agents/skills/gameup-verify-delivery/SKILL.md) | Unity regresyonu, gerçek build, lisans/teslim hazırlığı; T01, T07, T29, T53, QA ve D maddeleri |

Codex'te örnek: `$gameup-water-loop T19 su göstergesini mevcut WaterMeter'a bağla.` Claude Code'da karşılığı: `/gameup-water-loop T19 su göstergesini mevcut WaterMeter'a bağla.`
