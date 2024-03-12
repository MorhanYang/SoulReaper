using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class ReviveRangeMark : MonoBehaviour
{
    [SerializeField] SpriteRenderer rangeMarker;
    [SerializeField] Sprite bigMarker;
    [SerializeField] Sprite midMarker;
    [SerializeField] Sprite smallMarker;

    List<Minion> minionList = new List<Minion>();

    float radius = 1.6f; // check playercontrol's radius in rebirth fucntion;

    Vector3 initialScale;

    private void OnEnable(){
        rangeMarker.sprite = bigMarker;
        initialScale = rangeMarker.transform.localScale;

        // get minions on screen
        Collider[] minionsInRange = Physics.OverlapSphere(transform.position, radius *4 , LayerMask.GetMask("Minion"));
        for (int i = 0; i < minionsInRange.Length; i++){
            minionList.Add(minionsInRange[i].GetComponent<Minion>());
        }
    }

    private void Update()
    {
        for (int i = 0; i < minionList.Count; i++){
            if (Vector3.Distance(minionList[i].transform.position, transform.position) < radius){
                minionList[i].SetRebirthSelect(true);
            }
            else{
                minionList[i].SetRebirthSelect(false);
            }
        }
    }

    public void ShrinkMarker()
    {
        float multiplyer = 0.4f;
        float shrinkTime = 0.15f;
        rangeMarker.transform.DOScale(rangeMarker.transform.localScale * multiplyer, shrinkTime);

        Invoke("ChangeMarker", shrinkTime + 0.02f);
        radius *= multiplyer;
    }

    void ChangeMarker()
    {
        if (rangeMarker.sprite == bigMarker)
        {
            rangeMarker.transform.localScale = initialScale;
            rangeMarker.sprite = midMarker;
        }
        else if (rangeMarker.sprite == midMarker)
        {
            rangeMarker.transform.localScale = initialScale;
            rangeMarker.sprite = smallMarker;
        }
    }
}
