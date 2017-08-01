using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
    Infantry,
    Ranged,
    Flying,
    Sea,
}

public class Unit : MonoBehaviour
{
    public UnitType type;
    public int movementRange;

    private bool belongsToPlayer1;
    private int x;
    private int y;

    private List<Tile> currentMovementArea;

    public void SetUnitSide(bool belongsToPlayerSide)
    {
        belongsToPlayer1 = belongsToPlayerSide;
    }

    public void SetUnitPosition(Point p)
    {
        SetUnitPosition(p.x, p.y);
    }

    public void SetUnitPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        gameObject.transform.localPosition = new Vector3(x, y,-0.2f);
    }

    public void OnMouseDown()
    {
        if (!IsSelected())
        {
            UnitManager.Instance.SetSelectedUnit(this);
            HighlightMovementArea();
        }
    }

    public void HighlightMovementArea()
    {
        currentMovementArea = GetMovementArea();

        foreach(Tile tile in currentMovementArea)
        {
            tile.Highlight(true);
        }
    }

    public List<Tile> GetMovementArea()
    {
        List<Tile> movementArea = new List<Tile>();
        Point currentPoint = new Point(x, y);
        for(int xOffset = -movementRange; xOffset <= movementRange; xOffset++)
        {
            for(int yOffset = -movementRange; yOffset <= movementRange; yOffset++)
            {
                if (xOffset == 0 && yOffset == 0) continue;

                Point offset = currentPoint + new Point(xOffset, yOffset);
                if (Utility.ManhattanDistance(currentPoint, offset) <= movementRange &&
                    Utility.IsInsideGrid(offset) &&
                    GameManager.Instance.GetTile(offset).IsTraversableByUnit(type))
                {
                    movementArea.Add(GameManager.Instance.GetTile(offset));
                }
            }
        }
        return movementArea;
    }

    public bool IsSelected()
    {
        return this == UnitManager.Instance.GetCurrentSelectedUnit();
    }
}
