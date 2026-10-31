# Su Damlası Oyunu — Genel Yapılacaklar

31 Ekim 2026 ek kapsamı: kullanıcının açık talebiyle Köz Ocağı sabit çevresel düşmanı, yalnızca yatay çekim ve normal tüketimden bağımsız puan/saniye su kaybıyla kapsama alındı (T09/T14–T16, ilgili QA01–QA09/QA16). Gövde temas hasarı/knockback eklenmez. Mevcut suya bağlı boyut/hız/zıplama kullanıcının netleştirmesiyle korunur. [Karar ve uygulama](docs/koz-ocagi.md).

**Sürüm:** 1.0 — 30 Ekim 2026, 23.41 itibarıyla verilen yanıtlara göre.
**Okuyucular:** İki kişilik geliştirme takımı ve AI agent’lar.
**Amaç:** Son fidanı yeşertmek için ilerleyen, hareket ettikçe suyunu kaybeden bir damlayı konu alan kısa bir Unity 2D platform oyununun tamamlanması.
**Mevcut durum:** Yeni oyuna henüz başlanmadı; elde eğitim takip edilerek hazırlanmış bir Unity kurs projesi var. Bu proje incelenip çalıştırılmış kabul edilmez.
**Çıktı:** Bu dosya bir görev listesidir; tamamlanmış iş raporu değildir. İşaretlenmemiş maddeler yapılmış sayılmaz.

## Belgeyi kullanma kuralları

- **Onaylı:** Kullanıcının açıkça belirlediği veya kabul ettiği karar. Uygulama buna uymalıdır.
- **Teknik öneri:** Aynı hedefe ulaşmak için önerilen yöntem; mevcut kod uygunsa gereksiz yeniden yazım yapılmaz.
- **Karar kapısı:** Kullanıcı onayı veya organizatör bilgisi gerektiren konu. İlgili özellik, cevap alınmadan kesinleştirilmez.
- **İsteğe bağlı:** Temel sürüm bitmeden başlanmayacak ek fikir.
- **P0:** Oynanabilir temel sürüm ve teslim için öncelikli görev.
- **P1:** Temel sürüm çalıştıktan sonra okunabilirlik ve sunum iyileştirmesi.
- **P2:** Yalnızca zaman kalırsa ve ayrıca onaylanırsa ek içerik.

Önceki `oyun-dizayni.md` ve yüklenen `assetler-ve-damla-karakteri.md`, öneriler ve başvuru malzemesidir. Özellikle asset dosyasında Kenney paketleri, dört boy sprite ve bitki checkpoint’leri kesin karar olarak yazılmıştır; sonraki kullanıcı yanıtları nedeniyle bunların tümü artık kesin şart değildir. Çelişki olduğunda bu belgedeki onaylı kararlar esas alınır. [cite:17]

## Onaylanan oyun tasarımı

| Konu | Geçerli karar |
|---|---|
| Hikâye | Dünyada kalan son fidanı yeşertmek için yolculuk eden su damlası |
| Tür | Platform odaklı oyun; ateş etme/dövüş sistemi yok |
| Ana kaynak | Hareketle tüketilen su; oyuncunun hayatta kalmasını ve küçülme hissini temsil eder |
| Durma | Yerde dururken su kaybı yok |
| Havada olma | Normal su kaybı; yatay giriş verilmese de zıplama/düşme sırasında geçerli |
| Zeminde hareket | Tüketim, zemin türüne göre değişir |
| Ara duraklar | Su birikintileri; yaşayan ara fidan/ağaç checkpoint’leri yok |
| Final | Yaşayan tek fidan yolculuğun sonunda |
| İz | İlerlerken geride su izi bırakma fikri korunur |
| Boyut — 31 Ekim güncellemesi | Su azaldıkça görsel ve collider birlikte küçülür; [K03 kararı](docs/kararlar.md) önceki sabit collider kararının yerini alır |
| Hareket fiziği — 31 Ekim güncellemesi | Kök ölçek sabit; su azaldıkça yürüme/hava hızı ve zıplama kalkış hızı ayrı Inspector çarpanlarıyla artar ([karar](docs/kararlar.md)) |
| Nihai görseller | Görsel tarz hâlâ ayrı karardır; mevcut görsel ve collider için sürekli küçülme kapsam içindedir |
| Çalışma çıktısı | Takım ve AI agent’lar için indirilebilir Markdown görev listesi |

**Önemli ayrım:** “İlk prototipte tek sprite” kararı, teslim oyununda küçülmenin tamamen kaldırıldığı anlamına gelmez. İlk aşamada su kaybını göstergeyle test etmek mümkündür; karakterin son görsel küçülme yöntemi görsel karar kapısından sonra uygulanır.

## Kapsam ve karar kapıları

### Önerilen en küçük teslim sürümü

