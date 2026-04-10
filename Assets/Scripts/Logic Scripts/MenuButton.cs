using UnityEngine;

public class MenuButton : MonoBehaviour
{
    private object tempWarning = TempWarning.Log("MenuButton.cs is a temporary script!");

    public GameObject gameManager;
    public AudioClip commonMusic;
    public AudioSource backgroundMusicPlayer;

    void Awake()
    {
        FrameRateLock.LockFrameRate();
    }

    public void OnClickStart()
    {
        gameManager.SetActive(true);
        backgroundMusicPlayer.clip = commonMusic;
        backgroundMusicPlayer.volume = 0.6f;
        backgroundMusicPlayer.Play();
        Destroy(transform.parent.gameObject);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}
