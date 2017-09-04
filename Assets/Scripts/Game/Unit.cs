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
    private Point coordinates;

    private SpriteRenderer sprite;
    private GameObject selectionMarker;

    #region MonoBehaviour Overrides
    private void Start()
    {
        
    }
    private void Awake()
    {
        currentPhase = TurnPhase.MovePhase;
    }
    #endregion
    #region Action-related Methods

    public void HighlightAttackRange(bool showHighlight)
    {
        //HighlightTiles
    }

    public bool CanAttackTile(Tile tileWithEnemy)
    {
        return attackableTiles.Contains(tileWithEnemy);
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
    }
    #endregion

    #region Movement-related Methods

    public bool CanMoveToTile(Tile tile)
    {
        return currentMovementRange.Contains(tile) && CanTraversePoint(tile.coordinates);
    }

    public void StartMovement(Tile targetTile)
    {
        List<Point> path = AStarPathSearch.FindPath(coordinates, targetTile.coordinates,CanTraversePoint);
        StartCoroutine(LerpThroughPath(path));
    }

    public bool CanTraversePoint(Point p)
    {
        return traversableTerrainTypes.Contains((LevelGenerator.Instance.TerrainAtPoint(p)));
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
        //Show Action Menu
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
        sprite.material.color = color;
    }

    public void SetUnitPosition(Point p)
    {
        
        GameManager.Instance.GetTile(p).SetUnitOnTile(this);
    }
}
