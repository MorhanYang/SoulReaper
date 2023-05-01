using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segments : MonoBehaviour
{
    public Color MyColor;
    [SerializeField]
    GameObject MySpriteObject;
    [SerializeField]
    GameObject MyLeaf;
    SpriteRenderer StemRenderer;
    SpriteRenderer LeafRenderer;
    float size;

    bool Dying = false;
    void Start()
    {
        StemRenderer = MySpriteObject.GetComponent<SpriteRenderer>();
        LeafRenderer = MySpriteObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        MyLeaf.SetActive(false);
        if (Random.Range(0, 2f) > 1)
        {
            MyLeaf.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0, -90f));
        }
        else
        {
            MyLeaf.transform.localEulerAngles = new Vector3(180, 0, Random.Range(0, -90f));
        }
        StartCoroutine(Grow());
    }

    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            StartDie();
        }
        */
    }

    IEnumerator Grow()
    {
        MyColor = MySpriteObject.GetComponent<SpriteRenderer>().color;
        if (Random.Range(0, 5f) < 1f)
        {
            MyLeaf.SetActive(true);
            //If succeeds, spawn with leaf
        }
        else
        {
            MyLeaf.SetActive(false);
        }
        size = MySpriteObject.transform.localScale.x;
        while (MySpriteObject.transform.localScale.x < 1)
        {
            if (Dying) 
            {
                break;
            }
            size += Time.deltaTime * 2;
            MySpriteObject.transform.localScale = new Vector3(1, 1, 1) * size;
            yield return new WaitForEndOfFrame();
        }
    }

    public void StartDie()
    {
        Dying = true;
        StartCoroutine(Die());

    }

    IEnumerator Die() 
    {
        Color TargetColor = new Color(0.4f, 0.3f, 0.3f, 1);
        Vector3 ColorDifference = new Vector3(TargetColor.r - StemRenderer.color.r, TargetColor.g - StemRenderer.color.g, TargetColor.b - StemRenderer.color.b);
        float timer = 0;
        while (MySpriteObject.transform.localScale.x > 0)
        {
            timer += Time.deltaTime * 2;
            size -= Time.deltaTime * 2;
            MySpriteObject.transform.localScale = new Vector3(1, 1, 1) * size;
            yield return new WaitForEndOfFrame();
            StemRenderer.color = new Color(
                                            StemRenderer.color.r + ColorDifference.x * Time.deltaTime,
                                            StemRenderer.color.g + ColorDifference.y * Time.deltaTime,
                                            StemRenderer.color.b + ColorDifference.z * Time.deltaTime,
                                            1
                                          );
            MySpriteObject.transform.position += new Vector3(0,-1f,0) * Time.deltaTime * timer;
        }
        Destroy(gameObject);
    }
}
