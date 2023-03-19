using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static GameObject[] EnemyList;
    [SerializeField] GameObject startUI;

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

    private void Awake(){
        instance= this;
    }

    private void Start(){
        EnemyList = GameObject.FindGameObjectsWithTag("Enemy");
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
        if (canvasGroup.alpha <= 0.1f)
        {
            startUI.SetActive(false);
            canvasGroup.alpha = 1f;
        }

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
