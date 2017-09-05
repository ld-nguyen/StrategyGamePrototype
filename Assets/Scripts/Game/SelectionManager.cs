using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    public Unit selectedUnit;

    private void Awake()
    {
        Instance = this;
    }

    public void OnTileClicked(Tile clickedTile)
    {
        if (clickedTile.IsOccupiedByUnit())
        {
            Unit unitOnTile = clickedTile.GetUnitOnTile();
            if (selectedUnit)
            {
                if (selectedUnit.currentPhase == Unit.TurnPhase.ActionPhase)
                {
                    if (!unitOnTile.BelongsToCurrentSide() 
                        && selectedUnit.CanAttackEnemy(unitOnTile))
                    {
                        selectedUnit.HighlightAttackRange(false);
                        selectedUnit.Attack(unitOnTile);
                        selectedUnit.currentPhase = Unit.TurnPhase.EndPhase;
                        selectedUnit = null;
                    }
                }
            }
            else
            {
                if (unitOnTile.BelongsToCurrentSide() && unitOnTile.currentPhase == Unit.TurnPhase.MovePhase)
                {
                    selectedUnit = unitOnTile;
                    selectedUnit.ShowMovementArea();
                }
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

    public void OnAttackActionSelected()
    {
        UIManager.Instance.DisplayActionMenu(false);
        selectedUnit.HighlightAttackRange(true);
    }

    public void OnWaitActionSelected()
    {
        UIManager.Instance.DisplayActionMenu(false);
        selectedUnit.OnEndTurn();
        ClearSelectedUnit();
    }

    public void ClearSelectedUnit()
    {
        selectedUnit = null;
    }
}
