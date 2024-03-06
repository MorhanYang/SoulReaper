using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    Health health;
    SoundManager mySoundManager;
    float showHealthBarTimer;
    private void Start()
    {
        health = GetComponent<Health>();
        mySoundManager = SoundManager.Instance;
    }

    private void OnMouseEnter()
    {
        GameManager.instance.GetComponent<CursorManager>().ActivateCombatCursor();
    }

    private void Update()
    {
        // health bar
        if (showHealthBarTimer >= 0)
        {
            showHealthBarTimer -= Time.deltaTime;
            if (showHealthBarTimer < 0f){
                health.HideHPUI();
            }
        }
    }


    public void TakeDamage(float damage, Transform subject, Vector3 attackPos)
    {
        float hideHealthBarDelay = 4f;

        health.TakeDamage(damage);

        health.ShowHPUI();
        showHealthBarTimer = hideHealthBarDelay;


        //died
        if (health.presentHealth <= 0)
        {
            Destroy(gameObject);
            GameManager.instance.GetComponent<CursorManager>().ActivateDefaultCursor();
        }

        // play sound 
        mySoundManager.PlaySoundAt(PlayerManager.instance.player.gameObject.transform.position, "Hurt", false, false, 1, 0.5f, 100, 100);

    }

}
