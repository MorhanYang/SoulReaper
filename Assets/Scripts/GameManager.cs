using Fungus;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameObject[] EnemyList;
    public Flowchart fungusFlowchart;
    [SerializeField] GameObject startUI;

    private void Start(){
        EnemyList = GameObject.FindGameObjectsWithTag("Enemy");
        startUI.SetActive(true);
        Time.timeScale = 0f;


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
        SceneManager.LoadScene("Tutorial Level");
    }

}
