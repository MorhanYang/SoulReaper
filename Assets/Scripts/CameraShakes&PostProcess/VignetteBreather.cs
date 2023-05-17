using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class VignetteBreather : MonoBehaviour
{
    public static VignetteBreather instance;

    private PostProcessVolume MyVolume;
    [SerializeField]
    private GameObject PostProcessGameObject;
    private Vignette MyVignette;
    private float timer;
    private float timerMax;
    private float intensityBonusMax;
    private float intensityBonus;

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

    // Start is called before the first frame update
    void Start()
    {
        MyVolume = PostProcessGameObject.GetComponent<PostProcessVolume>();
        MyVolume.profile.TryGetSettings(out MyVignette);
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer = Mathf.Max(0, timer - Time.deltaTime);
            intensityBonus = intensityBonusMax / timerMax * timer;
        }
        MyVignette.intensity.value = 0.3f + Mathf.Sin(Time.time) * 0.01f + intensityBonus;
    }

    public void VignetteSpike(float Intensity, float time)
    {
        intensityBonusMax = Intensity;
        timerMax = time;
        timer = time;
    }
}
