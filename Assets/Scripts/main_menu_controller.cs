using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject leafIntro;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playOptions;
    [SerializeField] private GameObject settingsPanel;

    private bool introFinished = false;

    private void Start()
    {
        // Starting screen
        leafIntro.SetActive(true);

        mainMenuPanel.SetActive(false);
        playOptions.SetActive(false);
        settingsPanel.SetActive(false);
    }

    private void Update()
    {
        // Click anywhere to remove leaves and reveal main menu
        if (!introFinished &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShowMainMenu();
        }
    }

    private void ShowMainMenu()
    {
        introFinished = true;

        // Hide leaves
        leafIntro.SetActive(false);

        // Show main menu
        mainMenuPanel.SetActive(true);

        // Keep these hidden initially
        playOptions.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void PlayClicked()
    {
        // Toggle Local / Online buttons
        playOptions.SetActive(!playOptions.activeSelf);
    }

    public void LocalClicked()
    {
        // Load the fighting scene
        SceneManager.LoadScene("CharacterTest");
    }

    public void SettingsClicked()
    {
        // Hide main menu
        mainMenuPanel.SetActive(false);

        // Show settings
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        // Hide settings
        settingsPanel.SetActive(false);

        // Return to main menu
        mainMenuPanel.SetActive(true);

        // Close play options
        playOptions.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}