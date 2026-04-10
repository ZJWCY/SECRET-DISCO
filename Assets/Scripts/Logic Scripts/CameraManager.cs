using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [HideInInspector] public GameObject[] cameras;

    private object tempWarning = TempWarning.Log("CameraManager should be totally rewritten.");
    private int camIndex = 0;

    void Start()
    {
        var cameraRoot = Camera.main.transform.parent;
        var c = cameraRoot.childCount;
        cameras = new GameObject[c];
        for (int i = 0; i < c; i++)
            cameras[i] = cameraRoot.GetChild(i).gameObject;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.S))
        {
            camIndex += camIndex % 2 == 0 ? 1 : -1;
            var currentCam = Camera.main.gameObject;
            cameras[camIndex].SetActive(true);
            currentCam.SetActive(false);
            OldTelevision.mat = null;
        }
        else if (GameManager.gameStage == 0 && null == Dialog.Current)
        {
            if (Input.GetKeyUp(KeyCode.D))
                SetCameraNext();
            else if (Input.GetKeyUp(KeyCode.A))
                SetCameraPrev();
        }
    }

    public void SetCameraNext(int times = 1)
    {
        times = Mathf.Max(1, times % (cameras.Length / 2));
        for (int i = 0; i < times; i++)
        {
            camIndex = (camIndex + 2) % cameras.Length;
            var currentCam = Camera.main.gameObject;
            cameras[camIndex].SetActive(true);
            currentCam.SetActive(false);
        }
    }

    public void SetCameraPrev(int times = 1)
    {
        times = Mathf.Max(1, times % (cameras.Length / 2));
        for (int i = 0; i < times; i++)
        {
            camIndex -= 2;
            if (camIndex < 0)
                camIndex = camIndex % 2 == 0 ? cameras.Length - 2 : cameras.Length - 1;
            camIndex %= cameras.Length;
            var currentCam = Camera.main.gameObject;
            cameras[camIndex].SetActive(true);
            currentCam.SetActive(false);
        }
    }
}
