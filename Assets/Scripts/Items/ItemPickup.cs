using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{

    public enum ItemType
    {
        weapon,
        message,
    }
    public ItemType itemType;

    [SerializeField] MessageUIScript messageUI;
    [SerializeField] Canvas UICanvas;
    [SerializeField] string title;
    [SerializeField] string content;
    MessageUIScript myMessage;
    bool isShowingMessage;

    [SerializeField] string playerDialgoueAfterPickup;

    private void Update()
    {
        if (isShowingMessage)
        {
            if (myMessage == null)
            {
                PlayerManager.instance.player.GetComponent<PlayerDialogue>().ShowPlayerCall(playerDialgoueAfterPickup, 8f);
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (itemType)
        {
            case ItemType.weapon:
                if (other.GetComponent<PlayerControl>() != null)
                {
                    PlayerControl myPlayercontrol = other.GetComponent<PlayerControl>();
                    myPlayercontrol.canMeleeAttack = true;
                    other.GetComponent<PlayerDialogue>().ShowPlayerCall(playerDialgoueAfterPickup, 7f);
                    Destroy(gameObject);
                }
                break;

            case ItemType.message:
                if (other.GetComponent<PlayerControl>() != null)
                {
                    myMessage = Instantiate(messageUI, UICanvas.transform);
                    myMessage.ChangeMessageUIText(title, content);
                    isShowingMessage = true;
                }
                break;

            default:
                break;
        }
        
    }
}
