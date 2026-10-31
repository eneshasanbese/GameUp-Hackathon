using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Full-screen fade between levels. The screen darkens, the next scene loads
/// behind it (hiding the loading hitch), then the new level fades in.
/// Created on demand from code; nothing needs to be placed in a scene.
/// </summary>
public sealed class SceneFader : MonoBehaviour
{
    private static SceneFader active;

    private CanvasGroup group;

    public static bool IsBusy => active != null;

    // Keeps the static reference clean when domain reload is disabled.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => active = null;

    public static bool FadeAndLoad(int buildIndex, float fadeDuration, Color color) =>
        Begin(() => SceneManager.LoadSceneAsync(buildIndex), fadeDuration, color);

    public static bool FadeAndLoad(string sceneName, float fadeDuration, Color color) =>
        Begin(() => SceneManager.LoadSceneAsync(sceneName), fadeDuration, color);

    private static bool Begin(Func<AsyncOperation> startLoad, float fadeDuration, Color color)
    {
        if (active != null)
            return false;
        active = Create(color);
        active.StartCoroutine(active.Run(startLoad, Mathf.Max(0f, fadeDuration)));
        return true;
    }

    private static SceneFader Create(Color color)
    {
        var root = new GameObject("SceneFader");
        DontDestroyOnLoad(root);

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // above the water bar and any other HUD

        var fader = root.AddComponent<SceneFader>();
        fader.group = root.AddComponent<CanvasGroup>();
        fader.group.alpha = 0f;
        fader.group.interactable = false;

        var cover = new GameObject("Cover", typeof(RectTransform), typeof(Image));
        cover.transform.SetParent(root.transform, false);
        var rect = (RectTransform)cover.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        cover.GetComponent<Image>().color = color;
        return fader;
    }

    private IEnumerator Run(Func<AsyncOperation> startLoad, float duration)
    {
        yield return Fade(0f, 1f, duration);

        AsyncOperation load = startLoad();
        if (load != null)
        {
            while (!load.isDone)
                yield return null;
            // Let the new level run its first frames (Awake/Start, collider setup) while still covered.
            yield return null;
            yield return null;
        }

        yield return Fade(1f, 0f, duration);
        Destroy(gameObject);
    }

    // Unscaled time, so the fade also works if the game is paused.
    private IEnumerator Fade(float from, float to, float duration)
    {
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.SmoothStep(from, to, elapsed / duration);
            yield return null;
        }
        group.alpha = to;
    }

    private void OnDestroy()
    {
        if (active == this)
            active = null;
    }
}
