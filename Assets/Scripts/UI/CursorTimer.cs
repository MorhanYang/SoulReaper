using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CursorTimer : MonoBehaviour
{
    [SerializeField] Image timerBar;
    [SerializeField] Canvas canvas;
    [SerializeField] GameObject BarCover;
    [SerializeField] Vector2 offset;

    RectTransform recttransform;
    CanvasGroup myCanvasGroup;
    Vector2 pos;
    float countdown;
    float time;
    bool showingCusorTimer;

    private void Start()
    {
        recttransform = GetComponent<RectTransform>();
        myCanvasGroup = GetComponent<CanvasGroup>();
        myCanvasGroup.alpha = 0;
        BarCover.SetActive(false);
    }

    private void Update()
    {
        if (showingCusorTimer)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out pos);
            recttransform.localPosition = pos + offset;
            if (countdown != 0) timerBar.fillAmount = time / countdown;
            if (time >= countdown && timerBar.fillAmount == 1){
                HideCursorTimer();
            }

            time += Time.deltaTime;
        }
    }


    public void ShowCursorTimer(float countdownTime)
    {
        BarCover.SetActive(false);
        showingCusorTimer = true;
        countdown = countdownTime;
        myCanvasGroup.DOFade(1,0.1f);
    }
    public void HideCursorTimer()
    {
        if(time < countdown) BarCover.SetActive(true);
        showingCusorTimer = false;
        time = 0;
        myCanvasGroup.DOFade(0, 0.2f);
    }
}
