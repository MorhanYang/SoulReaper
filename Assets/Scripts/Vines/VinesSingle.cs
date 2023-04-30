using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VinesSingle : MonoBehaviour
{
    [SerializeField]
    float a;
    [SerializeField]
    float b;
    [SerializeField]
    float c;

    float gap;
    [SerializeField]
    float length;
    float ColorModifier;
    [SerializeField]
    private GameObject Segments;

    [SerializeField]
    private enum States { Spawning, Dying, Neutral }
    private States MyStates;

    [SerializeField]
    List<GameObject> MySegs;

    void Start()
    {
        MyStates = States.Neutral;
        a = Random.Range(2, 7);
        b = Random.Range(10, 15);
        c = Random.Range(0f, 2f);
        //SpawnVine();
        ColorModifier = Random.Range(0f,0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Q)) 
        {
            StartSpawnVine();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            StartKillVine();
        }
        */
    }

    public void StartSpawnVine() 
    {
        StartCoroutine(SpawnVine());
    }

    public void StartKillVine()
    {
        StartCoroutine(KillVine());
    }

    IEnumerator SpawnVine()
    {
        MyStates = States.Spawning;
        float x = 0;
        int count = Mathf.FloorToInt(length / gap);
        for (int i = 0; x < length; i++)
        {
            if (MyStates != States.Spawning) 
            {
                break;
            }

            float size = Random.Range(0.2f, 1f);
            gap = 0.12f * size;

            x += gap;
            float y = 0.07f * (Mathf.Sin(a * (x + c)) + 0.2f * Mathf.Sin(b * (x + c)));
            //I assume that all cameras are set at -45 degrees;
            //sqrt2 is about 1.414
            //we also want to 
            GameObject NextSegment = Instantiate(Segments, transform);
            NextSegment.transform.localPosition = new Vector3(x, y / 1.414f, y / 1.414f + -0.001f * (float)i);
            NextSegment.transform.eulerAngles = new Vector3(45f,0,0);
            Segments.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(0.3f + ColorModifier, 0.4f + ColorModifier, 0.2f, 1);
            NextSegment.transform.localScale = new Vector3(size, size, size);
            MySegs.Add(NextSegment);
            yield return new WaitForSeconds(0.05f);
        }
        MyStates = States.Neutral;
    }

    IEnumerator KillVine()
    {
        MyStates = States.Dying;
        for (int i = MySegs.Count - 1; i > -1; i--)
        {
            Debug.Log("aa");
            MySegs[i].GetComponent<Segments>().StartDie();
            MySegs.Remove(MySegs[i]);
            yield return new WaitForSeconds(0.05f);
        }
        MyStates = States.Neutral;
    }
}
