---
name: gameup-player-controller
description: GameUp Unity projesinde oyuncu input, hareket, zıplama, zemin algılama ve ability/state machine davranışını geliştir veya düzelt. T08–T11 için kullan; su tüketimi ve bölüm görselleri için ilgili diğer skill'i seç.
---

# GameUp oyuncu kontrolü

[Ortak yönergeleri](../../../AGENT.md) ve [oyuncu belgesini](../../../docs/player-state-machine.md) oku. İlgili T08–T11 maddesinin beklenen davranışını belirle; suyla etkileşim varsa [su belgesine](../../../docs/water-player.md) de bak.

## Değişiklik noktasını seç

Unity proje kökü depo altındaki `GameUp Hackathon/` klasörüdür. Önce `Assets/Scripts/Player/` içindeki ilgili component'leri ve `Assets/Scenes/Components.unity` bağlantılarını incele.

| İhtiyaç | Mevcut sorumlu |
|---|---|
| Action asset'i, girişin açılması/kapanması, kısa basma/bırakma | `GatherInput` |
| Rigidbody2D, iki zemin raycast'i ve hız uygulaması | `PhysicsControl` |
| Ability bulma ve yaşam döngüsünü çalıştırma | `Player` |
| İzinli component'e Enter/Exit geçişi | `StateMachine`, `BaseAbility`, `PlayerStates` |
| Durma/yerde yürüme/havada hareket | `IdleAbility`, `MoveAbility`, `JumpAbility` |

Yeni bir state gerçekten gerekiyorsa `BaseAbility` türevi component ve enum değeri ekle; Player'ın aynı objeden keşfetmesini kullan. Mevcut üç durumla çözülebilen iş için yeni state oluşturma.

## Korunacak sözleşmeler

- Oyuncuya `Assets/PlayerInput.inputactions` bağlanır; `Player/Move` Vector2, `Player/Jump` Button'dır. `InputSystem_Actions.inputactions` başka bir asset'tir; adını görüp yerine kullanma.
- `GatherInput` asset'in kendi kopyasını oluşturur. Map kapandığında bekleyen basma/bırakma olayları temizlenir. Kısa Space basışı fizik adımları arasında kaybolmamalı, tek olay iki zıplama üretmemeli.
- Zıplama basılı tutularak inişte tekrarlanmaz; havada ikinci zıplama yoktur. Yükselirken Space bırakıldığında mevcut davranış dikey hızı sıfırlamaktır; bunu değiştirmen istenmediyse koru.
- Kenardan düşmek, yukarı itiş uygulamadan `Jump` durumuna geçirir. Durum değişimi başarısız/izinsizken zıplama kuvveti uygulanmaz. Aynı aktif state'e geçiş Enter/Exit'i tekrar çalıştırmaz.
- Mevcut yürütme sırası: `GatherInput` -300, `PhysicsControl` -200, `JumpAbility` -100, `Player` varsayılan, `WaterMeter` +100. `PhysicsControl.FixedUpdate` zemini yeniler; `JumpAbility.FixedUpdate` isteği işler; `Player.FixedUpdate` aktif ability'nin fiziğini yürütür. Bu sırayı gereksiz değiştirme.
- Ground-Dry ve Ground-Wet ayrı LayerMask alanlarıdır. Üst üste yüzeylerde yakın raycast sonucu kullanılır; yükselişte eski yüzey yeniden grounded sayılmaz. Collider sabittir; bugün `CircleCollider2D` kullanılır.
- Hız/ivme ayarları ability Inspector'ındadır. Player'a ikinci hareket motoru veya su hesabı ekleme; UI için mevcut `CurrentState` ve `IsGrounded` okunabilir.

## Doğrula ve bağla

Göreve göre düz/kuru ve nemli zeminde hareket-duruş, rampa, kenardan düşme, dikey zıplama, havada yön değiştirme, kısa Space basıp bırakma, basılı tutarak iniş ve component/map kapatıp açma senaryolarını seç. Fizik/input sırasını değiştirdiysen farklı fizik adımı/kare hızında kısa basışları da dene.

Yeni component için aynı objede gereken ability'leri, Rigidbody2D/collider'ı, Input Actions referansını ve iki zemin maskesini belirt. Testte yalnızca sınıfın derlenmesini değil hareketin ve state geçişinin sonucunu gözle. Değişen Inspector bağlantılarıyla testin nerede yapıldığını raporla; dokümandaki geçmiş 13 test kaydını bu oturumun sonucu diye sunma.
