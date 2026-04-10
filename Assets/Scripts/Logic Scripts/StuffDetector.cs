using UnityEngine;

[RequireComponent(typeof(GameManager))]
[DisallowMultipleComponent]
public class StuffDetector : MonoBehaviour
{
    public Transform dialogsParent;

    static private Transform activeStuff;

    void Update()
    {
        if (null != activeStuff)
            activeStuff.GetChild(0).gameObject.SetActive(false);

        Physics.Raycast(Camera.main.ScreenPointToRay(TransformMousePos(Input.mousePosition)), out RaycastHit hit);
        var hitCollider = hit.collider;
        if (null != hitCollider && hitCollider.tag == "Stuff")
        {
            activeStuff = hitCollider.transform;
            activeStuff.GetChild(0).gameObject.SetActive(true);

            if (Input.GetMouseButtonUp(0))
            {
                activeStuff.TryGetComponent(out Stuff s);
                if (null == s)
                    activeStuff.gameObject.AddComponent<Stuff>().OnClick(dialogsParent);
                else
                    s.OnClick();
            }
        }
    }

    private Vector2 TransformMousePos(Vector2 mousePos)
    {
        var distortion = OldTelevision.Distortion;
        var resolution = Screen.currentResolution;

        mousePos.x /= resolution.width;
        mousePos.y /= resolution.height;

        var x = mousePos.x;
        mousePos.x += Mathf.Pow(mousePos.y * 2f - 1f, 2f) * distortion * (mousePos.x - 0.5f);
        mousePos.y += Mathf.Pow(x * 2f - 1f, 2f) * distortion * (mousePos.y - 0.5f);

        mousePos.x *= resolution.width;
        mousePos.y *= resolution.height;

        return mousePos;
    }

    void OnDisable()
    {
        if (null != activeStuff)
            activeStuff.GetChild(0).gameObject.SetActive(false);
    }
}
