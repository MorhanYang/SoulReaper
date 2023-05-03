using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Test_YouWin : MonoBehaviour
{
    [SerializeField] Animator myAnim;
    [SerializeField] CanvasGroup cover;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerControl>() != null){
            myAnim.SetBool("FadeOut", true);
            cover.gameObject.SetActive(true);
            cover.alpha = 0f;
            cover.DOFade(1, 2f);

            StartCoroutine(TurntoCredit());
        }
    }

    IEnumerator TurntoCredit()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("CreditScene");
    }
}

    // you win

