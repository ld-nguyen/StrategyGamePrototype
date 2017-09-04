using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager: MonoBehaviour {

    public static UnitManager Instance { get; private set; }

    private List<Unit>[] unitListForSide;
    public Color[] sideColors { get; private set; }


    void Awake()
    {
        Instance = this;
    }

    public void PlaceUnits()
    {
        unitListForSide = new List<Unit>[GameManager.Instance.sides];
        sideColors = new Color[GameManager.Instance.sides];

        for (int side = 0; side < GameManager.Instance.sides; side++)
        {
            unitListForSide[side] = new List<Unit>();
            sideColors[side] = new Color(Random.value, Random.value, Random.value);

            Point centerPoint; //Centerpoint to spawn units around
            do
            {
                centerPoint = Point.GetRandomPoint();
            } while (LevelGenerator.Instance.TerrainAtPoint(centerPoint) != TerrainType.Grass);

            for (int unit = 0; unit < GameManager.Instance.unitCountPerSide; unit++)
            {
                //TODO: Selecting different unit types
                Unit newUnit = Instantiate(Resources.Load("Unit", typeof(Unit)),LevelGenerator.Instance.parentGO.transform) as Unit;
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

    public List<Unit> GetUnitListOfSide(int side)
    {
        return unitListForSide[side];
    }

    public Color GetCurrentSideColor()
    {
        return sideColors[GameManager.Instance.turnOfSide];
    }

}
