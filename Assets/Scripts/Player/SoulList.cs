using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulList : MonoBehaviour
{
    [SerializeField] GameObject[] soulItemTemp;
    [SerializeField] Transform firstLineTarget;
    [SerializeField] Transform secondLineTarget;
    [SerializeField] Transform generatingFolder;

    public int soulNum = 4;
    List<GameObject> soulList = new List<GameObject>();

    // soul selection
    [SerializeField] LayerMask followingSoulMask;
    GameObject hitedSoulItem = null;
    GameObject previousHited = null;


    private void Start()
    {
        // this script will change the prefab's PrensentIntervalX, so I must rest the prefab value when game start
        for (int i = 0; i < soulItemTemp.Length; i++){
            soulItemTemp[i].GetComponent<SoulListItem>().presentIntervalX = Mathf.Abs(soulItemTemp[i].GetComponent<SoulListItem>().presentIntervalX);
        }

        SetupSoulList();
    }


    //************************Method**************************
    public void FlipSoulList()
    {
        // change prefab
        for (int i = 0; i < soulItemTemp.Length; i++){
            soulItemTemp[i].GetComponent<SoulListItem>().presentIntervalX *= -1;
        }


        //change list direction
        for (int i = 0; i < soulList.Count; i++)
        {
            soulList[i].GetComponent<SoulListItem>().presentIntervalX *= -1;
        }

    }

    void SetupSoulList() {
        for (int i = 0; i < soulNum; i++)
        {
            GameObject soul = Instantiate(soulItemTemp[0], transform.position, transform.rotation, generatingFolder);
            soul.GetComponent<SoulListItem>().presentTarget = firstLineTarget;
            soulList.Add(soul);
            if (i == 0){
                soul.GetComponent<SoulListItem>().presentTarget = firstLineTarget;
            }
            else{
                soul.GetComponent<SoulListItem>().presentTarget = soulList[i-1].transform;
            }
        }
    }

    public void AddSoul(int soulType){ // soulType:0-normal, 1-special !!! Set the same number at SoulistItem prefabs

        GameObject soul = Instantiate(soulItemTemp[soulType], transform.position, transform.rotation, generatingFolder);
        soulList.Add(soul);

        if (soulNum == 0){
            soul.GetComponent<SoulListItem>().presentTarget = firstLineTarget;
        }
        else{
            soul.GetComponent<SoulListItem>().presentTarget = soulList[soulNum - 1].transform;
        }

        soulNum++;
    }

    public int UseSoul() {
        if (soulNum > 0){
            //tell player control what kind of soul is used
            int soulType = soulList[0].GetComponent<SoulListItem>().soulType;

            Destroy(soulList[0].gameObject);
            soulList.RemoveAt(0);
            soulNum--;

            // new soulList[0] set target if the soul remains
            if (soulNum > 0)
            {
                soulList[0].GetComponent<SoulListItem>().presentTarget = firstLineTarget;
            }

            return soulType;
        }
        else return -1;
    }

    //***************************Select Soul Method
    public void SelectSoul()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100, followingSoulMask)){
            hitedSoulItem = hitInfo.collider.gameObject;
        }
        else hitedSoulItem = null; // hit nothing



        // hited scale up, the last hited scale down
        if (hitedSoulItem != previousHited){
            if (hitedSoulItem != null){
                hitedSoulItem.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            }
            if (previousHited != null){
                previousHited.transform.localScale = Vector3.one;
            }

            previousHited = hitedSoulItem;
        }

    }


}
