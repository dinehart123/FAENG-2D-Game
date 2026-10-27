using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject leafIntro;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playOptions;
    [SerializeField] private GameObject settingsPanel;

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
        leafIntro.SetActive(true);

        mainMenuPanel.SetActive(false);
        playOptions.SetActive(false);
        settingsPanel.SetActive(false);

        leavesStartPosition = leavesImage.anchoredPosition;
        topLeavesStartPosition = topLeavesImage.anchoredPosition;
    }

    private void Update()
    {
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

    public void PlayClicked()
    {
        playOptions.SetActive(!playOptions.activeSelf);
    }

    public void LocalClicked()
    {
        SceneManager.LoadScene("Character SelectV2");
    }

    public void SettingsClicked()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        playOptions.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}