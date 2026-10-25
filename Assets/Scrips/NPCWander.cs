
using System.Collections;
using UnityEngine;

public class NPCWander : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visual;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float minMoveTime = 1f;
    [SerializeField] private float maxMoveTime = 3f;
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 4f;
    [SerializeField] private float movementRange = 5f;

    private float startingX;
    private int direction = 1;
    private bool isMoving;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (visual == null && animator != null)
            visual = animator.transform;

        startingX = transform.position.x;
    }

    private void Start()
    {
        StartCoroutine(WanderRoutine());
    }

    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            isMoving = false;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (animator != null)
                animator.SetBool("IsMoving", false);

            yield return new WaitForSeconds(
                Random.Range(minIdleTime, maxIdleTime)
            );

            float targetX = Random.Range(
                startingX - movementRange,
                startingX + movementRange
            );

            direction = targetX >= transform.position.x ? 1 : -1;

            if (Mathf.Abs(targetX - transform.position.x) < 0.3f)
                continue;

            isMoving = true;

            if (animator != null)
                animator.SetBool("IsMoving", true);

            if (visual != null)
            {
                Vector3 scale = visual.localScale;
                scale.x = Mathf.Abs(scale.x) * direction;
                visual.localScale = scale;
            }

            float moveTimer = 0f;
            float moveDuration = Random.Range(minMoveTime, maxMoveTime);

            while (moveTimer < moveDuration)
            {
                float distanceToTarget = targetX - transform.position.x;

                if (Mathf.Abs(distanceToTarget) < 0.1f)
                    break;

                float velocityX = direction * moveSpeed;

                if (Mathf.Abs(distanceToTarget) < moveSpeed * Time.deltaTime)
                    velocityX = distanceToTarget / Time.deltaTime;

                rb.linearVelocity = new Vector2(
                    velocityX,
                    rb.linearVelocity.y
                );

                moveTimer += Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            isMoving = false;

            if (animator != null)
                animator.SetBool("IsMoving", false);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (animator != null)
            animator.SetBool("IsMoving", false);
    }

    private void OnDrawGizmosSelected()
    {
        float centerX = Application.isPlaying
            ? startingX
            : transform.position.x;

        Gizmos.color = Color.cyan;

        Vector3 left = new Vector3(
            centerX - movementRange,
            transform.position.y,
            0f
        );

        Vector3 right = new Vector3(
            centerX + movementRange,
            transform.position.y,
            0f
        );

        Gizmos.DrawLine(left, right);
        Gizmos.DrawWireSphere(left, 0.15f);
        Gizmos.DrawWireSphere(right, 0.15f);
    }
}
