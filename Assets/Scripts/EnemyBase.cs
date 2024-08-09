using System;
using System.Collections; // For IEnumerator, List, etc.
using System.Collections.Generic; // For List, Dictionary, etc.
using UnityEngine; // For Unity-specific classes like MonoBehaviour, GameObject, etc.
using UnityEngine.UI; // For UI components like Slider, Canvas, etc.
using UnityEngine.AI; // For NavMeshAgent and related classes

public class EnemyBase : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    public float attackDistance = 1.5f;
    public float lookRadius = 10f;
    protected List<PathNode> path;
    protected int currentPathIndex;
    protected Grid grid;
    public float attackRate = 1f; // Time between attacks
    protected float nextAttackTime = 0f;

    protected Animator animator;
    protected Rigidbody rb;
    protected Collider col;
    protected NavMeshAgent agent;

    public float maxHealth = 100f;
    protected float currentHealth;
    public GameObject healthBarPrefab; // Reference to the health bar prefab
    protected Slider healthBarSlider;
    protected Canvas healthBarCanvas;

    protected virtual void Start()
    {
        // Automatically find the player in the scene
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        agent = GetComponent<NavMeshAgent>();
        grid = FindObjectOfType<Grid>();
        InvokeRepeating("UpdatePath", 0f, 1f); // Update path every second

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        currentHealth = maxHealth;

        // Ensure gravity is enabled
        rb.useGravity = true;

        // Instantiate health bar and set its parent to the enemy
        if (healthBarPrefab != null)
        {
            GameObject healthBarInstance = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
            healthBarInstance.transform.SetParent(transform);
            healthBarCanvas = healthBarInstance.GetComponent<Canvas>();
            healthBarSlider = healthBarInstance.GetComponentInChildren<Slider>();
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }
    }

    protected virtual void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= lookRadius)
        {
            if (!agent.isOnNavMesh)
            {
                // Try to place the agent on the NavMesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }

            if (agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
            }

            if (Vector3.Distance(transform.position, player.position) <= attackDistance && Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackRate;
            }

            UpdateAnimator();
            UpdateHealthBar();
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("IsIdle", true);
                animator.SetBool("IsRunning", false);
            }
        }
    }

    protected virtual void UpdatePath()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= lookRadius)
        {
            Vector2Int start = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
            Vector2Int end = new Vector2Int(Mathf.RoundToInt(player.position.x), Mathf.RoundToInt(player.position.z));

            if (grid.IsWithinBounds(start) && grid.IsWithinBounds(end))
            {
                path = grid.FindPath(start, end);
                currentPathIndex = 0;
            }
        }
    }

    protected virtual void Attack() { }

    protected void UpdateAnimator()
    {
        if (animator != null)
        {
            animator.SetBool("IsIdle", !IsMoving());
            animator.SetBool("IsRunning", IsMoving());
        }
    }

    protected bool IsMoving()
    {
        return path != null && currentPathIndex < path.Count;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null)
            {
                animator.SetTrigger("IsGettingHit");
            }
        }
    }

    protected virtual void Die()
    {
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }

        // Call AddKill with the appropriate enemy type
        ScoreManager.Instance.AddKill(GetType().Name);

        // Disable enemy components to stop movement and attacks
        enabled = false;
        rb.isKinematic = true;
        col.enabled = false;

        // Remove the enemy GameObject after the death animation is complete
        StartCoroutine(RemoveAfterAnimation(animator.GetCurrentAnimatorStateInfo(0).length));
    }

    public IEnumerator RemoveAfterAnimation(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject);
    }

    protected void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
