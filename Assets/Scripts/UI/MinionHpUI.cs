using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinionHpUI : MonoBehaviour
{
    [SerializeField] GameObject selectedImage;
    [SerializeField] GameObject offset;
    [SerializeField] Image minionIcon;
    [SerializeField] Image healthBar;

    public void SelectThisMinion( bool state )
    {
        selectedImage.SetActive(state);
    }

    public void SwitchThisMinionSlot( bool state )
    {
        offset.SetActive(state);

    }

    public void FreshMinionHpBar( float hpPercentage )
    {
        healthBar.fillAmount = hpPercentage;
    }
}
