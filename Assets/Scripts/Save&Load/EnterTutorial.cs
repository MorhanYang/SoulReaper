using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class EnterTutorial : MonoBehaviour
{
    [SerializeField] PlayableDirector director;
    [SerializeField] GameObject StartUI;

    [SerializeField] AudioSource activeSound;
    [SerializeField] AudioSource buttonSound;

    private void Start()
    {
        activeSound= GetComponent<AudioSource>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            EnterTutorialLevel();
        }
    }

    public void EnterTutorialLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Sewer_Tutorial");
    }

    public void PlayCutscene()
    {

        StartUI.SetActive(false);
        director.Play();
        activeSound.Play();
        buttonSound.Play();
    }
}