Aşağıdaki içerik miktarları kapsam önerisidir; konuşmada kararlaştırılmış zorunlu sayılar değildir.

- Tek kısa bölüm ve tek oyuncu karakteri.
- Kuru/normal ve nemli olmak üzere iki okunabilir zemin türü.
- İlk aşamada bir su birikintisi; bölüm gerektiriyorsa ikinci birikinti.
- Bir final fidanı, su göstergesi, su izi ve anlaşılır yeniden deneme akışı.
- Başlangıç kontrol açıklaması, kazanma bildirimi ve yeniden başlatma.
- İlk kez oynayan biri için birkaç dakikalık tamamlanabilir deneyim; kesin süre testle belirlenecek.

### Karar gerektiren konular

Bu maddeler görev listesini hazırlamayı değil, ilgili uygulamayı sınırlar. Cevabı olmayan bölüm otomatik doldurulmaz; etkilenmeyen prototip işleri ilerleyebilir.

| Kapı | Ne netleşmeli? | Ne zaman? | Yanıt yoksa |
|---|---|---|---|
| K01 — Etkinlik | Resmî tema, son teslim saati/saat dilimi, platform, hazır kod/asset ve AI kuralları | Proje kurulurken | Uygunluk onayı veya teslim formatı iddia edilmez |
| K02 — Görsel tarz | Kenney piksel ailesi mi, başka bir tarz mı? | Gri kutu prototip çalıştıktan sonra | Geçici görsellerle devam edilir |
| K03 — Küçülme | 31 Ekim kullanıcı kararı: mevcut görsel ve collider birlikte küçülür; [karar kaydı](docs/kararlar.md) | Karar alındı; boyut ve eğim birlikte test edilir | Dört boy/30 kare üretimi zorunlu değildir |
| K04 — Teslim içeriği | Bölüm uzunluğu, su birikintisi sayısı ve finalde tek/çok kademeli sonuç | Test bölümünün ilk denemesinden sonra | Ek bölüm ve çok kademeli son eklenmez |
| K05 — Kurulum | Unity sürümü, mevcut render hattı ve input yöntemi | Mevcut proje kontrolünde | Sürüm yükseltme veya render hattı dönüşümü yapılmaz |

K01 organizatörden, K02–K04 oyun sahibinden alınır. K05 önce projedeki gerçek durum okunarak belirlenir; değişiklik gerekiyorsa ayrıca onay istenir. Teknik dosya varlığı uzaktan tahmin edilmez.

## P0 — Başlangıç ve iş bölümü

### Projeyi güvenle kur

- [ ] **T01 — Kuralları topla.** K01 cevaplarını `docs/kararlar.md` içine tarih ve kaynak bağlantısıyla yaz. Doğrulanmayan bilgiyi “belirsiz” olarak tut.
- [ ] **T02 — Kurs projesini kontrol et.** Unity sürümü, render hattı, input yöntemi, hareket kodu ve bağımlılıkları incele. Gerçekte çalışan parçaları kaydet.
- [ ] **T03 — Temiz başlangıç seç.** Kullanım kuralları izin veriyorsa yalnızca anlaşılan ve gerekli kurs parçalarını taşı; tüm metroidvania sistemlerini otomatik aktarma. İzin belirsizse izin gerektiren kodu teslim sürümüne dahil etme.
- [ ] **T04 — Ortak kurulumu doğrula.** İki kişi aynı Unity sürümüyle projeyi açsın. Gereksiz paket veya yeni render hattı ekleme.
- [ ] **T05 — Git deposunu hazırla.** Unity `.gitignore` kullan; `Assets`, `Packages`, `ProjectSettings` ve ilgili `.meta` dosyalarını takip et. Üretilen `Library`, `Temp`, `Obj` ve `Logs` klasörlerini depoya ekleme. Unity, asset ile `.meta` dosyasının birlikte sürümlenmesini ve `Library` önbelleğinin dışarıda tutulmasını açıklıyor. [cite:18]
- [ ] **T06 — Dosya sahipliğini belirle.** Ana sahneyi bir kişi birleştirsin. Diğer kişi test sahnesi, ayrı prefab veya kendisine ayrılan dosyalarda çalışsın. Ortak dosya değişiminden önce haber verilsin.
- [ ] **T07 — Boş/test build al.** Kuralların istediği platform öğrenilir öğrenilmez dışa aktarma zincirini dene. Test build’i başka bilgisayarda aç.

**Çıkış ölçütü:** Her iki bilgisayarda proje açılır, çalışma dosyalarının sahipliği bellidir, ilk çıktı alınabilir.

### Önerilen görev paylaşımı

Bu dağılım kesin kişi ataması değil; takımın onaylayacağı başlangıç önerisidir.

