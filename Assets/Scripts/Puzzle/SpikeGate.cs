using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeGate : MonoBehaviour
{
    public bool open;
    [SerializeField]
    private float gap_between_spikes;
    [SerializeField]
    private GameObject SpikePrefab;

    [SerializeField]
    private Sprite SpriteOpen;

    [SerializeField]
    private Sprite SpriteClosed;

    [SerializeField]
    private List<GameObject> Spikes;

    [SerializeField]
    GameObject myBlocker;


    public void closeDoor() 
    {
        if (open) 
        {
            for (int i = 0; i < Spikes.Count; i++) 
            {
                SpriteRenderer myRenderer = Spikes[i].GetComponent<SpriteRenderer>();
                myRenderer.sprite = SpriteOpen;
            }
            myBlocker.SetActive(true);
        }
        open = false;
    }

    public void openDoor() 
    {
        if (!open) 
        {
            for (int i = 0; i < Spikes.Count; i++)
            {
                SpriteRenderer myRenderer = Spikes[i].GetComponent<SpriteRenderer>();
                myRenderer.sprite = SpriteClosed;
            }
            myBlocker.SetActive(false);
        }
        open = true;
    }



}
