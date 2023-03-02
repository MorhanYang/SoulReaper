using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Spell_Item : MonoBehaviour
{
    [SerializeField] Image Mask;
    [SerializeField] TMP_Text cdNumber;

    [SerializeField]float cd;
    float cdTimer;
    bool isReady = true;// help other script to check if the spell is avalible

    private void Update()
    {
        // CD runs
        if (cdTimer > 0) {
            cdTimer -= Time.deltaTime;
            // UI display
            Mask.fillAmount = cdTimer / cd;
            cdNumber.text = Mathf.Round(cdTimer) + "s";
        }
        else if (isReady == false || cdTimer <= cd) {
            isReady = true;
            Mask.gameObject.SetActive(false);
        }
    }

    public void ActivateSpell()
    {
        if (isReady){
            isReady = false;
            cdTimer = cd;
            Mask.gameObject.SetActive(true);
        }
    }

    public bool IsReady()
    {
        return isReady;
    }
    
}
