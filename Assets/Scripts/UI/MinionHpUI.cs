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
    Image Bg;

    private void Awake(){
        Bg = GetComponent<Image>();
    }

    public void SelectThisMinion( bool state )
    {
        selectedImage.SetActive(state);
    }

    public void ShowMinionUIBg(bool state)
    {
        Bg.enabled = state;
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
