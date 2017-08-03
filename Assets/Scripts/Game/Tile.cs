using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tile : MonoBehaviour
{
    public Point coordinates { get; private set; }

    private TextMesh debugCoords;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isHighlighted;

    public int cost;
    public List<UnitType> traversableByUnitType;
    public Unit unitOnTile { get; private set; }

    public void SetCoords(int x, int y)
    {
        coordinates = new Point(x, y);

        if(debugCoords)
            debugCoords.text = x + "/" + y;
    }

    public void Setup(int xCoord, int yCoord, Vector3 unityPos)
    {
        SetCoords(xCoord, yCoord);
        gameObject.transform.localPosition = unityPos;
    }

    #region MonoBehavior Overrides
    void Awake()
    {
        debugCoords = GetComponentInChildren<TextMesh>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer) originalColor = spriteRenderer.material.color;
    }

    void OnMouseEnter()
    {
        spriteRenderer.material.color = GameManager.Instance.mouseOverTileColor;
    }

    void OnMouseDown()
    {
        spriteRenderer.material.color = Color.blue;
        Unit currentSelectedUnit = UnitManager.Instance.GetCurrentSelectedUnit();
        if (currentSelectedUnit && currentSelectedUnit.TileIsInMovementArea(this))
        {
            currentSelectedUnit.MoveToTile(this);
        }
    }

    void OnMouseUp()
    {
        if (isHighlighted) spriteRenderer.material.color = GameManager.Instance.highlightTileColor;
        else spriteRenderer.material.color = originalColor;
    }

    void OnMouseExit()
    {
        if (isHighlighted) spriteRenderer.material.color = GameManager.Instance.highlightTileColor;
        else spriteRenderer.material.color = originalColor;
    }
    #endregion
    public bool IsTraversableByUnit(UnitType unit)
    {
        return traversableByUnitType.Contains(unit) && !unitOnTile;
    }

    public void Highlight(bool shouldHighlight)
    {
        isHighlighted = shouldHighlight;
        if (shouldHighlight) spriteRenderer.material.color = GameManager.Instance.highlightTileColor;
        else spriteRenderer.material.color = originalColor;
    }
    
    public void SetUnitOnTile(Unit unit)
    {
        unitOnTile = unit;
    }

    public void OnUnitExit()
    {
        SetUnitOnTile(null);
    }
}
