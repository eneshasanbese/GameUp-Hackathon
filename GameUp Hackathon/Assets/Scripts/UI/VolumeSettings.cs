using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Music and sound-effect volume (0–1) through an Audio Mixer, saved between sessions.
/// Put it on an object that is always active (not inside the hidden settings panel),
/// so the saved volumes are applied when the scene starts.
/// </summary>
public class VolumeSettings : MonoBehaviour
{
    // Also the names of the exposed mixer parameters.
    public const string MusicParameter = "MusicVolume";
    public const string SfxParameter = "SfxVolume";

    [SerializeField] private AudioMixer mixer;
    [Tooltip("Mixer group that sound effects (Sfx) play through.")]
    [SerializeField] private AudioMixerGroup sfxGroup;
    [Tooltip("Optional. Range 0–1.")]
    [SerializeField] private Slider musicSlider;
    [Tooltip("Optional. Range 0–1.")]
    [SerializeField] private Slider sfxSlider;

    // AudioMixer.SetFloat is ignored in Awake, so the values are applied in Start.
    private void Start()
    {
        if (sfxGroup != null)
            Sfx.Output = sfxGroup;

        float music = PlayerPrefs.GetFloat(MusicParameter, 1f);
        float sfx = PlayerPrefs.GetFloat(SfxParameter, 1f);
        Apply(MusicParameter, music);
        Apply(SfxParameter, sfx);

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(music);
            musicSlider.onValueChanged.AddListener(SetMusic);
        }
        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(sfx);
            sfxSlider.onValueChanged.AddListener(SetSfx);
        }
    }

    private void OnDestroy()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(SetMusic);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(SetSfx);
        PlayerPrefs.Save();
    }

    public void SetMusic(float value) => Save(MusicParameter, value);
    public void SetSfx(float value) => Save(SfxParameter, value);

    private void Save(string parameter, float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(parameter, value);
        Apply(parameter, value);
    }

    private void Apply(string parameter, float value)
    {
        if (mixer == null)
            return;
        // Slider is linear (0–1); the mixer works in decibels. 0 means silent (-80 dB).
        float decibels = value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
        if (!mixer.SetFloat(parameter, decibels))
            Debug.LogWarning($"{name}: the mixer has no exposed parameter named '{parameter}'.", this);
    }
}
