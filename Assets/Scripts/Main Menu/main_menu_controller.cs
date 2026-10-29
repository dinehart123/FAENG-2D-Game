using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    // Controls the intro animation, menu panels, music, and navigation.
    public static bool SkipIntroOnNextLoad { get; set; }

    [Header("Menu Panels")]
    [SerializeField] private GameObject leafIntro;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playOptions;
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings UI")]
    [SerializeField] private TMP_Text musicToggleText;
    [SerializeField] private Slider musicVolumeSlider;

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField, Range(0f, 1f)] private float backgroundMusicVolume = 0.5f;

    [Header("Button Sound")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip[] buttonClickSounds;
    [SerializeField, Range(0f, 1f)] private float buttonClickVolume = 1f;

    [Header("Intro Animation")]
    [SerializeField] private RectTransform leavesImage;
    [SerializeField] private RectTransform topLeavesImage;

    [SerializeField] private GameObject clickToStart;

    [SerializeField] private float leafMoveDistance = 800f;
    [SerializeField] private float animationDuration = 2f;

    private bool introFinished = false;
    private Vector2 leavesStartPosition;
    private Vector2 topLeavesStartPosition;

    private void Start()
    {
        // Initialize the menu, music settings, and leaf positions.
        MenuMusic.Play(backgroundMusic, backgroundMusicVolume);

        leafIntro.SetActive(true);

        mainMenuPanel.SetActive(false);
        playOptions.SetActive(false);
        settingsPanel.SetActive(false);

        UpdateMusicToggleText();

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(MenuMusic.MusicVolume);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        leavesStartPosition = leavesImage.anchoredPosition;
        topLeavesStartPosition = topLeavesImage.anchoredPosition;

        bool skipIntro = SkipIntroOnNextLoad;
        SkipIntroOnNextLoad = false;

        if (skipIntro)
        {
            OpenMainMenuImmediately();
        }
    }

    private void Update()
    {
        // Detect intro clicks and the back button each frame.
        if (!introFinished && 
        ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
        (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)))
        {
            StartCoroutine(ShowMainMenu());
        }

        // Back button - only works after intro
        if (introFinished &&
            ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)))
        {
            GoBack();
        }
    }

    private void GoBack()
    {
        // Close the currently open submenu.
        // If Settings is open -> return to main menu
        if (settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            playOptions.SetActive(false);
            return;
        }

        // If Local/Online options are open -> close them
        if (playOptions.activeSelf)
        {
            playOptions.SetActive(false);
            return;
        }

        // Already at the main menu -> do nothing
    }

    private IEnumerator ShowMainMenu()
    {
        // Animate the leaves away and reveal the main menu.
        introFinished = true;

        // Hide the prompt immediately when the intro animation begins
        clickToStart.SetActive(false);


        Vector2 bottomStartPosition = leavesStartPosition;
        Vector2 topStartPosition = topLeavesStartPosition;

        // Bottom leaves move DOWN
        Vector2 bottomEndPosition =
            bottomStartPosition + Vector2.down * leafMoveDistance;

        // Top leaves move UP
        Vector2 topEndPosition =
            topStartPosition + Vector2.up * leafMoveDistance;

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / animationDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            // Bottom leaves move down
            leavesImage.anchoredPosition =
                Vector2.Lerp(bottomStartPosition, bottomEndPosition, t);

            // Top leaves move up
            topLeavesImage.anchoredPosition =
                Vector2.Lerp(topStartPosition, topEndPosition, t);

            yield return null;
        }

        leavesImage.anchoredPosition = bottomEndPosition;
        topLeavesImage.anchoredPosition = topEndPosition;

        leafIntro.SetActive(false);

        // The menu is already visible behind the leaves as they move away
        mainMenuPanel.SetActive(true);
        // mainMenuCanvasGroup.alpha = 1f;

        playOptions.SetActive(false);
        settingsPanel.SetActive(false);
    }

    private void OpenMainMenuImmediately()
    {
        // Skip the intro animation when returning from another scene.
        introFinished = true;
        clickToStart.SetActive(false);

        leavesImage.anchoredPosition =
            leavesStartPosition + Vector2.down * leafMoveDistance;
        topLeavesImage.anchoredPosition =
            topLeavesStartPosition + Vector2.up * leafMoveDistance;

        leafIntro.SetActive(false);
        mainMenuPanel.SetActive(true);
        playOptions.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void PlayClicked()
    {
        // Toggle the local/online play options.
        PlayButtonSound();
        playOptions.SetActive(!playOptions.activeSelf);
    }

    public void LocalClicked()
    {
        // Open the local character-selection scene.
        PlayButtonSound();
        SceneManager.LoadScene("Character SelectV2");
    }

    public void SettingsClicked()
    {
        // Replace the main menu with the settings panel.
        PlayButtonSound();
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        // Close settings and restore the main menu.
        PlayButtonSound();
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        playOptions.SetActive(false);
    }

    public void ToggleMusic()
    {
        // Toggle music and update the button label.
        PlayButtonSound();
        bool musicEnabled = MenuMusic.ToggleMusic();

        // Explicitly restart the assigned track when music is turned back on.
        // This also works if the AudioSource was stopped by an older version
        // of the music script.
        if (musicEnabled)
        {
            MenuMusic.Play(backgroundMusic, backgroundMusicVolume);
        }

        UpdateMusicToggleText();
    }

    public void SetMusicVolume(float value)
    {
        // Save the selected music volume.
        MenuMusic.SetVolume(value);
    }

    private void UpdateMusicToggleText()
    {
        if (musicToggleText != null)
        {
            musicToggleText.text = MenuMusic.MusicEnabled
                ? "MUSIC: ON"
                : "MUSIC: OFF";
        }
    }

    public void QuitGame()
    {
        // Close the game application.
        PlayButtonSound();
        Application.Quit();
    }

    private void PlayButtonSound()
    {
        MenuButtonSound.Play(
            buttonClickSounds,
            buttonClickSound,
            buttonClickVolume
        );
    }
}
