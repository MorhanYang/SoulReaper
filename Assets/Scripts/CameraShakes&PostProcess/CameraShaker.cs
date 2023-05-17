using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    //Attach this to a camera to allow it to shake.
    public static CameraShaker instance;
    private Vector3 initialLocalPos;
    private float RotateMagnitude;
    [SerializeField]
    private float frequency = 5;
    private Vector2 RotateVector;
    private float timer;

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
    }

    void Start()
    {
        transform.localPosition = initialLocalPos;
    }

    void Update()
    {

        if (timer > 0)
        {
            timer = Mathf.Max(0, timer - Time.deltaTime);
            RotateMagnitude = Mathf.Min(timer * 5, 2);
            transform.localEulerAngles = new Vector3(Mathf.Sin(frequency * timer * 3.14f) * RotateVector.x, Mathf.Sin(frequency * timer * 3.14f) * RotateVector.y,0) * RotateMagnitude;
        }

        if (Input.GetKeyDown(KeyCode.X)) 
        {
            AddCamShake(new Vector2(5,5),0.5f);
            VignetteBreather.instance.VignetteSpike(0.2f, 0.5f);
        }

    }

    public void AddCamShake(Vector2 NewDirection, float time)
    {
        NewDirection = new Vector2( - NewDirection.y / 2, NewDirection.x);
        timer = time;
        RotateVector = NewDirection.normalized;
    }
}
