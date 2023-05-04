using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitControl : MonoBehaviour
{
    public GameObject[] array;

    public void AdjustCircuit( int presentNum, int needNum ){

        if (presentNum <= needNum)
        {
            //Debug.Log("presentNum" + presentNum);
            //Debug.Log("needNum" + needNum);
            int showNum = (presentNum * array.Length)/ needNum;
            if (presentNum > 0 && showNum == 0) showNum = 1;
            //Debug.Log("ShowNum" + showNum);

            for (int i = 0; i < array.Length; i++){
                if (i < showNum) array[i].gameObject.SetActive(true);
                else if (i >= showNum) array[i].gameObject.SetActive(false);
            }
        }
        else Debug.Log("present num greater than needed num");

    }
}
