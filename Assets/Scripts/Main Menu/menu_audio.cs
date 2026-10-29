using UnityEngine;

public sealed class MenuMusic : MonoBehaviour
{
    // Maintains one looping menu-music source across scene changes.
    private const string MusicVolumeKey = "MenuMusicVolume";
    private static MenuMusic instance;
    private AudioSource audioSource;
    private static bool musicEnabled = true;

    public static bool MusicEnabled => musicEnabled;

    public static float MusicVolume =>
        Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 0.5f));

    public static void Play(AudioClip clip, float volume)
    {
        // Start or resume the requested track using the saved volume.
        if (!PlayerPrefs.HasKey(MusicVolumeKey))
        {
            SetVolume(volume);
        }

        if (!MusicEnabled)
        {
            if (instance != null && instance.audioSource != null)
            {
                instance.audioSource.volume = 0f;
            }

            return;
        }

        if (clip == null)
        {
            return;
        }

        CreateInstanceIfNeeded();

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
        // Switch music between enabled and muted states.
        SetMusicEnabled(!MusicEnabled);
        return MusicEnabled;
    }

    public static void SetMusicEnabled(bool enabled)
    {
        // Apply the music toggle to the persistent audio source.
        musicEnabled = enabled;

        if (instance == null || instance.audioSource == null)
        {
            return;
        }

        if (!enabled)
        {
            instance.audioSource.volume = 0f;
        }
        else
        {
            instance.audioSource.volume = MusicVolume;
            instance.audioSource.loop = true;

            if (instance.audioSource.clip != null &&
                !instance.audioSource.isPlaying)
            {
                instance.audioSource.Play();
            }
        }
    }

    public static void SetVolume(float volume)
    {
        // Clamp and save volume, then update the active source.
        float clampedVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MusicVolumeKey, clampedVolume);
        PlayerPrefs.Save();

        if (instance != null && instance.audioSource != null &&
            MusicEnabled)
        {
            instance.audioSource.volume = clampedVolume;
        }
    }

    public static void Stop()
    {
        // Stop the persistent music source.
        if (instance != null && instance.audioSource != null)
        {
            instance.audioSource.Stop();
        }
    }

    private static void CreateInstanceIfNeeded()
    {
        // Create a music object when no persistent instance exists.
        if (instance != null)
        {
            return;
        }

        GameObject musicObject = new GameObject("Menu Music");
        instance = musicObject.AddComponent<MenuMusic>();
    }

    private void Awake()
    {
        // Keep only one instance and configure its AudioSource.
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
    }
}

public sealed class MenuButtonSound : MonoBehaviour
{
    // Plays menu button sounds from a persistent one-shot AudioSource.
    private static MenuButtonSound instance;
    private AudioSource audioSource;

    private void Awake()
    {
        // Keep one instance and configure the button sound source.
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    public static void Play(
        AudioClip[] clips,
        AudioClip fallbackClip,
        float volume)
    {
        // Choose a random sound or use the fallback clip.
        AudioClip clip = GetRandomClip(clips);

        if (clip == null)
        {
            clip = fallbackClip;
        }

        if (clip == null)
        {
            return;
        }

        if (instance == null)
        {
            GameObject soundObject = new GameObject("Menu Button Sound");
            instance = soundObject.AddComponent<MenuButtonSound>();
        }

        if (instance.audioSource != null)
        {
            instance.audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }
    }

    private static AudioClip GetRandomClip(AudioClip[] clips)
    {
        // Select randomly from only the non-null clips.
        if (clips == null || clips.Length == 0)
        {
            return null;
        }

        int validClipCount = 0;

        foreach (AudioClip clip in clips)
        {
            if (clip != null)
            {
                validClipCount++;
            }
        }

        if (validClipCount == 0)
        {
            return null;
        }

        int selectedClip = Random.Range(0, validClipCount);
        int validIndex = 0;

        foreach (AudioClip clip in clips)
        {
            if (clip == null)
            {
                continue;
            }

            if (validIndex == selectedClip)
            {
                return clip;
            }

            validIndex++;
        }

        return null;
    }
}
