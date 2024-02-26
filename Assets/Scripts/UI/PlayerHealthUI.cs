using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] Image selectedImage;

    [SerializeField] Image hpBar;

    public void SelectPlayerSlot()
    {
        selectedImage.gameObject.SetActive(true);
    }

    public void UnselectPlayerSlot()
    {
        selectedImage.gameObject.SetActive(false);
    }

    public void FreshHpInfo( float presentHp, float maxHp )
    {
        if (maxHp != 0){
             hpBar.fillAmount = presentHp / maxHp;
        }
        else Debug.Log("MaxHp should not be 0");
        
    }
}
