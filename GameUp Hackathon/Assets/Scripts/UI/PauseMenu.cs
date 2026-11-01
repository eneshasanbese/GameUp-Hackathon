using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ESC pauses a level: time stops, enemy and world sounds pause (music keeps playing),
/// player input is ignored and the pause panel opens. ESC again or Resume continues.
/// Buttons: Resume, GoToMainMenu, QuitGame.
/// No pause during a death/respawn, a level exit or the ending cutscene.
/// </summary>
[DisallowMultipleComponent]
public class PauseMenu : MonoBehaviour
{
    [Tooltip("Pause screen; hidden at start and shown while paused.")]
    [SerializeField] private GameObject panel;
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField, Min(0f)] private float fadeDuration = 0.5f;

    public static bool IsPaused { get; private set; }

    private Player player;
    private bool leaving;

    // Keeps the static flag clean when domain reload is disabled.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => IsPaused = false;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
        else
            Debug.LogWarning($"{name}: PauseMenu has no Panel assigned.", this);
    }

    private void Update()
    {
        if (leaving || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;
        if (IsPaused)
            Resume();
        else
            TryPause();
    }

    private void TryPause()
    {
        if (SceneFader.IsBusy)
            return;
        player = FindAnyObjectByType<Player>();
        // The player is switched off during a level exit and the ending cutscene.
        if (player == null || !player.isActiveAndEnabled)
            return;
        WaterMeter water = player.GetComponent<WaterMeter>();
        if (water != null && water.IsRespawning)
            return;

        IsPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        player.gatherInput.DisablePlayerMap();
        if (panel != null)
            panel.SetActive(true);
    }

    /// <summary>For the Resume button; ESC does the same.</summary>
    public void Resume()
    {
        if (!IsPaused || leaving)
            return;
        Unfreeze();
        if (player != null && player.isActiveAndEnabled)
            player.gatherInput.EnablePlayerMap();
        if (panel != null)
            panel.SetActive(false);
    }

    /// <summary>For the main menu button. The game stays frozen behind the fade.</summary>
    public void GoToMainMenu()
    {
        if (leaving)
            return;
        if (SceneFader.FadeAndLoad(mainMenuScene, fadeDuration, Color.black))
            leaving = true;
    }

    /// <summary>Closes the game. In the Editor it stops Play Mode instead.</summary>
    public void QuitGame()
    {
        Unfreeze();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Leaving the level never carries a frozen game into the next scene.
    private void OnDestroy()
    {
        if (IsPaused || leaving)
            Unfreeze();
    }

    private static void Unfreeze()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}
