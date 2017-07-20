using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour {

    public static float EuclidianDistance(Point x1, Point x2)
    {
       return Mathf.Sqrt(((x1.x - x2.x) * (x1.x - x2.x) + (x1.y - x2.y) * (x1.y - x2.y)));
    }

    public static int ManhattanDistance(Point start, Point end)
    {
        int deltaX = Mathf.Abs(start.x - end.x);
        int deltaY = Mathf.Abs(start.y - end.y);

        // Multiply with a scale D if needed
        return (deltaX + deltaY);
    }

    public static float GetGridValue(float[] grid, Point p)
    {
        return grid[p.y * LevelGenerator.Instance.mapDimensions.width + p.x];
    }

    public static bool IsInsideGrid(Point p)
    {
        MapDimensions map = LevelGenerator.Instance.mapDimensions;
        return p.x >= 0 && p.x < map.width && p.y >= 0 && p.y < map.height;
    }
}
