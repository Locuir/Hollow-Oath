
using UnityEngine;

public class BossHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 1000f;

    [Header("Defense")]
    [SerializeField, Range(0f, 1f)]
    private float defenseDamageMultiplier = 0.25f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private BossAI bossAI;

    [Header("Colliders")]
    [SerializeField] private Collider2D aliveCollider;
    [SerializeField] private Collider2D deathCollider;

    private float currentHealth;
    private bool isDead;
    private bool isDefending;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => currentHealth / maxHealth;
    public bool IsDead => isDead;
    public bool IsPhaseTwo => HealthPercentage <= 0.5f;

    private void Awake()
    {

        if (aliveCollider != null)
            aliveCollider.enabled = true;

        if (deathCollider != null)
            deathCollider.enabled = false;

        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (bossAI == null)
            bossAI = GetComponent<BossAI>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        if (isDefending)
            damage *= defenseDamageMultiplier;

        currentHealth = Mathf.Max(0f, currentHealth - damage);

        Debug.Log($"Boss HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        if (bossAI != null)
            bossAI.OnDamaged();
    }

    public void SetDefending(bool value)
    {
        isDefending = value;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (aliveCollider != null)
            aliveCollider.enabled = false;

        if (deathCollider != null)
            deathCollider.enabled = true;

       
        isDefending = false;

        if (bossAI != null)
            bossAI.OnDeath();

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();



        if (animator != null)
        {
            animator.ResetTrigger("Hurt");
            animator.ResetTrigger("Attack1");
            animator.ResetTrigger("Attack2");
            animator.ResetTrigger("Attack3");
            animator.ResetTrigger("RunAttack");
            animator.ResetTrigger("Defend");

            animator.SetTrigger("Dead");
        }

        Debug.Log("BOSS DEFEATED!");
    }
}
