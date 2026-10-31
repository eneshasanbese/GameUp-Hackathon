---
name: gameup-verify-delivery
description: GameUp Unity değişikliklerini doğrula, ilgili QA senaryolarını çalıştır, build ve hackathon teslim hazırlığını yap. T01, T07, T29, T53, QA01–QA20 ve D01–D09 için kullan; belge kontrolünü oynanış/build testiyle karıştırma.
---

# GameUp doğrulama ve teslim

[Ortak yönergeleri](../../../AGENT.md) ve [genel plandaki](../../../genel-yapilacaklar.md) ilgili T/QA/D kabul ölçütlerini oku. İstenen işi test, build hazırlığı veya teslim hazırlığı olarak belirle; yalnızca hazırlık talebi dış servise yayın/yükleme yetkisi vermez. Açıkça verilmiş yükleme yetkisini yeniden isteme.

## Başlangıç kanıtını topla

Depo kökünde `git status --short` ve ilgili diff'i kontrol et. Unity kökü `GameUp Hackathon/` klasörüdür. `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, varsa test dosyaları/asmdef'ler ve build ayarlarını incele. Makinedeki Unity yolunu veya kurulu platform modülünü varsayma.

[Oyuncu](../../../docs/player-state-machine.md) ve [su](../../../docs/water-player.md) belgeleri önceki Play Mode denemeleri bildirir; testler geçici projede çalıştırılmış olabilir. Depoda `Assets/Tests` veya yeniden çalıştırılabilir test komutu bulunduğunu varsayma. Önceki 13/17 test sayısını yeni doğrulama sonucu diye kullanma.

## Değişikliğe göre doğrulama seç

| Değişiklik | Gerekli kanıt |
|---|---|
| Yalnızca belge/skill | Yol ve bağlantı geçerliliği, frontmatter, tasarım/kodla tutarlılık; Unity açmak gerekmez |
| Input/hareket/zemin | Derleme ve ilgili Play Mode/oynanış; kısa basış, iniş, rampa, kenardan düşüş, kapatma/açma |
| Su/kaynak/respawn | QA01–QA10, QA14, QA16 içinden etkilenen durumlar; miktar, geçiş ve sıfırlama gözlemi |
| Görsel/iz/final/yeniden oyna | QA11–QA15 ve QA19 içinden etkilenen durumlar; collider, iz temizliği, tek kazanma, başlangıca dönüş |
| Build/teslim | Hedef platformda gerçek dışa aktarılan sürüm; QA17 ve D03–D04 |

Var olan testleri uygun şekilde çalıştır. Davranış değişikliği gerektiriyorsa küçük regresyon testi ekle; yalnızca uygulama ayrıntısını tekrar eden testler yazma. Sahne ve component bağlantılarının gerçekten çalışmasını doğrula. Unity erişimi yoksa kaynak/serialized bağlantı incelemesini yap ve Play Mode/build doğrulamasını bekleyen iş olarak bildir.

## Unity çalıştırma

Editor açık projeyi kullanıyorsa aynı proje üzerinde ikinci batch Unity başlatma; açık oturumun Test Runner'ını kullan veya ayrıca gerekliyse temiz, ayrı geçici proje ile çalış. Başka oturumu kapatma ve lock dosyası silme. Geçici testte hangi sahne/kod/ayarların kullanıldığını kaydet; asıl projeyi veya teslim build'ini test etmiş gibi yazma.

CLI gerçekten kullanılabiliyorsa Unity çalıştırılabilir dosyasının yolunu ve proje sürümünü önce doğrula. Test Framework komutunu kurulu sürümün yardımına/belgesine göre kur; `-projectPath` Unity kökünü, `-testResults` ve `-logFile` yazılabilir çıktı yollarını göstermeli. PowerShell'de boşluklu yolları tek argüman olarak tırnakla. Sonuç XML'i, hata kayıtları ve koşulan test sayısını kontrol et; sadece süreç çıkışını “oynanış geçti” sayma.

`dotnet build` veya IDE derlemesi scene, input, collider ve build akışının çalıştığını kanıtlamaz. Depoda özel bir build `-executeMethod` metodu yoksa varmış gibi komut uydurma.

## Build ve teslim hazırlığı

1. K01 için tema, resmi son saat/saat dilimi, platform, hazır kod/asset ve AI kurallarını kaynaklarıyla doğrula. `docs/kararlar.md` yoksa kararlar gerçekten elde edildiğinde oluştur; tarihli eski takvimden “kalan saat” çıkarma. Yanıt yoksa uygunluk veya teslim formatı garantisi verme; bağımsız hazırlıkları sürdür.
2. Build Profile ile `ProjectSettings/EditorBuildSettings.asset` sahne listesini kontrol et. Bugünkü listede `SampleScene.unity` etkindir; prototip `Components.unity` içindedir. Hedef sahne ve başlangıç sırasını açıkça seç ve doğrula.
3. Hedef platform modülüyle build al; Unity Editor dışında aç. Başla → hareket/su kaybı → birikinti → ölüm/düşme ve güvenli dönüş → pozitif suyla final → bölüm başından yeniden oyna akışını kontrol et. Kamera, input, UI ve sesin hedef çözünürlükte çalıştığını gözle.
4. Başlık, kısa açıklama, gerçek kontroller, ekip, bilinen sorunlar ve görselleri gerçek sürüme göre hazırla. Dış görsel/ses/font/kod için kaynak, lisans, tarih ve gerekli atfı dosya bazında kaydet; AI açıklaması gerekip gerekmediğini resmi kurallara göre belirle.
5. Yükleme yetkilendirildiyse son dosyayı yükle ve erişilebiliyorsa bağlantıdan yeniden indirip aç. Uygulanmayan dış oturum/ikinci bilgisayar kontrolünü yapılmış sayma. Teslim dosyasını, hedef platformu, commit kimliğini ve varsa commit edilmemiş değişiklikleri kaydet; kirli çalışma ağacının build'ini yalnızca commit hash'iyle tanımlama.

Build çıktısını kaynak dosyalarına veya üretilen Unity klasörlerini Git'e ekleme. Son kabul testinden sonra yeni mekanik eklenirse etkilenen kontrolleri tekrar çalıştır.

## Sonucu kaydet

Yapılan kontrolün yeri/sürümü, kapsamı, sonucu ve varsa log/XML/build yolunu bildir. T/QA/D kutusunu yalnızca o maddenin tamamı kanıtlandıysa işaretle; otomatik test, elle oynanış, dış build ve başka bilgisayar doğrulamasını ayrı belirt. Test yoksa “uygulandı; editör/build testi bekliyor” yaz. Mevcut oturumun doğrulaması olmadan geçmiş raporları tamamlanma kanıtına dönüştürme.
