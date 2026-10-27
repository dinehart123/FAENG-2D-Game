using TMPro;
using UnityEngine;

public class DamagePercentDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FighterHealth fighterHealth;
    [SerializeField] private TMP_Text percentText;

    private void Awake()
    {
        if (percentText == null)
        {
            percentText = GetComponent<TMP_Text>();
        }
    }

    private void Update()
    {
        if (fighterHealth == null || percentText == null)
        {
            return;
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

        percentText.text = damagePercent.ToString("0");
        percentText.color = damageColor;
    }
}
