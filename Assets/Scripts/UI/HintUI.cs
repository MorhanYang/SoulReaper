using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HintUI : MonoBehaviour
{
    [SerializeField] GameObject offest;
    [SerializeField] Image diagram;
    [SerializeField] TMP_Text text;

    [SerializeField] Sprite[] diagramList;
    [SerializeField] string[] textList;

    public void ShowHint(int hintID)
    {
        offest.SetActive(true);
        switch (hintID)
        {
            case 0:
                diagram.sprite = diagramList[0];

                break;

            case 1:
                break;
            case 2:
                break;
            default:
                break;
        }
    }
    public void HideHint() 
    { 
        offest.SetActive(false);
    }
}
