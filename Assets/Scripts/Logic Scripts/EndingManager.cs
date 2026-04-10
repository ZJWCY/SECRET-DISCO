using System.Collections;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

[RequireComponent(typeof(GameManager))]
[DisallowMultipleComponent]
public class EndingManager : MonoBehaviour
{
    public Transform endingDoor;
    public GameObject canvasCommon;
    public Image[] imageLogoGame;
    public GameObject imageLogoZ;
    public AudioClip endingMusic;
    public AudioSource backgroundMusicPlayer;
    public Light mainLight;
    public CameraManager cameraManager;

    private bool flagMoveObj = false;
    private Vector3 normalGravity = Physics.gravity;
    private ArrayList forces = new ArrayList();
    private Rigidbody[] rigidbodies;

    void Start()
    {
        GetComponent<GameManager>().StopAllCoroutines();

#if !DEBUG
        WinOSController.DisableInput();
        WinOSController.LockVolume(0.2f);
#endif
        backgroundMusicPlayer.Stop();
        Cursor.lockState = CursorLockMode.Locked;
        canvasCommon.SetActive(false);
        GameManager.stuffDetector.enabled = false;
        FrameRateLock.UnlockFrameRate();

        StartCoroutine(PlayEnding());
    }

    /*
    void Update()
    {
        //根据WinOSController.LockedVolume，实时调节audioSource音量
    }
    */

    void OnTriggerEnter(Collider other)
    {
        other.transform.position = new Vector3(9.2f, 5f, 18.4f);
    }

    private IEnumerator PlayEnding()
    {
        backgroundMusicPlayer.clip = endingMusic;
        backgroundMusicPlayer.volume = 1f;
        backgroundMusicPlayer.Play();
        backgroundMusicPlayer.loop = false;

        Color oldLightCol = mainLight.color, oldBackCol = cameraManager.cameras[2].GetComponent<Camera>().backgroundColor;
        OldTelevision.stabilityOne = 0.91f; OldTelevision.stabilityTwo = 4.55f;

        yield return new WaitForSecondsRealtime(4.5f); // 4.5s
        cameraManager.SetCameraNext();
        foreach (var s in GameObject.FindGameObjectsWithTag("Stuff"))
        {
            Stuff tmp;
            if (!s.TryGetComponent(out tmp) || tmp.GetType() == typeof(Stuff))
            {
                s.GetComponent<MeshCollider>().convex = true;
                s.AddComponent<Rigidbody>();
            }
        }
        rigidbodies = FindObjectsOfType<Rigidbody>();

        yield return new WaitForSecondsRealtime(3.9f); // 8.4s
        cameraManager.SetCameraNext(2);

        yield return new WaitForSecondsRealtime(0.5f); // 8.9s
        StartCoroutine(ColorFlash(oldLightCol, oldBackCol));
        Physics.gravity = Vector3.zero;
        MoveObjects(ref flagMoveObj, true);

        yield return new WaitForSecondsRealtime(3.1f); // 12.0s
        cameraManager.SetCameraNext(3);
        MoveObjects(ref flagMoveObj);
        yield return new WaitForSecondsRealtime(0.2f); // 12.2s
        cameraManager.SetCameraNext();
        yield return new WaitForSecondsRealtime(0.2f); // 12.4s
        cameraManager.SetCameraNext();
        yield return new WaitForSecondsRealtime(0.3f); // 12.7s
        cameraManager.SetCameraNext();

        yield return new WaitForSecondsRealtime(1.2f); // 13.9s
        cameraManager.SetCameraNext();
        yield return new WaitForSecondsRealtime(1f);   // 14.9s
        cameraManager.SetCameraNext();

        yield return new WaitForSecondsRealtime(1.1f); // 16.0s
        cameraManager.SetCameraNext();
        MoveObjects(ref flagMoveObj);
        yield return new WaitForSecondsRealtime(1.7f); // 17.7s
        cameraManager.SetCameraNext();
        MoveObjects(ref flagMoveObj);
        yield return new WaitForSecondsRealtime(1.7f); // 19.4s
        cameraManager.SetCameraNext(2);
        MoveObjects(ref flagMoveObj);
        yield return new WaitForSecondsRealtime(1.8f); // 21.2s
        OldTelevision.stabilityOne = 0.21f; OldTelevision.stabilityTwo = 1.05f;
        mainLight.color = Color.red;
        endingDoor.Rotate(Vector3.up, -80f, Space.Self);
        endingDoor.localPosition = new Vector3(10.92f, 0f, -1.3f);
        MoveObjects(ref flagMoveObj);

        yield return new WaitForSecondsRealtime(1.8f); // 23.0s
        var cameraFlashFast = CameraFlashFast();
        StartCoroutine(cameraFlashFast);
        while (imageLogoGame[0].fillAmount < 1f)
        {
            foreach (var i in imageLogoGame)
                i.fillAmount += 0.0029f;
            yield return new WaitForFixedUpdate();
        }                                              // 30.0s
        foreach (var i in imageLogoGame)
            i.fillOrigin = 0;
        MoveObjects(ref flagMoveObj);
        while (imageLogoGame[0].fillAmount > 0f)
        {
            foreach (var i in imageLogoGame)
                i.fillAmount -= 0.0029f;
            yield return new WaitForFixedUpdate();
        }                                              // 37.0s
        Physics.gravity = normalGravity;
        StopCoroutine(cameraFlashFast);
        SetColors(oldLightCol, oldBackCol);

        yield return new WaitForSecondsRealtime(2f);   // 39.0s
        OldTelevision.stabilityOne = 0.91f; OldTelevision.stabilityTwo = 4.55f;
        var cameraFlashSlow = CameraFlashSlow();
        StartCoroutine(cameraFlashSlow);

        yield return new WaitForSecondsRealtime(91.3f);// 2m 10.3s
        OldTelevision.stabilityOne = 0.21f; OldTelevision.stabilityTwo = 1.05f;
        StartCoroutine(ColorFlash(oldLightCol, oldBackCol));

        yield return new WaitForSecondsRealtime(0.5f); // 2m 10.8s
        mainLight.color = Color.red;
        Physics.gravity = Vector3.zero;

        yield return new WaitForSecondsRealtime(27.2f);// 2m 38.0s
        OldTelevision.stabilityOne = 0.91f; OldTelevision.stabilityTwo = 4.55f;
        SetColors(oldLightCol, oldBackCol);
        Physics.gravity = normalGravity;

        yield return new WaitForSecondsRealtime(75f);  // 3m 53.0s
        SetColors(Color.red, Color.red);
        Physics.gravity = Vector3.zero;

        yield return new WaitForSecondsRealtime(9f);   // 4m 2.0s
        StopCoroutine(cameraFlashSlow);
        cameraFlashFast = CameraFlashFast();
        StartCoroutine(cameraFlashFast);

        yield return new WaitForSecondsRealtime(7f);   // 4m 9.0s
        StopCoroutine(cameraFlashFast);
        foreach (var c in cameraManager.cameras)
        {
            c.GetComponent<OldTelevision>().enabled = false;
            c.GetComponent<PixelPerfectCamera>().enabled = false;
        }

        yield return new WaitForSecondsRealtime(7f);   // 4m 16.0s
        StartCoroutine(CameraFlashFast(0.05f));
        MoveObjects(ref flagMoveObj, true);

        yield return new WaitForSecondsRealtime(6.5f); // 4m 22.5s
        imageLogoZ.SetActive(true);
#if !DEBUG
        yield return new WaitForSecondsRealtime(0.7f); // 4m 23.2s (end of music)
        WinOSController.Sleep();

        yield return new WaitForSecondsRealtime(5f);
        System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
    }

