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
    private static List<Point> activeSamplePoints;
    private static List<TerrainType> terrainToDistributeOn;

    public static List<Point> Distribute(TerrainType[] map, PoissonDiskParameters parameters, List<TerrainType> allowedTerrain)
    {
        grid = map;
        param = parameters;
        dimensions = LevelGenerator.Instance.mapDimensions;
        terrainToDistributeOn = allowedTerrain;

        List<Point> samplePoints = new List<Point>();

        activeSamplePoints = new List<Point>();
        Point initialSamplePoint;
        //InitialSample
        do
        {
            initialSamplePoint = Point.GetRandomPoint();
        }
        while (!IsProperTerrainOnPoint(initialSamplePoint));

        activeSamplePoints.Add(initialSamplePoint);
        samplePoints.Add(initialSamplePoint);

        while (activeSamplePoints.Count > 0)
        {
            Point index = activeSamplePoints[Random.Range(0, activeSamplePoints.Count)];

            bool noSuitablePointFound = true;

            for(int k = 0; k < param.sampleSize; k++)
            {
                Point offset;
                do
                {
                    offset = Point.GetRandomOffset(index, 2 * param.radius, param.radius);
                } while (!IsProperTerrainOnPoint(offset));

                if (!CheckNeighboursForSamples(offset, samplePoints))
                {
                    activeSamplePoints.Add(offset);
                    samplePoints.Add(offset);
                    noSuitablePointFound = false;
                }
            }
            if(noSuitablePointFound) activeSamplePoints.Remove(index);

        }

        return samplePoints;
    }


    private static bool CheckNeighboursForSamples(Point point, List<Point> allPoints)
    {
        for(int xOffset = -param.radius; xOffset <= param.radius; xOffset++)
        {
            for(int yOffset = -param.radius; yOffset <= param.radius; yOffset++)
            {
                Point offset = point + new Point(xOffset, yOffset);
                if (offset.IsInsideGrid())
                {
                    if (allPoints.Contains(offset))
                    {
                        if(Utility.ManhattanDistance(point,offset) < param.radius)
                        {
                            return true;
                        }
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
        if (terrainToDistributeOn.Contains(grid[point.y * dimensions.width + point.x]))
        {
            return true;
        }
        else return false;
    }
}
