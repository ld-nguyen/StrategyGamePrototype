using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { get; private set; } 

    public Text turnLabel;   

    void Awake()
    {
        Instance = this;
    }

    public void SetTurnLabel(Color sideColor)
    {
        if (turnLabel)
        {
            turnLabel.text = "TURN: <color=#"+ColorUtility.ToHtmlStringRGB(sideColor)+"> PLAYER"+(GameManager.Instance.turnOfSide+1)+"</color>";
        }
    }

    public void OnNextTurn()
    {
        SetTurnLabel(UnitManager.Instance.sideColors[GameManager.Instance.turnOfSide]);
    }
}
