using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using static Fungus.StopMotionRigidBody2D;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static GameObject[] EnemyList;
    PlayerControl playerControl;
    PlayerHealthBar playerHealthBar;
    [SerializeField] GameObject startUI;
    [SerializeField] CameraFollow camMain;

    // spell
    [SerializeField] Transform spellSet;
    List<Spell_Item> spellList = new List<Spell_Item>();

    // Inventory
    [SerializeField] Transform InventorySet;
    List<Inventory_Item> inventoryList = new List<Inventory_Item>();

    // Popup Information
    [SerializeField] TMP_Text popUpInfo;

    // HitMarker
    [SerializeField] GameObject AimMarker;

    // load data
    [SerializeField] CanvasGroup transportUI;
    [SerializeField] GameObject[] minionTemp;

    private void Awake(){
        instance= this;
    }

    private void Start(){
        EnemyList = GameObject.FindGameObjectsWithTag("Enemy");
        playerControl = PlayerManager.instance.player.GetComponent<PlayerControl>();
        playerHealthBar = PlayerManager.instance.player.GetComponent<PlayerHealthBar>();
        startUI.SetActive(true);
        Time.timeScale = 0f;

        // setup Spell List
        foreach (Transform item in spellSet){
            spellList.Add(item.GetComponent<Spell_Item>());
        }

        // setup Inventory List
        foreach (Transform item in InventorySet){
            inventoryList.Add(item.GetComponent<Inventory_Item>());
        }
    }

    private void Update(){

        if (Input.GetKeyDown(KeyCode.R)){
            Restart();
        }

        if (Input.GetKeyDown(KeyCode.F3)){
            LoadPlayerData();
        }
    }

    // ********************************************** Scene Control **************************************************
    public static void Restart(){
        SceneManager.LoadScene("Sewer_Tutorial");
    }
    public void StartGame()
    {
        Time.timeScale = 1.0f;
        CanvasGroup canvasGroup = startUI.GetComponent<CanvasGroup>();
        canvasGroup.DOFade(0, 0.5f);

        Invoke("DisableStartUI", 0.5f);
    }
    void DisableStartUI(){
        startUI.SetActive(false);
    }
    // Save and Load
    public void LoadPlayerData()
    {
        // show load cover
        transportUI.DOFade(1,0.4f);
        playerControl.SetPlayerToTeleporting();

        Invoke("SetLoadPlayerData", 0.3f);
    }
    void SetLoadPlayerData()
    {
        PlayerData data = SavingSystem.LoadPlayer();

        Vector3 position = new Vector3();
        position.x = data.position[0];
        position.y = data.position[1];
        position.z = data.position[2];

        // change position + camera
        playerControl.transform.position = position;
        camMain.transform.position = position + camMain.GetCamOffset();

        // set health
        playerHealthBar.cellNum = data.hpCellNum;
        playerHealthBar.InitiatePlayerHPBar();
        playerHealthBar.SetPlayerHealth(data.hpCellNum, data.healthMax, data.health);

        // remove previous minion
        List<MinionTroop> minionTroops = playerHealthBar.GetActivedTroop();
        for (int i = 0; i < minionTroops.Count; i++)
        {
            //destory Minion
            List<Minion> minionList = minionTroops[i].GetMinionList();
            Debug.Log("MinionNum" + minionList.Count);
            for (int j = 0; j < minionList.Count; j++)
            {
                Destroy(minionList[j].gameObject);
            }
            //remove troop bar Ui
            playerHealthBar.RemoveTroopFromPlayerHealth(minionTroops[i], false);
        }
        playerHealthBar.CleanTroopList();

        // add minion
        List<Minion> allMinion = new List<Minion>();
        for (int i = 0; i < data.normalMinionNum; i++){
            GameObject minion = Instantiate(minionTemp[0], position + new Vector3(-0.8f, 0, 0), transform.rotation);
            allMinion.Add(minion.GetComponent<Minion>());
        }
        for (int i = 0; i < data.specialMinionNum; i++){
            GameObject minion = Instantiate(minionTemp[1], position + new Vector3(-0.8f, 0, 0), transform.rotation);
            allMinion.Add(minion.GetComponent<Minion>());
        }

        // Revive Minions
        playerHealthBar.ReviveTroopLoading(allMinion);
        allMinion.Clear();

        Invoke("HideTransportUI", 0.5f);
    }
    void HideTransportUI(){
        transportUI.DOFade(0, 0.4f);
        playerControl.inActivateTeleporting();
    }

    // ********************************************* Spell CD ************************************************
    public bool IsSpellIsReady(int spellID) // 1 is the first instead of 0
    {
        if (spellID == 0){
            Debug.Log("Spell ID starts from 1"); 
            return false;
        }
        else{
            bool spellIsReady = spellList[spellID - 1].IsReady();
            return spellIsReady;
        }
    }
    public void ActivateSpellCDUI(int spellID) // 1 is the first instead of 0
    {
        if (spellID == 0){
            Debug.Log("Spell ID starts from 1");
        }
        else{
            // Activate
            spellList[spellID - 1].ActivateSpell();
        }
    }
    // ********************************************** Inventory ************************************************
    public bool IsItemIsReady(int ItemId) // 1 is the first instead of 0
    {
        if (ItemId == 0){
            Debug.Log("Item ID starts from 1");
            return false;
        }
        else{
            bool spellIsReady = spellList[ItemId - 1].IsReady();
            return spellIsReady;
        }
    }
    public void UseItem(int ItemId) // 1 is the first instead of 0
    {
        if (ItemId == 0){
            Debug.Log("Item ID starts from 1");
        }
        else{
            // Activate
            inventoryList[ItemId - 1].UseThis();
        }
    }
    // ****************************************** UI ****************************************
    public void PopUpUI( Vector3 startPosOnScreen, string text)
    {
        float fadeTime = 1.5f;
        CanvasGroup cg = popUpInfo.GetComponent<CanvasGroup>();

        if (cg.alpha == 0f){
            // set up
            cg.alpha = 1.0f;
            popUpInfo.rectTransform.localPosition = startPosOnScreen;

            // move 
            popUpInfo.text = text;
            popUpInfo.rectTransform.DOAnchorPos(new Vector2(0, startPosOnScreen.y + 25f), fadeTime);
            cg.DOFade(0, fadeTime);
        }
    }
    // ****************************************** world UI *********************************************
    public void GenerateMarker(Vector3 pos, Transform subject)
    {
        GameObject myMarker;
        // don't hit object
        if (subject == null){
            myMarker = Instantiate(AimMarker, pos, transform.rotation);
        }
        // hit object
        else{
            myMarker = Instantiate(AimMarker, subject.position, subject.rotation, subject);
        }


        Destroy(myMarker, 0.8f);
    }


}
