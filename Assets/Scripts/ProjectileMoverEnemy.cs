using UnityEngine;

public class ProjectileMoverEnemy : ProjectileMoverBase
{
    public float turnSpeed = 1f; // Speed at which the projectile turns towards the player
    private Transform player;

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player").transform; // Find the player
    }

    protected override void FixedUpdate()
    {
        if (player != null)
        {
            // Calculate the direction to the player
            Vector3 direction = (player.position - transform.position).normalized;

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
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("Damage applied: " + damage);
            }
        }
    }
}
