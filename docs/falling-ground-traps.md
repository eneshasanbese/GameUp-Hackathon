# Basınca düşen zeminler

1 Kasım 2026 kullanıcı talebi (T30/T42 ek kapsamı): `FallingGround_Sand.prefab` ve `FallingGround_Rock.prefab`, oyuncu üstlerine bastıktan kısa süre sonra düşer.

İki prefabın kökünde `FallingGroundTrap`, `Rigidbody2D`, iki tile genişliğinde trigger olmayan `BoxCollider2D` ve mevcut `GroundSurface` bulunur. Kum `Ground-Dry / Sand`, kaya `Ground-Extra-Dry / BlackStone` kullanır; mevcut oyuncu zemin algısı ve su tüketimi bunları tanır. Sahnede ek oyuncu referansı veya sensör bağlantısı gerekmez. Mevcut prefab GUID'leri, nesne kimlikleri, iki alt görsel ve Transform değerleri korunur.

| FallingGroundTrap ayarı | Varsayılan | İşlev |
|---|---|---|
| Fall Delay | 0.25 saniye | İlk üst yüzey temasından düşüşe kadar bekleme |
| Gravity Scale | 3 | Düşüş yerçekimi çarpanı |
| Maximum Fall Speed | 14 | Aşağı hız sınırı |
| Despawn Delay | 3 saniye | Düşüş başladıktan sonra silinme süresi |

Platform başlangıçta yerçekimsiz Kinematic gövdedir ve oyuncuyu destekler. Yalnızca etkin ve yaşayan gerçek Player'ın üst yüzeye fizik teması sayacı başlatır; yan/alt temas ve başka nesneler başlatmaz. Sayaç tek kez başlar; oyuncu zıplasa veya ayrılsa da tekrar başlamaz, kısa beklemenin sonunda düşüş gerçekleşir.

Düşüşte destek collider'ı kapanır; platform oyuncuyu taşımaz ve diğer zeminlere takılmaz. Yatay konum ve dönüş kilitlidir. Bu davranış ek temas hasarı üretmez. Sahne yeniden yüklendiğinde platformlar başlangıç hâline döner. Bileşeni kapatmak sayaç ve hareketi durdurur; açmak kaldığı yerden sürdürür. Süreler oyun zamanını izler ve pause sırasında ilerlemez.

## Ses alanları

1 Kasım 2026 ses hazırlığı (T52): İki prefab'ın `FallingGroundTrap / Sounds` alanlarında **Warning Sound** ilk geçerli basışta, **Fall Sound** gerçek düşüş başlangıcında bir kez çalar. **Sound Volume 0.6 / Hearing Distance 12** hazırdır; dosyalar boş bırakıldı. Destek collider'ı kapandığı için zemin çarpması sesi eklenmedi. [Dosya atama ve ses doğrulaması](ses-kurulumu.md#düşen-platformlar-ve-sarkıt).

## Doğrulama

`Assets/Editor/Traps/FallingGroundTrapVerification.cs` yalnızca ayrı batch Unity projesinde çalışır. `ConfigureAndRun` bu kopyadaki iki prefabı Unity API'siyle bağlayıp test eder; `Run` hazır prefabları değiştirmeden doğrular. Açık ana proje ve sahne kullanılmaz. Gerçek güncel Player prefab'ı, script'ler ve input kullanılır; testte normal su tüketimi sıfırlanır.

Bu oturumda Unity `6000.5.0f1` ile **44 kontrol geçti (12 asset, 32 Play Mode)**. İki gerçek prefab için kimlikler, sprite bağlantıları, fizik kurulumu, oyuncu yokken sabit kalma, oyuncu olmayan ağırlık, yan/alt temas, kapalı Player, gerçek iniş ve zemin/malzeme algısı, beklemede destek, oyuncu ayrıldıktan sonra düşme, dikey hareket/hız sınırı ve silinme doğrulandı. Ayrıca Inspector'dan bekleme ayarı, bileşen kapatma/açma, pause ve düşüşte oyuncunun desteğini kaybetmesi kontrol edildi. [Sonuç](verification/falling-ground-result.txt), [geçen kontroller](verification/falling-ground-checks.txt).

Açık ana projeden ayrı, önceki testlerden kalan `GameUp Hackathon/Temp/StoneSpikeTask/Verification` projesi kullanıldı; ilgili runtime script'leri, Player/input/animasyonları ve özgün zemin sprite'ları güncel kaynaklardan kopyalandı. Son koşu `FallingGroundTrapVerification.Run` ile kaydedilmiş prefabları değiştirmeden test etti. Log `GameUp Hackathon/Temp/StoneSpikeTask/falling-ground-verification.log` konumundadır. Unity Search başlangıç indeksleyicisinde `ArgumentOutOfRangeException` görüldü; oyun kodu derlendi ve fizik kontrolleri tamamlandı. İlk kurulum denemelerinde Unity null kontrolü, eksik test sprite bağımlılığı ve oyuncunun yeni teması sonraki fizik adımında okumasını bekleyen test düzeneği düzeltildi; sayı son başarılı koşuya aittir.

Unity API'siyle kaydedilip doğrulanan iki prefab ana projeye aktarıldı; özgün nesne blokları/GUID'ler ve aktarılan dosyaların hash'leri karşılaştırıldı. Runtime/test script'leri ve `.meta` dosyaları test kopyasıyla aynıdır. Ana sahne dosyalarına yazılmadı. Asıl bölümde elle oynanış ve dış build testi bekliyor; T/QA kabul kutuları kapatılmadı.
