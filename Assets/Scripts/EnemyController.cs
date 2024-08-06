using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public Transform player;
    public float lookRadius = 10f;
    private NavMeshAgent agent;
    public float health = 100f;
    private Animator animator;

    private bool isDead = false;
    private Vector3 velocity;
    public float gravity = -9.81f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        SetIdle();
    }

    void Update()
    {
        if (isDead) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= lookRadius)
        {
            if (distance <= agent.stoppingDistance)
            {
                // Attack the player
                FacePlayer();
                SetAttacking();
                Debug.Log("Enemy Attacks!");
            }
            else
            {
                SetRunning();
                agent.SetDestination(player.position);
            }
        }
        else
        {
            SetIdle();
        }

        ApplyGravity();
    }

    public void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        SetGettingHit();
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        SetDead();
        agent.isStopped = true;
        // You might want to disable the enemy's collider here to prevent further interactions
        // GetComponent<Collider>().enabled = false;
        // Optionally, destroy the enemy after a delay to allow the death animation to play
        // Destroy(gameObject, 2f);
    }

    void SetIdle()
    {
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsIdle", true);
        animator.SetBool("IsAttacking", false);
        animator.SetBool("SenseSomething", false);
        animator.SetBool("Victory", false);
        animator.SetBool("IsGettingHit", false);
        animator.SetBool("Dead", false);
    }

    void SetRunning()
    {
        animator.SetBool("IsRunning", true);
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsAttacking", false);
        animator.SetBool("SenseSomething", false);
        animator.SetBool("Victory", false);
        animator.SetBool("IsGettingHit", false);
        animator.SetBool("Dead", false);
    }

    void SetAttacking()
    {
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsAttacking", true);
        animator.SetBool("SenseSomething", false);
        animator.SetBool("Victory", false);
        animator.SetBool("IsGettingHit", false);
        animator.SetBool("Dead", false);
    }

    void SetGettingHit()
    {
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsAttacking", false);
        animator.SetBool("SenseSomething", false);
        animator.SetBool("Victory", false);
        animator.SetBool("IsGettingHit", true);
        animator.SetBool("Dead", false);
    }

    void SetDead()
    {
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsAttacking", false);
        animator.SetBool("SenseSomething", false);
        animator.SetBool("Victory", false);
        animator.SetBool("IsGettingHit", false);
        animator.SetBool("Dead", true);
    }

    // Add a method to set the victory state
    public void SetVictory()
    {
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsAttacking", false);
        animator.SetBool("SenseSomething", false);
        animator.SetBool("Victory", true);
        animator.SetBool("IsGettingHit", false);
        animator.SetBool("Dead", false);
    }

    void ApplyGravity()
    {
        if (agent.isOnNavMesh)
        {
            if (agent.isStopped)
            {
                velocity.y += gravity * Time.deltaTime;
                transform.position += velocity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }
    }
}
