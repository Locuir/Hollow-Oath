
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform visual;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private float acceleration = 35f;
    [SerializeField] private float deceleration = 45f;

    [Header("Combat")]
    [SerializeField] private float comboInputWindow = 0.5f;
    [SerializeField] private float rollSpeed = 9f;

    private float horizontal;
    private int facingDirection = 1;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isBusy;
    private bool isDead;
    private bool isDefending;
    private bool wasFalling;

    private Keyboard keyboard;
    private Mouse mouse;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (visual == null && animator != null)
            visual = animator.transform;

        keyboard = Keyboard.current;
        mouse = Mouse.current;
    }

    private void Update()
    {
        keyboard = Keyboard.current;
        mouse = Mouse.current;

        if (isDead || animator == null || groundCheck == null || keyboard == null)
            return;

        wasGrounded = isGrounded;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (isBusy)
        {
            animator.SetBool("IsMoving", false);
            return;
        }

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            if (isGrounded)
                StartCoroutine(AttackCombo());
            else
                StartCoroutine(PlayTrigger("AirAttack"));

            return;
        }

        if (keyboard.fKey.wasPressedThisFrame && isGrounded)
        {
            StartCoroutine(PlayTrigger("Special Attack"));
            return;
        }

        if (keyboard.leftShiftKey.wasPressedThisFrame && isGrounded)
        {
            StartCoroutine(PerformRoll());
            return;
        }

        isDefending = mouse.rightButton.isPressed && isGrounded;

        if (isDefending)
        {
            horizontal = 0f;
            animator.SetBool("IsMoving", false);
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            SetTriggerOnce("Defend");
            return;
        }

        if (keyboard.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            animator.SetBool("InAir", true);
            wasFalling = false;
        }

        horizontal = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            horizontal -= 1f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            horizontal += 1f;

        if (horizontal != 0f)
        {
            facingDirection = horizontal > 0f ? 1 : -1;

            if (visual != null)
            {
                Vector3 scale = visual.localScale;
                scale.x = Mathf.Abs(scale.x) * facingDirection;
                visual.localScale = scale;
            }
        }

        animator.SetBool(
            "IsMoving",
            Mathf.Abs(horizontal) > 0.01f && isGrounded && !isBusy
        );

        if (!isGrounded)
        {
            animator.SetBool("InAir", true);

            if (rb.linearVelocity.y <= 0f && !wasFalling)
            {
                animator.SetTrigger("Fall");
                wasFalling = true;
            }
        }
        else
        {
            animator.SetBool("InAir", false);
            wasFalling = false;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || rb == null)
            return;

        if (isBusy || isDefending)
        {
            if (isDefending)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            return;
        }

        float targetSpeed = horizontal * moveSpeed;

        float rate = Mathf.Abs(horizontal) > 0.01f
            ? acceleration
            : deceleration;

        float newSpeed = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            rate * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(
            newSpeed,
            rb.linearVelocity.y
        );
    }


    private IEnumerator AttackCombo()
    {
        isBusy = true;
        horizontal = 0f;
        animator.SetBool("IsMoving", false);

        int roll = Random.Range(1, 101);
        int attackNumber;

        if (roll <= 50)
            attackNumber = 1;
        else if (roll <= 80)
            attackNumber = 2;
        else
            attackNumber = 3;

        Debug.Log("Selected Attack: " + attackNumber);

        yield return PlayTriggerAndWait("Attack " + attackNumber);

        isBusy = false;
    }

    private IEnumerator PerformRoll()
    {
        isBusy = true;
        animator.SetBool("IsMoving", false);

        rb.linearVelocity = new Vector2(
            facingDirection * rollSpeed,
            rb.linearVelocity.y
        );

        yield return PlayTriggerAndWait("Roll");

        isBusy = false;
    }

    private IEnumerator PlayTrigger(string triggerName)
    {
        isBusy = true;
        horizontal = 0f;
        animator.SetBool("IsMoving", false);

        yield return PlayTriggerAndWait(triggerName);

        isBusy = false;
    }

    private IEnumerator PlayTriggerAndWait(string triggerName)
    {
        animator.ResetTrigger(triggerName);
        animator.SetTrigger(triggerName);

        yield return null;

        float elapsed = 0f;
        const float timeout = 5f;

        while (elapsed < timeout)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            if (state.normalizedTime >= 1f && !animator.IsInTransition(0))
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void SetTriggerOnce(string triggerName)
    {
        if (string.IsNullOrEmpty(triggerName))
            return;

        if (!animator.GetBool("InAir"))
        {
            animator.ResetTrigger(triggerName);
            animator.SetTrigger(triggerName);
        }
    }

    public void TakeHit()
    {
        if (!isDead && !isBusy)
            StartCoroutine(PlayTrigger("TakeHit"));
    }

    public void Die()
    {
        if (isDead)
            return;

        StopAllCoroutines();

        isDead = true;
        isBusy = true;
        rb.linearVelocity = Vector2.zero;

        animator.SetBool("IsMoving", false);
        animator.SetTrigger("Death");
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}
