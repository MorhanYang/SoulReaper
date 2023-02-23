using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PuzzleTrigger : MonoBehaviour
{
    Health hp;
    [SerializeField] Puzzle_Bridge myBridge;

    float showHealthBarTimer = 0f;

    private void Start()
    {
        hp= GetComponent<Health>();
    }

    private void Update()
    {
        // health bar
        if (showHealthBarTimer >= 0)
        {
            showHealthBarTimer -= Time.deltaTime;
            if (showHealthBarTimer < 0f){
                hp.HideHPUI();
            }
        }
    }

    //****************************************************** Trigger ***************************************
    public void TakeDamage(float damage, GameObject subject)
    {
        float hideHealthBarDelay = 5f;
        hp.ShowHPUI();
        showHealthBarTimer = hideHealthBarDelay;

        hp.TakeDamage(damage);

        //If died
        if (hp.presentHealth <= 0)
        {
            Debug.Log("trigger event");
            hp.HideHPUI();

            myBridge.ActiveScript();
            Destroy(gameObject);
        }
    }
}
