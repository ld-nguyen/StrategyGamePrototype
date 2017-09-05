using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { get; private set; }

    public Text turnLabel;
    //UnitTooltip
    public Text unitTooltipType;
    public Text unitTooltipHealth;

    public GameObject actionMenu;
    public Button attackButton;

    public GameObject winPanel;
    public Text winPanelText;

    private void Awake()
    {
        Instance = this;
    }

    public void DisplayActionMenu(bool show)
    {
        actionMenu.SetActive(show);
        GameManager.Instance.playerCanInteract = true;
    }

    public void OnShowActionMenu(Vector3 position)
    {
        actionMenu.transform.localPosition = GameManager.Instance.mainCamera.WorldToViewportPoint(position);
        attackButton.interactable = ShouldEnableAttackButton();
        DisplayActionMenu(true);
    }

    public void UpdateTurnLabel()
    {
        if (GameManager.Instance.turnOfSide == 0) { turnLabel.text = "TURN: Player RED"; }
        else { turnLabel.text = "TURN: Player " + GameManager.Instance.turnOfSide; }
    }

    public bool ShouldEnableAttackButton()
    {
        return SelectionManager.Instance.selectedUnit.HasAttackableUnits();
    }

    public void ShowWinPanel()
    {
        winPanel.SetActive(true);
    }
}
