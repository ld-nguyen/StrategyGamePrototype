using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum TurnPhase {MovePhase, ActionPhase, EndPhase }

    public int maxHealth;
    public int movementRange;
    public List<TerrainType> traversableTerrainTypes;
    public int attackRange;
    public int[] attackDamageRange;
    public TurnPhase currentPhase;

    private int currentHealth;
    private int belongsToSide;

    private List<Tile> currentMovementRange;
    private List<Tile> attackableTiles;
    private List<Unit> attackableUnits;
    private Point coordinates;

    private SpriteRenderer sprite;
    private GameObject selectionMarker;

    #region MonoBehaviour Overrides
    private void Awake()
    {
        currentPhase = TurnPhase.MovePhase;
        sprite = GetComponent<SpriteRenderer>();
        attackableTiles = new List<Tile>();
        currentMovementRange = new List<Tile>();
        attackableUnits = new List<Unit>();
    }
    #endregion
    #region Action-related Methods
    public bool HasAttackableUnits()
    {
        return attackableUnits.Count > 0;
    }

    public void GetAttackableUnits()
    {
        attackableUnits.Clear();
        foreach(Unit u in UnitManager.Instance.GetUnitListOfSide((belongsToSide + 1) % 2))
        {
            if (Utility.ManhattanDistance(this.coordinates, u.coordinates) <= attackRange)
            {
                attackableUnits.Add(u);
            }
        }
    }

    public void HighlightAttackRange(bool showHighlight)
    {
        AreaHighlight(showHighlight, attackableTiles);
    }
    public bool CanAttackEnemy(Unit enemy)
    {
        return attackableUnits.Contains(enemy);
    }
    public void Attack(Unit target)
    {
        target.GetDamage(UnityEngine.Random.Range(this.attackDamageRange[0], this.attackDamageRange[1]));
    }
    public void GetDamage(int damageValue)
    {
        currentHealth = currentHealth - damageValue;
        if (currentHealth <= 0)
        {
            OnDeath();
        }
        
    }
    private void OnDeath()
    {
        UnitManager.Instance.GetUnitListOfSide(belongsToSide).Remove(this);
        GameManager.Instance.GetTile(coordinates).OnUnitExit();
        Destroy(this.gameObject);
        GameManager.Instance.CheckWinCondition();
    }
    #endregion

    #region Movement-related Methods

    public bool CanMoveToTile(Tile tile)
    {
        return !tile.IsOccupiedByUnit() && currentMovementRange.Contains(tile) && CanTraversePoint(tile.coordinates);
    }

    public void ShowMovementArea()
    {
        AreaHighlight(true, currentMovementRange);
    }

    public void StartMovement(Tile targetTile)
    {
        AreaHighlight(false,currentMovementRange);
        GameManager.Instance.GetTile(coordinates).OnUnitExit();
        targetTile.OnUnitEnter(this);
        GameManager.Instance.playerCanInteract = false;

        List<Point> path = AStarPathSearch.FindPath(coordinates, targetTile.coordinates,CanTraversePoint);
        path.Reverse();
        coordinates = targetTile.coordinates;
        StartCoroutine(LerpThroughPath(path));
    }

    public bool CanTraversePoint(Point p)
    {
        return traversableTerrainTypes.Contains((LevelGenerator.Instance.TerrainAtPoint(p)));
    }
    
    public void AreaHighlight(bool shouldShow, List<Tile> area)
    {
        foreach (Tile tile in area)
        {
            tile.Highlight(shouldShow);
        }
    }

    private List<Tile> GetTileRange(int range)
    {
        List<Tile> area = new List<Tile>();
        Point currentPoint = coordinates;
        for (int xOffset = -range; xOffset <= range; xOffset++)
        {
            for (int yOffset = -range; yOffset <= range; yOffset++)
            {
                if (xOffset == 0 && yOffset == 0) continue;

                Point offset = currentPoint + new Point(xOffset, yOffset);
                if (Utility.ManhattanDistance(currentPoint, offset) <= range &&
                    Utility.IsInsideGrid(offset) &&
                    CanTraversePoint(offset))
                {
                    area.Add(GameManager.Instance.GetTile(offset));
                }
            }
        }
        return area;
    }
    IEnumerator LerpThroughPath(List<Point> path)
    {
        foreach (Point p in path)
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
    }
    public void OnUnitMoveComplete()
    {
        currentPhase = TurnPhase.ActionPhase;
        //get attackable range
        attackableTiles = GetTileRange(attackRange);
        GetAttackableUnits();
        //Show Action Menu
        UIManager.Instance.OnShowActionMenu(coordinates.GetVector3());
    }
    #endregion 

    public void OnNewTurn()
    {
        //Reset stuff;
        currentPhase = TurnPhase.MovePhase;
        currentMovementRange = GetTileRange(movementRange);
    }
    public bool IsSelected() { return true; }

    public void OnActionSelected()
    {
        currentPhase = TurnPhase.ActionPhase;
    }

    public void OnEndTurn()
    {
        currentPhase = TurnPhase.EndPhase;
    }

    public bool BelongsToCurrentSide()
    {
        return GameManager.Instance.turnOfSide == this.belongsToSide;
    }

    public void SetUnitSide(int side, Color color)
    {
        belongsToSide = side;
        sprite.color = color;
    }

    public void SetUnitPosition(Point p)
    {
        coordinates = p;
        transform.localPosition = p.GetVector3();
        GameManager.Instance.GetTile(p).SetUnitOnTile(this);
    }

}
