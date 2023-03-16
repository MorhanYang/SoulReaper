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

    int hintId = 0;
    public void ShowHint()
    {
        offest.SetActive(true);
        diagram.sprite = diagramList[hintId];
        text.text = textList[hintId];
        if(hintId < (textList.Length - 1) && hintId < (diagramList.Length - 1)) hintId++;
    }
    public void HideHint() 
    { 
        offest.SetActive(false);
        Time.timeScale= 1.0f;
    }
}
