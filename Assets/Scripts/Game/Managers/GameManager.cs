﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    //References
    public static GameManager Instance { get; private set; }
    private LevelGenerator levelGen;
    private Tile[] gameMap;
    //Variables
    public bool playerCanInteract { get; private set; }
    public int turnOfSide;
    //Parameters for Unity
    [Header("Game Parameters")]
    public Color mouseOverTileColor;
    public Color highlightTileColor;
    public int sides;
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
        UIManager.Instance.OnNextTurn();
        SetGameInteractable(true);
    }

    public void SetGameMap(Tile[] map)
    {
        gameMap = map;
    }

    public void NextTurn()
    {
        UnitManager.Instance.ResetUnits(turnOfSide);
        turnOfSide = (turnOfSide + 1) % sides;
        UIManager.Instance.OnNextTurn();
    }

    public Tile GetTile(Point p)
    {
        return gameMap[p.y * levelGen.mapDimensions.width + p.x];
    }

    public Tile GetTile(int x,int y)
    {
        return gameMap[y * levelGen.mapDimensions.width + x];
    }

    public void SetGameInteractable(bool canInteract) { playerCanInteract = canInteract; }
}
