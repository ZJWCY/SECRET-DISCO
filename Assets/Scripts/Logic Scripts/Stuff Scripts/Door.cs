using UnityEngine;

public class Door : Stuff
{
    private object tempWarning = TempWarning.Log("Door.OnClick should be rewriten as the actual Door.OnClick instand of a temp method.");

    public override void OnClick(Transform _ = null)
    {
        if (name.EndsWith("Main"))
        {
            TempWarning.InvokeDebug(() => { GameManager.endingManager.enabled = true; });

            if (DataManager.uncheckedMyths.Count > 0)
                return;
            var ms = DataManager.isMythUnlocked;
            for (int i = 1; i < ms.Length; i++)
                if (!ms[i])
                    return;
            GameManager.endingManager.enabled = true;
        }
        else
            TempWarning.InvokeDebug(() => { Debug.Log("The door is open."); });
    }
}
