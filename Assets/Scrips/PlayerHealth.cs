using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Defense")]
    [SerializeField, Range(0f, 1f)]
    private float defenseDamageMultiplier = 0.35f;

    [Header("Damage Settings")]
    [SerializeField] private float invincibilityDuration = 0.7f;

    [Header("Events")]
    [SerializeField] private UnityEvent<float, float> onHealthChanged;
    [SerializeField] private UnityEvent onDeath;

    private PlayerController playerController;
    private float currentHealth;
    private float nextDamageTime;
    private bool isDead;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => currentHealth / maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        currentHealth = maxHealth;

        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        if (Time.time < nextDamageTime)
            return;

        if (playerController != null && playerController.IsRolling)
            return;

        bool isDefending = playerController != null &&
                               playerController.IsDefending;

        if (isDefending)
            damage *= defenseDamageMultiplier;

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        nextDamageTime = Time.time + invincibilityDuration;

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"Player HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        if (!isDefending && playerController != null)
            playerController.PlayHitReaction();
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"Player Healed: {currentHealth}/{maxHealth}");
    }

    public void RestoreHealth()
    {
        if (isDead)
            return;

        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        onDeath?.Invoke();

        if (playerController != null)
            playerController.Die();

        Debug.Log("PLAYER DIED!");
    }
}