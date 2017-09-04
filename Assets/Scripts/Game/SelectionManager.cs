using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    public Unit selectedUnit;
    public Tile targetTile;

    private void Awake()
    {
        Instance = this;
    }

    public void OnUnitClicked(Unit clickedUnit)
    {
        if (selectedUnit)
        {

        }
        else
        {
            selectedUnit = clickedUnit;
        }
    }

    public void OnMouseOver()
    {
        
    }

    public void OnTileClicked(Tile clickedTile)
    {
        if (clickedTile.IsOccupiedByUnit())
        {
            if (selectedUnit)
            {
                if (selectedUnit.currentPhase == Unit.TurnPhase.ActionPhase)
                {
                    if (!clickedTile.GetUnitOnTile().BelongsToCurrentSide() 
                        && selectedUnit.CanAttackTile(clickedTile))
                    {
                        selectedUnit.Attack(clickedTile.GetUnitOnTile());
                        selectedUnit.currentPhase = Unit.TurnPhase.EndPhase;
                        selectedUnit = null;
                    }
                }
            }
            else
            {
                OnUnitClicked(clickedTile.GetUnitOnTile());
            }
        }
        else if (selectedUnit && selectedUnit.currentPhase == Unit.TurnPhase.MovePhase)
        {
            if (selectedUnit.CanMoveToTile(clickedTile))
            {
                selectedUnit.StartMovement(clickedTile);
            }
        }
    }
    public void ClearSelectedUnit()
    {
        selectedUnit = null;
    }

    public void ClearSelectedTile()
    {
        targetTile = null;
    }

    public void Clear()
    {
        ClearSelectedTile();
        ClearSelectedUnit();
    }
}