| İş paketi | Önerilen sorumlu | Birleştirme koşulu |
|---|---|---|
| Hareket, su mantığı, yeniden doğma | Kullanıcı | Test sahnesinde çalışan küçük parçalar |
| Bölüm yerleşimi ve zemin prefab’ları | Takım arkadaşı | Önce birlikte bir örnek platform hazırlanır |
| Su göstergesi ve kontrol açıklaması | Takım arkadaşı, gerektiğinde destekle | Su sisteminin olay/aralık sözleşmesi bellidir |
| Ana sahne, entegrasyon ve build | Kullanıcı | Tek sahne sorumlusu |
| Oynanış testi, lisans listesi ve teslim görselleri | İki kişi | Kontrol listesi ortak tutulur |
| AI ile kod üretimi | O görevden sorumlu kişi | Küçük görev, değişen dosya listesi ve test raporu |

## P0 — Gri kutu prototip

Gri kutu prototip, nihai resim beklemeden basit şekillerle oyunun tamamlanabilen ilk hâlidir.

### Hareket ve kamera

- [ ] **T08 — Oyuncu prefab’ını kur.** Fizik bileşenlerini kök objede, `SpriteRenderer` bileşenini `Visual` alt objesinde tut. İlk prototipte çarpışma kutusu sabit olsun.
- [ ] **T09 — Yürüme ve zıplamayı çalıştır.** Sağ-sol, yere basma kontrolü, zıplama ve düşme test edilsin. Mevcut çalışan kodu sırf farklı mimari uğruna yeniden yazma.
- [ ] **T10 — Kamerayı ayarla.** İlerideki iniş platformu görülsün; karakter ekran dışında kaybolmasın. İlk testte sabit kamera da yeterli olabilir.
- [ ] **T11 — Yere düşme sınırı koy.** Boşluğa düşen oyuncu sonsuza kadar düşmesin; yeniden deneme akışına girsin.

**Çıkış ölçütü:** Karakter düz zeminde, platform kenarında ve zıplama sırasında kontrol edilebilir; görsel değiştirmek fiziği bozmaz.

### Su sistemi

- [ ] **T12 — Tek su kaynağı oluştur.** Su değerini tek bileşen yönetsin. UI ve görseller bu değeri okusun; farklı script’lerde birbirinden bağımsız su değerleri tutulmasın.
- [ ] **T13 — Su aralığını sınırla.** Önerilen normalizasyon 0–100’dür; değer negatife veya üst sınırın üzerine çıkmasın.
- [ ] **T14 — Durum önceliğini uygula.** Oyun aktif değilse tüketim yok; kaynak işlemi sırasında su doldurulur; havada normal tüketim; ısı/duman dışında yerde dururken sıfır; yerde hareket ederken zemin katsayılı tüketim. Slime Fire'ın etkin ısı alanında veya HapsirikKoz'un geçici dumanında hareket şartı olmadan zemin tüketimi ısı çarpanıyla uygulanır.
- [ ] **T15 — Kuru/normal ve nemli zemini ayırt et.** Bilinmeyen zeminin nemli kabul edilip bedava yol oluşturmaması için açık bir varsayılan tanımla. Teknik öneri: bilinmeyen zeminde normal tüketim.
- [ ] **T16 — Tüketimi kare hızından bağımsız yap.** Tüketimi “her karede sabit miktar” yerine geçen süre üzerinden hesapla. Bu prototipte önerilen model, aktif hareket veya Slime Fire/HapsirikKoz ısı-duman etkisi süresine bağlı tüketimdir; kat edilen mesafeye dayalı ayrı bir model ekleme.
- [ ] **T17 — Hareket algısını test et.** Düz zemindeki fizik titreşimi durmayı hareket saymasın; duvara doğru basılı tutmakla gerçekten ilerlemek arasındaki davranışı kontrol et. Teknik öneri: yerde gerçek yatay ilerleme ve küçük bir tolerans kullan.
- [ ] **T18 — Havada tüketimi doğrula.** Dikey zıplama, yatay giriş bırakma ve serbest düşüş normal tüketim üretmeli. Havada nemli zeminin önceki katsayısı taşınmamalı.
- [ ] **T19 — Su göstergesini bağla.** Su azalması ve dolması anında görünür olsun; prototip bu göstergeyle oynanabilsin.
- [ ] **T20 — Ayarları erişilebilir tut.** Normal tüketim, nemli zemin katsayısı ve su kapasitesi Inspector’dan değiştirilebilsin. Yalnızca bunun için ScriptableObject altyapısı kurmak zorunlu değil.

### Su durum tablosu

| Durum | Beklenen davranış |
|---|---|
| Yerde, hareketsiz ve ısı dışında | Su değişmez |
| Slime Fire ısısı / HapsirikKoz dumanında, yerde dururken veya hareket ederken | Zemin tüketimi × etkin ısı çarpanı |
| Normal/kuru zeminde ilerleme | Normal tüketim |
| Nemli zeminde ilerleme | Normalden düşük tüketim; kesin oran testle belirlenir |
| Zıplama veya düşme | Normal tüketim |
| Kaynağa ulaşma | Su doldurma işlemi |
| Yeniden doğma | Önerilen prototip davranışı: dolu su ve sıfırlanmış hız |
| Kazanma/ölüm geçişi | Ek tüketim yok |

