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
    // [SerializeField] private CanvasGroup mainMenuCanvasGroup;
    [SerializeField] private GameObject clickToStart;

    [SerializeField] private float leafMoveDistance = 800f;
    [SerializeField] private float animationDuration = 2f;

    private bool introFinished = false;
    private Vector2 leavesStartPosition;

    private void Start()
    {
        leafIntro.SetActive(true);

        mainMenuPanel.SetActive(false);
        playOptions.SetActive(false);
        settingsPanel.SetActive(false);

        leavesStartPosition = leavesImage.anchoredPosition;
    }

    private void Update()
    {
        if (!introFinished &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartCoroutine(ShowMainMenu());
        }
    }

    private IEnumerator ShowMainMenu()
    {
        introFinished = true;

        // Hide the prompt immediately when the intro animation begins
        clickToStart.SetActive(false);

        // The menu is already visible behind the leaves as they move away
        mainMenuPanel.SetActive(true);
        // mainMenuCanvasGroup.alpha = 1f;

        Vector2 startPosition = leavesStartPosition;

        Vector2 endPosition =
            startPosition + Vector2.down * leafMoveDistance;

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / animationDuration;

            // Makes animation start and stop smoothly
            t = Mathf.SmoothStep(0f, 1f, t);

            // Move leaves downward
            leavesImage.anchoredPosition =
                Vector2.Lerp(startPosition, endPosition, t);

            yield return null;
        }

        // Make sure final values are exact
        leavesImage.anchoredPosition = endPosition;
        // mainMenuCanvasGroup.alpha = 1f;

        // Entire intro can now be disabled
        leafIntro.SetActive(false);

        playOptions.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void PlayClicked()
    {
        playOptions.SetActive(!playOptions.activeSelf);
    }

    public void LocalClicked()
    {
        SceneManager.LoadScene("CharacterTest");
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