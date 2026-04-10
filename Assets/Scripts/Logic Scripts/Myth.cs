using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Myth : MonoBehaviour, IPointerEnterHandler
{
    public string flagsToSet;
    public string mythsToUnlock;
    public string[] answersToUnhide;
    [Header("_____________________ Preconditions _____________________")]
    public string[] requisiteMyths;
    public string[] requisiteDialogs;
    public string[] requisiteFlags;

    [Header("_____________________________ UI _____________________________")]
    public GameObject mythTip;
    public GameObject mythLock;
    public int localPositionLimit;

    static public GameObject MythTip;
    static public Myth[] allMyths;

    public void OpenCloseCanvas()
    {
        var canvas = transform.parent.gameObject;
        if (canvas.activeSelf)
        {
            GameManager.gameStage = 0;
            StopCoroutine(nameof(OnOpenCanvas));
            StartCoroutine(nameof(OnCloseCanvas), canvas);
        }
        else if (Kink.count == 0)
        {
            canvas.SetActive(true);
            StopCoroutine(nameof(OnCloseCanvas));
            StartCoroutine(nameof(OnOpenCanvas), canvas);
            Dialog.Current = null;
            GameManager.gameStage = 1;
        }
    }

    private IEnumerator OnOpenCanvas(GameObject canvas)
    {
        GameManager.stuffDetector.enabled = false;

        var time = Time.fixedDeltaTime;
        var cg = canvas.GetComponent<CanvasGroup>();
        while (cg.alpha < 1)
        {
            cg.alpha += time * 5f;
            yield return new WaitForFixedUpdate();
        }

        StartCoroutine(nameof(ProcessInput));
    }

    private IEnumerator ProcessInput()
    {
        Vector3 mousePos, prevMousePos = Vector3.zero, mouseVelocity = Vector3.zero;
        while (true)
        {
            var scale = transform.localScale;

            //var deltaScale = Input.GetAxis("Mouse ScrollWheel");
            var deltaScale = Input.mouseScrollDelta.y * 0.2f;
            if (deltaScale != 0f)
            {
                scale += new Vector3(deltaScale, deltaScale);
                if (scale.x > 0.4f && scale.x < 3f)
                {
                    transform.position += (Input.mousePosition - transform.position) * (1f - scale.x / transform.localScale.x);
                    transform.localScale = scale;
                }
            }

            if (Input.GetMouseButtonDown(0))
                prevMousePos = Input.mousePosition;
            else if (Input.GetMouseButton(0))
            {
                mousePos = Input.mousePosition;
                mouseVelocity = mousePos - prevMousePos;
                transform.localPosition += mouseVelocity;
                prevMousePos = mousePos;
            }
            else if (mouseVelocity.magnitude > 10f)
            {
                var limit = (localPositionLimit + 200) * scale.x;
                var pos = transform.localPosition + mouseVelocity;
                if (pos.x < limit && pos.x > -limit && pos.y < limit && pos.y > -limit)
                {
                    transform.localPosition = pos;
                    mouseVelocity *= 0.6f;
                }
                else
                    mouseVelocity = Vector3.zero;
            }
            else
            {
                var limit = localPositionLimit * scale.x;
                var pos = transform.localPosition;
                if (pos.x > limit)
                    pos.x -= 80f;
                else if (pos.x < -limit)
                    pos.x += 80f;
                if (pos.y > limit)
                    pos.y -= 80f;
                else if (pos.y < -limit)
                    pos.y += 80f;
                if (Input.mouseScrollDelta.y == 0f)
                    transform.localPosition = pos;
            }
            yield return null;
        }
    }

    private IEnumerator OnCloseCanvas(GameObject canvas)
    {
        StopCoroutine(nameof(ProcessInput));

        var time = Time.fixedDeltaTime;
        var cg = canvas.GetComponent<CanvasGroup>();
        while (cg.alpha > 0)
        {
            cg.alpha -= time * 5f;
            yield return new WaitForFixedUpdate();
        }
        canvas.SetActive(false);

        if (Kink.count == 0)
            GameManager.stuffDetector.enabled = true;

        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
    }

    static public bool TryUnlock(ushort mythNum)
    {
        if (DataManager.isMythUnlocked[mythNum])
            return true;

        var myth = allMyths[mythNum];

        var result = false;
        var len = myth.requisiteMyths.Length;
        for (int i = 0; i < len; i++)
        {
            var myths = myth.requisiteMyths[i].Split(' ');
            for (int j = 0; j < myths.Length; j++)
            {
                if (!DataManager.CheckMyth(myths[j]))
                    break;
                else if (j == myths.Length - 1)
                    result = true;
            }

            if (result)
                break;
            else if (i == len - 1)
                return false;
        }

        result = false;
        len = myth.requisiteDialogs.Length;
        for (int i = 0; i < len; i++)
        {
            var dialogs = myth.requisiteDialogs[i].Split(' ');
            for (int j = 0; j < dialogs.Length; j++)
            {
                if (!DataManager.CheckDialog(dialogs[j]))
                    break;
                else if (j == dialogs.Length - 1)
                    result = true;
            }

            if (result)
                break;
            else if (i == len - 1)
                return false;
        }

        result = false;
        len = myth.requisiteFlags.Length;
        for (int i = 0; i < len; i++)
        {
            var flags = myth.requisiteFlags[i].Split(' ');
            for (int j = 0; j < flags.Length; j++)
            {
                if (!DataManager.CheckFlag(flags[j]))
                    break;
                else if (j == flags.Length - 1)
                    result = true;
            }

            if (result)
                break;
            else if (i == len - 1)
                return false;
        }

        DataManager.isMythUnlocked[mythNum] = true;
        DataManager.uncheckedMyths.Add(mythNum);
        MythTip.SetActive(true);
        myth.mythTip.SetActive(true);
        myth.mythLock.SetActive(false);

        foreach (var info in myth.answersToUnhide)
        {
            if (info.StartsWith('!'))
                Dialog.TryHideAnswer(info.Remove(0, 1));
            else
                Dialog.TryUnhideAnswer(info);
        }
        if (myth.flagsToSet.Length > 0)
            foreach (var f in myth.flagsToSet.Split(' '))
            {
                if (f.StartsWith('!'))
                    DataManager.flags.Remove(f.Remove(0, 1));
                else
                    DataManager.flags.Add(f);
            }
        if (myth.mythsToUnlock.Length > 0)
            foreach (var m in myth.mythsToUnlock.Split(' '))
                TryUnlock(ushort.Parse(m));

        return true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!mythTip.activeSelf || !ushort.TryParse(name.Split(' ')[0], out ushort result))
            return;

        DataManager.uncheckedMyths.Remove(result);
        if (DataManager.uncheckedMyths.Count == 0)
            MythTip.SetActive(false);
        mythTip.SetActive(false);
    }
}
