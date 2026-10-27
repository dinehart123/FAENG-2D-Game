using TMPro;
using UnityEngine;

public class DamageDisplay : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private FighterHealth fighterHealth;
    [SerializeField] private bool isPlayerOne;
    [SerializeField] private StockManager stockManager;

    [Header("UI")]
    [SerializeField] private TMP_Text percentText;

    private void Awake()
    {
        if (percentText == null)
        {
            percentText = GetComponent<TMP_Text>();
        }

        if (stockManager == null)
        {
            stockManager = FindFirstObjectByType<StockManager>();
        }
    }

    private void Update()
    {
        if (fighterHealth == null || percentText == null)
        {
            return;
        }

        if (stockManager == null)
        {
            stockManager = FindFirstObjectByType<StockManager>();
        }

        float damagePercent = Mathf.Max(0f, fighterHealth.DamagePercent);
        Color damageColor;

        if (damagePercent <= 100f)
        {
            float colorAmount = damagePercent / 100f;
            damageColor = Color.Lerp(Color.green, Color.red, colorAmount);
        }
        else
        {
            float colorAmount = Mathf.Clamp01((damagePercent - 100f) / 100f);
            damageColor = Color.Lerp(Color.red, Color.black, colorAmount);
        }

        int lives = 0;

        if (stockManager != null)
        {
            lives = isPlayerOne
                ? stockManager.PlayerOneStocks
                : stockManager.PlayerTwoStocks;
        }

        percentText.text = damagePercent.ToString("0") +
            "\nLives: " + lives;
        percentText.color = damageColor;
    }
}
