using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Teleport : MonoBehaviour
{
    [SerializeField] Vector3 PlayerNextPos;
    [SerializeField] CanvasGroup transportUI;
    [SerializeField] CameraFollow camMain;
    [SerializeField] string sceneName = null; // only use to change scene
    [SerializeField] float RangeToTeleport;
    PlayerControl player;
    bool isTransiting = false;
    bool isFadingOut = false;// decide when transportUI will fade in or out
    float fadeoutTimer = 0;

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
                    // save data
                    SavingSystem.SavePlayer(player);
                    isFadingOut = true;
                }
            }
            // fade out
            if (isFadingOut && fadeoutTimer > 0.8f){
                if (transportUI.alpha > 0) transportUI.alpha -= Time.deltaTime * 2f;
                if (transportUI.alpha <= 0){
                    Debug.Log("Fade Our runing");
                    player.inActivateTeleporting();
                    isTransiting = false;
                    isFadingOut = false;
                    transportUI.gameObject.SetActive(false);
                    fadeoutTimer = 0;
                }
            }else if (isFadingOut){
                fadeoutTimer += Time.deltaTime;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.GetComponent<PlayerControl>() != null){
            player.SetPlayerToTeleporting();
            isTransiting = true;
            transportUI.gameObject.SetActive(true);
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
                // check if the minion is inside the range
                Debug.Log("Distance " + Vector3.Distance(myMinions[j].transform.position, player.transform.position));
                if (Vector3.Distance(myMinions[j].transform.position, player.transform.position) < RangeToTeleport)
                {
                    
                    // send minions to the position
                    myMinions[j].GetComponent<NavMeshAgent>().enabled = false;
                    myMinions[j].transform.position = MinionsNextPos;
                    myMinions[j].GetComponent<NavMeshAgent>().enabled = true;
                }
                else // kill the out range minion and regain hp
                {
                    myMinions[j].GetComponent<Minion>().GetTroop().RemoveTroopMember(myMinions[j]);
                }
                
            }
        }

        
    }
}
