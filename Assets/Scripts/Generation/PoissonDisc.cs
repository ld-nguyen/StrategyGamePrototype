using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//TODO: Move some parts to utility class

[System.Serializable]
public struct PoissonDiscParameters
{
    public int amountOfSeeds;
    public int radius;
    public int sampleSize;
    public TerrainType terrainToDistributeOn;
    public TerrainType desiredNewTerrain;
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
}

public class PoissonDisc {

    private static TerrainType[] grid;
    private static PoissonDiscParameters param;
    private static MapDimensions dimensions;

    public static TerrainType[] DistributeTerrainTile(TerrainType[] map, PoissonDiscParameters parameters, MapDimensions mapSettings)
    {
        grid = map;
        param = parameters;
        dimensions = mapSettings;
        List<Vector2> activeSamplePoints = new List<Vector2>();
        Vector2 initialSamplePoint;
        //InitialSample
        do
        {
            initialSamplePoint = new Vector2(Random.Range(0, dimensions.width), Random.Range(0, dimensions.height));
        }
        while (!IsProperTerrainOnPoint(initialSamplePoint));

        activeSamplePoints.Add(initialSamplePoint);
        PlaceSample(initialSamplePoint);

        while (activeSamplePoints.Count > 0)
        {
            Vector2 index = activeSamplePoints[Random.Range(0, activeSamplePoints.Count)];
            for(int n = 0; n < param.sampleSize; n++)
            {
                Vector2 offset = Random.insideUnitCircle;
                offset.Normalize();
                offset *= Random.Range(param.radius, 2 * param.radius);
                Vector2 newSample = index + offset;

                if (CheckNeighboursForSamples(newSample))
                {
                    activeSamplePoints.Add(newSample);
                    PlaceSample(newSample);
                }
            }
        }

        return null;
    }


    private static bool CheckNeighboursForSamples(Vector2 point)
    {
        for(int xOffset = -1 ; xOffset <= 1; xOffset++)
        {
            for(int yOffset = -1; yOffset <= 1; yOffset++)
            {
                Vector2 offset = point + new Vector2(xOffset, yOffset);
                if (IsInsideGrid(offset))
                {
                    if (!IsProperTerrainOnPoint(offset))
                    {
                        if(grid[Mathf.FloorToInt(point.y) * dimensions.width + Mathf.FloorToInt(point.x)] != param.desiredNewTerrain && Vector2.Distance(point,offset) < param.radius)
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private static bool CheckPoint(Vector2 point)
    {
        if (IsInsideGrid(point) && IsProperTerrainOnPoint(point)) return true;
        else return false;
    }

    private static bool IsProperTerrainOnPoint(Vector2 point)
    {
        if (grid[Mathf.FloorToInt(point.y) * dimensions.width + Mathf.FloorToInt(point.x)] == param.terrainToDistributeOn)
        {
            return true;
        }
        else return false;
    }

    private static bool IsInsideGrid(Vector2 p)
    {
        if (p.x >= 0 && p.x < dimensions.width && p.y >= 0 && p.y < dimensions.height)
        {
            return true;
        }
        else return false;
    }

    private static void PlaceSample(Vector2 p)
    {
        int coord = Mathf.FloorToInt(p.y) * dimensions.width + Mathf.FloorToInt(p.x);
        grid[coord] = param.desiredNewTerrain;
    }
}
