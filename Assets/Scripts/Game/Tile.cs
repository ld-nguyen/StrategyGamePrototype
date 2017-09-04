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
    private Unit unitOnTile;

    public int cost;
    public bool showDebugCoords;

    public void SetCoords(int x, int y)
    {
        coordinates = new Point(x, y);

        if (debugCoords && showDebugCoords)
            debugCoords.text = x + "/" + y;
        else
            debugCoords.gameObject.SetActive(false);
    }

    public void Setup(int xCoord, int yCoord, Vector3 unityPos, float colorOffset = 0)
    {
        SetCoords(xCoord, yCoord);
        gameObject.transform.localPosition = unityPos;
        if(colorOffset != 0)
        {
            Color newColor = new Color(originalColor.r + colorOffset,originalColor.g + colorOffset, originalColor.b + colorOffset);
            SetColor(newColor);
            originalColor = newColor;
        }
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
        SetColor(GameManager.Instance.mouseOverTileColor);
    }

    void OnMouseDown()
    {
        if (GameManager.Instance.playerCanInteract)
        {
            SetColor(Color.blue);
            SelectionManager.Instance.OnTileClicked(this);
        }
    }

    void OnMouseUp()
    {
        if (isHighlighted) SetColor(GameManager.Instance.highlightTileColor);
        else SetColor(originalColor);
    }

    void OnMouseExit()
    {
        if (isHighlighted) SetColor(GameManager.Instance.highlightTileColor);
        else SetColor(originalColor);
    }

    private void SetColor(Color newColor)
    {
        spriteRenderer.material.color = newColor;
    }
    #endregion

    public bool IsOccupiedByUnit()
    {
        return unitOnTile != null;
    }

    public void SetUnitOnTile(Unit u) { unitOnTile = u; }
    public Unit GetUnitOnTile() { if (unitOnTile) return unitOnTile; else return null; }

    public void OnUnitExit()
    {
        SetUnitOnTile(null);
    }

    public void OnUnitEnter(Unit u)
    {
        SetUnitOnTile(u);
    }

    public void Highlight(bool shouldHighlight)
    {
        isHighlighted = shouldHighlight;
        if (shouldHighlight) SetColor(GameManager.Instance.highlightTileColor);
        else SetColor(originalColor);
    }
}
