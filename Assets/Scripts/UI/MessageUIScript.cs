using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageUIScript : MonoBehaviour
{
    [SerializeField] TMP_Text title;
    [SerializeField] TMP_Text content;

    public void ChangeMessageUIText(string myTitle, string myContent)
    {
        title.text = myTitle;
        content.text = myContent;
    }

    public void CloseMessageUI()
    {
        Destroy(gameObject);
    }
}
