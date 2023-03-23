using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField]
    AudioSource MySource;
    public bool loop;
    public bool Infinite;
    public float volume;
    public float MinDistance;
    public float MaxDistance;
    public float timer;
    public AudioClip MyClip;

    void Start()
    {
        MySource = GetComponent<AudioSource>();
        if (MySource == null) 
        {
            Destroy(this);
        }
        MySource.clip = MyClip;
        MySource.loop = loop;
        MySource.volume = volume;
        MySource.maxDistance = MaxDistance;
        MySource.minDistance = MinDistance;
        MySource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Infinite) 
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }
}
