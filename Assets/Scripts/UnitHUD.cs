using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitHUD : MonoBehaviour
{
    [Header("Texts")]
    public TMP_Text nameText;
    public TMP_Text hpText;
    public TMP_Text mpText;

    [Header("Bars")]
    public Image hpFill;
    public Image mpFill;

    public void UpdateHUD(BattleUnit unit)
    {
        if (unit == null)
            return;

        if (nameText != null)
            nameText.text = unit.unitName;

        if (hpText != null)
            hpText.text = $"{unit.currentHP} / {unit.maxHP}";

        if (mpText != null)
            mpText.text = $"{unit.currentMP} / {unit.maxMP}";

        if (hpFill != null)
        {
            hpFill.fillAmount =
                unit.maxHP > 0
                ? (float)unit.currentHP / unit.maxHP
                : 0f;
        }

        if (mpFill != null)
        {
            mpFill.fillAmount =
                unit.maxMP > 0
                ? (float)unit.currentMP / unit.maxMP
                : 0f;
        }
    }

    public void UpdateHP(int currentHP, int maxHP)
    {
        float percentage = (float)currentHP / maxHP;

        hpFill.fillAmount = percentage;

        if (hpText != null)
            hpText.text = currentHP + " / " + maxHP;
    }

    public void UpdateMP(int currentMP, int maxMP)
    {
        float percentage = (float)currentMP / maxMP;

        mpFill.fillAmount = percentage;

        if (mpText != null)
            mpText.text = currentMP + " / " + maxMP;
    }
}