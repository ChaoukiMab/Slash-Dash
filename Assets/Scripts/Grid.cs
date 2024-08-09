using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public int width, height;
    public float cellSize;
    public LayerMask unwalkableMask;
    public PathNode[,] grid;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        grid = new PathNode[width, height];
        Vector3 worldBottomLeft = transform.position - Vector3.right * width / 2 - Vector3.forward * height / 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * cellSize + cellSize / 2) + Vector3.forward * (y * cellSize + cellSize / 2);
                bool walkable = !(Physics.CheckSphere(worldPoint, cellSize / 2, unwalkableMask));
                grid[x, y] = new PathNode(new Vector2Int(x, y), walkable);
            }
        }
    }

    public List<PathNode> FindPath(Vector2Int start, Vector2Int end)
    {
        if (IsWithinBounds(start) && IsWithinBounds(end))
        {
            return Pathfinder.FindPath(grid, start, end);
        }
        return null;
    }

    public bool IsWithinBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
    }
}
