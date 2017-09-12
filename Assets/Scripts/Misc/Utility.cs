using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour {

    public static float EuclidianDistance(Point x1, Point x2)
    {
       return Mathf.Sqrt(((x1.x - x2.x) * (x1.x - x2.x) + (x1.y - x2.y) * (x1.y - x2.y)));
    }

    public static float EuclidianDistanceFromCenter(int x, int y)
    {
        int centerX = LevelGenerator.Instance.mapDimensions.height / 2 ;
        int centerY = LevelGenerator.Instance.mapDimensions.width / 2;

        return EuclidianDistance(new Point(x, y), new Point(centerX, centerY));
    }

    public static int ManhattanDistance(Point start, Point end)
    {
        int deltaX = Mathf.Abs(start.x - end.x);
        int deltaY = Mathf.Abs(start.y - end.y);

        // Multiply with a scale D if needed
        return (deltaX + deltaY);
    }

    public static float[] AddSameLengthArrays(float[] firstArray, float[] secondArray, float weightOfSecondArray)
    {
        float[] newArray = new float[firstArray.Length];
        for (int i = 0; i < newArray.Length; i++)
        {
            newArray[i] = firstArray[i] + (weightOfSecondArray * secondArray[i]);
        }
        return newArray;
    }

    public static float[] ClampPerlinValues(float[] grid, float min = 0, float max = 1)
    {
        for (int i = 0; i < grid.Length; i++)
        {
            if (grid[i] > max) grid[i] = max;
            if (grid[i] < min) grid[i] = min;
        }
        return grid;
    }

    public static float[] StretchValuesToZeroAndOne(float[] grid, float minValue, float maxValue)
    {
        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = Mathf.InverseLerp(minValue, maxValue, grid[i]);
        }

        return grid;
    }

    public static float GetElevationDifference(Point start, Point end)
    {
        if (LevelGenerator.Instance)
        {
            float[] elevationMap = LevelGenerator.Instance.elevationMap;
            return elevationMap[start.gridIndex] - elevationMap[end.gridIndex];
        }
        else
        {
            Debug.LogWarning("RoadGenerator: LevelGenerator.Instance does not exist!");
            return 0f;
        }
    }
}

public class Point
{
    public int x;
    public int y;
    public int gridIndex {
        get
        {
            return y * LevelGenerator.Instance.mapDimensions.width + x;
        }
        private set
        {
            gridIndex = value;
        }
    }

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool IsSamePoint(Point p)
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

    public Point GetRandomOffset(int maxOffsetValue,int minDistance = 0)
    {
        Point newPoint;
        do
        {
            Point offset = new Point(Random.Range(-maxOffsetValue, maxOffsetValue), Random.Range(-maxOffsetValue, maxOffsetValue));
            newPoint = this + offset;
        } while (!newPoint.IsInsideGrid() || Utility.EuclidianDistance(this,newPoint) < minDistance);

        return newPoint;
        
    }

    public Point GetRandomPointInCircle(int minDistance)
    {
        float radius = minDistance * (Random.value + 1); //+1 so minDistance gets multiplied by 1 - 2
        float angle = 2 * Mathf.PI * Random.value;
        int newX = (int)(x + radius * Mathf.Cos(angle));
        int newY = (int)(y + radius * Mathf.Sin(angle));
        return new Point(newX, newY);
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

