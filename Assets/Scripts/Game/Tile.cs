using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x { get; private set; }
    public int y { get; private set; }

    public TextMesh debugCoords;

    public void SetCoords(int x, int y)
    {
        this.x = x;
        this.y = y;

        if(debugCoords)
            debugCoords.text = x + "/" + y;
    }

    void Awake()
    {
        debugCoords = GetComponentInChildren<TextMesh>();
    }
}
