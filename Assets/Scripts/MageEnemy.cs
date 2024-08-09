using System;
using System.Collections; // For IEnumerator, List, etc.
using System.Collections.Generic; // For List, Dictionary, etc.
using UnityEngine; // For Unity-specific classes like MonoBehaviour, GameObject, etc.
using UnityEngine.UI; // For UI components like Slider, Canvas, etc.
using UnityEngine.AI; // For NavMeshAgent and related classes

public class MageEnemy : EnemyBase
{
    public float attackRange = 5f; // Distance for ranged attack
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float attackDamage = 15f; // Set the attack damage to 15 for the mage

    private bool isAttacking = false; // Track whether the Mage is currently attacking

    protected override void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackRate;
            }
            StopMoving();
        }
        else if (!isAttacking)
        {
            base.Update();
        }
    }

    protected override void Attack()
    {
        Debug.Log("Mage attacks!");
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.LookRotation(player.position - transform.position));
        var projectileMover = projectile.GetComponent<ProjectileMoverEnemy>();
        if (projectileMover != null)
        {
            projectileMover.damage = attackDamage;
        }

        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
            isAttacking = true;
            StartCoroutine(ResetAttackAnimation(animator.GetCurrentAnimatorStateInfo(0).length));
        }
    }

    private IEnumerator ResetAttackAnimation(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
        }
        isAttacking = false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            StopMoving();
        }
        else
        {
            animator.SetBool("IsRunning", true);
        }
    }

    private void StopMoving()
    {
        agent.isStopped = true;
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsIdle", true);
    }

    protected override void Die()
    {
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }
        enabled = false;
        agent.isStopped = true;

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }

        StartCoroutine(RemoveAfterAnimation(animator.GetCurrentAnimatorStateInfo(0).length));
    }
}
