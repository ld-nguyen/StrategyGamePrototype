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
    public float minimumDistance;
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
        List<Point> seeds = new List<Point>();
        if (parameters.amountOfSeeds > 0)
        {
            seeds = PoissonDisc.Distribute(grid, LevelGenerator.Instance.poissonParam, param.allowedBiomes);
        }
        for(int seed = 0; seed < param.amountOfSeeds; seed++)
        {
            //do
            //{
            //    seedLocation = new Point(Random.Range(0, mapSettings.width), Random.Range(0, mapSettings.height));
            //} while (!IsCorrectTile(seedLocation) && !IsFarEnoughFromOtherSeeds(seedLocation,otherPoints));
            // otherPoints.Add(seedLocation);

            seedLocation = seeds[Random.Range(0,seeds.Count)];

            for(int step = 0; step < param.amountOfGrowthSteps.GetRandomValue(); step++)
            {
                int kernelSize = parameters.kernelSize.GetRandomValue();
                for(int xOffset = -kernelSize ; xOffset <= kernelSize; xOffset++)
                {
                    for(int yOffset = - kernelSize; yOffset <= kernelSize; yOffset++)
                    {
                        Point neighbour = new Point(seedLocation.x + xOffset, seedLocation.y + yOffset);
                        if (neighbour.IsInsideGrid() && IsCorrectTile(neighbour))
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
                } while (!nextPoint.IsInsideGrid() && (Mathf.Abs((float) xRandomOffset) == kernelSize || Mathf.Abs((float)yRandomOffset) == kernelSize));

                seedLocation = nextPoint;
            }
        }
        return grid;
    }

    private static float CalculatePlacementChance(Point origin, Point goal, int kernelSize)
    {
        float distance = Utility.EuclidianDistance(origin, goal);
        float maxPossibleDistance = Utility.EuclidianDistance(origin, new Point(origin.x + kernelSize, origin.y + kernelSize));
        float lerp = 1 - Mathf.InverseLerp(0, maxPossibleDistance, distance);
        return Mathf.Clamp(lerp + parameters.placementChanceBaseChance,0,1);
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

    public static bool IsFarEnoughFromOtherSeeds(Point p, HashSet<Point> otherSeeds) //deprecated
    {
        if(otherSeeds.Count <= 0) { return true; }
        else
        {
            foreach(Point seed in otherSeeds)
            {
                if (Utility.EuclidianDistance(p, seed) < parameters.minimumDistance) return false;
            }
            return true;
        }
    }
}
