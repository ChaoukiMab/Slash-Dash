using UnityEngine;

public class LaserController : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Transform laserOrigin;
    public float laserLength = 50f;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        DrawLaser();
    }

    void DrawLaser()
    {
        lineRenderer.SetPosition(0, laserOrigin.position);

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.nearClipPlane;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(laserOrigin.position, (worldPosition - laserOrigin.position).normalized, out hit, laserLength))
        {
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            lineRenderer.SetPosition(1, laserOrigin.position + (worldPosition - laserOrigin.position).normalized * laserLength);
        }
    }
}
