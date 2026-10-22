using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Roster")]
    [SerializeField] private CharacterRoster roster;

    [Header("Settings")]
    [SerializeField] private string arenaSceneName = "Arena";

    private bool player1Joined;
    private bool player2Joined;

    private bool player1Ready;
    private bool player2Ready;

    private int player1CharacterIndex;
    private int player2CharacterIndex;

    private float player1MoveCooldown;
    private float player2MoveCooldown;

    private const float MoveCooldown = 0.2f;

    private void Start()
    {
        PlayerSelectionData.ResetSelections();

        player1CharacterIndex = 0;
        player2CharacterIndex = 0;
    }

    private void Update()
    {
        player1MoveCooldown -= Time.deltaTime;
        player2MoveCooldown -= Time.deltaTime;

        CheckForPlayersJoining();

        if (player1Joined && !player1Ready)
        {
            HandlePlayer1Selection();
        }

        if (player2Joined && !player2Ready)
        {
            HandlePlayer2Selection();
        }

        if (player1Ready && player2Ready)
        {
            StartGame();
        }
    }

    private void CheckForPlayersJoining()
    {
        // Keyboard joins Player 1
        if (!player1Joined &&
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            player1Joined = true;

            PlayerSelectionData.Player1ControlMode =
                FighterController.ControlMode.Keyboard;

            Debug.Log("Player 1 joined with Keyboard + Mouse");
        }

        // Check every connected controller
        foreach (Gamepad gamepad in Gamepad.all)
        {
            if (!gamepad.buttonSouth.wasPressedThisFrame)
            {
                continue;
            }

            // Don't allow the same controller twice
            if (gamepad == PlayerSelectionData.Player1Gamepad ||
                gamepad == PlayerSelectionData.Player2Gamepad)
            {
                continue;
            }

            if (!player1Joined)
            {
                player1Joined = true;

                PlayerSelectionData.Player1ControlMode =
                    FighterController.ControlMode.Controller;

                PlayerSelectionData.Player1Gamepad = gamepad;

                Debug.Log("Player 1 joined with Controller");

                return;
            }

            if (!player2Joined)
            {
                player2Joined = true;

                PlayerSelectionData.Player2ControlMode =
                    FighterController.ControlMode.Controller;

                PlayerSelectionData.Player2Gamepad = gamepad;

                Debug.Log("Player 2 joined with Controller");

                return;
            }
        }
    }

    private void HandlePlayer1Selection()
    {
        if (PlayerSelectionData.Player1ControlMode ==
            FighterController.ControlMode.Keyboard)
        {
            HandleKeyboardSelection(
                ref player1CharacterIndex,
                ref player1Ready,
                ref player1MoveCooldown,
                1
            );
        }
        else
        {
            HandleControllerSelection(
                PlayerSelectionData.Player1Gamepad,
                ref player1CharacterIndex,
                ref player1Ready,
                ref player1MoveCooldown,
                1
            );
        }
    }

    private void HandlePlayer2Selection()
    {
        if (PlayerSelectionData.Player2ControlMode ==
            FighterController.ControlMode.Keyboard)
        {
            HandleKeyboardSelection(
                ref player2CharacterIndex,
                ref player2Ready,
                ref player2MoveCooldown,
                2
            );
        }
        else
        {
            HandleControllerSelection(
                PlayerSelectionData.Player2Gamepad,
                ref player2CharacterIndex,
                ref player2Ready,
                ref player2MoveCooldown,
                2
            );
        }
    }

    private void HandleKeyboardSelection(
        ref int characterIndex,
        ref bool ready,
        ref float cooldown,
        int playerNumber)
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (cooldown <= 0f)
        {
            if (Keyboard.current.aKey.isPressed)
            {
                ChangeCharacter(ref characterIndex, -1);
                cooldown = MoveCooldown;
            }

            if (Keyboard.current.dKey.isPressed)
            {
                ChangeCharacter(ref characterIndex, 1);
                cooldown = MoveCooldown;
            }
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SelectCharacter(playerNumber, characterIndex);
            ready = true;
        }
    }

    private void HandleControllerSelection(
        Gamepad gamepad,
        ref int characterIndex,
        ref bool ready,
        ref float cooldown,
        int playerNumber)
    {
        if (gamepad == null)
        {
            return;
        }

        if (cooldown <= 0f)
        {
            float horizontal = gamepad.leftStick.ReadValue().x;

            if (horizontal < -0.5f)
            {
                ChangeCharacter(ref characterIndex, -1);
                cooldown = MoveCooldown;
            }

            if (horizontal > 0.5f)
            {
                ChangeCharacter(ref characterIndex, 1);
                cooldown = MoveCooldown;
            }
        }

        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            SelectCharacter(playerNumber, characterIndex);
            ready = true;
        }
    }

    private void ChangeCharacter(ref int index, int direction)
    {
        if (roster == null ||
            roster.characters == null ||
            roster.characters.Length == 0)
        {
            return;
        }

        index += direction;

        if (index < 0)
        {
            index = roster.characters.Length - 1;
        }

        if (index >= roster.characters.Length)
        {
            index = 0;
        }

        Debug.Log(
            "Highlighted: " +
            roster.characters[index].characterName
        );
    }

    private void SelectCharacter(int playerNumber, int index)
    {
        if (roster == null ||
            roster.characters == null ||
            roster.characters.Length == 0)
        {
            return;
        }

        CharacterData character = roster.characters[index];

        if (playerNumber == 1)
        {
            PlayerSelectionData.Player1Character = character;
        }
        else
        {
            PlayerSelectionData.Player2Character = character;
        }

        Debug.Log(
            "Player " +
            playerNumber +
            " selected " +
            character.characterName
        );
    }

    private void StartGame()
    {
        SceneManager.LoadScene(arenaSceneName);
    }
}