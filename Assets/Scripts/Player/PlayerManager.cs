using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    private void Awake()
    {
        instance= this;
    }

    public GameObject player;
}
