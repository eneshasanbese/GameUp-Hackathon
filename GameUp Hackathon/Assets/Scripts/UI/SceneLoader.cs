using UnityEngine;

/// <summary>
/// Button helpers: load a scene behind the screen fade, or quit the game.
/// Hook the public methods to a Button's On Click or any UnityEvent.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [SerializeField, Min(0f)] private float fadeDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    /// <summary>Loads a scene by name, e.g. "Intro" or "Level_1". It must be in the build list.</summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning($"{name}: scene '{sceneName}' is not in the build list. Add it in File > Build Profiles > Scene List.", this);
            return;
        }
        SceneFader.FadeAndLoad(sceneName, fadeDuration, fadeColor);
    }

    /// <summary>Closes the game. In the Editor it stops Play Mode instead. Has no effect in a WebGL build.</summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
