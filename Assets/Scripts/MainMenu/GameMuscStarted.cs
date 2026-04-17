using UnityEngine;

public class GameMusicStarter : MonoBehaviour
{
    void Start()
    {
        if (MusicManagerPersistent.Instance != null)
        {
            MusicManagerPersistent.Instance.SwitchToGameMusic(1.0f);
        }
    }
}