using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySoulLogic : MonoBehaviour
{
    [SerializeField]GameObject haveSoulIcon;
    [SerializeField]GameObject EnemySoul;

    public bool isDead = false;
    // change when mouse hover it
    public bool isPreparingRecall = false;
    private void OnEnable()
    {
        if (isDead){
            ShowHaveSoulIcon();
        }
    }
    private void Update()
    {
        DisableHaveSoulIcon();
        if (isPreparingRecall)
        {

        }
    }


    //******************Method************************
    public void ShowHaveSoulIcon() {
        if (haveSoulIcon != null)
        {
            haveSoulIcon.SetActive(true);
        }
        if (EnemySoul != null)
        {
            EnemySoul.SetActive(true);
        }
    }

    void DisableHaveSoulIcon() { 
        if (EnemySoul == null) {
            if (haveSoulIcon != null)
            {
                haveSoulIcon.SetActive(false);
            }
        } 
    }

    public void DeathRitual(){
        isDead= true;
        ShowHaveSoulIcon();
    }

    public void PrepareForRecall()
    {

    }
}
