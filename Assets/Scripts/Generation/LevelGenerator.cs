using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ElevationBiome : IComparable
{
    public float perlinElevationThreshold;
    public SubBiome[] subBiomes;

    public int CompareTo(object obj)
    {
        ElevationBiome i = (ElevationBiome)obj;
        return perlinElevationThreshold.CompareTo(i.perlinElevationThreshold);
    }
}

[System.Serializable]
public class SubBiome : IComparable
{
    public TerrainType type;
    public GameObject prefab;
    public float perlinMoistureThreshold;

    public int CompareTo(object obj)
    {
        SubBiome i = (SubBiome)obj;
        return perlinMoistureThreshold.CompareTo(i.perlinMoistureThreshold);
    }
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
    None // Used for debug purposes
}

[System.Serializable]
public struct PerlinMapParameters
{
    public int octaves;
    [Range(2, 100)]
    public float scale;
    [Range(1, 2)]
    public float lacunarity;
    [Range(0, 1)]
    public float persistence;
}

public class LevelGenerator : MonoBehaviour
{
    public enum LevelGenerationType { Debug, Procedual }
    public LevelGenerationType levelGeneration;

    [Header("General Settings")]
    public int mapWidth;
    public int mapHeight;
    public GameObject parentGO;
    [Header("Tiles")]
    public ElevationBiome[] perlinTilePool;
    public int tileSize;
    [Header("Perlin Parameters")]
    public bool showDebugText;
    public Renderer debugElevationPlane;
    public Renderer debugMoisturePlane;
    public PerlinMapParameters elevationSettings;
    public PerlinMapParameters moistureSettings;

    //
    private float[] elevationMap;
    private float[] moistureMap;

    private TerrainType[] terrainMap;

    void Awake()
    {
        //Presort Biome Arrays
        Array.Sort(perlinTilePool);
        foreach(ElevationBiome e in perlinTilePool)
        {
            Array.Sort(e.subBiomes);
        }
    }

    void Start()
    {
        switch (levelGeneration)
        {
            case LevelGenerationType.Debug:
                break;
            case LevelGenerationType.Procedual:
                GenerateProcedualLevel();
                break;
        }
    }

    private void GenerateProcedualLevel()
    {
        elevationMap = PerlinMapGenerator.GeneratePerlinMap(mapWidth, mapHeight, elevationSettings);
        moistureMap = PerlinMapGenerator.GeneratePerlinMap(mapWidth, mapHeight, moistureSettings);
        terrainMap = CombinePerlinMaps();

        if (showDebugText) ShowPerlinOnTexture();
        InitializePerlinMap();
    }

    private TerrainType[] CombinePerlinMaps()
    {
        TerrainType[] perlinTerrain = new TerrainType[mapWidth * mapHeight];
        for(int i = 0; i < perlinTerrain.Length; i++)
        {
            perlinTerrain[i] = GetBiome(elevationMap[i], moistureMap[i]).type;
        }
        return perlinTerrain;
    }

    //TODO: Fix shit naming
    private SubBiome GetBiome(float elevationValue, float moistureValue)
    {
        for (int i = 0; i < perlinTilePool.Length; i++)
        {
            if (elevationValue <= perlinTilePool[i].perlinElevationThreshold)
            {
                for(int j = 0; j < perlinTilePool[i].subBiomes.Length; j++)
                {
                    if (moistureValue <= perlinTilePool[i].subBiomes[j].perlinMoistureThreshold)
                    {
                        return perlinTilePool[i].subBiomes[j];
                    }
                }
            }
        }
        return null;
    }
    
    private SubBiome GetBiome(TerrainType desiredType)
    {
        foreach(ElevationBiome elevationTier in perlinTilePool)
        {
            foreach(SubBiome moistureBiome in elevationTier.subBiomes)
            {
                if(moistureBiome.type == desiredType)
                {
                    return moistureBiome;
                }
            }
        }
        return null;
    }

    public void InitializePerlinMap()
    {
        Vector3 spawnPos;
        for (int i = 0; i < terrainMap.Length; i++)
        {
            int x = (i % mapWidth);
            int y = Mathf.FloorToInt(i / mapWidth);
            spawnPos = new Vector3(x, y, 0);

            GameObject tile = Instantiate(GetBiome(terrainMap[i]).prefab, parentGO.transform);
            tile.transform.localPosition = spawnPos;
        }
    }
    public void RegenerateTerrain()
    {
        foreach (Transform child in parentGO.transform)
        {
            Destroy(child.gameObject);
        }

        GenerateProcedualLevel();
    }
    private void ShowPerlinOnTexture()
    {
        int width = mapWidth;
        int height = mapHeight;

        Texture2D elevation = new Texture2D(mapWidth, mapHeight);
        Texture2D moisture = new Texture2D(mapWidth, mapHeight);
        Color[] colorMapElevation = new Color[mapWidth * mapHeight];
        Color[] colorMapMoisture = new Color[mapWidth * mapHeight];

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

        debugMoisturePlane.material.mainTexture = moisture;
        debugElevationPlane.material.mainTexture = elevation;
    }

}