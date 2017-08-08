using UnityEngine;
using Assets.Scripts.Extensions;
using Assets.Scripts.Arenas;
using UnityEngine.UI;
using System.Collections.Generic;

public class CameraOptions : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 worldSize;

    private Button faceAngled;
    private Button faceX;
    private Button faceY;
    private Button faceZ;
    private Button centreArenaCorner;
    private Button centreArena;
    private Button centreOldNest;
    private List<Button> centreNewNest;
    public GameObject newNestButton;    // prefab for the new nest button

    private List<Vector3> nestPositions;
    private Quaternion newRotation;
    private Vector3 newPosition;
    private Vector3 angledAxis;
    private float nestViewHeight;
    private float cameraSpeed = 0.03f;
    private float cameraUpdateFrequency = 0.005f;

    private void Update()
    {
        // If the user inputs any controls cancel all current position/angle changes
        if ((Input.GetAxisRaw("Horizontal") != 0f) || 
            (Input.GetAxisRaw("Vertical") != 0f) ||
            Input.GetAxisRaw("Up") != 0f || 
            Input.GetAxisRaw("Down") != 0f || 
            Input.GetMouseButtonDown(1))
        {
            CancelInvoke("SmoothAngle");
            CancelInvoke("SmoothPosition");
        }
    }

    public void SetUpCameras()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        worldSize = GameObject.Find("Arena Loader").GetComponent<ArenaLoader>().worldSize;

        InitialiseCameraPosition();
        InitialiseNestButtons();

        faceAngled = this.ButtonByName("FaceAngled");
        faceAngled.onClick.AddListener(() => ChangeAngle(new Vector3(45, -45, 45)));
        faceX = this.ButtonByName("FaceX");
        faceX.onClick.AddListener(() => ChangeAngle(Vector3.right));
        faceY = this.ButtonByName("FaceY");
        faceY.onClick.AddListener(() => ChangeAngle(-Vector3.up));
        faceZ = this.ButtonByName("FaceZ");
        faceZ.onClick.AddListener(() => ChangeAngle(Vector3.forward));

        centreArenaCorner = this.ButtonByName("CentreArenaCorner");
        centreArenaCorner.onClick.AddListener(() => ChangePosition(new Vector3(0, nestViewHeight * 3.5f, 0)));
        centreArena = this.ButtonByName("CentreArena");
        centreArena.onClick.AddListener(() => ChangePosition(new Vector3(worldSize.x / 2, nestViewHeight * 3.5f, worldSize.z / 2)));
    }

    private void InitialiseCameraPosition()
    {
        nestViewHeight = (worldSize.x + worldSize.z) / 8;
        mainCamera.transform.position = new Vector3(0, nestViewHeight, 0);
        mainCamera.transform.LookAt(new Vector3(worldSize.x / 4.5f, 0, worldSize.z / 4.5f));
    }

    private void InitialiseNestButtons()
    {
        List<Transform> nests = GameObject.Find("SimulationManager").GetComponent<SimulationManager>().nests;
        nestPositions = new List<Vector3>();

        // Set up the Old Nest button
        nestPositions.Add(nests[0].position);
        centreOldNest = this.ButtonByName("CentreOldNest");
        centreOldNest.onClick.AddListener(() => ChangePosition(new Vector3(nestPositions[0].x, nestViewHeight, nestPositions[0].z)));

        // Add a New Nest button for each new nest in the arena
        centreNewNest = new List<Button>();
        int i;
        for (i = 1; i < nests.Count; i++)
        {
            // Create the new button and set its text and name
            nestPositions.Add(nests[i].position);
            GameObject newButtonGO = Instantiate(newNestButton, transform);
            newButtonGO.name = "CentreNewNest" + (i - 1);
            newButtonGO.GetComponentInChildren<Text>().text = "New Nest " + (i - 1);
            
            // Move the button to the correct position
            RectTransform newButtonRect = newButtonGO.GetComponent<RectTransform>();
            newButtonRect.anchoredPosition = new Vector2(newButtonRect.anchoredPosition.x, newButtonRect.anchoredPosition.y - (i - 1) * 30);
            Button newButton = this.ButtonByName("CentreNewNest" + (i - 1));
            centreNewNest.Add(newButton);
            Vector3 requiredCamPos = new Vector3(nestPositions[i].x, nestViewHeight, nestPositions[i].z);
            centreNewNest[i-1].onClick.AddListener(() => ChangePosition(requiredCamPos));
        }

        // Increase the size of the Camera Options box to fit the newly added nest buttons
        RectTransform cameraOptions = GetComponent<RectTransform>();
        cameraOptions.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cameraOptions.rect.height + 30 * (i - 1));
    }

    private void ChangeAngle(Vector3 axis)
    {
        newRotation = Quaternion.LookRotation(axis);
        CancelInvoke("SmoothAngle");
        InvokeRepeating("SmoothAngle", 0, cameraUpdateFrequency);
    }

    private void SmoothAngle()
    {
        if (Quaternion.Angle(mainCamera.transform.rotation, newRotation) < 0.05f)
        {
            CancelInvoke("SmoothAngle");
            return;
        }
        else
        {
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, newRotation, cameraSpeed);
        }
    }

    private void ChangePosition(Vector3 position)
    {
        newPosition = position;
        CancelInvoke("SmoothPosition");
        InvokeRepeating("SmoothPosition", 0, 0.005f);
    }

    private void SmoothPosition()
    {
        if (Vector3.Distance(mainCamera.transform.position, newPosition) < 0.05f)
        {
            CancelInvoke("SmoothPosition");
            return;
        }
        else
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, newPosition, cameraSpeed);
        }
    }
}
