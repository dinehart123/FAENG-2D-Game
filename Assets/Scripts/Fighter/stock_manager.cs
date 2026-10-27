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

    private int playerOneStocks;
    private int playerTwoStocks;
    private bool playerOneRespawning;
    private bool playerTwoRespawning;
    private bool playerOneEliminated;
    private bool playerTwoEliminated;

    public int PlayerOneStocks => playerOneStocks;
    public int PlayerTwoStocks => playerTwoStocks;

    private void Awake()
    {
        playerOneStocks = startingStocks;
        playerTwoStocks = startingStocks;
    }

    private void Update()
    {
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

        Debug.Log(playerName + " lost a stock. Stocks remaining: " + stocks);

        if (stocks <= 0)
        {
            eliminated = true;
            player.gameObject.SetActive(false);
            Debug.Log(playerName + " has been eliminated.");
            return;
        }

        respawning = true;
        StartCoroutine(RespawnPlayer(
            player,
            spawnPoint,
            isPlayerOne
        ));
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
            playerObject.transform.rotation = spawnPoint.rotation;
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
}
