using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    //References
    public static GameManager Instance { get; private set; }
    private LevelGenerator levelGen;
    private Tile[] gameMap;
    public Camera mainCamera;
    //Variables
    public bool playerCanInteract;
    public int turnOfSide;
    //Parameters for Unity
    [Header("Game Parameters")]
    public Color mouseOverTileColor;
    public Color highlightTileColor;
    public const int SIDES = 2;
    


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
        UnitManager.Instance.ResetAllUnits();
        playerCanInteract = true;
        UIManager.Instance.UpdateTurnLabel();
    }

    public void OnTurnEnded()
    {
        if (playerCanInteract)
        {
            UnitManager.Instance.ResetUnits(turnOfSide);
            SelectionManager.Instance.ClearSelectedUnit();
            turnOfSide = (turnOfSide + 1) % SIDES;
            UIManager.Instance.UpdateTurnLabel();
        }
    }

    public void CheckWinCondition()
    {
        List<Unit> enemySide = UnitManager.Instance.GetUnitListOfSide((turnOfSide + 1) % 2);
        if (enemySide.Count <= 0)
        {
            UIManager.Instance.ShowWinPanel();
            playerCanInteract = false;
        }
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
