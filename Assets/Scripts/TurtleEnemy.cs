using System;
using System.Collections; // For IEnumerator, List, etc.
using System.Collections.Generic; // For List, Dictionary, etc.
using UnityEngine; // For Unity-specific classes like MonoBehaviour, GameObject, etc.
using UnityEngine.UI; // For UI components like Slider, Canvas, etc.
using UnityEngine.AI; // For NavMeshAgent and related classes


public class TurtleEnemy : EnemyBase
{
    public float attackDamage = 20f;
    public float meleeAttackDistance = 1.5f; // Close range for melee attack

    protected override void Update()
    {
        base.Update();
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= meleeAttackDistance)
        {
            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackRate;
            }
        }
    }

    protected override void Attack()
    {
        // Implement melee attack logic
        Debug.Log("Turtle attacks!");
        if (player.TryGetComponent(out PlayerController playerController))
        {
            playerController.TakeDamage(attackDamage);
        }

        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
            Invoke("ResetAttackAnimation", attackRate);
        }
    }

    void ResetAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
        }
    }
}
