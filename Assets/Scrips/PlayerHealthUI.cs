
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;

    private void Start()
    {
        if (playerHealth == null || healthFill == null)
        {
            Debug.LogError("PlayerHealthUI references are missing!");
            enabled = false;
            return;
        }

        UpdateHealthUI();
    }

    private void Update()
    {
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        float percentage = playerHealth.HealthPercentage;

        healthFill.fillAmount = percentage;

        if (healthText != null)
        {
            healthText.text =
                    $"{playerHealth.CurrentHealth:0} / {playerHealth.MaxHealth:0}";
        }
    }
}
