using UnityEngine;

public class UsefulItems : MonoBehaviour
{
    public static UsefulItems instance;
    public Transform minionSet;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
}