Nemli zeminin tamamen bedelsiz mi yoksa az tüketimli mi olduğu bir denge ayarıdır. İlk denemede düşük ama pozitif tüketim önerilir; oranı test etmeden kesin değer diye belgeleme. Sıcak bölge ilk sürüm için zorunlu üçüncü zemin değildir.

### Kaynak ve yeniden deneme

- [ ] **T21 — Su birikintisi prefab’ı oluştur.** Görünür bir havuz ve etkileşim alanı yeterli. Kaynak gerçek su simülasyonu gerektirmez.
- [ ] **T22 — Doldurmayı bağla.** Prototip için öneri: birikintiye girince suyu tam doldur; ayrı etkileşim tuşu ve bekleme sayacı ekleme.
- [ ] **T23 — Checkpoint bağla.** Önerilen uygulama: ulaşılan birikinti güvenli yeniden doğma noktası olsun. İlk birikintiden önceki dönüş noktası bölüm başlangıcı olsun.
- [ ] **T24 — Kaynağı suistimal ve kilitlenme açısından dene.** Birikinti tehlikeli yere doğurmasın; sürekli giriş olayı birden fazla ölüm/kazanma üretmesin. Yeniden doğma konumu çarpışma yüzeylerinin dışında olsun.
- [ ] **T25 — Su bitince tek seferlik ölüm başlat.** Birden çok event aynı anda yeniden doğma işlemi başlatmasın.
- [ ] **T26 — Yeniden doğmayı tamamla.** Konum, hız, su, hareket izni ve gerekli görsel/UI durumları sıfırlansın. Ölüm animasyonu hazır değilse animasyon beklemeden çalışsın.
- [ ] **T27 — Final fidanını bağla.** Pozitif suyla fidana varınca kazanma bildirimi göster; tüketimi ve oyuncu kontrolünü kapat. Kaynak gibi davranıp suyu doldurmasın.
- [ ] **T28 — Tekrar oynamayı ekle.** Tekrar başlatınca son checkpoint değil bölüm başlangıcı gelsin; kazanma durumu temizlensin.
- [ ] **T29 — Baştan sona ilk build al.** Başlangıç → su harcama → birikinti → gerekirse ölüm/yeniden doğma → fidan → tekrar oyna akışı dışa aktarılan sürümde çalışsın.

**Çıkış ölçütü:** Oyun henüz çirkin olabilir; fakat dışarıdan biri başlayıp kaybedebilir, yeniden deneyebilir ve kazanabilir.

## P0 — Bölüm ve oynanış dengesi

- [ ] **T30 — Kısa rota taslağını çiz.** Başlangıç, öğretici düz alan, ilk zıplama, nemli zemin, su birikintisi, son platform alanı ve fidanı sıraya koy. Yeni sistem eklemeden mevcut kurallarla alan çeşitlendir.
- [ ] **T31 — Mekanikleri sırayla öğret.** Önce yürüme/zıplama, sonra su kaybı, sonra zemin farkı, ardından kaynak. İlk su birikintisine ulaşmak deneyimli oyuncu refleksi gerektirmesin.
- [ ] **T32 — Güvenli durma alanları bırak.** Durmak ücretsiz olduğuna göre oyuncu platform üzerinde düşünmek için durabilmeli. İniş alanları dar ve görünmez olmasın.
- [ ] **T33 — Kaynaklar arası bölümleri test et.** Dolu suyla başlayan oyuncu makul hatalarla bir sonraki durağa ulaşabilsin. Kesin sıçrama ve tek kusursuz rota zorunluluğundan kaçın.
- [ ] **T34 — Tüketim kuralının anlamlılığını dene.** Sürekli zıplamanın zeminden kaçıp oyunu gereksiz biçimde kolaylaştırıp kolaylaştırmadığını kontrol et. Normal hava tüketimi nedeniyle çok kuru zemini zıplayarak geçmek avantajlıysa bunun kabul edilir olup olmadığını not et; kuralları sessizce değiştirme.
- [ ] **T35 — Kaynağı gerçekten gerekli kıl.** Bütün yol başlangıç suyuyla rahatça geçiliyorsa ana döngü görünmez kalabilir; tüketim veya mesafeyi testle düzenle. İlk öğretici alanı bu nedenle sertleştirme.
- [ ] **T36 — Düşme ve yeniden deneme mesafelerini ayarla.** Kaybetmek bütün oyunu uzun süre tekrar oynatmasın. Kaynak konumlarını buna göre düzenle.
- [ ] **T37 — Dışarıdan bir oyuncuyla test et.** Önce açıklama yapmadan izle: hedefi anlıyor mu, durunca kayıp olmadığını fark ediyor mu, su birikintisini tanıyor mu, finali buluyor mu? Takıldığı yerleri yaz.
- [ ] **T38 — K04’ü kapat.** Teste göre bölüm uzunluğu ve kaynak sayısını sabitle. İkinci bölüm veya dallanan rota eklemek temel sürümün şartı değildir.

