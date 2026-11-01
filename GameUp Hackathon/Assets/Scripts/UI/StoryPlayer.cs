using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Shows story paragraphs one at a time: each fades in, stays, then fades out.
/// Pressing the skip key (E by default) moves on to the next paragraph, one paragraph per press.
/// After the last paragraph On Finished is called, e.g. to load Level_1 (intro)
/// or the main menu (ending).
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

    [Header("Skipping")]
    [SerializeField] private bool allowSkip = true;
    [Tooltip("Key that moves on to the next paragraph.")]
    [SerializeField] private Key skipKey = Key.E;
    [Tooltip("A paragraph must be on screen this long before the key works, so one press never skips two paragraphs.")]
    [SerializeField, Min(0f)] private float minShowTime = 0.4f;
    [Tooltip("Fade-out time after the key is pressed.")]
    [SerializeField, Min(0f)] private float skipFadeDuration = 0.25f;
    [Tooltip("Optional hint at the bottom, e.g. \"Devam etmek için E tuşuna basın\". Shown and gently pulsing only while the story plays.")]
    [SerializeField] private TMP_Text skipHint;

    [Header("After the story")]
    [SerializeField] private UnityEvent onFinished;

    private Coroutine running;
    private bool skipRequested;
    private bool acceptingSkip;
    private float paragraphStart;

    private void Awake()
    {
        if (text == null)
            Debug.LogWarning($"{name}: StoryPlayer has no Text assigned; the story will not be shown.", this);
        else
            text.alpha = 0f;
        if (background != null && !playOnStart)
            background.alpha = 0f;
        if (skipHint != null)
            skipHint.gameObject.SetActive(false);
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
        if (skipHint != null)
            skipHint.gameObject.SetActive(allowSkip);
        running = StartCoroutine(Run());
    }

    private void Update()
    {
        if (running == null)
            return;
        if (allowSkip && acceptingSkip && Time.unscaledTime - paragraphStart >= minShowTime && SkipPressed())
            skipRequested = true;
        // Slow pulse between 40% and 90% opacity.
        if (skipHint != null && skipHint.gameObject.activeSelf)
            skipHint.alpha = Mathf.Lerp(0.4f, 0.9f, 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 2.5f));
    }

    // Unscaled time, so the story also plays if the game is paused.
    private IEnumerator Run()
    {
        if (background != null && background.alpha < 1f)
            yield return Fade(a => background.alpha = a, background.alpha, 1f, fadeDuration);

        if (text != null && paragraphs != null)
        {
            foreach (string paragraph in paragraphs)
            {
                if (string.IsNullOrWhiteSpace(paragraph))
                    continue;
                text.text = paragraph;
                text.alpha = 0f;
                paragraphStart = Time.unscaledTime;
                skipRequested = false;
                acceptingSkip = true;

                // Fade in, then hold. The skip key ends both early and moves to the next paragraph.
                for (float t = 0f; t < fadeDuration && !skipRequested; t += Time.unscaledDeltaTime)
                {
                    text.alpha = t / fadeDuration;
                    yield return null;
                }
                if (!skipRequested)
                {
                    text.alpha = 1f;
                    float hold = holdDuration + paragraph.Length * secondsPerCharacter;
                    for (float t = 0f; t < hold && !skipRequested; t += Time.unscaledDeltaTime)
                        yield return null;
                }

                // Fade out, quicker after a press. Presses now wait for the next paragraph.
                acceptingSkip = false;
                float fadeOut = skipRequested ? skipFadeDuration : fadeDuration;
                yield return Fade(a => text.alpha = a, text.alpha, 0f, fadeOut);
            }
        }

        running = null;
        if (skipHint != null)
            skipHint.gameObject.SetActive(false);
        onFinished?.Invoke();
    }

    private static IEnumerator Fade(System.Action<float> apply, float from, float to, float duration)
    {
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            apply(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        apply(to);
    }

    private bool SkipPressed()
    {
        return skipKey != Key.None && Keyboard.current != null && Keyboard.current[skipKey].wasPressedThisFrame;
    }
}
