using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PoissonDiskParameters
{
    public int radius;
    public int sampleSize;
}
public class PoissonDisc {

    private static TerrainType[] grid;
    private static PoissonDiskParameters param;
    private static MapDimensions dimensions;
    private static List<TerrainType> terrainsToDistributeOn;

    private static float cellSize;
    private static bool[] backgroundGrid;
    private static int backgroundColumns;
    private static int backgroundRows;

    public static List<Point> Distribute(TerrainType[] map, PoissonDiskParameters parameters, List<TerrainType> allowedTerrain)
    {
        grid = map;
        param = parameters;
        dimensions = LevelGenerator.Instance.mapDimensions;
        terrainsToDistributeOn = allowedTerrain;

        //step 0
        cellSize = param.radius / Mathf.Sqrt(2);
        backgroundColumns = Mathf.CeilToInt(dimensions.width / cellSize);
        backgroundRows = Mathf.CeilToInt(dimensions.height / cellSize);
        backgroundGrid = new bool[backgroundColumns*backgroundRows];
        
        List<Point> samplePoints = new List<Point>();
        List<Point> activeSamplePoints = new List<Point>();

        //Step 1
        Point initialSamplePoint;
        do
        {
            initialSamplePoint = Point.GetRandomPoint();
        }
        while (!IsProperTerrainOnPoint(initialSamplePoint));

        activeSamplePoints.Add(initialSamplePoint);
        samplePoints.Add(initialSamplePoint);
        SetSampleInBackgroundGridAt(initialSamplePoint);
        //Step 2
        while (activeSamplePoints.Count > 0)
        {
            Point index = activeSamplePoints[Random.Range(0, activeSamplePoints.Count)];


            bool noSuitablePointFound = true;
            for(int k = 0; k < param.sampleSize; k++)
            {
                Point offset = index.GetRandomPointInCircle(param.radius);
                if (!activeSamplePoints.Contains(offset) && CheckPoint(offset)
                    && !CheckNeighboursForSamples(offset))
                {
                    activeSamplePoints.Add(offset);
                    samplePoints.Add(offset);
                    SetSampleInBackgroundGridAt(offset);
                    noSuitablePointFound = false;
                }
            }
            if(noSuitablePointFound) activeSamplePoints.Remove(index);
        }
        return samplePoints;
    }


    private static bool CheckNeighboursForSamples(Point sample)
    {
        Point backgroundSample = GetBackgroundGridPoint(sample); //Coodinates of sample in the background grid
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                Point backgroundOffset = backgroundSample + new Point(xOffset, yOffset);
                if (IsInsideBackgroundGrid(backgroundOffset))
                {
                    if (backgroundGrid[backgroundOffset.y * backgroundColumns + backgroundOffset.x] == true)
                    {
                        return true;
                    }
                }
            }
        }
        return false;

    }

    private static bool CheckPoint(Point point)
    {
        if (point.IsInsideGrid() && IsProperTerrainOnPoint(point)) return true;
        else return false;
    }

    private static bool IsProperTerrainOnPoint(Point point)
    {
        if (terrainsToDistributeOn.Contains(grid[point.gridIndex]))
        {
            return true;
        }
        else return false;
    }

    private static int GetBackgroundGridIndex(Point p)
    {
        Point backgroundPoint = GetBackgroundGridPoint(p);
        return backgroundPoint.y * backgroundColumns + backgroundPoint.x;
    }

    private static Point GetBackgroundGridPoint(Point gamePoint)
    {
        int backgroundX = (int)(gamePoint.x / cellSize);
        int backgroundY = (int)(gamePoint.y / cellSize);

        return new Point(backgroundX, backgroundY);
    }

    private static bool IsInsideBackgroundGrid(Point b)
    {
        if (b.x < backgroundColumns && b.x >= 0 && b.y >= 0 && b.y < backgroundRows)
        {
            return true;
        }
        else return false;
    }

    private static void SetSampleInBackgroundGridAt(Point p)
    {
        backgroundGrid[GetBackgroundGridIndex(p)] = true;
    }
}