**Denge yöntemi:** Bir defada tek büyük değişken değiştir; örneğin önce normal tüketimi, sonra nemli zemin oranını. Bu dokümanda verilmemiş sayıları doğrulanmış denge değeri sanma.

## P0/P1 — Görselleri ve assetleri birleştir

### Önce kararı al

- [ ] **T39 — K02 ve K03 için küçük karşılaştırma hazırla.** Geçici damlayı bir test platformunda göster. Nihai tarzı ve küçülme yöntemini kullanıcıya onaylat; onay alınmadan bütün sprite setini üretme veya topluca paket indirme.
- [ ] **T40 — Asset envanterini doğrula.** Dosyada sözü edilen `damla_sprite.zip` gerçekten takımda var mı, açılıyor mu, hangi kareleri içeriyor kontrol et. Yüklenen Markdown’da varlığı yazıyor; bu çalışmada ZIP’in kendisi veya kareleri incelenmiş değil. [cite:17]

### Kenney seçilirse uygulanacaklar

Kenney Pixel Platformer ana paketinin resmî sayfası 18×18 tile ölçüsü ve CC0 lisansı bildiriyor. Bu, paket için doğrulanmış bilgidir; diğer bütün müzik/asset kaynaklarının aynı lisansa sahip olduğu anlamına gelmez. [cite:20]

- [ ] **T41 — Yalnızca gereken paketleri al.** Ana paket zemin/dekor için, Blocks zemin ayrımı için, Farm Expansion yalnızca final fidanı veya uygun cansız dekor için adaydır. Yaşayan ara ağaç/fidan ekleme.
- [ ] **T42 — Import ayarlarını eşleştir.** 18 PPU seçilirse ilgili sprite’lar ve kamera aynı ölçekte çalışsın. Piksel görsellerde Point filtre ve uygun sıkıştırmasız ayarı test et.
- [ ] **T43 — Görsel ayrımı kontrol et.** Damla zeminden seçilsin; nemli zemin yalnızca maviye boyanmasın. Doku, şekil veya küçük simge farkı ekle; su birikintisi düz zeminden ayırt edilsin.
- [ ] **T44 — Kamera görüntüsünü test et.** Pixel Perfect Camera kullanılabilir; farklı çözünürlük ve kamera hareketinde incele. Yalnızca sprite kademeli diye titremenin kesin bittiğini iddia etme: Unity’nin belgesi PPU, viewport ve render ayarlarının birlikte ele alındığını açıklıyor. [cite:24]

### Küçülme onaylandıktan sonra

- [ ] **T45 — Görsel ve collider küçülmesini uygula.** 31 Ekim kullanıcı kararlarına göre `Visual` ve çarpışma boyutu birlikte değişsin; kök ölçek sabit kalsın. Yürüme/hava hızı ve zıplama artışı mevcut ability Inspector ayarlarıyla suya bağlansın. Animasyon esnemesi collider'ı değiştirmesin.
- [ ] **T46 — En küçük damlayı kontrol et.** Görsel/collider uyumu, eğimde durma, platform kenarı ve dar alanda dolum test edilsin; dolum collider'ı tavan veya duvar içine büyütmesin.
- [ ] **T47 — Su izini ekle.** İlk sürümde iz görseldir; yeni nemli zemin, yürünebilir yol veya geri toplanabilir su oluşturmaz. Particle veya basit geçici sprite kullanılabilir; eski izler temizlensin.
- [ ] **T48 — Küçülme, dolma ve yeniden doğmayı birlikte test et.** Havada sprite değişince karakterin ayak hizası sıçramasın; kaynakta görünüm tekrar dolu hâle gelsin.

**Dört boy seçilirse:** Asset dosyasındaki 18/15/12/9 piksel yüksekliği ve L/M/S/XS düzeni aday şartnamedir. İlk olarak her boy için tek kare yeterlidir; 30 karelik tam animasyon seti P0 değildir. Eşiklerde hangi kademenin kullanılacağı çakışmasız tanımlansın; 0 su canlı XS durumu değil ölüm durumudur. Bu ölçüler ve tam kare listesi yüklenen dosyadan gelir, kullanıcı tarafından henüz kesinleştirilmiş değildir. [cite:17]

