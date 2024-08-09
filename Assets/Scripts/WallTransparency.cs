using System.Collections;
using UnityEngine;

public class WallTransparency : MonoBehaviour
{
    public Material transparentMaterial;
    public Material originalMaterial;
    private Renderer wallRenderer;
    private Transform player;
    private bool isTransparent = false;

    void Start()
    {
        wallRenderer = GetComponent<Renderer>();
        originalMaterial = wallRenderer.material;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        Vector3 playerPosition = player.position;
        Vector3 wallPosition = transform.position;
        Vector3 directionToPlayer = (playerPosition - wallPosition).normalized;

        RaycastHit hit;
        if (Physics.Raycast(wallPosition, directionToPlayer, out hit))
        {
            if (hit.transform.CompareTag("Player") && !isTransparent)
            {
                SetTransparent();
            }
            else if (!hit.transform.CompareTag("Player") && isTransparent)
            {
                SetOpaque();
            }
        }
    }

    void SetTransparent()
    {
        wallRenderer.material = transparentMaterial;
        isTransparent = true;
    }

    void SetOpaque()
    {
        wallRenderer.material = originalMaterial;
        isTransparent = false;
    }
}
