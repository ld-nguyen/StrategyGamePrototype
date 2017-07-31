using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tile : MonoBehaviour
{
    public int x { get; private set; }
    public int y { get; private set; }

    private TextMesh debugCoords;
    private SpriteRenderer renderer;
    private Color originalColor;
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
        renderer = GetComponent<SpriteRenderer>();
        if(renderer) originalColor = renderer.material.color;
    }

    void OnMouseEnter()
    {
        renderer.material.color = GameManager.Instance.tileHigh;
    }

    void OnMouseDown()
    {
        renderer.material.color = Color.blue;
    }

    void OnMouseUp()
    {
        renderer.material.color = GameManager.Instance.tileHigh;
    }

    void OnMouseExit()
    {
        renderer.material.color = originalColor;
    }

    public bool IsTraversableByUnit(UnitType unit)
    {
        return traversableByUnitType.Contains(unit);
    }
}