**Kesintisiz ölçek — güncel uygulama:** Transform ölçeği yalnızca `Visual` üzerinde değişir; collider yarıçapı/offset'i suya göre ayrıca ayarlanır. Piksel tarzıyla birlikte kullanılacaksa kenar tutarlılığı ve kamera hareketi gerçek build’de denensin; görünüm onayı alınsın.

## P1 — Arayüz, ses ve sunum

- [ ] **T49 — Kontrolleri görünür yaz.** Gerçek input tuşlarıyla aynı olsun. “Hareket et, su birikintilerinde yenilen, son fidana ulaş” hedefi kısa biçimde açıklansın.
- [ ] **T50 — Su bilgisini okunabilir yap.** Çubuk yeterli olabilir; düşük su durumunu yalnızca renkle anlatma. Göstergeyi platformları kapatmayacak yere koy.
- [ ] **T51 — Başlangıç/son arayüzünü tamamla.** Ayrı menü sahnesi zorunlu değil; aynı sahnede başlangıç ve sonuç paneli yeterli. Başla ve yeniden oyna akışları çalışsın.
- [ ] **T52 — Önce işlevsel sesleri ekle.** Su dolma, kaybetme, kazanma; zaman varsa zıplama/iniş. Müzik zorunlu değil; okunabilirlik ve tamamlanabilirlikten önce gelmesin.
- [ ] **T53 — Kaynak ve lisans kaydı tut.** Kullanılan her görsel, ses, font, müzik ve kod için kaynak adresi, lisans, indirme tarihi ve gerekli atıf kaydedilsin. Pixabay veya Freesound’da dosya bazında lisansı doğrula; platform adı izin belgesi yerine geçmez.
- [ ] **T54 — Finali anlaşılır göster.** Fidana ulaşınca yeşerme ve bitiş mesajı yeterli. Üç kademeli fidan, kalan suya göre puan veya farklı son otomatik eklenmesin.
- [ ] **T55 — Gereksiz efektleri azalt.** Oyuncuyu örten buhar, yoğun ekran titremesi veya aşırı parçacık kullanma. Sıcak bölge eklenmediyse “hot” yüz ifadeleri üretmek gerekmez.

## Teknik sorumluluk sınırları

Aşağıdaki sınıf adları örnektir; çalışan mevcut eşdeğerleri varsa kullanılabilir. Bu bir framework kurma görevi değildir.

| Bileşen | Tek ana sorumluluk | Sınır |
|---|---|---|
| `PlayerController` | Hareket ve yerde/havada bilgisi | UI veya su değerini yönetmez |
| `WaterMeter` | Tek su değeri, tüketim ve dolma | Sprite üretmez |
| `GroundType` / zemin algılama | Mevcut yüzey türü | Nemli/kuru katsayısını birden fazla script’te kopyalamaz |
| `WaterSource` | Dolum ve güvenli dönüş noktası | Final fidanı değildir |
| `RespawnController` | Başlangıç/son kaynak konumuna dönüş | Birden çok eşzamanlı yeniden doğma başlatmaz |
| `PlayerSizeVisual` | Suya bağlı görsel güncelleme | Collider, hız veya zıplamayı değiştirmez |
| `TrailFX` | Kısa ömürlü görsel iz | Su tüketiminin sahibi değildir |
| `SaplingGoal` | Final tetikleme | Kaynak gibi su doldurmaz |
| `GameFlow` | Oynuyor / yeniden doğuyor / kazandı akışı | Tüketim ve girişin ne zaman açık olduğunu belirler |
| Su arayüzü | Su bilgisini gösterme | Kendi ayrı su sayacını tutmaz |

Önerilen dizinler: `Assets/_Game/Scripts`, `Scenes`, `Prefabs`, `Art`, `Audio`, isteğe bağlı `Data`; belgeler için depo kökünde `docs/`. Mevcut düzen uygunsa yalnızca bu öneri için yeniden taşınmasın. Asset taşınırken `.meta` eşleşmesi korunsun. [cite:18]

## Takvim ve öncelik

Plan hazırlanırken 30 Ekim gecesine gelinmiştir; ilk günün tamamı hâlâ kullanılabilir sanılmamalıdır. Resmî teslim saati doğrulanmadan “72 saat kaldı” varsayımı yapılmaz.

| Çalışma penceresi | Öncelik | Kontrol noktası |
|---|---|---|
| İlk çalışma oturumu | T01–T20 | Proje açılır; hareket, su göstergesi ve tüketim kuralları test edilir |
| Sonraki odaklı oturum | T21–T29 | İlk baştan sona oynanabilir build |
| 31 Ekim’in kalan uygun zamanı | T30–T48 | Bölüm dengesi; görsel karar alındıysa minimum görsel uygulama |
| 1 Kasım, teslimden önce | T49–T55 ve son testler | Yeni mekanik yerine okunabilirlik, hata düzeltme ve teslim |
| Son teslim öncesi ayrılmış tampon | Paketleme ve yükleme | Son dosya yüklenir, bağlantıdan yeniden indirilip denenir |

