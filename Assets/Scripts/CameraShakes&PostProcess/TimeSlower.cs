using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSlower : MonoBehaviour
{
    public static TimeSlower instance;
    private float MyfixedDeltaTime;
    [SerializeField]
    private float Slowtimer;
    private float SlowtimerMax;
    private float SlowScaleCurrent;
    private float SlowScaleStart;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        MyfixedDeltaTime = Time.fixedDeltaTime;
    }

    void Start()
    {

    }

    void Update()
    {
        /*
        if (Time.timeScale < 1)
        {
            Time.timeScale = Mathf.Min(Time.timeScale + Time.deltaTime, 1);
        }
        */
        //Time.fixedDeltaTime = MyfixedDeltaTime * Time.timeScale;
    }

    private void FixedUpdate()
    {

        if (Slowtimer > 0)
        {
            Slowtimer = Mathf.Max(0, Slowtimer - Time.fixedDeltaTime);
            Time.timeScale = SlowScaleCurrent;
        }
        else 
        {
            Time.timeScale = 1;
        }
    }

    public void SlowTime(float NewTimeScale, float duration)
    {
        SlowScaleStart = NewTimeScale;
        SlowScaleCurrent = SlowScaleStart;
        SlowtimerMax = duration;
        Slowtimer = SlowtimerMax;
    }
}
