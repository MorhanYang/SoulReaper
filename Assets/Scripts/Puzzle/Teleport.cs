using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Teleport : MonoBehaviour
{
    [SerializeField] Vector3 PlayerNextPos;
    [SerializeField] GameObject landSet;
    List<Transform> landList;
    [SerializeField] int presentLandID;
    [SerializeField] int nextLandID;
    [SerializeField] CanvasGroup transportUI;
    [SerializeField] CameraFollow camMain;
    [SerializeField] string sceneName = null; // only use to change scene
    [SerializeField] float RangeToTeleport;
    PlayerControl player;
    bool isTransiting = false;
    bool isFadingOut = false;// decide when transportUI will fade in or out
    float fadeoutTimer = 0;

    Vector3 previousPos;

    // teleport minions
    [SerializeField] Vector3 MinionsNextPos;

    private void Start()
    {
        player = PlayerManager.instance.player.GetComponent<PlayerControl>();

        landList = new List<Transform>();
        foreach (Transform child in landSet.transform){
            landList.Add(child);
        }
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
                    // show land
                    landList[nextLandID - 1].gameObject.SetActive(true);// ID start from 1

                    previousPos = player.transform.position;// record previous position for minion teleportation

                    player.transform.position = PlayerNextPos;
                    camMain.transform.position = PlayerNextPos + camMain.GetCamOffset();
                    TeleportAllMinions();
                    // save data
                    player.landID = nextLandID - 1;// ID start from 1
                    SavingSystem.SavePlayer(player);
                    isFadingOut = true;

                    // hide other lands
                    for (int i = 0; i < landList.Count; i++){
                        if (i!= nextLandID -1 && i != presentLandID -1)
                        {
                            landList[i].gameObject.SetActive(false);
                        }
                    }
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
        int MytroopOriginalLength = myTroops.Count;
        for (int i = 0; i < myTroops.Count;) 
        {

            // get all minion in each troop
            List<Minion> myMinionList = myTroops[i].GetMinionList();

            for (int j = 0; j < myMinionList.Count;) // Removing minions will change myMinionList.Count value.
            {
                // Removing minions will change myMinionList.Count value. break for loop when there is no items

                // check if the minion is inside the range
                if (Vector3.Distance(myMinionList[j].transform.position, previousPos) > RangeToTeleport){
                    // kill the out range minion and regain hp
                    MinionTroop myTroop = myMinionList[j].GetComponent<Minion>().GetTroop();
                    myTroop.RemoveTroopMember(myMinionList[j]);
                }
                else 
                {
                    // send minions to the position
                    MinionAI myAI = myMinionList[j].GetComponent<MinionAI>();
                    NavMeshAgent myAgent = myMinionList[j].GetComponent<NavMeshAgent>();
                    myAgent.enabled = false;
                    myMinionList[j].transform.position = MinionsNextPos;
                    myAgent.enabled = true;
                    myAgent.SetDestination(player.transform.position);
                    myAI.StartRoam();

                    j++;
                }

                // Mytroop.count can change so we need to control when i should add.
                if (myTroops.Count != MytroopOriginalLength){
                    MytroopOriginalLength = myTroops.Count;
                }
                else i++; 

            }
        }

        
    }
}
