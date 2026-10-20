using System.Collections;
using UnityEngine;

public class StockManager : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private FighterHealth playerOne;
    [SerializeField] private FighterHealth playerTwo;

    [Header("Respawn Points")]
    [SerializeField] private Transform playerOneSpawn;
    [SerializeField] private Transform playerTwoSpawn;

    [Header("Stocks")]
    [SerializeField] private int startingStocks = 3;
    [SerializeField] private float blastZoneY = -10f;
    [SerializeField] private float respawnDelay = 1.5f;

    private int playerOneStocks;
    private int playerTwoStocks;
    private bool playerOneRespawning;
    private bool playerTwoRespawning;

    private void Start()
    {
        playerOneStocks = startingStocks;
        playerTwoStocks = startingStocks;
    }

    private void Update()
    {
        CheckPlayer(playerOne, ref playerOneStocks, playerOneSpawn, ref playerOneRespawning);
        CheckPlayer(playerTwo, ref playerTwoStocks, playerTwoSpawn, ref playerTwoRespawning);
    }

    private void CheckPlayer(
        FighterHealth player,
        ref int stocks,
        Transform spawnPoint,
        ref bool respawning)
    {
        if (player == null || respawning || player.transform.position.y > blastZoneY)
        {
            return;
        }

        stocks--;
        respawning = true;
        StartCoroutine(RespawnPlayer(player, spawnPoint, stocks));
    }

    private IEnumerator RespawnPlayer(
        FighterHealth player,
        Transform spawnPoint,
        int remainingStocks)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        FighterController controller = player.GetComponent<FighterController>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (controller != null)
        {
            controller.enabled = false;
        }

        yield return new WaitForSeconds(respawnDelay);

        if (remainingStocks >= 0 && spawnPoint != null)
        {
            player.transform.position = spawnPoint.position;
            player.ResetDamage();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            if (controller != null)
            {
                controller.enabled = true;
            }
        }
        else
        {
            player.gameObject.SetActive(false);
        }

        if (player == playerOne)
        {
            playerOneRespawning = false;
        }
        else if (player == playerTwo)
        {
            playerTwoRespawning = false;
        }
    }
}