using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TileSeedParameters
{
    public int amountOfSeeds;
    public int radius;
    public int sampleSize;
}

public class PoissonDisc {

    public static TerrainType[] DistributeTerrainTile(TerrainType desiredBiome, TerrainType[] map, TileSeedParameters param)
    {
        return null;
    }
}
