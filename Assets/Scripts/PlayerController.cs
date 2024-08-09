using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public float health = 100f;
    public float moveSpeed = 5f;
    public float backwardSpeed = 2f;
    public float rotationSpeed = 720f;
    public CharacterController controller;
    public Animator animator;

    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public GameObject laserPrefab;
    public Image damageImage;

    public float fireRate = 1f;
    private float nextFireTime = 0f;
    private bool isShooting;

    private Vector3 moveDirection;
    private Camera mainCamera;
    private bool isRunning;
    private bool isWalkingBackward;
    private bool isIdle;
    public bool isDead;

    private GameObject laserInstance;
    private Color originalColor;
    private float fadeSpeed = 5f;

    void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        laserInstance = Instantiate(laserPrefab, Vector3.zero, Quaternion.identity);
        laserInstance.GetComponent<LaserController>().laserOrigin = projectileSpawnPoint;

        if (damageImage != null)
        {
            originalColor = damageImage.color;
            damageImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        }
    }

    void Update()
    {
        if (isDead)
            return;

        Aim();
        HandleActions();
        Move();
        UpdateAnimator();

        if (damageImage != null)
        {
            damageImage.color = Color.Lerp(damageImage.color, new Color(originalColor.r, originalColor.g, originalColor.b, 0), fadeSpeed * Time.deltaTime);
        }
    }

    void Move()
    {
        if (isShooting)
        {
            isRunning = false;
            isWalkingBackward = false;
            isIdle = true;
            return;
        }

        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;

        float speed = vertical > 0 ? moveSpeed : backwardSpeed;

        if (vertical != 0)
        {
            isWalkingBackward = vertical < 0;
            isRunning = vertical > 0;
            isIdle = false;

            moveDirection = transform.forward * vertical;
            controller.Move(moveDirection * speed * Time.deltaTime);
        }
        else
        {
            isRunning = false;
            isWalkingBackward = false;
            isIdle = true;
        }
    }

    void Aim()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;

        if (groundPlane.Raycast(ray, out rayLength))
        {
            Vector3 pointToLook = ray.GetPoint(rayLength);
            Vector3 aimDirection = (pointToLook - transform.position).normalized;
            aimDirection.y = 0;

            Quaternion lookRotation = Quaternion.LookRotation(aimDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void HandleActions()
    {
        if (Input.GetMouseButton(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (Time.time > nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            isShooting = true;

            Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

            animator.SetBool("IsShooting", true);
            Invoke("ResetShooting", fireRate);
        }
    }

    void ResetShooting()
    {
        isShooting = false;
        animator.SetBool("IsShooting", false);
    }

    void UpdateAnimator()
    {
        if (animator != null)
        {
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsWalkingBackward", isWalkingBackward);
            animator.SetBool("IsIdle", isIdle);
            animator.SetBool("IsShooting", isShooting);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }

        if (damageImage != null)
        {
            damageImage.color = originalColor;
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Player has died. Attempting to show Game Over screen.");

        GameOverManager gameOverManager = FindObjectOfType<GameOverManager>();
        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOver();
        }
        else
        {
            Debug.LogError("GameOverManager not found in the scene!");
        }
    }

    public Vector3 GetMoveDirection()
    {
        return moveDirection;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Handle any collisions here if necessary
    }
}
