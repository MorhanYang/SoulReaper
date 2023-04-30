using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ReviveRangeMark : MonoBehaviour
{
    [SerializeField] SpriteRenderer rangeMarker;
    [SerializeField] Sprite bigMarker;
    [SerializeField] Sprite midMarker;
    [SerializeField] Sprite smallMarker;

    Vector3 initialScale;

    private void OnEnable(){
        rangeMarker.sprite = bigMarker;
        initialScale = rangeMarker.transform.localScale;
    }

    public void ShrinkMarker()
    {
        float mutiplyer = 0.4f;
        float shrinkTime = 0.15f;
        rangeMarker.transform.DOScale(rangeMarker.transform.localScale * mutiplyer, shrinkTime);
        Invoke("ChangeMarker", shrinkTime+0.02f);
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
