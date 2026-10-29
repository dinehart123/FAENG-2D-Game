using System.Collections;
using UnityEngine;

public class StockManager : MonoBehaviour
{
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

    public int PlayerOneStocks => playerOneStocks;
    public int PlayerTwoStocks => playerTwoStocks;

    private void Awake()
    {
        playerOneStocks = startingStocks;
        playerTwoStocks = startingStocks;

        // Player 2 always faces Player 1 at the start of the match.
        FacePlayerTwo(playerTwo == null ? null : playerTwo.gameObject);

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
                spawnPoint.rotation,
                isPlayerOne
            );
        }
        else if (!isPlayerOne)
        {
            FacePlayerTwo(playerObject);
        }

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
        Quaternion spawnRotation,
        bool isPlayerOne)
    {
        if (isPlayerOne)
        {
            return spawnRotation;
        }

        Vector3 eulerAngles = spawnRotation.eulerAngles;
        eulerAngles.y = 180f;
        return Quaternion.Euler(eulerAngles);
    }

    private void FacePlayerTwo(GameObject playerObject)
    {
        if (playerObject == null)
        {
            return;
        }

        Vector3 eulerAngles = playerObject.transform.eulerAngles;
        eulerAngles.y = 180f;
        playerObject.transform.eulerAngles = eulerAngles;
    }
}
