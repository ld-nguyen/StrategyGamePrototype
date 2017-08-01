﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager: MonoBehaviour {

    public static UnitManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    private List<Unit> playerUnits;
    private List<Unit> enemyUnits;

    private Unit selectedUnit;

    public void PlaceUnits()
    {
        playerUnits =  new List<Unit>();
        enemyUnits = new List<Unit>();


        for (int side = 0; side < 2; side++)
        {
            for (int unit = 0; unit < GameManager.Instance.unitCountPerSide; unit++)
            {
                //TODO: Selecting different unit types
                Unit newUnit = Instantiate(Resources.Load("Unit", typeof(Unit)),LevelGenerator.Instance.parentGO.transform) as Unit;
                //TODO: Rework side assignment (This is horrible :( )
                bool isFriendly = side == 0 ? true : false;

                newUnit.SetUnitSide(isFriendly);
                if (isFriendly) playerUnits.Add(newUnit);
                else enemyUnits.Add(newUnit);

                Point spawnpoint;
                do
                {
                    spawnpoint = Point.GetRandomPoint();
                }while(!GameManager.Instance.GetTile(spawnpoint).IsTraversableByUnit(newUnit.unitType));

                newUnit.SetUnitPosition(spawnpoint);
            }
        }
    }

    public void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
    }

    public Unit GetCurrentSelectedUnit()
    {
        return selectedUnit;
    }

    public void OnUnitUnselected()
    {
        if(selectedUnit) selectedUnit.HighlightMovementArea(false);
    }

}
