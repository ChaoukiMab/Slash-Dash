//using System.Collections.Generic;
//using UnityEngine;

//public static class AStarPathfinding
//{
//    public static List<Coord> FindPath(Coord start, Coord end, int[,] map)
//    {
//        List<Coord> openSet = new List<Coord>();
//        HashSet<Coord> closedSet = new HashSet<Coord>();
//        openSet.Add(start);

//        Dictionary<Coord, Coord> cameFrom = new Dictionary<Coord, Coord>();
//        Dictionary<Coord, int> gScore = new Dictionary<Coord, int>();
//        Dictionary<Coord, int> fScore = new Dictionary<Coord, int>();

//        gScore[start] = 0;
//        fScore[start] = Heuristic(start, end);

//        while (openSet.Count > 0)
//        {
//            Coord current = GetLowestFScore(openSet, fScore);

//            if (current.Equals(end))
//                return ReconstructPath(cameFrom, current);

//            openSet.Remove(current);
//            closedSet.Add(current);

//            foreach (Coord neighbor in GetNeighbors(current, map))
//            {
//                if (closedSet.Contains(neighbor))
//                    continue;

//                int tentativeGScore = gScore[current] + 1;

//                if (!openSet.Contains(neighbor))
//                    openSet.Add(neighbor);
//                else if (tentativeGScore >= gScore[neighbor])
//                    continue;

//                cameFrom[neighbor] = current;
//                gScore[neighbor] = tentativeGScore;
//                fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, end);
//            }
//        }

//        return new List<Coord>(); // Return an empty path if no path found
//    }

//    private static Coord GetLowestFScore(List<Coord> openSet, Dictionary<Coord, int> fScore)
//    {
//        Coord lowest = openSet[0];
//        int lowestScore = fScore.ContainsKey(lowest) ? fScore[lowest] : int.MaxValue;

//        foreach (Coord coord in openSet)
//        {
//            int score = fScore.ContainsKey(coord) ? fScore[coord] : int.MaxValue;
//            if (score < lowestScore)
//            {
//                lowest = coord;
//                lowestScore = score;
//            }
//        }

//        return lowest;
//    }

//    private static List<Coord> GetNeighbors(Coord coord, int[,] map)
//    {
//        List<Coord> neighbors = new List<Coord>();

//        for (int x = -1; x <= 1; x++)
//        {
//            for (int y = -1; y <= 1; y++)
//            {
//                if (x == 0 && y == 0)
//                    continue;

//                int checkX = coord.tileX + x;
//                int checkY = coord.tileY + y;

//                if (checkX >= 0 && checkX < map.GetLength(0) && checkY >= 0 && checkY < map.GetLength(1))
//                {
//                    neighbors.Add(new Coord(checkX, checkY));
//                }
//            }
//        }

//        return neighbors;
//    }

//    private static int Heuristic(Coord a, Coord b)
//    {
//        return Mathf.Abs(a.tileX - b.tileX) + Mathf.Abs(a.tileY - b.tileY);
//    }

//    private static List<Coord> ReconstructPath(Dictionary<Coord, Coord> cameFrom, Coord current)
//    {
//        List<Coord> totalPath = new List<Coord> { current };

//        while (cameFrom.ContainsKey(current))
//        {
//            current = cameFrom[current];
//            totalPath.Add(current);
//        }

//        totalPath.Reverse();
//        return totalPath;
//    }
//}

//public struct Coord
//{
//    public int tileX;
//    public int tileY;

//    public Coord(int x, int y)
//    {
//        tileX = x;
//        tileY = y;
//    }

//    public override bool Equals(object obj)
//    {
//        if (!(obj is Coord))
//            return false;

//        Coord coord = (Coord)obj;
//        return tileX == coord.tileX && tileY == coord.tileY;
//    }

//    public override int GetHashCode()
//    {
//        return tileX.GetHashCode() ^ tileY.GetHashCode();
//    }
//}