**Kapsam kesme sırası:** Yeni fikirler → fazladan animasyon → müzik/dekor → alternatif rota → ek su birikintisi ve alanlar. Hareket, su kaybı, bir kaynak, yeniden deneme, iz, final ve çalışan build korunur. Nihai küçülme görseli ertelenirse bunun bilinçli bir kapsam değişikliği olduğu kullanıcıya bildirilir; prototip tavizi sessizce kalıcılaştırılmaz.

## P2 — Onay gerektiren ek fikirler

Bu liste uygulama talimatı değildir. Her ek fikir tamamlanma süresini ve test yükünü artırır; P0 bitmeden başlanmaz.

| Fikir | Kazanım | Göreli yük / koşul |
|---|---|---|
| Kısa-kuru / uzun-nemli alternatif yol | Kaynak üzerinden rota kararı | Orta; uzun yolun toplam su bedeli gerçekten daha düşük mü test edilmeli |
| Dolum ve bitişte küçük sıçrama/parıltı | Eylemin anlaşılması | Düşük; okunabilirliği bozmasın |
| Basit göz kırpma, zıplama ve iniş kareleri | Karakter hissi | Düşük-orta; nihai tarzdan sonra |
| Kalan su yüzdesini sonuç ekranında gösterme | Yeniden oynama hedefi | Düşük; yeni başarısızlık şartı yaratmasın |
| Kalan suya göre üç fidan görünümü | Kaynak yönetimine görsel ödül | Orta; final eşiği ve gerekli kareler ayrıca onaylanmalı |
| Özel sıcak/kuru alan ve buhar efekti | Daha güçlü çevresel anlatım | Orta; yerde durunca yine su azalmayacağı kuralıyla tutarlı olmalı |
| Küçük su pickup’ları | Bölüm içinde ek hedef | Orta; kaynak/checkpoint dengesini değiştirebilir |
| Kolay tüketim ayarı | Daha erişilebilir deneme | Orta; temel denge oturduktan sonra |
| Geçici yeşil görsel iz | Umut/hayat teması | Hikâye onayı gerekir; başka canlı bitki üretip “son fidan” fikrini çeliştirmesin |

**Şimdilik dışarıda:** İkinci bölüm, hareket eden platformlar, ayrıca tasarlanmış dar geçit/puzzle mekaniği, dinamik gölge sistemi, diğer düşmanlar, silah, karmaşık su fiziği, envanter, kayıt/yükleme ve çok oyunculu mod. Suya bağlı collider küçülmesi ve hareket/zıplama çarpanları 31 Ekim kullanıcı kararlarıyla kapsam içindedir. Aynı günkü açık kullanıcı talepleriyle Saw, Slime Fire ve HapsirikKoz'un duran/git-gel türleri, Slime yakınlık erime çarpanı, HapsirikKoz'un rastgele bekleme/cooldown ile hapşırması ve birkaç saniyelik hasarlı duman alanı kapsama alındı. Sonraki açık taleple altı düşman gövdesine knockback, Inspector'dan ayarlı tek su kaybı ve birkaç saniyelik hasar karesi/flash eklendi. Flash yeni gövde darbelerini engellerken ısı/duman tüketimi sürer; bu geniş alanlar knockback tetiklemez. Duman görseli/animasyonu bekliyor ([karar](docs/kararlar.md), [prefab ayarları](docs/enemies.md)).

## Son test kontrol listesi

