using UnityEngine;

public class Stuff : MonoBehaviour
{
    static private Transform dialogsParent = null;

    public virtual void OnClick(Transform dialogsParent = null)
    {
        if (null == Stuff.dialogsParent)
            Stuff.dialogsParent = dialogsParent;
        var mousePos = Input.mousePosition;
        var resolution = Screen.currentResolution;
        mousePos.x = Mathf.Clamp(mousePos.x, GameManager.HALF_DIALOG_WIDTH, resolution.width - GameManager.HALF_DIALOG_WIDTH);
        mousePos.y = Mathf.Clamp(mousePos.y, GameManager.HALF_DIALOG_HEIGHT, resolution.height - GameManager.HALF_DIALOG_HEIGHT);

        var dialogIdx = name.Split(' ')[0];
        Dialog.Current = Instantiate(Resources.Load<GameObject>($"{dialogIdx}/{dialogIdx}"), mousePos, Quaternion.identity, Stuff.dialogsParent).GetComponent<Dialog>();
    }
}
