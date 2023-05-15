using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.AI;

public class NextScene : MonoBehaviour
{
    [SerializeField] Animator myAnim;
    [SerializeField] CanvasGroup cover;
    [SerializeField] string nextSceneName;
    [SerializeField] Vector3 playerPosition;
    [SerializeField] Vector3 minionPosition;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerControl>() != null){
            myAnim.SetBool("FadeOut", true);
            cover.gameObject.SetActive(true);
            cover.alpha = 0f;
            cover.DOFade(1, 1f);

            StartCoroutine(GoToNextScene());
        }
    }

    IEnumerator GoToNextScene()
    {
        yield return new WaitForSeconds(2f);
        // reset player pos
        PlayerManager.instance.player.transform.position = playerPosition;
        // reset minions' pos
        //Transform minionSet = UsefulItems.instance.minionSet;
        foreach (Transform item in UsefulItems.instance.minionSet)
        {
            Debug.Log(item.name);
            item.GetComponent<NavMeshAgent>().enabled = false;
            item.position = minionPosition;
            item.GetComponent<NavMeshAgent>().enabled = true;
        }
        SceneManager.LoadScene(nextSceneName);

        cover.DOFade(0,1f);
        Invoke("EnterSceneFadeout", 1.2f);
    }

    void EnterSceneFadeout()
    {
        cover.gameObject.SetActive(false);
    }
}

