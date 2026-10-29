using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class StockManager : MonoBehaviour
{
    // Spawns fighters, tracks stocks, respawns players, and ends the match.
    [Header("Players")]
    [SerializeField] private FighterHealth playerOne;
    [SerializeField] private FighterHealth playerTwo;

    [Header("Spawn Points")]
    [SerializeField] private Transform playerOneSpawn;
    [SerializeField] private Transform playerTwoSpawn;

    [Header("Stock Settings")]
    [SerializeField] private int startingStocks = 3;
    [SerializeField] private float blastZoneY = -18f;
    [SerializeField] private float respawnDelay = 1f;
    [SerializeField] private float respawnFloatDuration = 5f;

    [Header("End Game")]
    [SerializeField] private string endGameSceneName = "EndGame";

    [Header("Death Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathSound;

    private int playerOneStocks;
    private int playerTwoStocks;
    private bool playerOneRespawning;
    private bool playerTwoRespawning;
    private bool playerOneEliminated;
    private bool playerTwoEliminated;
    private bool matchEnded;

    private void SpawnSelectedPlayers()
    {
        // Replace scene placeholders with the players chosen in character select.
        FighterHealth scenePlayerOne = playerOne;
        FighterHealth scenePlayerTwo = playerTwo;

        CharacterData selectedPlayerOne =
            PlayerSelectionData.Player1Character;
        CharacterData selectedPlayerTwo =
            PlayerSelectionData.Player2Character;

        // This fallback lets you open Arena directly from the editor while
        // still using the players placed in the scene for testing.
        if (selectedPlayerOne == null || selectedPlayerTwo == null)
        {
            if (scenePlayerOne != null)
            {
                scenePlayerOne.gameObject.SetActive(true);
            }

            if (scenePlayerTwo != null)
            {
                scenePlayerTwo.gameObject.SetActive(true);
            }

            Debug.LogWarning(
                "No character selections found. Using Arena scene players.");
            return;
        }

        if (scenePlayerOne != null)
        {
            scenePlayerOne.gameObject.SetActive(false);
        }

        if (scenePlayerTwo != null)
        {
            scenePlayerTwo.gameObject.SetActive(false);
        }

        playerOne = CreateSelectedPlayer(
            selectedPlayerOne,
            playerOneSpawn,
            true
        );

        playerTwo = CreateSelectedPlayer(
            selectedPlayerTwo,
            playerTwoSpawn,
            false
        );

        BindDamageDisplaysToSpawnedPlayers();
    }

    private FighterHealth CreateSelectedPlayer(
        CharacterData character,
        Transform spawnPoint,
        bool isPlayerOne)
    {
        // Instantiate one selected fighter and assign its controls.
        if (character == null || character.fighterPrefab == null)
        {
            Debug.LogError(
                "Character " +
                (character == null ? "null" : character.characterName) +
                " has no fighterPrefab assigned.");
            return null;
        }

        Vector3 spawnPosition = spawnPoint == null
            ? Vector3.zero
            : spawnPoint.position;
        Quaternion spawnRotation = spawnPoint == null
            ? Quaternion.identity
            : GetSpawnRotation(spawnPoint);

        GameObject fighterObject = Instantiate(
            character.fighterPrefab,
            spawnPosition,
            spawnRotation
        );

        // A prefab can be saved inactive. Fighters spawned for a match must
        // be active before their components can participate in gameplay.
        fighterObject.SetActive(true);

        fighterObject.name =
            character.characterName + (isPlayerOne ? " (P1)" : " (P2)");

        fighterObject.transform.position = spawnPosition;
        fighterObject.transform.rotation = spawnRotation;

        FighterController controller =
            fighterObject.GetComponentInChildren<FighterController>(true);

        if (controller != null)
        {
            FighterController.ControlMode mode = isPlayerOne
                ? PlayerSelectionData.Player1ControlMode
                : PlayerSelectionData.Player2ControlMode;
            Gamepad gamepad = isPlayerOne
                ? PlayerSelectionData.Player1Gamepad
                : PlayerSelectionData.Player2Gamepad;

            controller.SetControls(mode, gamepad);
            controller.SetFacingDirection(isPlayerOne);
        }

        FighterHealth health =
            fighterObject.GetComponentInChildren<FighterHealth>(true);

        if (health == null)
        {
            Debug.LogError(
                character.characterName +
                " fighterPrefab needs a FighterHealth component.");
            Destroy(fighterObject);
        }

        return health;
    }

    private void BindDamageDisplaysToSpawnedPlayers()
    {
        // Connect the UI damage displays to the newly spawned fighters.
        if (playerOne == null || playerTwo == null)
        {
            return;
        }

        DamageDisplay[] displays =
            FindObjectsOfType<DamageDisplay>(true);

        FieldInfo healthField = typeof(DamageDisplay).GetField(
            "fighterHealth",
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic
        );

        FieldInfo playerOneField = typeof(DamageDisplay).GetField(
            "isPlayerOne",
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic
        );

        if (healthField == null || playerOneField == null)
        {
            Debug.LogWarning(
                "Could not automatically bind DamageDisplay references.");
            return;
        }

        foreach (DamageDisplay display in displays)
        {
            bool isPlayerOne =
                (bool)playerOneField.GetValue(display);
            healthField.SetValue(
                display,
                isPlayerOne ? playerOne : playerTwo
            );
        }
    }

    public int PlayerOneStocks => playerOneStocks;
    public int PlayerTwoStocks => playerTwoStocks;

    private void Awake()
    {
        // Spawn players, initialize stocks, and prepare death audio.
        SpawnSelectedPlayers();

        playerOneStocks = startingStocks;
        playerTwoStocks = startingStocks;

        SetPlayerFacing(playerOne, true);
        SetPlayerFacing(playerTwo, false);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

    }

    private void Update()
    {
        // Check whether either fighter has fallen below the blast zone.
        if (matchEnded)
        {
            return;
        }

        CheckPlayerForBlastZone(
            playerOne,
            playerOneSpawn,
            ref playerOneStocks,
            ref playerOneRespawning,
            ref playerOneEliminated,
            "Player 1",
            true
        );

        CheckPlayerForBlastZone(
            playerTwo,
            playerTwoSpawn,
            ref playerTwoStocks,
            ref playerTwoRespawning,
            ref playerTwoEliminated,
            "Player 2",
            false
        );
    }

    private void CheckPlayerForBlastZone(
        FighterHealth player,
        Transform spawnPoint,
        ref int stocks,
        ref bool respawning,
        ref bool eliminated,
        string playerName,
        bool isPlayerOne)
    {
        // Remove a stock when a fighter crosses the blast-zone boundary.
        if (player == null || respawning || eliminated || !player.gameObject.activeInHierarchy)
        {
            return;
        }

        if (player.transform.position.y < blastZoneY)
        {
            LoseStock(
                player,
                spawnPoint,
                ref stocks,
                ref respawning,
                ref eliminated,
                playerName,
                isPlayerOne
            );
        }
    }

    private void LoseStock(
        FighterHealth player,
        Transform spawnPoint,
        ref int stocks,
        ref bool respawning,
        ref bool eliminated,
        string playerName,
        bool isPlayerOne)
    {
        // Play the death response, then respawn or eliminate the player.
        stocks--;
        player.ResetDamage();

        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        Debug.Log(playerName + " lost a stock. Stocks remaining: " + stocks);

        if (stocks <= 0)
        {
            eliminated = true;
            player.gameObject.SetActive(false);
            Debug.Log(playerName + " has been eliminated.");

            EndMatch(isPlayerOne);
            return;
        }

        respawning = true;
        StartCoroutine(RespawnPlayer(
            player,
            spawnPoint,
            isPlayerOne
        ));
    }

    private void EndMatch(bool playerOneWasEliminated)
    {
        // Save the winner and load the end-game scene.
        if (matchEnded)
        {
            return;
        }

        matchEnded = true;

        FighterHealth playerOneHealth = playerOne;
        FighterHealth playerTwoHealth = playerTwo;

        // In a two-player match, the damage dealt by one player equals the
        // opponent's total damage taken across all of their stocks.
        float playerOneDamageDealt = playerTwoHealth == null
            ? 0f
            : playerTwoHealth.TotalDamageTaken;
        float playerTwoDamageDealt = playerOneHealth == null
            ? 0f
            : playerOneHealth.TotalDamageTaken;

        string winnerName = playerOneWasEliminated
            ? "Player 2"
            : "Player 1";

        MatchResults.SetResults(
            winnerName,
            playerOneDamageDealt,
            playerTwoDamageDealt
        );

        if (!string.IsNullOrWhiteSpace(endGameSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                endGameSceneName);
        }
        else
        {
            Debug.LogError("StockManager has no end-game scene name assigned.");
        }
    }

    private IEnumerator RespawnPlayer(
        FighterHealth player,
        Transform spawnPoint,
        bool isPlayerOne)
    {
        // Freeze the fighter, move it to the spawn point, and restore control.
        GameObject playerObject = player.gameObject;
        Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        playerObject.SetActive(false);
        yield return new WaitForSeconds(respawnDelay);

        if (spawnPoint != null)
        {
            playerObject.transform.position = spawnPoint.position;
            playerObject.transform.rotation = GetSpawnRotation(
                spawnPoint
            );
        }

        SetPlayerFacing(player, isPlayerOne);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        player.ResetDamage();
        playerObject.SetActive(true);

        FighterController controller =
            playerObject.GetComponent<FighterController>();

        if (controller != null)
        {
            controller.BeginRespawn(respawnFloatDuration);
        }

        if (isPlayerOne)
        {
            playerOneRespawning = false;
        }
        else
        {
            playerTwoRespawning = false;
        }
    }

    private Quaternion GetSpawnRotation(
        Transform spawnPoint)
    {
        // Use the spawn point's rotation when one is available.
        if (spawnPoint == null)
        {
            return Quaternion.identity;
        }

        return Quaternion.Euler(
            0f,
            0f,
            spawnPoint.eulerAngles.z
        );
    }

    private void SetPlayerFacing(
        FighterHealth player,
        bool shouldFaceRight)
    {
        // Set the fighter's initial left/right facing direction.
        if (player == null)
        {
            return;
        }

        FighterController controller =
            player.GetComponentInChildren<FighterController>(true);

        if (controller != null)
        {
            controller.SetFacingDirection(shouldFaceRight);
        }
    }
}
