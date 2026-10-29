using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndGameController : MonoBehaviour
{
    [Header("End Game Text")]
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text playerOneDamageText;
    [SerializeField] private TMP_Text playerTwoDamageText;

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField, Range(0f, 1f)] private float backgroundMusicVolume = 0.5f;

    [Header("Button Sound")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip[] buttonClickSounds;
    [SerializeField, Range(0f, 1f)] private float buttonClickVolume = 1f;

    [Header("Navigation")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string characterSelectSceneName = "Character SelectV2";

    private void Start()
    {
        MenuMusic.Play(backgroundMusic, backgroundMusicVolume);

        if (!MatchResults.HasResults)
        {
            SetText(winnerText, "NO MATCH RESULTS");
            SetText(playerOneDamageText, "PLAYER 1 DAMAGE: 0");
            SetText(playerTwoDamageText, "PLAYER 2 DAMAGE: 0");
            return;
        }

        SetText(winnerText, MatchResults.WinnerName + " WINS!");
        SetText(
            playerOneDamageText,
            "PLAYER 1\nDAMAGE: " +
            MatchResults.PlayerOneDamageDealt.ToString("0.0")
        );
        SetText(
            playerTwoDamageText,
            "PLAYER 2\nDAMAGE: " +
            MatchResults.PlayerTwoDamageDealt.ToString("0.0")
        );
    }

    public void GoToCharacterSelect()
    {
        PlayButtonSound();
        ResetMatchState();
        SceneManager.LoadScene(characterSelectSceneName);
    }

    public void GoToMainMenu()
    {
        PlayButtonSound();
        ResetMatchState();
        MainMenuController.SkipIntroOnNextLoad = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Keep these method names available if buttons were already wired to them.
    public void Rematch()
    {
        GoToCharacterSelect();
    }

    public void ReturnToMainMenu()
    {
        GoToMainMenu();
    }

    private void ResetMatchState()
    {
        // Prevent a paused game from opening the next scene paused.
        Time.timeScale = 1f;

        // Clear the previous winner and damage totals. Stocks, health,
        // positions, and character selections are recreated/reset when the
        // next scene loads.
        MatchResults.Clear();
    }

    private void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
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
