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

    private void OnTriggerEnter(Collider other)
    {
        switch (itemType)
        {
            case ItemType.weapon:
                if (other.GetComponent<PlayerControl>() != null)
                {
                    PlayerControl myPlayercontrol = other.GetComponent<PlayerControl>();
                    myPlayercontrol.canMeleeAttack = true;
                    Destroy(gameObject);
                }
                break;

            case ItemType.message:
                if (other.GetComponent<PlayerControl>() != null)
                {
                    MessageUIScript myMessage = Instantiate(messageUI, UICanvas.transform);
                    myMessage.ChangeMessageUIText(title, content);
                    Destroy(gameObject);
                }
                break;

            default:
                break;
        }
        
    }
}
