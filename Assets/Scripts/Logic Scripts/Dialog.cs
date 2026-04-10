using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    const float DISAPPEAR_PACE = -0.1f;

    public ushort[] hiddenAnswers;
    public Transform rayBlocker;

    private float timeToDisappear;
    private Dialog previous;

    static public Dialog Current
    {
        get => current;
        set
        {
            if (null != current)
                foreach (var img in current.GetComponentsInChildren<Image>())
                    img.raycastTarget = false;
            if (null != value)
            {
                value.previous = current;
                isToExpand = true;
            }
            current = value;
        }
    }
    static private Dialog current = null;
    static private bool isToExpand;
    static private Vector3 initialScale = new Vector3(1f, 0.2f, 1f);
    static private int[] ANSWER_POSITIONS = { -240, -210, -180, -150, -120 };

    void Start()
    {
        name = name.Remove(name.Length - 7);

        rayBlocker = Instantiate(rayBlocker, null);
        var cam = Camera.main;
        var pos = transform.position;
        rayBlocker.position = cam.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 1f));
        rayBlocker.rotation = cam.transform.rotation;
        rayBlocker.localScale *= Mathf.Tan(0.5f * Mathf.Deg2Rad * cam.fieldOfView) / 0.5773505f;

        if (isToExpand)
            StartCoroutine(nameof(Expand));

        foreach (var a in hiddenAnswers)
        {
            var answerIndex = name + '-' + a;
            if (!DataManager.isAnswerUnhidden.TryGetValue(answerIndex, out bool result))
                DataManager.isAnswerUnhidden[answerIndex] = false;
        }

        StartCoroutine(nameof(UpdateInCoroutine));
    }

    private IEnumerator UpdateInCoroutine()
    {
        var time = Time.fixedDeltaTime;
        var cg = GetComponent<CanvasGroup>();
        var rayBlockerObj = rayBlocker.gameObject;

        while (true)
        {
            JudgeAnswers();

            if (ReferenceEquals(current, this))
            {
                timeToDisappear = 3f;
                cg.alpha += time * 4f;
                rayBlockerObj.SetActive(true);
            }
            else if (cg.alpha <= 0f)
            {
                Destroy(rayBlockerObj);
                Destroy(gameObject);
            }
            else
            {
                if (null == current)
                    timeToDisappear = DISAPPEAR_PACE;

                if (timeToDisappear <= 1f)
                {
                    cg.alpha -= time / (timeToDisappear + 0.4f);
                    if (cg.alpha < 0.2f)
                        rayBlockerObj.SetActive(false);
                }
                else
                    timeToDisappear -= time;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator Expand()
    {
        isToExpand = false;
        transform.localScale = initialScale;
        while (transform.localScale.y < 0.9f)
        {
            transform.localScale += Vector3.up * 0.15f;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    private void JudgeAnswers()
    {
        var posIndex = 0;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var a = transform.GetChild(i).gameObject;
            if (!DataManager.isAnswerUnhidden.TryGetValue(name + '-' + a.name, out bool result) || result)
            {
                a.GetComponent<RectTransform>().localPosition = new Vector3(0f, ANSWER_POSITIONS[posIndex++], 0f);
                a.SetActive(true);
            }
            else
                a.SetActive(false);
        }
    }

    /// <summary>
    /// 放在对话选项按钮点击事件的首位。参数内容为 空字符串，或 "点击后要加载的对话编号"，或 "点击后要加载的对话编号 (!)加载前置flag1 (!)加载前置flag2 (!)加载前置flag3..."
    /// </summary>
    /// <param name="info">空字符串，或 "点击后要加载的对话编号"，或 "点击后要加载的对话编号 (!)加载前置flag1 (!)加载前置flag2 (!)加载前置flag3..."</param>
    public void EndDialog(string info)
    {
        DataManager.answeredDialogs.Add(name);
        if (info.Length == 0)
        {
            current = previous;
            if (null != current)
                foreach (var img in current.GetComponentsInChildren<Image>())
                    img.raycastTarget = true;

            foreach (var img in GetComponentsInChildren<Image>())
                img.raycastTarget = false;
            timeToDisappear = DISAPPEAR_PACE;
        }
        else
        {
            var infos = info.Split(' ');
            for (int i = 1; i < infos.Length; i++)
            {
                if (!DataManager.CheckFlag(infos[i]))
                {
                    foreach (var img in GetComponentsInChildren<Image>())
                        img.raycastTarget = false;
                    timeToDisappear = DISAPPEAR_PACE;

                    return;
                }
            }
            current = Instantiate(Resources.Load<GameObject>($"{infos[0][0]}/{infos[0]}"), transform.position, Quaternion.identity, transform.parent).GetComponent<Dialog>();
            current.previous = previous;

            Destroy(rayBlocker.gameObject);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 参数内容为 "(-)San值"，或 "(-)San值 (!)前置flag1 (!)前置flag2 (!)前置flag3..."
    /// </summary>
    /// <param name="info">"(-)San值"，或 "(-)San值 (!)前置flag1 (!)前置flag2 (!)前置flag3..."</param>
    public void TryAddSan(string info)
    {
        if (DataManager.answeredDialogs.Contains(name))
            return;

        var infos = info.Split(' ');
        for (int i = 1; i < infos.Length; i++)
            if (!DataManager.CheckFlag(infos[i]))
                return;
        DataManager.San += float.Parse(infos[0]);
    }

    /// <summary>
    /// 参数内容为 "对话选项编号"，或 "对话选项编号 (!)前置flag1 (!)前置flag2 (!)前置flag3..."
    /// </summary>
    /// <param name="info">"对话选项编号"，或 "对话选项编号 (!)前置flag1 (!)前置flag2 (!)前置flag3..."</param>
    static public void TryUnhideAnswer(string info)
    {
        var infos = info.Split(' ');
        if (DataManager.isAnswerUnhidden.TryGetValue(infos[0], out bool result) && result)
            return;

        for (int i = 1; i < infos.Length; i++)
            if (!DataManager.CheckFlag(infos[i]))
                return;
        DataManager.isAnswerUnhidden[infos[0]] = true;
    }

    public void TryUnhidePrevAnswer(string info)
    {
        if (null != previous && info.Remove(info.LastIndexOf('-')) == previous.name)
            DataManager.isAnswerUnhidden[info] = true;
    }

    static public void TryHideAnswer(string info)
    {
        var infos = info.Split(' ');
        if (DataManager.isAnswerUnhidden.TryGetValue(infos[0], out bool result) && !result)
            return;

        for (int i = 1; i < infos.Length; i++)
            if (!DataManager.CheckFlag(infos[i]))
                return;
        DataManager.isAnswerUnhidden[infos[0]] = false;
    }

    /// <summary>
    /// 参数内容为 "迷思序号"
    /// </summary>
    /// <param name="mythNum">"迷思序号"</param>
    static public void TryUnlockMyth(string mythNum)
    {
        Myth.TryUnlock(ushort.Parse(mythNum));
    }

    /// <summary>
    /// 参数内容为 "(!)flag名"，或 "(!)flag名 (!)前置flag1 (!)前置flag2 (!)前置flag3..."
    /// </summary>
    /// <param name="info">"(!)flag名"，或 "(!)flag名 (!)前置flag1 (!)前置flag2 (!)前置flag3..."</param>
    static public void TrySetFlag(string info)
    {
        var infos = info.Split(' ');
        info = infos[0].Remove(0, 1);
        if (info.StartsWith('!'))
        {
            if (DataManager.flags.Contains(info))
            {
                for (int i = 1; i < infos.Length; i++)
                    if (!DataManager.CheckFlag(infos[i]))
                        return;
                DataManager.flags.Remove(info);
            }
        }
        else if (!DataManager.flags.Contains(infos[0]))
        {
            for (int i = 1; i < infos.Length; i++)
                if (!DataManager.CheckFlag(infos[i]))
                    return;
            DataManager.flags.Add(infos[0]);
        }
    }
}
