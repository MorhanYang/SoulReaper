using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MinionBarIcon : MonoBehaviour
{
    [SerializeField] GameObject normalList;
    [SerializeField] GameObject[] normalListIcon;
    [SerializeField] GameObject specialMinionIcon;
    [SerializeField] GameObject minionTriggerIcon;

    [SerializeField] GameObject normalListSelected;
    [SerializeField] GameObject specialMinionIconSelected;
    [SerializeField] GameObject minionTriggerIconSelected;

    public void UpdateMinionIcon(int iconType, int number)
    {
        switch (iconType)
        {
            case 0:
                normalList.SetActive(true);
                normalListSelected.SetActive(true);
                for (int i = 0; i < normalListIcon.Length; i++){
                    if (i < number){
                        normalListIcon[i].SetActive(true);
                    }
                    else{
                        normalListIcon[i].SetActive(false);
                    }
                }
                break;
            case 1:
                specialMinionIcon.SetActive(true);
                specialMinionIconSelected.SetActive(true);
                break;
            case 2:
                minionTriggerIcon.SetActive(true);
                minionTriggerIconSelected.SetActive(true);
                break;
            default:
                break;
        }
    }
}
