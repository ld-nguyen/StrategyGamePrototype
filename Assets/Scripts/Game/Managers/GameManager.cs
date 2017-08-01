using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private LevelGenerator levelGen;
    private Tile[] gameMap;

    [Header("Game Parameters")]
    public Color mouseOverTileColor;
    public Color highlightTileColor;
    public int unitCountPerSide;

    void Awake()
    {
        Instance = this;
        levelGen = LevelGenerator.Instance;
    }

    void Start()
    {
        SetupGame();
    }

    public void SetupGame()
    {
        levelGen.StartLevelGeneration();
        UnitManager.Instance.PlaceUnits();
    }

    public void SetGameMap(Tile[] map)
    {
        gameMap = map;
    }

    public Tile GetTile(Point p)
    {
        return gameMap[p.y * levelGen.mapDimensions.width + p.x];
    }

    public Tile GetTile(int x,int y)
    {
        return gameMap[y * levelGen.mapDimensions.width + x];
    }

    
}
