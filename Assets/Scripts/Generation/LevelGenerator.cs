using System;
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
    [Header("Tiles")]
    public Biome[] biomePrefabs;
    public int tileSize;
    [Header("Perlin Parameters")]
    public bool showDebugText;
    public ElevationTier[] perlinBiomeSettings;
    public Renderer debugElevationPlane;
    public Renderer debugMoisturePlane;
    public PerlinMapParameters elevationSettings;
    public PerlinMapParameters moistureSettings;

    [Header("Seed Growth Parameters")]
    public SeedGrowthParameters forestParam;
    public SeedGrowthParameters cities;
    [Header("Road Generation Parameters")]
    public RoadGeneratorParameters roadParam;
    public RoadGeneratorParameters riversParam;
    [Header("PossionDisc Parametrs")]
    public PoissonDiscParameters poissonParam;
    //
    public static LevelGenerator Instance { get; private set; }

    private Dictionary<TerrainType, GameObject> prefabDictionary;
    private float[] elevationMap;
    private float[] moistureMap;

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
        elevationMap = PerlinMapGenerator.GeneratePerlinMap(mapDimensions.width, mapDimensions.height, elevationSettings);
        moistureMap = PerlinMapGenerator.GeneratePerlinMap(mapDimensions.width, mapDimensions.height, moistureSettings);

        terrainMap = CombinePerlinMaps();

        //terrainMap = SeedGrowth.PopulateGrid(terrainMap, forestParam, mapDimensions);
        //terrainMap = SeedGrowth.PopulateGrid(terrainMap, cities, mapDimensions);
        
        //terrainMap = RoadGenerator.GenerateRoads(terrainMap, riversParam);
        //terrainMap = RoadGenerator.GenerateRoads(terrainMap, roadParam);

        if (showDebugText) ShowPerlinOnTexture();
        InitializePerlinMap();
    }

    private void GenerateDebugLevel()
    {
        terrainMap = new TerrainType[mapDimensions.width * mapDimensions.height];
        for(int i = 0; i < mapDimensions.width * mapDimensions.height; i++)
        {
            terrainMap[i] = TerrainType.Grass;
        }

        List<Point> forestPoints = PoissonDisc.Distribute(terrainMap, poissonParam);
        foreach (Point p in forestPoints)
        {
            terrainMap[p.y * mapDimensions.width + p.x] = TerrainType.Forest;
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
            int x = (i % mapDimensions.width);
            int y = Mathf.FloorToInt(i / mapDimensions.width);
            spawnPos = new Vector3(x, y, 0);

            GameObject tile = Instantiate(prefabDictionary[terrainMap[i]], parentGO.transform);
            Tile newTile = tile.GetComponent<Tile>();
            newTile.Setup(x, y, spawnPos);
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

    private void ShowPerlinOnTexture() //Debug Method
    {
        int width = mapDimensions.width;
        int height = mapDimensions.height;

        Texture2D elevation = new Texture2D(mapDimensions.width, mapDimensions.height);
        Texture2D moisture = new Texture2D(mapDimensions.width, mapDimensions.height);
        Color[] colorMapElevation = new Color[mapDimensions.width * mapDimensions.height];
        Color[] colorMapMoisture = new Color[mapDimensions.width * mapDimensions.height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int coordiante = y * width + x;
                colorMapElevation[coordiante] = Color.Lerp(Color.black, Color.white, elevationMap[coordiante]);
                colorMapMoisture[coordiante] = Color.Lerp(Color.black, Color.white, moistureMap[coordiante]);
            }
        }
        moisture.SetPixels(colorMapMoisture);
        elevation.SetPixels(colorMapElevation);
        moisture.Apply();
        elevation.Apply();

        if (debugMoisturePlane)
        {
            debugMoisturePlane.gameObject.SetActive(true);
            debugMoisturePlane.material.mainTexture = moisture;
        }
        if (debugElevationPlane)
        {
            debugElevationPlane.gameObject.SetActive(true);
            debugElevationPlane.material.mainTexture = elevation;
        }
    }

    public TerrainType TerrainAtPoint(Point p)
    {
        return terrainMap[p.y * mapDimensions.width + p.x];
    }

}