using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform attackPoint;

    [Header("Attack Detection")]
    [SerializeField] private float attackRadius = 1.2f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Attack Damage")]
    [SerializeField] private float attack1Damage = 20f;
    [SerializeField] private float attack2Damage = 30f;
    [SerializeField] private float attack3Damage = 45f;
    [SerializeField] private float airAttackDamage = 15f;
    [SerializeField] private float specialAttackDamage = 60f;

    private float currentDamage;

    private readonly HashSet<MonoBehaviour> hitTargets = new();

    public void PrepareAttack(int attackNumber)
    {
        currentDamage = attackNumber switch
        {
            1 => attack1Damage,
            2 => attack2Damage,
            3 => attack3Damage,
            _ => 0f
        };

        hitTargets.Clear();
    }

    public void PrepareAirAttack()
    {
        currentDamage = airAttackDamage;
        hitTargets.Clear();
    }

    public void PrepareSpecialAttack()
    {
        currentDamage = specialAttackDamage;
        hitTargets.Clear();
    }

    public void ApplyAttackDamage()
    {
        if (attackPoint == null || currentDamage <= 0f)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
                        attackPoint.position,
                                    attackRadius,
                                                enemyLayer);

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

                Debug.Log($"Player dealt {currentDamage} damage.");

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