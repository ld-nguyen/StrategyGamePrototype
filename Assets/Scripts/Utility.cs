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

    public delegate bool Condition(Point origin, Point offset);

    public static bool CheckNeighbours(Point p,int kernelSize,Condition breakCondition)
    {
        for(int xOffset = -kernelSize; xOffset <= kernelSize; xOffset++)
        {
            for(int yOffset = -kernelSize; yOffset <= kernelSize; yOffset++)
            {
                Point offset = p + new Point(xOffset, yOffset);
                if (!IsInsideGrid(offset)) continue;
                else
                {
                    if (breakCondition(p,offset))
                    {
                        return true;
                    }
                } 
            }
        }
        return false;
    }

}

public struct Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool Equals(Point p)
    {
        return x == p.x && y == p.y;
    }

    public Point GetNeighbour(int xOffset, int yOffset)
    {
        return new Point(x + xOffset, y + yOffset);
    }

    public static Point GetRandomPoint()
    {
        return new Point(Random.Range(0, LevelGenerator.Instance.mapDimensions.width), Random.Range(0, LevelGenerator.Instance.mapDimensions.height));
    }

    public static Point GetRandomOffset(Point p, int maxOffsetValue)
    {
        Point newPoint;
        do
        {
            newPoint = p + new Point(Random.Range(-maxOffsetValue, maxOffsetValue), Random.Range(-maxOffsetValue, maxOffsetValue));
        } while (!newPoint.IsInsideGrid());

        return newPoint;
        
    }

    public override string ToString()
    {
        return " x: " + x + " y: " + y;
    }

    public Vector3 GetVector3()
    {
        return new Vector3(x, y, 0);
    }

    public static Point operator +(Point a, Point b)
    {
        return new Point(a.x + b.x, a.y + b.y);
    }


    public bool IsInsideGrid()
    {
        MapDimensions map = LevelGenerator.Instance.mapDimensions;
        if (x >= 0 && x < map.width && y >= 0 && y < map.height)
        {
            return true;
        }
        else return false;
    }

    
}

