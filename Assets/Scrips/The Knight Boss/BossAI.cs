
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visual;
    [SerializeField] private Transform player;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private BossCombat bossCombat;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float runAttackDistance = 4f;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 5.5f;
    [SerializeField] private float acceleration = 35f;

    [Header("Attack Cooldowns")]
    [SerializeField] private float phaseOneCooldown = 1.8f;
    [SerializeField] private float phaseTwoCooldown = 0.8f;

    [Header("Attack Durations")]
    [SerializeField] private float attack1Duration = 0.9f;
    [SerializeField] private float attack2Duration = 1.2f;
    [SerializeField] private float attack3Duration = 1.6f;
    [SerializeField] private float runAttackDuration = 1.3f;
    [SerializeField] private float defendDuration = 1f;
    [SerializeField] private float hurtDuration = 0.45f;

    [Header("Attack Damage")]
    [SerializeField] private float attack1Damage = 20f;
    [SerializeField] private float attack2Damage = 30f;
    [SerializeField] private float attack3Damage = 45f;
    [SerializeField] private float runAttackDamage = 35f;

    [Header("Boss Behavior")]
    [SerializeField, Range(0f, 1f)]
    private float defendChance = 0.15f;

    [SerializeField, Range(0f, 1f)]
    private float phaseTwoDefendChance = 0.05f;

    [SerializeField, Range(0f, 1f)]
    private float attack3Chance = 0.4f;

    private float actionUntil;
    private float nextAttackTime;
    private float horizontal;
    private int facingDirection = 1;

    private bool isBusy;
    private bool isDead;
    private bool isDefending;
    private bool isMoving;
    private bool isRunning;

    public bool IsDefending => isDefending;
    public bool IsDead => isDead;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (visual == null && animator != null)
            visual = animator.transform;

        if (bossHealth == null)
            bossHealth = GetComponent<BossHealth>();

        if (bossCombat == null)
            bossCombat = GetComponent<BossCombat>();

        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (isDead || player == null || animator == null)
            return;

        float distance = Vector2.Distance(
                        transform.position,
                                    player.position);

        FacePlayer();

        if (isBusy)
        {
            if (Time.time >= actionUntil)
                FinishAction();

            return;
        }

        if (distance > detectionRange)
        {
            horizontal = 0f;
            SetMovementAnimation(false, false);
            return;
        }

        if (distance <= attackRange)
        {
            horizontal = 0f;
            SetMovementAnimation(false, false);

            if (Time.time >= nextAttackTime)
                ChooseAction();

            return;
        }

        bool phaseTwo = bossHealth != null &&
                            bossHealth.IsPhaseTwo;

        bool shouldRun = phaseTwo &&
                             distance >= runAttackDistance;

        horizontal = facingDirection;

        SetMovementAnimation(true, shouldRun);
    }

    private void FixedUpdate()
    {
        if (rb == null || isDead)
            return;

        if (isBusy)
        {
            rb.linearVelocity = new Vector2(
                                0f,
                                                rb.linearVelocity.y);

            return;
        }

        float speed = isRunning ? runSpeed : walkSpeed;

        float targetSpeed = horizontal * speed;

        float newSpeed = Mathf.MoveTowards(
                        rb.linearVelocity.x,
                                    targetSpeed,
                                                acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(
                        newSpeed,
                                    rb.linearVelocity.y);
    }

    private void ChooseAction()
    {
        bool phaseTwo = bossHealth != null &&
                            bossHealth.IsPhaseTwo;

        float currentDefendChance = phaseTwo ? phaseTwoDefendChance : defendChance;

        float roll = Random.value;

        if (roll < currentDefendChance)
        {
            StartDefend();
            return;
        }

        if (phaseTwo)
        {
            float distance = Vector2.Distance(
                                transform.position,
                                                player.position);

            if (distance >= runAttackDistance &&
                    Random.value < 0.65f)
            {
                StartRunAttack();
                return;
            }

            if (Random.value < attack3Chance)
            {
                StartAttack("Attack3",attack3Damage,attack3Duration);

                return;
            }
        }

        if (Random.value < 0.5f)
        {
            StartAttack("Attack1",attack1Damage,attack1Duration);
        }
        else
        {
            StartAttack("Attack2",attack2Damage,attack2Duration);
        }
    }

    private void StartAttack(
                string trigger,
                        float damage,
                                float duration)
    {
        isBusy = true;
        horizontal = 0f;

        SetMovementAnimation(false, false);

        if (bossCombat != null)
            bossCombat.SetAttackDamage(damage);

        PlayTrigger(trigger);

        actionUntil = Time.time + duration;
        SetNextAttackTime();
    }

    private void StartRunAttack()
    {
        isBusy = true;
        horizontal = 0f;

        SetMovementAnimation(false, false);

        if (bossCombat != null)
            bossCombat.SetAttackDamage(runAttackDamage);

        PlayTrigger("RunAttack");

        actionUntil = Time.time + runAttackDuration;
        SetNextAttackTime();
    }

    private void StartDefend()
    {
        isBusy = true;
        isDefending = true;
        horizontal = 0f;

        SetMovementAnimation(false, false);

        if (bossHealth != null)
            bossHealth.SetDefending(true);

        PlayTrigger("Defend");

        actionUntil = Time.time + defendDuration;
        SetNextAttackTime();
    }

    private void FinishAction()
    {
        if (isDefending)
        {
            isDefending = false;

            if (bossHealth != null)
                bossHealth.SetDefending(false);
        }

        isBusy = false;
        horizontal = 0f;
    }

    private void SetNextAttackTime()
    {
        bool phaseTwo = bossHealth != null &&
                            bossHealth.IsPhaseTwo;

        float cooldown = phaseTwo ? phaseTwoCooldown : phaseOneCooldown;

        nextAttackTime = Time.time + cooldown;
    }

    private void PlayTrigger(string trigger)
    {
        animator.ResetTrigger(trigger);
        animator.SetTrigger(trigger);
    }

    private void SetMovementAnimation(bool moving, bool running)
    {
        isMoving = moving;
        isRunning = running;

        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsRunning", isRunning);
    }

    private void FacePlayer()
    {
        if (player == null || visual == null)
            return;

        float difference = player.position.x - transform.position.x;

        if (Mathf.Abs(difference) < 0.05f)
            return;

        facingDirection = difference > 0f ? 1 : -1;

        Vector3 scale = visual.localScale;
        scale.x = Mathf.Abs(scale.x) * facingDirection;
        visual.localScale = scale;
    }

    public void OnDamaged()
    {
        if (isDead)
            return;

        isBusy = true;
        isDefending = false;
        horizontal = 0f;

        if (bossHealth != null)
            bossHealth.SetDefending(false);

        SetMovementAnimation(false, false);

        PlayTrigger("Hurt");

        actionUntil = Time.time + hurtDuration;
    }

    public void OnDeath()
    {
        isDead = true;
        isBusy = true;
        isDefending = false;
        horizontal = 0f;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (bossHealth != null)
            bossHealth.SetDefending(false);

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsRunning", false);
        }

        enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, runAttackDistance);
    }
}
