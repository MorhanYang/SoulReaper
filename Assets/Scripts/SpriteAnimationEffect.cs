using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimationEffect : MonoBehaviour
{
    public bool Loop;
    [SerializeField]
    private List<Sprite> myFrames;
    [SerializeField]
    private List<Vector4> ShiftsOnFrame;//XYXshift and OrderInLayer
    [SerializeField]
    private float totalTime;
    private float timePerFrame;
    private float timer;
    [SerializeField]
    private int currentframe;
    [SerializeField]
    private SpriteRenderer MyRenderer;

    void Start()
    {
        timer = 0;
        if (myFrames.Count <= 0) 
        {
            Debug.Log("Effect " + gameObject.name + "has no sprite frames!");
            Destroy(gameObject);
        }
        timePerFrame = totalTime / myFrames.Count;
        if (MyRenderer == null)
        { 
            MyRenderer = gameObject.GetComponent<SpriteRenderer>(); 
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            PauseOnThisFrame(1);
        }

        timer += Time.deltaTime;
        MyRenderer.sprite = myFrames[currentframe];

        if (timer >= timePerFrame) 
        {
            currentframe++;
            if (currentframe >= myFrames.Count) 
            {
                if (Loop)
                {
                    currentframe = 0;
                }
                else 
                {
                    Destroy(gameObject);
                }
            }
            if (currentframe < ShiftsOnFrame.Count)
            {
                transform.localPosition += new Vector3(
                                                            ShiftsOnFrame[currentframe].x * flippedFactor(),
                                                            ShiftsOnFrame[currentframe].y,
                                                            ShiftsOnFrame[currentframe].z
                                                      );
                MyRenderer.sortingOrder = (int)ShiftsOnFrame[currentframe].w;
            }
            timer -= timePerFrame;        
        }
    }

    public void PauseOnThisFrame(float time)
    {
        timer -= time;
    }

    private float flippedFactor() 
    {
        return (transform.localScale.x / Mathf.Abs(transform.localScale.x));
    }
}
