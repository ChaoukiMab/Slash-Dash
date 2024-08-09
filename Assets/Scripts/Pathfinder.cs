using System.Linq;
using System;
using System.Collections; // For IEnumerator, List, etc.
using System.Collections.Generic; // For List, Dictionary, etc.
using UnityEngine; // For Unity-specific classes like MonoBehaviour, GameObject, etc.
using UnityEngine.UI; // For UI components like Slider, Canvas, etc.
using UnityEngine.AI; // For NavMeshAgent and related classes


public static class Pathfinder
{
    public static List<PathNode> FindPath(PathNode[,] grid, Vector2Int start, Vector2Int end)
    {
        if (!IsWithinBounds(grid, start) || !IsWithinBounds(grid, end))
        {
            return null;
        }

        List<PathNode> openList = new List<PathNode>();
        HashSet<PathNode> closedList = new HashSet<PathNode>();
        PathNode startNode = grid[start.x, start.y];
        PathNode endNode = grid[end.x, end.y];

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            PathNode currentNode = openList.OrderBy(node => node.fCost).First();
            if (currentNode == endNode)
            {
                return RetracePath(startNode, endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbor in GetNeighbors(grid, currentNode))
            {
                if (!neighbor.isWalkable || closedList.Contains(neighbor))
                    continue;

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openList.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, endNode);
                    neighbor.parent = currentNode;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        return null;
    }

    private static bool IsWithinBounds(PathNode[,] grid, Vector2Int position)
    {
        return position.x >= 0 && position.x < grid.GetLength(0) && position.y >= 0 && position.y < grid.GetLength(1);
    }

    private static List<PathNode> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        PathNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    private static int GetDistance(PathNode nodeA, PathNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
        int dstY = Mathf.Abs(nodeA.position.y - nodeB.position.y);
        return dstX + dstY;
    }

    private static List<PathNode> GetNeighbors(PathNode[,] grid, PathNode node)
    {
        List<PathNode> neighbors = new List<PathNode>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.position.x + x;
                int checkY = node.position.y + y;

                if (IsWithinBounds(grid, new Vector2Int(checkX, checkY)))
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbors;
    }
}

public class PathNode
{
    public Vector2Int position;
    public bool isWalkable;
    public int gCost, hCost;
    public PathNode parent;

    public PathNode(Vector2Int position, bool isWalkable)
    {
        this.position = position;
        this.isWalkable = isWalkable;
    }

    public int fCost => gCost + hCost;
}
