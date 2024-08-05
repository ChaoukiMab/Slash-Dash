using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid squareGrid;
    public MeshFilter walls;
    public MeshFilter cave;
    public MeshFilter floor; // New MeshFilter for the floor

    public bool is2D;

    List<Vector3> vertices;
    List<int> triangles;

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();

    public void GenerateMesh(int[,] map, float squareSize)
    {
        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        squareGrid = new SquareGrid(map, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        cave.mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        int tileAmount = 10;
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].z) * tileAmount;
            uvs[i] = new Vector2(percentX, percentY);
        }
        mesh.uv = uvs;

        if (is2D)
        {
            Generate2DColliders();
        }
        else
        {
            CreateWallMesh(squareSize);
            CreateFloorMesh(map, squareSize); // Call the new method to generate the floor
        }

        // Call the method with the map width and height
        AssignTagsToWalls(map.GetLength(0), map.GetLength(1));
    }

    void CreateWallMesh(float squareSize)
    {
        MeshCollider currentCollider = GetComponent<MeshCollider>();
        if (currentCollider != null)
            Destroy(currentCollider);

        CalculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        List<Vector2> wallUVs = new List<Vector2>();

        Mesh wallMesh = new Mesh();
        float wallHeight = 7;
        float wallThickness = 0.001f; // Adjust as needed

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                // Vertices for the current wall segment
                Vector3 start = vertices[outline[i]];
                Vector3 end = vertices[outline[i + 1]];

                // Calculate wall direction and normal
                Vector3 wallDir = (end - start).normalized;
                Vector3 wallNormal = Vector3.Cross(wallDir, Vector3.up);

                // Calculate wall tangents
                Vector3 wallTangent = Vector3.Cross(Vector3.up, wallNormal).normalized;

                // Calculate vertices for the wall segment
                Vector3 topLeft = start + Vector3.up * wallHeight;
                Vector3 topRight = end + Vector3.up * wallHeight;
                Vector3 bottomLeft = start;
                Vector3 bottomRight = end;

                // Extrude vertices for thickness
                topLeft += wallNormal * wallThickness;
                topRight += wallNormal * wallThickness;
                bottomLeft += wallNormal * wallThickness;
                bottomRight += wallNormal * wallThickness;

                // Add vertices to the list
                wallVertices.Add(topLeft);
                wallVertices.Add(topRight);
                wallVertices.Add(bottomLeft);
                wallVertices.Add(bottomRight);

                // Add UVs for texturing
                wallUVs.Add(new Vector2(0, wallHeight));
                wallUVs.Add(new Vector2(Vector3.Distance(start, end) / squareSize, wallHeight));
                wallUVs.Add(new Vector2(0, 0));
                wallUVs.Add(new Vector2(Vector3.Distance(start, end) / squareSize, 0));

                // Add triangles
                int startIndex = wallVertices.Count - 4;
                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }

        // Create the mesh
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        wallMesh.uv = wallUVs.ToArray();
        wallMesh.RecalculateNormals();

        // Assign the mesh to the walls object
        walls.mesh = wallMesh;

        // Add mesh collider
        MeshCollider wallCollider = gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }

    public void AssignTagsToWalls(int mapWidth, int mapHeight)
    {
        // Your logic to assign tags
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Wall"))
            {
                if (IsEdgeWall(child.position, mapWidth, mapHeight))
                {
                    child.tag = "EdgeWall";
                }
                else
                {
                    child.tag = "InnerWall";
                }
            }
        }
    }

    bool IsEdgeWall(Vector3 position, int width, int height)
    {
        // logic to determine if a wall is an edge wall or inner wall
        // assume walls close to the edges of the map are edge walls
        float edgeThreshold = 5f; // Adjust based on your map size
        return position.x < edgeThreshold || position.x > width - edgeThreshold || position.z < edgeThreshold || position.z > height - edgeThreshold;
    }

    void CreateFloorMesh(int[,] map, float squareSize)
    {
        Mesh floorMesh = new Mesh();
        floor.mesh = floorMesh;

        List<Vector3> floorVertices = new List<Vector3>();
        List<int> floorTriangles = new List<int>();

        // Calculate the size of the floor mesh based on the map dimensions and square size
        Vector2 floorSize = new Vector2(map.GetLength(0) * squareSize, map.GetLength(1) * squareSize);
        Vector3 floorCenter = new Vector3(floorSize.x / 2, 0, floorSize.y / 2); // Center of the floor mesh

        // Add vertices for the entire floor
        floorVertices.Add(new Vector3(0, 0, 0) - floorCenter); // Bottom left
        floorVertices.Add(new Vector3(floorSize.x, 0, 0) - floorCenter); // Bottom right
        floorVertices.Add(new Vector3(0, 0, floorSize.y) - floorCenter); // Top left
        floorVertices.Add(new Vector3(floorSize.x, 0, floorSize.y) - floorCenter); // Top right

        // Add thickness to the floor (adjust as needed)
        float floorThickness = 0.1f;
        for (int i = 0; i < floorVertices.Count; i++)
        {
            floorVertices[i] += Vector3.down * floorThickness;
        }

        // Add triangles for the entire floor
        floorTriangles.Add(0);
        floorTriangles.Add(2);
        floorTriangles.Add(3);
        floorTriangles.Add(3);
        floorTriangles.Add(1);
        floorTriangles.Add(0);

        // Loop through the map to determine which parts of the floor should be removed
        for (int x = 0; x < map.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < map.GetLength(1) - 1; y++)
            {
                // Check if the area is occupied by walls
                if (!IsAvailableArea(x, y, map))
                {
                    // Calculate the position of the wall
                    Vector3 wallPosition = new Vector3(-map.GetLength(0) / 2 + x * squareSize, 0, -map.GetLength(1) / 2 + y * squareSize);

                    // Calculate the vertices of the floor to be removed around the wall
                    Vector3 bottomLeft = wallPosition;
                    Vector3 bottomRight = wallPosition + new Vector3(squareSize, 0, 0);
                    Vector3 topLeft = wallPosition + new Vector3(0, 0, squareSize);
                    Vector3 topRight = wallPosition + new Vector3(squareSize, 0, squareSize);

                    // Remove the floor vertices and triangles around the wall
                    floorVertices.Remove(bottomLeft - floorCenter);
                    floorVertices.Remove(bottomRight - floorCenter);
                    floorVertices.Remove(topLeft - floorCenter);
                    floorVertices.Remove(topRight - floorCenter);

                    // Remove the floor triangles around the wall
                    floorTriangles.RemoveAll(t => t == floorVertices.Count); // Remove triangle 0
                    floorTriangles.RemoveAll(t => t == floorVertices.Count); // Remove triangle 1
                    floorTriangles.RemoveAll(t => t == floorVertices.Count); // Remove triangle 2
                    floorTriangles.RemoveAll(t => t == floorVertices.Count); // Remove triangle 3
                    floorTriangles.RemoveAll(t => t == floorVertices.Count); // Remove triangle 4
                    floorTriangles.RemoveAll(t => t == floorVertices.Count); // Remove triangle 5
                }
            }
        }

        floorMesh.vertices = floorVertices.ToArray();
        floorMesh.triangles = floorTriangles.ToArray();
        floorMesh.RecalculateNormals();
    }

    bool IsAvailableArea(int x, int y, int[,] map)
    {
        // Check if the coordinates are within the bounds of the map
        if (x >= 0 && x < map.GetLength(0) - 1 && y >= 0 && y < map.GetLength(1) - 1)
        {
            // Check if the area is not occupied by walls (assuming 0 represents empty space and 1 represents a wall)
            if (map[x, y] == 0 && map[x + 1, y] == 0 && map[x, y + 1] == 0 && map[x + 1, y + 1] == 0)
            {
                return true;
            }
        }
        return false;
    }

    void Generate2DColliders()
    {
        EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D>();
        for (int i = 0; i < currentColliders.Length; i++)
        {
            Destroy(currentColliders[i]);
        }

        CalculateMeshOutlines();

        foreach (List<int> outline in outlines)
        {
            EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            Vector2[] edgePoints = new Vector2[outline.Count];

            for (int i = 0; i < outline.Count; i++)
            {
                edgePoints[i] = new Vector2(vertices[outline[i]].x, vertices[outline[i]].z);
            }

            edgeCollider.points = edgePoints;
        }
    }

    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 2:
                MeshFromPoints(square.centreRight, square.bottomRight, square.centreBottom);
                break;
            case 4:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 5:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 4 point:
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
        }
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);
    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    void CalculateMeshOutlines()
    {

        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if (!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }

    class Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public int this[int i]
        {
            get
            {
                return vertices[i];
            }
        }

        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }

    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            if (topLeft.active)
                configuration += 8;
            if (topRight.active)
                configuration += 4;
            if (bottomRight.active)
                configuration += 2;
            if (bottomLeft.active)
                configuration += 1;
        }
    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
        {
            active = _active;
            above = new Node(position + Vector3.forward * squareSize / 2);
            right = new Node(position + Vector3.right * squareSize / 2);
        }
    }
}
