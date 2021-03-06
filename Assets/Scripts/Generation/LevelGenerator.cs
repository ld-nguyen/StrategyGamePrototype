﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//TODO: Move some parts to utility class
[System.Serializable]
public class ElevationTier : IComparable
{
    public float perlinElevationThreshold;
    public MoistureTier[] subBiomes;

    public int CompareTo(object obj)
    {
        ElevationTier i = (ElevationTier)obj;
        return perlinElevationThreshold.CompareTo(i.perlinElevationThreshold);
    }
}

[System.Serializable]
public class MoistureTier : IComparable
{
    public float perlinMoistureThreshold;
    public TerrainType biome;

    public int CompareTo(object obj)
    {
        MoistureTier i = (MoistureTier)obj;
        return perlinMoistureThreshold.CompareTo(i.perlinMoistureThreshold);
    }
}

[System.Serializable]
public struct Biome
{
    public TerrainType type;
    public GameObject prefab;
}

public enum TerrainType
{
    Water,
    Grass,
    Hill,
    Mountain,
    Forest,
    Swamp,
    Beach,
    Desert,
    Road,
    City,
    DeepWater,
    River,
    None // Used for debug purposes
}

[System.Serializable]
public struct MapDimensions
{
    public int width;
    public int height;
}

public class LevelGenerator : MonoBehaviour
{
    public enum LevelGenerationType { Debug, Procedual }
    public LevelGenerationType levelGeneration;

    //INSPECTOR SETTINGS
    [Header("General Settings")]
    public MapDimensions mapDimensions;
    public GameObject parentGO;
    public bool useGraphicAsBase;
    public Texture2D baseGraphic;
    [Range(0f, 1f)]
    public float perlinWeightOnBaseGrafic;
    [Header("Tiles")]
    public Biome[] biomePrefabs;
    public int tileSize;
    public float maxColorOffsetRange = 0.05f;
    [Header("Perlin Parameters")]
    public ElevationTier[] perlinBiomeSettings;
    public PerlinMapParameters elevationSettings;
    public PerlinMapParameters moistureSettings;
    [Header("Seed Growth Parameters")]
    public SeedGrowthParameters forestParam;
    public SeedGrowthParameters citiesParam;
    [Header("Road Generation Parameters")]
    public RoadGeneratorParameters roadParam;
    public RoadGeneratorParameters riversParam;
    //[Header("Debug")]
    //public bool showDebugTextureForPerlin;
    //public Renderer debugElevationPlane;
    //public Renderer debugMoisturePlane;s
    public static LevelGenerator Instance { get; private set; }

    private Dictionary<TerrainType, GameObject> prefabDictionary;
    public float[] elevationMap { get; private set; }
    public float[] moistureMap { get; private set; }

    private TerrainType[] terrainMap;

    void Awake()
    {
        //Singleton Setup
        Instance = this;

        //Presort Biome Arrays
        Array.Sort(perlinBiomeSettings);
        foreach(ElevationTier e in perlinBiomeSettings)
        {
            Array.Sort(e.subBiomes);
        }
        //Populate Dictionary
        prefabDictionary = new Dictionary<TerrainType, GameObject>();
        foreach(Biome biome in biomePrefabs)
        {
            prefabDictionary.Add(biome.type, biome.prefab);
        }
        //Safeguard
        if (!baseGraphic && useGraphicAsBase) { useGraphicAsBase = false; }
    }

    public void StartLevelGeneration()
    {
        switch (levelGeneration)
        {
            case LevelGenerationType.Debug:
                GenerateDebugLevel();
                break;
            case LevelGenerationType.Procedual:
                GenerateProcedualLevel();
                break;
        }
    }
    private void GenerateProcedualLevel()
    {
        if (useGraphicAsBase)
        {
            elevationMap = ProcessBaseGraphicIntoArray();
            moistureMap = ProcessBaseGraphicIntoArray();
            float[] perlinElevation = PerlinMapGenerator.GeneratePerlinMap(mapDimensions.width, mapDimensions.height, elevationSettings, false);
            float[] perlinMoisture = PerlinMapGenerator.GeneratePerlinMap(mapDimensions.width, mapDimensions.height, moistureSettings, false);
            elevationMap = Utility.AddSameLengthArrays(elevationMap, perlinElevation, perlinWeightOnBaseGrafic);
            elevationMap = Utility.ClampPerlinValues(elevationMap);
            moistureMap = Utility.AddSameLengthArrays(moistureMap, perlinMoisture, perlinWeightOnBaseGrafic);
            moistureMap = Utility.ClampPerlinValues(moistureMap);
        }
        else
        {
            elevationMap = PerlinMapGenerator.GeneratePerlinMap(mapDimensions.width, mapDimensions.height, elevationSettings);
            moistureMap = PerlinMapGenerator.GeneratePerlinMap(mapDimensions.width, mapDimensions.height, moistureSettings);
        }
        terrainMap = CombinePerlinMaps();

        terrainMap = SeedGrowth.PopulateGrid(terrainMap, forestParam, mapDimensions);
        terrainMap = SeedGrowth.PopulateGrid(terrainMap, citiesParam, mapDimensions);
        
        terrainMap = RoadGenerator.GenerateRoads(terrainMap, riversParam);
        terrainMap = RoadGenerator.GenerateRoads(terrainMap, roadParam);

        InitializePerlinMap();
    }

