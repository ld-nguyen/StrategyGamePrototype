using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//TODO: Move some parts to utility class
[System.Serializable]
public struct IntegerRange
{
    public int min;
    public int max;

    public int GetRandomValue()
    {
        return Random.Range(min, max);
    }
}

[System.Serializable]
public struct SeedGrowthParameters
{
    public int amountOfSeeds;
    public float placementChanceBaseChance;
    public IntegerRange kernelSize;
    public IntegerRange amountOfGrowthSteps;
    public TerrainType desiredTerrain;
    public List<TerrainType> allowedBiomes;
}

public class SeedGrowth : MonoBehaviour {

    private static TerrainType[] grid;
    private static SeedGrowthParameters parameters;
    private static MapDimensions map;

    public static TerrainType[] PopulateGrid(TerrainType[] originalMap, SeedGrowthParameters param, MapDimensions mapSettings)
    {
        grid = originalMap;
        map = mapSettings;
        parameters = param;

        Point seedLocation;
        for(int seed = 0; seed < param.amountOfSeeds; seed++)
        {
            do
            {
                seedLocation = new Point(Random.Range(0, mapSettings.width), Random.Range(0, mapSettings.height));
            } while (!IsCorrectTile(seedLocation));    

            for(int step = 0; step < param.amountOfGrowthSteps.GetRandomValue(); step++)
            {
                int kernelSize = parameters.kernelSize.GetRandomValue();
                for(int xOffset = -kernelSize ; xOffset <= kernelSize; xOffset++)
                {
                    for(int yOffset = - kernelSize; yOffset <= kernelSize; yOffset++)
                    {
                        Point neighbour = new Point(seedLocation.x + xOffset, seedLocation.y + yOffset);
                        if (IsInsideGrid(neighbour) && IsCorrectTile(neighbour))
                        {
                            float chanceOffset = CalculatePlacementChance(seedLocation, neighbour, kernelSize); //Lower chance for placing a forest tile the farther you are away
                            if (Random.Range(0f, 1f) < chanceOffset)
                            {
                                grid[neighbour.y * mapSettings.width + neighbour.x] = parameters.desiredTerrain;
                            }
                        }
                    }
                }
                //Setting next starting points around the edges of the created area
                Point nextPoint;
                int xRandomOffset;
                int yRandomOffset;
                do
                {
                    xRandomOffset = Random.Range(-kernelSize, kernelSize + 1);
                    yRandomOffset = Random.Range(-kernelSize, kernelSize + 1);
                    nextPoint = new Point(seedLocation.x + xRandomOffset, seedLocation.y + yRandomOffset);
                } while (!IsInsideGrid(nextPoint) && (Mathf.Abs((float) xRandomOffset) == kernelSize || Mathf.Abs((float)yRandomOffset) == kernelSize));

                seedLocation = nextPoint;
            }
        }
        return grid;
    }

    private static float CalculatePlacementChance(Point origin, Point goal, int kernelSize)
    {
        float distance = Utility.Distance(origin, goal);
        float maxPossibleDistance = Utility.Distance(origin, new Point(origin.x + kernelSize, origin.y + kernelSize));
        float lerp = 1 - Mathf.InverseLerp(0, maxPossibleDistance, distance);
        return Mathf.Clamp(lerp + parameters.placementChanceBaseChance,0,1);
    }


    //TODO: Put into Utiity class
    private static bool IsInsideGrid(Point p)
    {
        if (p.x >= 0 && p.x < map.width && p.y >= 0 && p.y < map.height)
        {
            return true;
        }
        else return false;
    }

    public static bool IsCorrectTile(Point coords)
    {
        int index = coords.y * map.width + coords.x;

        if (grid[index] != parameters.desiredTerrain && parameters.allowedBiomes.Contains(grid[index]))
        {
            return true;
        }
        else return false;
    }

}
