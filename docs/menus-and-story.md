# Menü, hikâye ve ses ayarları

1 Kasım 2026'da kullanıcı isteğiyle eklendi (T49–T51 kapsamı). Kod yazıldı; Unity'de derleme, sahne kurulumu ve oynanış testi bekliyor.

## Akış

`MainMenu` → (Oyuna başla) → `Intro` (hikâye paragrafları) → `Level_1` → `Level_2` → `Level_3` → fidan sahnesi → son hikâyesi → **Tekrar oyna** (`Level_1`) / **Oyundan çık**.

Build listesi sırası: 0 `MainMenu`, 1 `Intro`, 2 `Level_1`, 3 `Level_2`, 4 `Level_3`. Sahneler adla yüklenir (`LevelExit` Next Scene Name, `SceneLoader.LoadScene`); `FinalSapling.RestartGame()` artık build index 0 yerine `Level_1`'i yükler.

## Script'ler

| Script | Nerede | Ne yapar |
|---|---|---|
| `Scripts/UI/SceneLoader.cs` | Menü/hikâye Canvas'ı | `LoadScene(string)` kararmayla sahne yükler (build listesinde değilse uyarı verir); `QuitGame()` oyunu kapatır (Editor'de Play'i durdurur, WebGL'de etkisi yoktur). Düğmelerin On Click'ine bağlanır. |
| `Scripts/UI/StoryPlayer.cs` | Hikâye metni | Paragrafları tek tek gösterir: fade in (0.8 sn) → bekleme (2.5 sn + harf başına 0.04 sn) → fade out. Tık, herhangi bir tuş, gamepad A ya da dokunma atlatır. Bitince **On Finished** olayı çağrılır. **Play On Start** açıksa (intro) kendiliğinden başlar; kapalıysa (son) `Play()` ile başlatılır. **Background** (CanvasGroup) isteğe bağlıdır, hikâye başlarken koyulaşır. TextMeshPro kullanır. |
| `Scripts/UI/VolumeSettings.cs` | Her zaman aktif bir nesne | Müzik/efekt slider'ları (0–1). Değerler PlayerPrefs'te saklanır ve Audio Mixer'daki `MusicVolume` / `SfxVolume` parametrelerine desibel olarak yazılır. `Sfx.Output` olarak efekt grubunu ayarlar. Slider'lar isteğe bağlıdır. |
| `Scripts/Audio/Sfx.cs` | — | `Output` mixer grubu eklendi; efektler efekt slider'ına uyar. |

Müzik: `Music.prefab`'ın Audio Source **Output**'u mixer'ın Music grubuna bağlanır. `MusicPlayer.SwitchTo` (final müziği) aynı grubu kopyaladığı için o da slider'a uyar. `MusicPlayer` kuralı: Music nesnesi olmayan sahnede müzik durur; menüde/introda müzik istenirse o sahneye de Music örneği konur.

## Açılış müziği ve oyun sonu sessizliği

- `MusicPlayer`: Yeni sahnenin Music nesnesi şu an çalan parçanın aynısını (aynı Audio Resource) çalıyorsa müzik baştan başlamaz, sürer. `MainMenu` ve `Intro`'ya `music_opening` atanmış birer Music örneği konunca menü → intro geçişinde açılış müziği kesintisiz devam eder. `Level_1`'in farklı parçası onu devralır.
- `Sfx.MuteWorld()`: Çalan efektleri durdurur ve `PlayAt` ile çalan dünya seslerini (yaratıklar, `SoundOnAnimationEvent` hapşırığı, `SoundOnEnable` ateş topu) sonraki sahne yüklenene kadar susturur. `FinalSapling` sahne başlarken çağırır; bu sayede fidan sahnesinde ve son hikâyesinde yaratık sesleri duyulmaz. "Tekrar oyna" yeni sahne yüklediği için sesler geri gelir. Oyuncu ve arayüz sesleri (`Sfx.Play`) etkilenmez.

## Audio Mixer kurulumu (bir kez)

1. `Assets/Audio` → Create → **Audio Mixer** → `GameMixer`.
2. Audio Mixer penceresinde **Groups** altında Master'a iki çocuk grup: `Music`, `SFX`.
3. `Music` grubunu seç → Inspector'da **Volume**'a sağ tık → **Expose 'Volume (of Music)' to script**. `SFX` için aynısı.
4. Pencerenin sağ üstündeki **Exposed Parameters** listesinde isimleri `MusicVolume` ve `SfxVolume` yap (büyük/küçük harf aynı olmalı).
5. `Prefabs/Music` → Audio Source → **Output** = `GameMixer/Music`.

## Bulanık arka plan

Önerilen yol kodsuzdur: Level 1'i Game görünümünde istediğin karede durdur, ekran görüntüsü al (Win+Shift+S), bir görsel düzenleyicide (ör. Photopea → Filter → Blur → Gaussian Blur, 8–15 px) bulanıklaştır, `Assets/UI/` altına PNG olarak koy. Import: Texture Type **Sprite (2D and UI)**. Menü Canvas'ında tam ekran **Image** olarak kullanılır. Gerçek zamanlı blur (shader) yükü ve riski nedeniyle seçilmedi.

## Açık seçimler

- Intro arka planı: siyah ya da bulanık görsel (kullanıcı karar verecek; `StoryPlayer` ikisiyle de çalışır).
- Hikâye metinleri: kullanıcı yazacak (Inspector'da Paragraphs listesi).
- Menü/intro müziği: isteğe bağlı.
- Teslim platformu WebGL olursa **Oyundan çık** düğmesi gizlenmeli (K01).