    private void GenerateDebugLevel()
    {
        elevationMap = new float[mapDimensions.width * mapDimensions.height];
        moistureMap = new float[mapDimensions.width * mapDimensions.height];
        terrainMap = new TerrainType[mapDimensions.width * mapDimensions.height];
        for (int i = 0; i < mapDimensions.width * mapDimensions.height; i++)
        {
            terrainMap[i] = TerrainType.Grass;
            elevationMap[i] = 0.5f;
            moistureMap[i] = 0.5f;
        }

        List<Point> forestPoints = PoissonDisc.Distribute(terrainMap, forestParam.poissonSeedParameters, forestParam.allowedBiomes   );
        foreach (Point p in forestPoints)
        {
            terrainMap[p.gridIndex] = TerrainType.Forest;
        }

        InitializePerlinMap();
    }

    private TerrainType[] CombinePerlinMaps()
    {
        TerrainType[] perlinTerrain = new TerrainType[mapDimensions.width * mapDimensions.height];
        for(int i = 0; i < perlinTerrain.Length; i++)
        {
            perlinTerrain[i] = GetBiome(elevationMap[i], moistureMap[i]);
        }
        return perlinTerrain;
    }

    private TerrainType GetBiome(float elevationValue, float moistureValue)
    {
        for (int i = 0; i < perlinBiomeSettings.Length; i++)
        {
            if (elevationValue <= perlinBiomeSettings[i].perlinElevationThreshold)
            {
                for(int j = 0; j < perlinBiomeSettings[i].subBiomes.Length; j++)
                {
                    if (moistureValue <= perlinBiomeSettings[i].subBiomes[j].perlinMoistureThreshold)
                    {
                        return perlinBiomeSettings[i].subBiomes[j].biome;
                    }
                }
            }
        }
        return TerrainType.None;
    }
    public void InitializePerlinMap()
    {
        Vector3 spawnPos;
        Tile[] gameMap = new Tile[terrainMap.Length];
        for (int i = 0; i < terrainMap.Length; i++)
        {
            int x = i % mapDimensions.width;
            int y = i / mapDimensions.width;
            spawnPos = new Vector3(x, y, 0);

            GameObject tile = Instantiate(prefabDictionary[terrainMap[i]], parentGO.transform);
            Tile newTile = tile.GetComponent<Tile>();
            float colorOffset = GetColorOffset(elevationMap[i],terrainMap[i]);
            newTile.Setup(x, y, spawnPos, colorOffset);
            gameMap[i] = newTile;
        }
        GameManager.Instance.SetGameMap(gameMap);
    }
    public void RegenerateTerrain()
    {
        foreach (Transform child in parentGO.transform)
        {
            Destroy(child.gameObject);
        }
        GameManager.Instance.SetupGame();
    }
    private void ShowPerlinOnTexture(float[] arr1, Renderer plane)//Debug Method
    {
        int width = mapDimensions.width;
        int height = mapDimensions.height;

        Texture2D texture = new Texture2D(mapDimensions.width, mapDimensions.height);
        Color[] colorMap = new Color[mapDimensions.width * mapDimensions.height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int coordiante = y * width + x;
                colorMap[coordiante] = Color.Lerp(Color.black, Color.white, arr1[coordiante]);
            }
        }
        texture.SetPixels(colorMap);
        texture.Apply();

        if (plane)
        {
            plane.gameObject.SetActive(true);
            plane.material.mainTexture = texture;
        }
    }
    //Calculates a offset depending on the elevation value (val) and the position in the value range of the terrain type it is in
    private float GetColorOffset(float val, TerrainType type)
    {
        float lowerEnd = 0f;
        if (TerrainTypeIsInPerlinBiomeSettings(type))
        {
            for (int i = 0; i < perlinBiomeSettings.Length; i++)
            {
                if (val <= perlinBiomeSettings[i].perlinElevationThreshold)
                {
                    float higherEnd = perlinBiomeSettings[i].perlinElevationThreshold;
                    return Mathf.Lerp(-maxColorOffsetRange, maxColorOffsetRange, Mathf.InverseLerp(lowerEnd, higherEnd, val));
                }
                else
                {
                    lowerEnd = perlinBiomeSettings[i].perlinElevationThreshold;
                }
            }
            return 0;
        }
        else
        {
            return Mathf.Lerp(-maxColorOffsetRange, maxColorOffsetRange, val);
        }
    }
    public bool TerrainTypeIsInPerlinBiomeSettings(TerrainType terrain)
    {
        for (int i = 0; i < perlinBiomeSettings.Length; i++)
        {
            for (int j = 0; j < perlinBiomeSettings[i].subBiomes.Length; j++)
            {
                if (perlinBiomeSettings[i].subBiomes[j].biome == terrain) return true;                
            }
        }
        return false;
    }
    public TerrainType TerrainAtPoint(Point p)
    {
        return terrainMap[p.gridIndex];
    }

    public float[] ProcessBaseGraphicIntoArray()
    {
        Color[] colorValues = baseGraphic.GetPixels();
        float[] outputArray = new float[colorValues.Length];
        mapDimensions.height = baseGraphic.height;
        mapDimensions.width = baseGraphic.width;

        for (int i = 0; i < colorValues.Length; i++)
        {
            outputArray[i] = colorValues[i].grayscale;
        }
        return outputArray;
    }
}