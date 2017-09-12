using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class IntegerRange
{
    public int min;
    public int max;

    public int GetRandomValue()
    {
        return Random.Range(min, max+1); //+1 to make max inclusive in Random range
    }
}

[System.Serializable]
public class SeedGrowthParameters
{
    public int amountOfSeeds;
    public PoissonDiskParameters poissonSeedParameters;
    public IntegerRange amountOfGrowthSteps;
    public IntegerRange kernelSize;
    public float placementBaseChance;
    public TerrainType desiredTerrain;
    public List<TerrainType> allowedBiomes;
    public SeedGrowth.PlacementCondition conditionType;
}

public class SeedGrowth : MonoBehaviour {
    public enum PlacementCondition{Forest,City}
    private static TerrainType[] grid;
    private static SeedGrowthParameters parameters;
    public delegate bool ExternalPlacementCondition(Point source, Point neighbour);


    public static TerrainType[] PopulateGrid(TerrainType[] originalMap, SeedGrowthParameters param, MapDimensions mapSettings)
    {
        grid = originalMap;
        parameters = param;
        ExternalPlacementCondition condition;
        switch (param.conditionType)
        {
            case PlacementCondition.Forest:
                condition = ForestConditon;
                break;
            case PlacementCondition.City:
                condition = CityCondition;
                break;
            default:
                condition = null;
                break;
        }

        Point seedLocation;
        List<Point> seeds = new List<Point>();
        if (parameters.amountOfSeeds > 0)
        {
            seeds = PoissonDisc.Distribute(grid, param.poissonSeedParameters, param.allowedBiomes);
        }
        for(int seed = 0; seed < param.amountOfSeeds; seed++)
        {
            seedLocation = seeds[Random.Range(0,seeds.Count)];

            for(int step = 0; step < param.amountOfGrowthSteps.GetRandomValue(); step++)
            {
                int kernelSize = parameters.kernelSize.GetRandomValue();
                List<Point> createdArea = new List<Point>();
                for(int xOffset = -kernelSize ; xOffset <= kernelSize; xOffset++)
                {
                    for(int yOffset = - kernelSize; yOffset <= kernelSize; yOffset++)
                    {
                        Point neighbour = new Point(seedLocation.x + xOffset, seedLocation.y + yOffset);
                        //Checking all conditions of the neighbour
                        if (neighbour.IsInsideGrid() && IsCorrectTile(neighbour) && condition(seedLocation, neighbour))
                        {
                            float chanceOffset = CalculatePlacementChance(seedLocation, neighbour, kernelSize); //Lower chance for placing a forest tile the farther you are away
                            if (Random.Range(0f, 1f) < chanceOffset)
                            {
                                grid[neighbour.gridIndex] = param.desiredTerrain;
                                createdArea.Add(neighbour);
                            }
                        }
                    }
                }
                if (createdArea.Count > 0)
                {
                    seedLocation = createdArea[Random.Range(0, createdArea.Count)];
                } 
            }
        }
        return grid;
    }

    private static float CalculatePlacementChance(Point origin, Point goal, int kernelSize)
    {
        float distance = Utility.EuclidianDistance(origin, goal);
        float maxPossibleDistance = Utility.EuclidianDistance(origin, new Point(origin.x + kernelSize, origin.y + kernelSize));
        float lerp = 1 - Mathf.InverseLerp(0, maxPossibleDistance, distance);
        return Mathf.Clamp(lerp + parameters.placementBaseChance,0,1);
    }

    public static bool IsCorrectTile(Point coords)
    {

        if (grid[coords.gridIndex] != parameters.desiredTerrain && parameters.allowedBiomes.Contains(grid[coords.gridIndex]))
        {
            return true;
        }
        else return false;
    }

    public static bool ForestConditon(Point source, Point neighbour)
    {
        float moistureValueSource = LevelGenerator.Instance.moistureMap[source.gridIndex];
        float moistureValueNeighbour = LevelGenerator.Instance.moistureMap[neighbour.gridIndex];
        return (moistureValueNeighbour - moistureValueSource) >= 0;
    }

    public static bool CityCondition(Point source, Point neighbour)
    {
        //Prefer even terrain
        float terrainValueSource = LevelGenerator.Instance.elevationMap[source.gridIndex];
        float terrainValueNeighbour = LevelGenerator.Instance.elevationMap[neighbour.gridIndex];
        return Mathf.Abs(terrainValueSource - terrainValueNeighbour) <= 0.1;
    }

}
