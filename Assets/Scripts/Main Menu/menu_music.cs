using UnityEngine;

/// <summary>
/// Keeps menu music playing while moving between the main menu and character
/// selection without creating duplicate AudioSources.
/// </summary>
public sealed class MenuMusic : MonoBehaviour
{
    private static MenuMusic instance;
    private AudioSource audioSource;

    public static void Play(AudioClip clip, float volume)
    {
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

        instance.audioSource.volume = volume;

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
