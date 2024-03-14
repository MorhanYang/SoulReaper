using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinionHpUI : MonoBehaviour
{
    [SerializeField] GameObject selectedImage;
    [SerializeField] GameObject minionSet;
    [SerializeField] Image minionIcon;
    [SerializeField] Image healthBar;
    [SerializeField] GameObject minionBg;

    public void SelectThisMinion( bool state )
    {
        selectedImage.SetActive(state);
    }

    public void ShowMinionUIBg(bool state)
    {
        minionBg.SetActive(state);
    }

    public void SwitchThisMinionSlot( bool state , Minion.MinionStyle style )
    {
        minionSet.SetActive(state);
        // Set Pic
        switch (style)
        {
            case Minion.MinionStyle.defualt:
                minionIcon.sprite = null;
                break;
            case Minion.MinionStyle.Rats:
                minionIcon.sprite = MinionProfileManager.GetSprite("Rat");
                break;
            case Minion.MinionStyle.Normal:
                minionIcon.sprite = MinionProfileManager.GetSprite("Normal");
                break;
            case Minion.MinionStyle.Range:
                minionIcon.sprite = MinionProfileManager.GetSprite("Range");
                break;
            case Minion.MinionStyle.Dash:
                minionIcon.sprite = MinionProfileManager.GetSprite("Dash");
                break;
            case Minion.MinionStyle.Vine:
                minionIcon.sprite = MinionProfileManager.GetSprite("Vine");
                break;
            default:
                break;
        }
    }

    public void FreshMinionHpBar( float hpPercentage )
    {
        healthBar.fillAmount = hpPercentage;
    }
}