- [ ] **QA01:** Isı/duman, Köz Ocağı ve düşman gövde teması dışında yerde hiç hareket etmeden bekleyince su aynı kalıyor; Slime Fire'ın etkin ısı alanı veya HapsirikKoz dumanında dururken su azalıyor. Köz Ocağı ayrıca mesafeye bağlı mutlak puan/saniye tüketir. Alan çıkışı/kapanışı veya dumanın ömrünün bitişi yalnızca ilgili etkiyi kaldırır. Gövde darbesi tek su kaybı üretir; flash yeni gövde darbelerini engeller, çevresel tüketimi engellemez.
- [ ] **QA02:** Normal zeminde yürüyüşte tüketim var; nemli zeminde daha düşük.
- [ ] **QA03:** Yalnızca dikey zıplarken ve düşerken normal tüketim var.
- [ ] **QA04:** Zeminden ayrılınca nemli zemin katsayısı havaya taşınmıyor.
- [ ] **QA05:** Zemin kenarında titreşim yanlış tüketim üretmiyor.
- [ ] **QA06:** Su sıfırın altına ve üst sınırın üzerine çıkmıyor.
- [ ] **QA07:** Sıfır su tek ölüm üretiyor; çukura düşme de kurtarılabilir.
- [ ] **QA08:** İlk kaynak öncesinde başlangıca, sonrasında güvenli kaynağa dönülüyor.
- [ ] **QA09:** Yeniden doğmada hız, su, UI, giriş ve görsel doğru sıfırlanıyor.
- [ ] **QA10:** Kaynağa art arda girip çıkmak tekrarlı olay veya kilitlenme yaratmıyor.
- [ ] **QA11:** En küçük görselde collider görselle birlikte küçülüyor; kök ölçek sabit, hız/zıplama artışı seçilen çarpanları izliyor. Eğim, kenar ve dolum çarpışmaları tutarlı; dolum/yeniden doğma eski hız bonusunu taşımıyor.
- [ ] **QA12:** Su izi zamanla temizleniyor; uzun oynayışta sınırsız obje birikmiyor.
- [ ] **QA13:** Fidana ulaşınca bir kez kazanılıyor, tüketim duruyor.
- [ ] **QA14:** Su bitmesi ve final teması aynı ana denk gelirse sonuç tutarlı. Prototip teknik önerisi: suyu bitmiş damla kazanamasın; bu sıra bilinçli test edilsin.
- [ ] **QA15:** Yeniden oyna bölümün tamamını sıfırlıyor.
- [ ] **QA16:** Farklı kare hızlarında tüketim belirgin biçimde değişmiyor.
- [ ] **QA17:** Dışa aktarılan sürümde kamera, UI, ses, input ve sahne açılışı çalışıyor.
- [ ] **QA18:** Oyunu daha önce görmemiş biri hedefi anlayıp sonuna ulaşabiliyor.
- [ ] **QA19:** Yaşayan ara fidan/ağaç yok; finalde tek fidan hikâyesi tutarlı.
- [ ] **QA20:** Kullanılacak son dosya iki takım üyesinin erişebildiği yerde yedekli.

## Teslim kontrol listesi

- [ ] **D01:** Resmî tema ve kurallara uygunluk son kez kontrol edildi.
- [ ] **D02:** Kesin son saat, saat dilimi ve yükleme adresi kaydedildi.
- [ ] **D03:** Doğru platform için son build alındı; geliştirme araçları olmadan açıldı.
- [ ] **D04:** Başlangıçtan finale kadar son build test edildi; sonra yeni mekanik eklenmedi.
- [ ] **D05:** Başlık, kısa açıklama, gerçek kontroller, ekip bilgisi ve bilinen sorunlar hazır.
- [ ] **D06:** Görsel/ses/font/kod kaynakları ve lisansları listelendi; gerekiyorsa AI kullanımı açıklandı.
- [ ] **D07:** Gereken ekran görüntüsü/video gerçek çalışan sürümden alındı.
- [ ] **D08:** Yüklenen dosya veya oyun bağlantısı farklı oturumdan açıldı; gerekli erişim izinleri kontrol edildi.
- [ ] **D09:** Yüklenen sürümün commit’i veya sürüm etiketi kaydedildi.

## AI agent çalışma sözleşmesi

- Önce bu dosyadaki onaylı kararları oku. Eski belgenin “değiştirme” talimatıyla son kullanıcı kararını geçersiz kılma.
- Tek seferde küçük bir görev al; hangi T/QA/D maddesine çalıştığını belirt.
- Mevcut dosyaları ve Unity sürümünü incelemeden var olmayan script/API veya kurulu paket varsayma.
- Onay gerektiren görsel kararları kendin verme. Dört boy sprite, 30 karelik sheet veya Kenney kullanımı otomatik şart değildir.
- Bitki checkpoint’i veya ısı/duman dışında yerde dururken tüketim ekleme. Slime Fire ısısı/HapsirikKoz dumanında hareketsiz su kaybı, suya bağlı collider değişikliği ve ayarlanabilir hareket/zıplama artışı onaylıdır; yeni bir su sayacı oluşturma veya zıplama itişini havada tekrarlama.
- Çalışan kodu gereksiz yeniden düzenleme; yeni bağımlılık ve paket eklemeden gerekçe/onay iste.
- Sahne, prefab ve `.meta` dosyalarını körlemesine yeniden üretme. Takım arkadaşının sahip olduğu dosyayı habersiz değiştirme.
- Kodun yazılmış olmasını testin geçmesiyle karıştırma. Unity’de çalıştıramadıysan “uygulandı; editör/build testi bekliyor” olarak raporla.
- Görev kapanışında değişen dosyaları, Inspector bağlantılarını, yapılan testi, kalan riski ve tamamlanma durumunu yaz.
- Bir test yapılmadıysa kutusunu işaretleme. Kurallar veya lisans doğrulanmadıysa uygunluk garantisi verme.

**İlk uygulanacak iş paketi:** T01–T07. Ardından T08–T20. İlk hedef güzel bir damla çizmek değil; hareketin su tükettiği, durmanın güvenli olduğu ve zemin farkının görüldüğü çalışan test sahnesidir.
