using UnityEngine;

public class CameraSettings : MonoBehaviour
{
    public Transform target; // The player transform
    public Vector3 offset = new Vector3(0, 10, -10); // Offset from the player
    public float smoothSpeed = 0.125f; // Smoothness factor
    public float lookAheadFactor = 2f; // Factor for look-ahead

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // Try to find the player if target is not set
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 lookAheadOffset = Vector3.zero;
        if (target.GetComponent<PlayerController>() != null)
        {
            Vector3 moveDirection = target.GetComponent<PlayerController>().GetMoveDirection();
            lookAheadOffset = moveDirection * lookAheadFactor;
        }

        // Desired position based on the target's position, the offset, and the look-ahead offset
        Vector3 desiredPosition = target.position + offset + lookAheadOffset;

        // Smoothly interpolate between the current position and the desired position
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        transform.position = smoothedPosition;

        // Always look at the target
        transform.LookAt(target.position + Vector3.up * 1.5f); // Adjust the look at position as needed
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
