using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectManager : MonoBehaviour
{
    // These are world-space positions. Do not use the UI RectTransform's
    // anchored position for the animated clones, because that can be in
    // Canvas coordinates and place the clone far away from the arena.
    private static readonly Vector3 Player1PreviewWorldPosition =
        new Vector3(-5f, -3f, 0f);
    private static readonly Vector3 Player2PreviewWorldPosition =
        new Vector3(5f, -3f, 0f);
    private static readonly Quaternion Player2PreviewWorldRotation =
        Quaternion.Euler(0f, 180f, 0f);
    private static readonly Vector3 PreviewWorldScale =
        new Vector3(1.5f, 1.5f, 1.5f);

    [Header("Roster")]
    [SerializeField] private CharacterRoster roster;

    [Header("Scene")]
    [SerializeField] private string arenaSceneName = "Arena";

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField, Range(0f, 1f)] private float backgroundMusicVolume = 0.5f;

    [Header("Optional UI")]
    [SerializeField] private Image player1Preview;
    [SerializeField] private Image player2Preview;
    [SerializeField] private TMP_Text player1NameText;
    [SerializeField] private TMP_Text player2NameText;
    [SerializeField] private TMP_Text player1StatusText;
    [SerializeField] private TMP_Text player2StatusText;

    [Header("Character Board")]
    [SerializeField] private Transform characterBoardParent;
    [SerializeField] private Vector2 boardCellSize = new Vector2(130f, 130f);
    [SerializeField] private int boardColumnCount = 4;
    [SerializeField] private Button[] characterButtons;
    [SerializeField] private Image[] characterButtonIcons;
    [SerializeField] private GameObject[] player1SelectionMarkers;
    [SerializeField] private GameObject[] player2SelectionMarkers;

    [Header("Animated Character Previews")]
    [SerializeField] private Transform player1PreviewSpawn;
    [SerializeField] private Transform player2PreviewSpawn;

    [Header("Input")]
    [SerializeField] private float moveCooldown = 0.2f;

    private bool player1Joined;
    private bool player2Joined;
    private bool player1CharacterSelected;
    private bool player2CharacterSelected;
    private bool player1Ready;
    private bool player2Ready;
    private bool loadingArena;

    private int player1CharacterIndex;
    private int player2CharacterIndex;
    private float player1MoveCooldown;
    private float player2MoveCooldown;

    private GameObject player1PreviewObject;
    private GameObject player2PreviewObject;

    private void Start()
    {
        MenuMusic.Play(backgroundMusic, backgroundMusicVolume);

        PlayerSelectionData.ResetSelections();

        // Player 1 is always reserved for the keyboard and mouse.
        player1Joined = true;
        PlayerSelectionData.Player1ControlMode =
            FighterController.ControlMode.Keyboard;

        player1CharacterIndex = 0;
        player2CharacterIndex = 0;
        EnsureCharacterBoardParent();
        SetupCharacterBoard();
        RefreshUI();
    }

    private void Update()
    {
        if (loadingArena) return;

        player1MoveCooldown -= Time.deltaTime;
        player2MoveCooldown -= Time.deltaTime;

        CheckForPlayersJoining();

        if (player1Joined)
            HandlePlayer1Selection();

        if (player2Joined)
            HandlePlayer2Selection();

        RefreshUI();

        if (player1Ready && player2Ready &&
            PlayerSelectionData.Player1Character != null &&
            PlayerSelectionData.Player2Character != null)
        {
            StartGame();
        }
    }

    private void CheckForPlayersJoining()
    {
        foreach (Gamepad gamepad in Gamepad.all)
        {
            if (!ControllerPressedThisFrame(gamepad)) continue;

            if (gamepad == PlayerSelectionData.Player1Gamepad ||
                gamepad == PlayerSelectionData.Player2Gamepad)
            {
                continue;
            }

            if (!player2Joined)
            {
                player2Joined = true;
                PlayerSelectionData.Player2ControlMode =
                    FighterController.ControlMode.Controller;
                PlayerSelectionData.Player2Gamepad = gamepad;
                Debug.Log("Player 2 joined with Controller.");
                return;
            }
        }
    }

    private bool ControllerPressedThisFrame(Gamepad gamepad)
    {
        return gamepad.buttonSouth.wasPressedThisFrame ||
               gamepad.buttonEast.wasPressedThisFrame ||
               gamepad.buttonWest.wasPressedThisFrame ||
               gamepad.buttonNorth.wasPressedThisFrame ||
               gamepad.startButton.wasPressedThisFrame ||
               gamepad.selectButton.wasPressedThisFrame ||
               gamepad.leftShoulder.wasPressedThisFrame ||
               gamepad.rightShoulder.wasPressedThisFrame ||
               gamepad.dpad.up.wasPressedThisFrame ||
               gamepad.dpad.down.wasPressedThisFrame ||
               gamepad.dpad.left.wasPressedThisFrame ||
               gamepad.dpad.right.wasPressedThisFrame;
    }

    private void HandlePlayer1Selection()
    {
        HandleKeyboardSelection(
            ref player1CharacterIndex,
            ref player1Ready,
            ref player1MoveCooldown,
            1);
    }

    private void HandlePlayer2Selection()
    {
        HandleControllerSelection(
            PlayerSelectionData.Player2Gamepad,
            ref player2CharacterIndex,
            ref player2Ready,
            ref player2MoveCooldown,
            2);
    }

    private void HandleKeyboardSelection(
        ref int characterIndex,
        ref bool ready,
        ref float cooldown,
        int playerNumber)
    {
        if (Keyboard.current == null) return;

        if (ready)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelReady(playerNumber, ref ready);
            }

            return;
        }

        if (cooldown <= 0f)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                ChangeCharacter(ref characterIndex, -1);
                cooldown = moveCooldown;
            }
            else if (Keyboard.current.dKey.wasPressedThisFrame)
            {
                ChangeCharacter(ref characterIndex, 1);
                cooldown = moveCooldown;
            }
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ConfirmReady(playerNumber, characterIndex, ref ready);
        }
    }

    private void HandleControllerSelection(
        Gamepad gamepad,
        ref int characterIndex,
        ref bool ready,
        ref float cooldown,
        int playerNumber)
    {
        if (gamepad == null) return;

        if (ready)
        {
            if (gamepad.buttonEast.wasPressedThisFrame)
            {
                CancelReady(playerNumber, ref ready);
            }

            return;
        }

        if (cooldown <= 0f)
        {
            float horizontal = gamepad.leftStick.ReadValue().x;

            if (horizontal < -0.5f)
            {
                ChangeCharacter(ref characterIndex, -1);
                cooldown = moveCooldown;
            }
            else if (horizontal > 0.5f)
            {
                ChangeCharacter(ref characterIndex, 1);
                cooldown = moveCooldown;
            }
        }

        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            ConfirmReady(playerNumber, characterIndex, ref ready);
        }
    }

    private void ConfirmReady(int playerNumber, int characterIndex, ref bool ready)
    {
        if (playerNumber == 1 && !player1CharacterSelected)
        {
            Debug.Log("Player 1 must click a character before readying up.");
            return;
        }

        SelectCharacter(playerNumber, characterIndex);
        ready = true;
        RefreshAnimatedPreviews();
        Debug.Log("Player " + playerNumber + " is ready.");
    }

    private void CancelReady(int playerNumber, ref bool ready)
    {
        ready = false;

        Debug.Log("Player " + playerNumber + " is no longer ready.");
    }

    private void ChangeCharacter(ref int index, int direction)
    {
        if (roster == null || roster.characters == null ||
            roster.characters.Length == 0)
        {
            Debug.LogError("CharacterSelectManager has no roster assigned.");
            return;
        }

        index += direction;

        if (index < 0)
            index = roster.characters.Length - 1;
        else if (index >= roster.characters.Length)
            index = 0;

        RefreshUI();
    }

    private void SelectCharacter(int playerNumber, int index)
    {
        if (roster == null || roster.characters == null ||
            roster.characters.Length == 0)
        {
            return;
        }

        CharacterData character = roster.characters[index];

        if (character == null)
        {
            return;
        }

        if (playerNumber == 1)
        {
            PlayerSelectionData.Player1Character = character;
            player1CharacterSelected = true;
        }
        else
        {
            PlayerSelectionData.Player2Character = character;
            player2CharacterSelected = true;
        }

        Debug.Log("Player " + playerNumber + " selected " +
                  character.characterName + ".");
    }

    private void SetupCharacterBoard()
    {
        if (roster == null || roster.characters == null)
        {
            return;
        }

        if ((characterButtons == null || characterButtons.Length == 0) &&
            characterBoardParent != null)
        {
            CreateCharacterBoardAutomatically();
            return;
        }

        int count = Mathf.Min(roster.characters.Length,
                              characterButtons == null ? 0 : characterButtons.Length);

        for (int i = 0; i < count; i++)
        {
            int characterIndex = i;

            if (characterButtons[i] != null)
            {
                characterButtons[i].onClick.AddListener(
                    () => SelectCharacterFromBoard(characterIndex));
            }

            if (characterButtonIcons != null &&
                i < characterButtonIcons.Length &&
                characterButtonIcons[i] != null)
            {
                characterButtonIcons[i].sprite =
                    roster.characters[i].characterIcon;
                characterButtonIcons[i].preserveAspect = true;
            }
        }
    }

    private void EnsureCharacterBoardParent()
    {
        if (characterBoardParent != null)
        {
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("CharacterSelectManager could not find a Canvas.");
            return;
        }

        GameObject board = new GameObject("Character Board");
        board.transform.SetParent(canvas.transform, false);

        RectTransform boardRect = board.AddComponent<RectTransform>();
        boardRect.anchorMin = new Vector2(0.5f, 1f);
        boardRect.anchorMax = new Vector2(0.5f, 1f);
        boardRect.pivot = new Vector2(0.5f, 1f);
        boardRect.anchoredPosition = new Vector2(0f, -100f);
        boardRect.sizeDelta = new Vector2(620f, 180f);

        Image boardBackground = board.AddComponent<Image>();
        boardBackground.color = new Color(0.02f, 0.02f, 0.02f, 0.65f);

        characterBoardParent = board.transform;
    }

    private void CreateCharacterBoardAutomatically()
    {
        GridLayoutGroup grid =
            characterBoardParent.GetComponent<GridLayoutGroup>();

        if (grid == null)
        {
            grid = characterBoardParent.gameObject.AddComponent<GridLayoutGroup>();
        }

        grid.cellSize = boardCellSize;
        grid.spacing = new Vector2(12f, 12f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Max(1, boardColumnCount);
        grid.childAlignment = TextAnchor.MiddleCenter;

        List<Button> buttons = new List<Button>();
        List<Image> icons = new List<Image>();
        List<GameObject> p1Markers = new List<GameObject>();
        List<GameObject> p2Markers = new List<GameObject>();

        for (int i = 0; i < roster.characters.Length; i++)
        {
            CharacterData character = roster.characters[i];

            if (character == null)
            {
                continue;
            }

            GameObject buttonObject = new GameObject(
                character.characterName + " Button");
            buttonObject.transform.SetParent(characterBoardParent, false);

            Image background = buttonObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = background;

            GameObject iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(buttonObject.transform, false);

            RectTransform iconRect = iconObject.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(8f, 8f);
            iconRect.offsetMax = new Vector2(-8f, -8f);

            Image icon = iconObject.AddComponent<Image>();
            icon.sprite = character.characterIcon;
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            GameObject p1Marker = CreateSelectionMarker(
                buttonObject.transform,
                "P1 Selection",
                new Color(1f, 0.8f, 0.1f, 0.35f));

            GameObject p2Marker = CreateSelectionMarker(
                buttonObject.transform,
                "P2 Selection",
                new Color(0.1f, 0.6f, 1f, 0.35f));

            int characterIndex = i;
            button.onClick.AddListener(
                () => SelectCharacterFromBoard(characterIndex));

            buttons.Add(button);
            icons.Add(icon);
            p1Markers.Add(p1Marker);
            p2Markers.Add(p2Marker);
        }

        characterButtons = buttons.ToArray();
        characterButtonIcons = icons.ToArray();
        player1SelectionMarkers = p1Markers.ToArray();
        player2SelectionMarkers = p2Markers.ToArray();
    }

    private GameObject CreateSelectionMarker(
        Transform parent,
        string markerName,
        Color markerColor)
    {
        GameObject markerObject = new GameObject(markerName);
        markerObject.transform.SetParent(parent, false);

        RectTransform markerRect = markerObject.AddComponent<RectTransform>();
        markerRect.anchorMin = Vector2.zero;
        markerRect.anchorMax = Vector2.one;
        markerRect.offsetMin = Vector2.zero;
        markerRect.offsetMax = Vector2.zero;

        Image markerImage = markerObject.AddComponent<Image>();
        markerImage.color = markerColor;
        markerImage.raycastTarget = false;
        markerObject.SetActive(false);

        return markerObject;
    }

    private void SelectCharacterFromBoard(int characterIndex)
    {
        if (!player1Joined || player1Ready)
        {
            return;
        }

        if (roster == null || roster.characters == null ||
            characterIndex < 0 || characterIndex >= roster.characters.Length)
        {
            return;
        }

        player1CharacterIndex = characterIndex;
        SelectCharacter(1, characterIndex);
        RefreshUI();
        RefreshAnimatedPreviews();
    }

    private void UpdateSelectionMarkers()
    {
        SetMarker(
            player1SelectionMarkers,
            player1CharacterSelected,
            player1CharacterIndex);

        SetMarker(
            player2SelectionMarkers,
            player2CharacterSelected,
            player2CharacterIndex);
    }

    private void SetMarker(GameObject[] markers, bool playerJoined, int index)
    {
        if (markers == null)
        {
            return;
        }

        for (int i = 0; i < markers.Length; i++)
        {
            if (markers[i] != null)
            {
                markers[i].SetActive(playerJoined && i == index);
            }
        }
    }

    private void RefreshUI()
    {
        if (roster == null || roster.characters == null ||
            roster.characters.Length == 0)
        {
            return;
        }

        CharacterData player1Character = GetCharacter(player1CharacterIndex);
        CharacterData player2Character = GetCharacter(player2CharacterIndex);

        SetPreview(
            player1Preview,
            player1CharacterSelected ? player1Character : null);
        SetPreview(
            player2Preview,
            player2CharacterSelected ? player2Character : null);

        SetText(
            player1NameText,
            player1CharacterSelected && player1Character != null
                ? player1Character.characterName
                : "P1 - Select a Character");
        SetText(
            player2NameText,
            player2CharacterSelected && player2Character != null
                ? player2Character.characterName
                : "P2 - Select a Character");

        SetText(player1StatusText, GetStatusText(1));
        SetText(player2StatusText, GetStatusText(2));

        UpdateSelectionMarkers();
    }

    private void SetPreview(Image preview, CharacterData character)
    {
        if (preview == null) return;

        preview.sprite = character == null ? null : character.characterIcon;
        preview.enabled = character != null && character.characterIcon != null;
        preview.preserveAspect = true;
    }

    private CharacterData GetCharacter(int index)
    {
        if (roster == null || roster.characters == null ||
            index < 0 || index >= roster.characters.Length)
        {
            return null;
        }

        return roster.characters[index];
    }

    private void RefreshAnimatedPreviews()
    {
        if (roster == null || roster.characters == null ||
            roster.characters.Length == 0)
        {
            return;
        }

        SpawnOrRemoveAnimatedPreview(
            ref player1PreviewObject,
            player1CharacterSelected ? GetCharacter(player1CharacterIndex) : null,
            player1PreviewSpawn);

        SpawnOrRemoveAnimatedPreview(
            ref player2PreviewObject,
            player2CharacterSelected ? GetCharacter(player2CharacterIndex) : null,
            player2PreviewSpawn);
    }

    private void SpawnOrRemoveAnimatedPreview(
        ref GameObject currentPreview,
        CharacterData character,
        Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            return;
        }

        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }

        if (character == null)
        {
            return;
        }

        GameObject prefab = character.selectionPreviewPrefab;

        if (prefab == null)
        {
            Debug.LogWarning(
                character.characterName +
                " has no selection preview prefab assigned.");
            return;
        }

        Vector3 previewPosition = spawnPoint == player1PreviewSpawn
            ? Player1PreviewWorldPosition
            : Player2PreviewWorldPosition;
        Quaternion previewRotation = spawnPoint == player1PreviewSpawn
            ? Quaternion.identity
            : Player2PreviewWorldRotation;

        currentPreview = Instantiate(
            prefab,
            previewPosition,
            previewRotation);

        // Set world values explicitly so the prefab cannot inherit an
        // unexpected Canvas/parent offset or a different prefab scale.
        currentPreview.transform.position = previewPosition;
        currentPreview.transform.rotation = previewRotation;
        currentPreview.transform.localScale = PreviewWorldScale;

        Animator animator = currentPreview.GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.enabled = true;
            animator.speed = 1f;
            animator.Rebind();

            // Force preview characters into the grounded idle condition.
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsGrounded", true);
            animator.SetBool("IsFalling", false);
            animator.ResetTrigger("Light");
            animator.ResetTrigger("Heavy");
            animator.ResetTrigger("Jump");
            animator.ResetTrigger("Up");
            animator.ResetTrigger("Down");
            animator.ResetTrigger("Neutral");
            animator.ResetTrigger("Forward");
            animator.ResetTrigger("Special");

            animator.Update(0f);

            if (!string.IsNullOrEmpty(character.previewIdleStateName))
            {
                animator.Play(character.previewIdleStateName, 0, 0f);
                animator.Update(0f);
            }
        }
        else
        {
            Debug.LogWarning(
                character.characterName +
                " selection preview has no Animator component.");
        }
    }

    private void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    private string GetStatusText(int playerNumber)
    {
        if (playerNumber == 1)
        {
            if (!player1CharacterSelected)
                return "Select a character";
            if (player1Ready) return "READY";
            return "UNREADY";
        }

        if (!player2Joined) return "Press to join";
        if (player2Ready) return "READY";
        return "UNREADY";
    }

    private void StartGame()
    {
        loadingArena = true;

        if (player1PreviewObject != null)
            Destroy(player1PreviewObject);

        if (player2PreviewObject != null)
            Destroy(player2PreviewObject);

        SceneManager.LoadScene(arenaSceneName);
    }
}
