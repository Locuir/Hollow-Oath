
using System.Collections.Generic;
using UnityEngine;

public class BossCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 1.2f;
    [SerializeField] private LayerMask playerLayer;

    private float currentDamage;
    private readonly HashSet<MonoBehaviour> hitTargets = new();

    public void SetAttackDamage(float damage)
    {
        currentDamage = damage;
        hitTargets.Clear();
    }

    public void ApplyAttackDamage()
    {
        if (attackPoint == null || currentDamage <= 0f)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
                        attackPoint.position,
                                    attackRadius,
                                                playerLayer);

        foreach (Collider2D hit in hits)
        {
            MonoBehaviour[] components =
                                hit.GetComponentsInParent<MonoBehaviour>();

            foreach (MonoBehaviour component in components)
            {
                if (component is not IDamageable target)
                    continue;

                if (hitTargets.Contains(component))
                    break;

                hitTargets.Add(component);
                target.TakeDamage(currentDamage);

                Debug.Log($"Boss dealt {currentDamage} damage.");

                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
