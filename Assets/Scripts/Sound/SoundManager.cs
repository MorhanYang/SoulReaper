using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string Name;
    public AudioClip Clip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField]
    public List<Sound> MySounds;

    [SerializeField]
    GameObject SoundPlayer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) 
        {
            PlaySoundAt(PlayerManager.instance.player.gameObject.transform.position, "Hurt", false, false, 1, 1, 100, 100);
            Debug.Log("sound played");
        }
    }

    public void PlaySoundAt (Vector3 Position, string SoundName, bool loop, bool infinite, float Time, float volume, float MaxDistance, float MinDistance)//Player
    {
        for (int i = 0; i < MySounds.Count; i++) 
        {
            if (MySounds[i].Name == SoundName) 
            {
                GameObject MySoundPlayer = Instantiate(SoundPlayer, Position, transform.rotation);
                SoundPlayer SoundPlayerScript = MySoundPlayer.GetComponent<SoundPlayer>();
                SoundPlayerScript.MyClip = MySounds[i].Clip;
                SoundPlayerScript.Infinite = infinite;
                SoundPlayerScript.loop = loop;
                SoundPlayerScript.timer = Time;
                SoundPlayerScript.volume = Mathf.Clamp(volume,0,1);
                SoundPlayerScript.MaxDistance = MaxDistance;
                SoundPlayerScript.MinDistance = MinDistance;

                i += MySounds.Count;
            }
        }
    }

}
