using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HintUI : MonoBehaviour
{
    [SerializeField] CanvasGroup offest;
    [SerializeField] Image diagram;
    [SerializeField] TMP_Text text;

    [SerializeField] Sprite[] diagramList;
    [SerializeField] string[] textList;

    int hintId = 0;

    bool needPause = false;

    private void Start()
    {
        offest.alpha = 0f;
        hintId = 0;
    }

    private void Update()
    {
        // Pause Game
        if (offest.alpha >= 1 && needPause)
        {
            needPause= false;
            Time.timeScale = 0f;
        }
    }
    public void ShowHint()
    {
        Invoke("ShowHintFunction", 1.5f);
    }

    void ShowHintFunction(){

        offest.DOFade(1, 0.3f);
        //offest.SetActive(true);
        diagram.sprite = diagramList[hintId];
        text.text = textList[hintId];
        if (hintId < (textList.Length - 1) && hintId < (diagramList.Length - 1)) hintId++;

        needPause = true;
    }
    public void HideHint() 
    {
        offest.DOFade(0, 0.6f);
        //offest.SetActive(false);
        Time.timeScale= 1f;
    }
}
