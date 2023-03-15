using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimationEffect : MonoBehaviour
{
    [SerializeField]
    private Vector3 InitialShift;
    public bool Loop;
    [SerializeField]
    private List<Sprite> myFrames;
    [SerializeField]
    private List<Vector4> ShiftsOnFrame;//XYXshift and Layer
    [SerializeField]
    private float totalTime;
    [SerializeField]
    private float timePerFrame;
    [SerializeField]
    private float timer;
    [SerializeField]
    private int currentframe;
    [SerializeField]
    private SpriteRenderer MyRenderer;
    private bool flipped;


    void Start()
    {
        timer = 0;
        if (myFrames.Count <= 0) 
        {
            Debug.Log("Effect " + gameObject.name + "has no sprite frames!");
            Destroy(gameObject);
        }
        transform.localPosition += InitialShift;
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
                                                            ShiftsOnFrame[currentframe].x * (System.Convert.ToSingle(flipped) * 2 - 1),
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
}
