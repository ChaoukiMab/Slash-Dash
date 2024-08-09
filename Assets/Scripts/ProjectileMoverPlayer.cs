using UnityEngine;
using System.Linq;

public class ProjectileMoverPlayer : ProjectileMoverBase
{
    public float turnSpeed = 5f; // Speed at which the projectile turns towards the closest enemy
    public float activationDistance = 10f; // Distance from the enemy at which the homing activates
    private Transform closestEnemy;

    protected override void Start()
    {
        base.Start();
        damage = 30f;
    }

    protected override void FixedUpdate()
    {
        FindClosestEnemy(); // Find the closest enemy every frame

        if (closestEnemy != null && Vector3.Distance(transform.position, closestEnemy.position) <= activationDistance)
        {
            // Calculate the direction to the closest enemy
            Vector3 direction = (closestEnemy.position - transform.position).normalized;

            // Calculate the new direction based on turn speed
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, turnSpeed * Time.deltaTime, 0.0f);

            // Update the projectile's rotation and velocity
            rb.velocity = newDirection * speed;
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

        base.FixedUpdate(); // Call the base class's FixedUpdate method
    }

    protected override void HandleCollision(Collision collision)
    {
        Debug.Log("Collision detected with: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Damage applied: " + damage);
            }
        }
    }

    private void FindClosestEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length > 0)
        {
            closestEnemy = enemies
                .Select(enemy => enemy.transform)
                .OrderBy(t => Vector3.Distance(transform.position, t.position))
                .FirstOrDefault();
        }
        else
        {
            closestEnemy = null;
        }
    }
}
