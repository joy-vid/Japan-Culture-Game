using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [Header("Party HUD")]
    public UnitHUD[] playerHUDs;

    [Header("Enemy HUD")]
    public UnitHUD enemyHUD;

    [Header("Command Buttons")]
    public Button attackButton;
    public Button skillButton;
    public Button itemButton;
    public Button guardButton;

    [Header("Optional Text")]
    public TMP_Text battleMessageText;

    public void RefreshAll(
        BattleUnit[] players,
        BattleUnit enemy)
    {
        for (int i = 0; i < playerHUDs.Length; i++)
        {
            if (i < players.Length)
                playerHUDs[i].UpdateHUD(players[i]);
        }

        if (enemyHUD != null)
            enemyHUD.UpdateHUD(enemy);
    }

    public void SetCommandsInteractable(bool value)
    {
        if (attackButton != null)
            attackButton.interactable = value;

        if (skillButton != null)
            skillButton.interactable = value;

        if (itemButton != null)
            itemButton.interactable = value;

        if (guardButton != null)
            guardButton.interactable = value;
    }

    public void SetMessage(string message)
    {
        if (battleMessageText != null)
            battleMessageText.text = message;

        Debug.Log(message);
    }
}