using Fungus;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameObject[] EnemyList;
    public Flowchart fungusFlowchart;
    [SerializeField] GameObject startUI;

    // spell
    [SerializeField] Transform spellSet;
    List<Spell_Item> spellList = new List<Spell_Item>();

    // Inventory
    [SerializeField] Transform InventorySet;
    List<Inventory_Item> inventoryList = new List<Inventory_Item>();

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
        // active game
        if (Input.GetKeyDown(KeyCode.Return)){
            startUI.SetActive(false);
            Time.timeScale = 1.0f;
        }
    }

    // ********************************************** Scene Control **************************************************
    public static void Restart(){
        SceneManager.LoadScene("Tutorial_Test");
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
}
