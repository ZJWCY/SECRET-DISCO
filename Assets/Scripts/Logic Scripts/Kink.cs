using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Kink : MonoBehaviour
{
    static public ushort count;

    void Start()
    {
        if (GameManager.gameStage == 1)
        {
            Destroy(gameObject);
            count = 0;
            return;
        }

        for (int i = 1; i < transform.childCount; i++)
        {
            var sub = transform.GetChild(i).gameObject;
            var subAnswerOnClick = sub.GetComponentInChildren<Button>().onClick;
            subAnswerOnClick.AddListener(() =>
            {
                Destroy(sub);
                subAnswerOnClick.RemoveAllListeners();
            });
        }

        if (++count == 1)
        {
            GameManager.stuffDetector.enabled = false;
            GameManager.gameStage = 2;

            if (Random.Range(0, 5 - (int)DataManager.San / 30) == 0)
                Dialog.Current = null;
            else if (null != Dialog.Current)
                foreach (var img in Dialog.Current.GetComponentsInChildren<Image>())
                    img.raycastTarget = false;
        }

        var buttonSlot = transform.GetChild(0);
        if (buttonSlot.childCount == 0)
        {
            var a = GameManager.DefaultKinkAnswers;
            var answer = Instantiate(a[Random.Range(0, a.Length)], buttonSlot);
            answer.onClick.AddListener(() =>
            {
                Erase(int.Parse(answer.name.Split(' ')[0]));
                answer.onClick.RemoveAllListeners();
            });
        }
    }

    public void Erase(int san)
    {
        DataManager.San += san;
        StartCoroutine(nameof(Shrink));

        if (--count == 0)
        {
            GameManager.gameStage = 0;
            GameManager.stuffDetector.enabled = true;

            if (null != Dialog.Current)
                foreach (var img in Dialog.Current.GetComponentsInChildren<Image>())
                    img.raycastTarget = true;
        }
    }

    private IEnumerator Shrink()
    {
        while (transform.localScale.y > 0.2f)
        {
            transform.localScale -= Vector3.up * 0.5f;
            yield return null;
        }
        Destroy(gameObject);
    }
}
