using UnityEngine;
using Assets.Scripts.Extensions;
using UnityEngine.UI;

public class CameraOptions : MonoBehaviour
{
    private Camera mainCamera;
    private Button faceAngled;
    private Button faceX;
    private Button faceY;
    private Button faceZ;
    private Button centreArena;
    private Button centreOldNest;
    private Button[] centreNewNest;

    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        faceAngled = this.ButtonByName("FaceAngled");
        faceAngled.onClick.AddListener(() => changeAngle(Vector3.zero));
        faceX = this.ButtonByName("FaceX");
        faceX.onClick.AddListener(() => changeAngle(Vector3.right));
        faceY = this.ButtonByName("FaceY");
        faceY.onClick.AddListener(() => changeAngle(-Vector3.up));
        faceZ = this.ButtonByName("FaceZ");
        faceZ.onClick.AddListener(() => changeAngle(Vector3.forward));
    }

    private void changeAngle(Vector3 axis)
    {
        mainCamera.transform.rotation = Quaternion.LookRotation(axis);
    }

    private void changePosition()
    {

    }
}
