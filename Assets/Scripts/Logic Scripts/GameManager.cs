using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    public float deltaSan;
    public Kink[] kinkPrefabs;
    [Min(0)] public int extraKinkCounts = 0;
    public Button[] defaultKinkAnswers;
    public Transform kinksParent;
    public Myth mythManager;
    public Eyelid eyelid;

    static public ushort gameStage; // 1：位于迷思界面；2: 当前存在kink
    static public Button[] DefaultKinkAnswers;
    static public StuffDetector stuffDetector;
    static public EndingManager endingManager;
    static private bool isToQuit;

    public const float HALF_DIALOG_WIDTH = 225f;
    public const float HALF_DIALOG_HEIGHT = 270f;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        TempWarning.Invoke(() =>
        {
            Myth.allMyths = mythManager.GetComponentsInChildren<Myth>();
            Myth.MythTip = mythManager.mythTip;

            DataManager.LoadDatas();
            if (DataManager.uncheckedMyths.Count > 0)
                Myth.MythTip.SetActive(true);

            stuffDetector = GetComponent<StuffDetector>();
            stuffDetector.enabled = true;

            if (!TryGetComponent(out endingManager))
            {
                endingManager = gameObject.AddComponent<EndingManager>();
                endingManager.enabled = false;
            }

            DefaultKinkAnswers = defaultKinkAnswers;

            eyelid.StartCoroutine(nameof(Eyelid.TryWink));

            StartCoroutine(nameof(JudgeSan));
            StartCoroutine(nameof(TryMakeKink));
        });
    }

    private IEnumerator JudgeSan()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            var san = DataManager.San;
            san += deltaSan;
            foreach (var f in DataManager.flags)
            {
                if (f.StartsWith('B'))
                    san += float.Parse(f.Split('_')[1]);
            }
            DataManager.San = san;

            switch ((int)san / 10)
            {
                case 9:
                    OldTelevision.stabilityOne = 6f;
                    OldTelevision.stabilityTwo = 50f;
                    break;
                case 1:
                    WinOSController.EnableInput();
                    break;
                case 0:
                    WinOSController.DisableInput();
                    if (san <= 0f)
                    {
                        DataManager.San = -10f;
                        StartCoroutine(Shut(8f));
                        StopCoroutine(nameof(JudgeSan));
                    }
                    break;
            }

            if (san <= 80f)
            {
                OldTelevision.stabilityOne = (san + 11f) * 0.01f;
                OldTelevision.stabilityTwo = (san + 11f) * 0.05f;
            }

            TempWarning.InvokeDebug(() => { Debug.Log(DataManager.San); });
        }
    }

    private IEnumerator TryMakeKink()
    {
        while (true)
        {
            if (gameStage == 1)
                yield return null;
            else
            {
                var san = DataManager.San;
                yield return new WaitForSeconds(san / 20f);

                var s = (int)san / 5;

                var count = Random.Range(0, 5 - s / 9);
                if (count > 3)
                    count = 3;
                for (int i = 0; i < count; i++)
                {
                    int x = Random.Range(0, Screen.width + 1), index = x % (kinkPrefabs.Length + extraKinkCounts + s);
                    if (index < kinkPrefabs.Length + extraKinkCounts)
                    {
                        var kink = index < kinkPrefabs.Length ? kinkPrefabs[index] : Resources.Load($"Kinks/{index}");

                        var y = Random.Range(0, Screen.height + 1);
                        var pos = new Vector2(x, y);
                        if (san >= 5f)
                        {
                            pos.x *= (Screen.width - HALF_DIALOG_WIDTH * 2f) / Screen.width;
                            pos.x += HALF_DIALOG_WIDTH;
                            pos.y *= (Screen.height - HALF_DIALOG_HEIGHT * 2f) / Screen.height;
                            pos.y += HALF_DIALOG_HEIGHT;
                            Instantiate(kink, pos, Quaternion.identity, kinksParent);
                        }
                        else
                            Instantiate(kink, pos, Quaternion.identity, kinksParent.parent);
                    }
                }
            }
        }
    }

    private IEnumerator Shut(float secondsToWait)
    {
        yield return new WaitForSeconds(secondsToWait);

        isToQuit = true;
#if !DEBUG
        WinOSController.Sleep();
#endif
        TempWarning.InvokeDebug(() => { Debug.Log("Shut is invoked."); });
    }

    void OnApplicationFocus()
    {
        WinOSController.KillTaskmgr();

        if (isToQuit)
            Application.Quit();
    }

    void OnApplicationQuit()
    {
        //TODO: Delete this
        TempWarning.InvokeDebug(() => { GetComponent<FrameRateLock>().enabled = true; });
    }
}
