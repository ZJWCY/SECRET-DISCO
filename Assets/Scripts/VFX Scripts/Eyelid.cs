using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Eyelid : MonoBehaviour
{
    [Range(0, 6)] public int winkRate = 4;
    public Image up, down;

    public IEnumerator TryWink()
    {
        float pace = Time.fixedDeltaTime * 22f, seed = 10 - winkRate;
        while (true)
        {
            yield return new WaitForSecondsRealtime(60f);
            var trigger = Random.Range(0, seed);
            if (trigger < 2)
                yield return Wink(pace);
            else if (trigger < 4)
            {
                yield return Wink(pace);
                yield return new WaitForFixedUpdate();
                yield return Wink(pace);
            }
            else
                Resources.UnloadUnusedAssets();
        }
    }

    private IEnumerator Wink(float pace)
    {
        while (up.fillAmount < 1f)
        {
            up.fillAmount += pace;
            down.fillAmount += pace;
            yield return null;
        }
        up.fillAmount = 0f;
        down.fillAmount = 0f;
    }
}
