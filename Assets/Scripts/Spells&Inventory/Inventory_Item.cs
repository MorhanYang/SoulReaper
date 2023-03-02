using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory_Item : MonoBehaviour
{
    [SerializeField] Image Mask;
    [SerializeField] TMP_Text cdNumber;

    [SerializeField] float cd;
    [SerializeField] TMP_Text displayItemNum;
    [SerializeField] int ItemNum;
    float cdTimer;
    bool isReady = true;// help other script to check if the spell is avalible

    private void Start()
    {
        displayItemNum.text = ItemNum.ToString();
    }

    private void Update()
    {
        // CD runs
        if (cdTimer > 0)
        {
            cdTimer -= Time.deltaTime;
            // UI display
            Mask.fillAmount = cdTimer / cd;
            cdNumber.text = Mathf.Round(cdTimer) + "s";
        }
        else if (isReady == false || cdTimer <= cd)
        {
            if (ItemNum>0) isReady = true;
            Mask.gameObject.SetActive(false);
        }
    }

    public void UseThis(){
        if (isReady){

            ItemNum--;

            isReady = false;
            displayItemNum.text = ItemNum.ToString();
            cdTimer = cd;
            Mask.gameObject.SetActive(true);
        }
    }

    public void CollectThis(){
        ItemNum++;
    }

    public bool IsReady()
    {
        return isReady;
    }
}
