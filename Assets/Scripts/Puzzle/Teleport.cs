using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Teleport : MonoBehaviour
{
    [SerializeField] Vector3 PlayerNextPos;
    [SerializeField] CanvasGroup transportUI;
    [SerializeField] CameraFollow camMain;
    PlayerControl player;
    bool isTransiting = false;
    bool isFadingOut = false;// decide when transportUI will fade in or out

    // teleport minions
    [SerializeField] Vector3 MinionsNextPos;

    private void Start()
    {
        player = PlayerManager.instance.player.GetComponent<PlayerControl>();
    }

    private void Update()
    {
        if (isTransiting){
            // fade in
            if (!isFadingOut){
                if (transportUI.alpha < 1){
                    transportUI.alpha += Time.deltaTime * 2f;
                }
                if (transportUI.alpha >= 1){
                    player.transform.position = PlayerNextPos;
                    camMain.transform.position = PlayerNextPos + camMain.GetCamOffset();
                    TeleportAllMinions();
                    isFadingOut = true;
                }
            }
            // fade out
            if (isFadingOut){
                if (transportUI.alpha > 0) transportUI.alpha -= Time.deltaTime * 1f;
                if (transportUI.alpha <= 0){
                    Debug.Log("Fade Our runing");
                    player.inActivateTeleporting();
                    isTransiting = false;
                    isFadingOut = false;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.GetComponent<PlayerControl>() != null){
            player.SetPlayerToTeleporting();
            isTransiting = true;
        }
    }

    void TeleportAllMinions()
    {
        // get all minions 
        List<MinionTroop> myTroops = player.GetComponent<PlayerHealthBar>().GetActivedTroop();
        for (int i = 0; i < myTroops.Count; i++)
        {
            // get all minion in each troop
            List<Minion> myMinions = myTroops[i].GetMinionList();
            for (int j = 0; j < myMinions.Count; j++)
            {
                // send minions to the position
                myMinions[j].GetComponent<NavMeshAgent>().enabled = false;
                myMinions[j].transform.position = MinionsNextPos;
                myMinions[j].GetComponent<NavMeshAgent>().enabled = true;
            }
        }

        
    }
}
