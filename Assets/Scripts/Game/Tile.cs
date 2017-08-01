using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tile : MonoBehaviour
{
    public int x { get; private set; }
    public int y { get; private set; }

    private TextMesh debugCoords;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isHighlighted;

    public int cost;
    public List<UnitType> traversableByUnitType;

    public void SetCoords(int x, int y)
    {
        this.x = x;
        this.y = y;

        if(debugCoords)
            debugCoords.text = x + "/" + y;
    }

    public void Setup(int xCoord, int yCoord, Vector3 unityPos)
    {
        SetCoords(xCoord, yCoord);
        gameObject.transform.localPosition = unityPos;
    }

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

    public bool IsTraversableByUnit(UnitType unit)
    {
        return traversableByUnitType.Contains(unit);
    }

    public void Highlight(bool shouldHighlight)
    {
        isHighlighted = shouldHighlight;
        if (shouldHighlight) spriteRenderer.material.color = GameManager.Instance.highlightTileColor;
        else spriteRenderer.material.color = originalColor;
    }

}
