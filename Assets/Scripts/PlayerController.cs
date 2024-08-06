using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float health = 100f;
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;
    public float jumpForce = 5f;
    public CharacterController controller;
    public Animator animator;

    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;

    // Shooting variables
    public float fireRate = 0.1f; // Time between shots
    private float nextFireTime = 0f;

    private Vector3 moveDirection;
    private Camera mainCamera;
    private bool isRunning;
    private bool isJumping;
    private bool isIdle;
    private bool isShooting;
    private bool isReloading;

    private Vector3 velocity;
    public float gravity = -9.81f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        Move();
        HandleActions();
        UpdateAnimator();
        ApplyGravity();
    }

    void Move()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            isRunning = true;
            isIdle = false;

            // Calculate the direction relative to the camera
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
        else
        {
            isRunning = false;
            isIdle = true;
        }
    }

    void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            velocity.y = -2f; // Ensure player sticks to the ground
            isJumping = false;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleActions()
    {
        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            Jump();
        }
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    void Jump()
    {
        isJumping = true;
        isIdle = false;
        velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
    }

    void Shoot()
    {
        if (Time.time > nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            isShooting = true;
            isIdle = false;

            Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

            // Additional shooting logic (e.g., play shooting animation/sound)
        }
    }

    void Reload()
    {
        isReloading = true;
        isIdle = false;
        // Implement reloading logic here
    }

    void UpdateAnimator()
    {
        if (animator != null)
        {
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsJumping", isJumping);
            animator.SetBool("IsIdle", isIdle);
            animator.SetBool("IsShooting", isShooting);
            animator.SetBool("IsReloading", isReloading);

            // Reset shooting and reloading after setting animator
            isShooting = false;
            isReloading = false;
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    public Vector3 GetMoveDirection()
    {
        return moveDirection;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Reset jump state when hitting the ground
        if (hit.collider.CompareTag("Ground"))
        {
            isJumping = false;
        }
    }
}