    private IEnumerator ColorFlash(Color oldLightCol, Color oldBackCol)
    {
        for (int i = 0; i < 11; i++)
        {
            if (i % 2 == 0)
                SetColors(Color.black, Color.red);
            else
                SetColors(oldLightCol, oldBackCol);

            yield return new WaitForSecondsRealtime(0.008f * i);
        }
    }

    private IEnumerator CameraFlashFast(float interval = 0.1f)
    {
        while (true)
        {
            cameraManager.SetCameraNext(Random.Range(1, 4));
            yield return new WaitForSecondsRealtime(interval);
        }
    }

    private IEnumerator CameraFlashSlow()
    {
        while (true)
        {
            var r = Random.Range(1, 5);
            cameraManager.SetCameraNext(r);
            yield return new WaitForSecondsRealtime(r * 2 + 1);

            MoveObjects(ref flagMoveObj, true);
        }
    }

    private void MoveObjects(ref bool negative, bool newForces = false)
    {
        negative = !negative;

        if (newForces)
        {
            forces.Clear();
            for (int i = 0; i < rigidbodies.Length; i++)
            {
                var force = Random.Range(800, 2000);
                if (force % 2 == 0)
                    force = -force;
                rigidbodies[i].AddForce(force, force, force);
                forces.Add(force);
            }
            return;
        }

        if (negative)
            for (int i = 0; i < rigidbodies.Length; i++)
                rigidbodies[i].AddForce(-(int)forces[i], -(int)forces[i], -(int)forces[i]);
        else
            for (int i = 0; i < rigidbodies.Length; i++)
                rigidbodies[i].AddForce((int)forces[i], (int)forces[i], (int)forces[i]);
    }

    private void SetColors(Color lightCol, Color backgroundCol)
    {
        mainLight.color = lightCol;
        cameraManager.cameras[4].GetComponent<Camera>().backgroundColor = backgroundCol;
        cameraManager.cameras[5].GetComponent<Camera>().backgroundColor = backgroundCol;
        cameraManager.cameras[8].GetComponent<Camera>().backgroundColor = backgroundCol;
        cameraManager.cameras[9].GetComponent<Camera>().backgroundColor = backgroundCol;
    }
}
