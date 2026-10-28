using UnityEngine;

/// <summary>
/// Keeps menu music playing while moving between the main menu and character
/// selection without creating duplicate AudioSources.
/// </summary>
public sealed class MenuMusic : MonoBehaviour
{
    private const string MusicEnabledKey = "MenuMusicEnabled";
    private const string MusicVolumeKey = "MenuMusicVolume";
    private static MenuMusic instance;
    private AudioSource audioSource;

    public static bool MusicEnabled =>
        PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;

    public static float MusicVolume =>
        Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 0.5f));

    public static void Play(AudioClip clip, float volume)
    {
        if (!PlayerPrefs.HasKey(MusicVolumeKey))
        {
            SetVolume(volume);
        }

        if (!MusicEnabled)
        {
            if (instance != null && instance.audioSource != null)
            {
                instance.audioSource.Stop();
            }

            return;
        }

        if (clip == null)
        {
            if (instance == null || instance.audioSource == null ||
                !instance.audioSource.isPlaying)
            {
                Debug.LogWarning("Menu music is not assigned.");
            }

            return;
        }

        if (instance == null)
        {
            GameObject musicObject = new GameObject("Menu Music");
            instance = musicObject.AddComponent<MenuMusic>();
            DontDestroyOnLoad(musicObject);
        }

        instance.audioSource.volume = MusicVolume;

        if (instance.audioSource.clip != clip)
        {
            instance.audioSource.clip = clip;
            instance.audioSource.loop = true;
            instance.audioSource.Play();
        }
        else if (!instance.audioSource.isPlaying)
        {
            instance.audioSource.Play();
        }
    }

    public static bool ToggleMusic()
    {
        SetMusicEnabled(!MusicEnabled);
        return MusicEnabled;
    }

    public static void SetMusicEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(MusicEnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (!enabled && instance != null && instance.audioSource != null)
        {
            instance.audioSource.Stop();
        }
    }

    public static void SetVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MusicVolumeKey, clampedVolume);
        PlayerPrefs.Save();

        if (instance != null && instance.audioSource != null)
        {
            instance.audioSource.volume = clampedVolume;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
    }
}
