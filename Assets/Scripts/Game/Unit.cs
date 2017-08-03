using System;
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

public enum UnitPhase
{
    MovePhase,
    ActionPhase,
    EndPhase,
}

public class Unit : MonoBehaviour
{
    public UnitType unitType;
    private UnitPhase turnPhase;
    public int movementRange;
    public int belongsToSide;
    private Point coordinates;

    private List<Tile> currentMovementArea;

    void Awake()
    {
        currentMovementArea = new List<Tile>();
    }

    public void OnMouseDown()
    {
        if (!IsSelected() && BelongsToCurrentPlayer()  &&GameManager.Instance.playerCanInteract)
        {
            UnitManager.Instance.OnUnitUnselected();
            UnitManager.Instance.SetSelectedUnit(this);
            if (turnPhase == UnitPhase.MovePhase)
            {
                HighlightMovementArea(true);
            }
        }
    }

    public void OnNewTurn()
    {
        turnPhase = UnitPhase.MovePhase;
    }

    public bool IsSelected()
    {
        return this == UnitManager.Instance.GetCurrentSelectedUnit();
    }

    public bool BelongsToCurrentPlayer()
    {
        if (GameManager.Instance.turnOfSide == belongsToSide)
        {
            return true;
        }
        else return false;
    }

    public void SetUnitSide(int faction, Color sideColor)
    {
        belongsToSide = faction;
        
        GetComponent<SpriteRenderer>().color = sideColor;
    }

    public void SetUnitPosition(Point p)
    {
        SetUnitPosition(p.x, p.y);
        GameManager.Instance.GetTile(p).SetUnitOnTile(this);
    }

    public void SetUnitPosition(int x, int y)
    {
        coordinates = new Point(x, y);
        gameObject.transform.localPosition = new Vector3(x, y,-0.2f);
    }

    public void HighlightMovementArea(bool setHighlight)
    {
        if(setHighlight) currentMovementArea = GetMovementArea();

        foreach(Tile tile in currentMovementArea)
        {
            tile.Highlight(setHighlight);
        }
    }

    public List<Tile> GetMovementArea()
    {
        List<Tile> movementArea = new List<Tile>();
        Point currentPoint = coordinates;
        for(int xOffset = -movementRange; xOffset <= movementRange; xOffset++)
        {
            for(int yOffset = -movementRange; yOffset <= movementRange; yOffset++)
            {
                if (xOffset == 0 && yOffset == 0) continue;

                Point offset = currentPoint + new Point(xOffset, yOffset);
                if (Utility.ManhattanDistance(currentPoint, offset) <= movementRange &&
                    Utility.IsInsideGrid(offset) &&
                    GameManager.Instance.GetTile(offset).IsTraversableByUnit(unitType))
                {
                    movementArea.Add(GameManager.Instance.GetTile(offset));
                }
            }
        }
        return movementArea;
    }

    public void MoveToTile(Tile tile)
    {
        HighlightMovementArea(false);
        GameManager.Instance.SetGameInteractable(false);
        GameManager.Instance.GetTile(coordinates).OnUnitExit();

        List<Point> path = AStarPathSearch.FindPath(coordinates, tile.coordinates, UnitCanEnterTile);
        path.Reverse();
        coordinates = tile.coordinates;
        StartCoroutine(LerpThroughPath(path));
    }

    public bool UnitCanEnterTile(Point p)
    {
        Tile tileToCheck = GameManager.Instance.GetTile(p);
        if (tileToCheck.IsTraversableByUnit(unitType))
        {
            return true;
        }
        else { return false; }
    }

    public bool TileIsInMovementArea(Tile t)
    {
        if(currentMovementArea.Count <= 0)
        {
            GetMovementArea();
        }
        return currentMovementArea.Contains(t);
    }

    public void OnUnitMoveComplete()
    {
        turnPhase = UnitPhase.ActionPhase;
        GameManager.Instance.GetTile(coordinates).SetUnitOnTile(this);
    }

    IEnumerator LerpThroughPath(List<Point> path)
    {
        foreach(Point p in path)
        {
            float lerpProgress = 0;
            float zOffset = transform.localPosition.z;
            Vector3 origin = transform.localPosition;
            Vector3 goal = p.GetVector3();
            goal.z = zOffset;
            do
            {
                gameObject.transform.localPosition = Vector3.Lerp(origin, goal, lerpProgress);
                lerpProgress += 0.10f; //TODO: Define Lerp speed somewhere else
                yield return new WaitForEndOfFrame();
            } while (lerpProgress <= 1);
        }
        OnUnitMoveComplete();
        GameManager.Instance.SetGameInteractable(true);
    }

}
