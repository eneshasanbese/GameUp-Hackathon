using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Shows story paragraphs one at a time: each fades in, stays, then fades out.
/// A click, any key or the gamepad's south button skips ahead. Calls On Finished at the end,
/// e.g. to load Level_1 (intro) or to show the replay/quit buttons (ending).
/// </summary>
public class StoryPlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField, TextArea(3, 8)] private string[] paragraphs;

    [Header("Playback")]
    [Tooltip("Starts by itself when the scene opens (intro). Turn off for the ending and call Play() instead.")]
    [SerializeField] private bool playOnStart = true;
    [Tooltip("Optional dark panel behind the text; fades in when the story starts.")]
    [SerializeField] private CanvasGroup background;
    [SerializeField, Min(0f)] private float fadeDuration = 0.8f;
    [Tooltip("Minimum time a paragraph stays on screen.")]
    [SerializeField, Min(0f)] private float holdDuration = 2.5f;
    [Tooltip("Extra time per letter, so long paragraphs stay longer.")]
    [SerializeField, Min(0f)] private float secondsPerCharacter = 0.04f;
    [SerializeField] private bool allowSkip = true;

    [Header("After the story")]
    [SerializeField] private UnityEvent onFinished;

    private Coroutine running;
    private bool skipRequested;

    private void Awake()
    {
        if (text != null)
            text.alpha = 0f;
        if (background != null && !playOnStart)
            background.alpha = 0f;
    }

    private void Start()
    {
        if (playOnStart)
            Play();
    }

    /// <summary>Starts the story from the first paragraph.</summary>
    public void Play()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        if (running != null)
            StopCoroutine(running);
        running = StartCoroutine(Run());
    }

    private void Update()
    {
        if (allowSkip && running != null && SkipPressed())
            skipRequested = true;
    }

    // Unscaled time, so the story also plays if the game is paused.
    private IEnumerator Run()
    {
        if (background != null && background.alpha < 1f)
            yield return Fade(a => background.alpha = a, background.alpha, 1f, false);

        if (text != null && paragraphs != null)
        {
            foreach (string paragraph in paragraphs)
            {
                if (string.IsNullOrWhiteSpace(paragraph))
                    continue;
                text.text = paragraph;
                skipRequested = false;
                yield return Fade(a => text.alpha = a, 0f, 1f, true);

                float hold = holdDuration + paragraph.Length * secondsPerCharacter;
                skipRequested = false;
                for (float t = 0f; t < hold && !skipRequested; t += Time.unscaledDeltaTime)
                    yield return null;

                skipRequested = false;
                yield return Fade(a => text.alpha = a, 1f, 0f, true);
            }
        }

        running = null;
        onFinished?.Invoke();
    }

    private IEnumerator Fade(System.Action<float> apply, float from, float to, bool skippable)
    {
        for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            if (skippable && skipRequested)
                break;
            apply(Mathf.Lerp(from, to, t / fadeDuration));
            yield return null;
        }
        apply(to);
    }

    private static bool SkipPressed()
    {
        return (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
               (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
               (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) ||
               (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);
    }
}
