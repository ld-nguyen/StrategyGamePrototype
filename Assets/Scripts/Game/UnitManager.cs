using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager: MonoBehaviour {

    public static UnitManager Instance { get; private set; }
    public List<Unit> unitCompositionPerSide;

    private List<Unit>[] unitListForSide;
    public Color[] sideColors;


    void Awake()
    {
        Instance = this;
    }

    public void PlaceUnits()
    {
        unitListForSide = new List<Unit>[GameManager.SIDES];

        for (int side = 0; side < GameManager.SIDES; side++)
        {
            unitListForSide[side] = new List<Unit>();

            Point centerPoint; //Centerpoint to spawn units around
            do
            {
                centerPoint = Point.GetRandomPoint();
            } while (LevelGenerator.Instance.TerrainAtPoint(centerPoint) != TerrainType.Grass);

            foreach(Unit unitPrefab in unitCompositionPerSide)
            {
                //TODO: Selecting different unit types
                Unit newUnit = Instantiate(unitPrefab,LevelGenerator.Instance.parentGO.transform) as Unit;
                //Assign unit to side
                unitListForSide[side].Add(newUnit);
                newUnit.SetUnitSide(side, sideColors[side]);
                //Calculate spawnpoint and set unit to tile on spawnpoint
                Point spawnpoint;
                do
                {
                    spawnpoint = Point.GetRandomOffset(centerPoint,4);
                }while(!spawnpoint.IsInsideGrid() || !newUnit.CanTraversePoint(spawnpoint));

                newUnit.SetUnitPosition(spawnpoint);
            }
        }
    }

    public void ResetUnits(int side)
    {
        List<Unit> unitList = unitListForSide[side];
        foreach(Unit u in unitList)
        {
            u.OnNewTurn();
        }
    }

    public void ResetAllUnits()
    {
        for (int i = 0; i < unitListForSide.Length; i++)
        {
            ResetUnits(i);
        }
    }

    public List<Unit> GetUnitListOfSide(int side)
    {
        return unitListForSide[side];
    }

    public Color GetCurrentSideColor()
    {
        return sideColors[GameManager.Instance.turnOfSide];
    }

}
