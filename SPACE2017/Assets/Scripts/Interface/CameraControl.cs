﻿using UnityEngine;
using Assets.Scripts;

public class CameraControl : MonoBehaviour
{
    private Camera _camera;
    private bool _readInput;

    float flySpeed = 10f;
    
    float accelerationRatio = 3;
    float slowDownRatio = 0.2f;

    // Variables for the ant following functionality
    public bool followingAnt = false;
    Transform targetAnt;
    
    // http://forum.unity3d.com/threads/a-free-simple-smooth-mouselook.73117/
    Vector2 _mouseAbsolute;
    Vector2 _smoothMouse;

    public Vector2 clampInDegrees = new Vector2(360, 180);
    public bool lockCursor;
    public Vector2 sensitivity = new Vector2(2, 2);
    public Vector2 smoothing = new Vector2(3, 3);
    public Vector2 targetDirection;
    public Vector2 targetCharacterDirection;

    // Assign this if there's a parent object controlling motion, such as a Character Controller.
    // Yaw rotation will affect this object instead of the camera if set.
    public GameObject characterBody;


    void Start()
    {
        _camera = GetComponent<Camera>();

        ResetView();

        // Set target direction for the character body to its inital state.
        if (characterBody) targetCharacterDirection = characterBody.transform.localRotation.eulerAngles;
    }

    void Update()
    {
        if (!_camera.enabled)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo;
            Ray clickRay = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(clickRay, out hitInfo, 50f, PhysicsLayers.Ants))
            { 
                followingAnt = true;
                targetAnt = hitInfo.transform;
                FollowAntMovement();
                _camera.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }
        else if (Input.GetMouseButton(1))
        {
            var targetOrientation = Quaternion.Euler(targetDirection);
            var targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);

            // Get raw mouse input for a cleaner reading on more sensitive mice.
            var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            // Scale input against the sensitivity setting and multiply that against the smoothing value.
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

            // Interpolate mouse movement over time to apply smoothing delta.
            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

            // Find the absolute mouse movement value from point zero.
            _mouseAbsolute += _smoothMouse;

            // Clamp and apply the local x value first, so as not to be affected by world transforms.
            if (clampInDegrees.x < 360)
                _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

            var xRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
            transform.localRotation = xRotation;

            // Then clamp and apply the global y value.
            if (clampInDegrees.y < 360)
                _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

            transform.localRotation *= targetOrientation;

            // If there's a character body that acts as a parent to the camera
            if (characterBody)
            {
                var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, characterBody.transform.up);
                characterBody.transform.localRotation = yRotation;
                characterBody.transform.localRotation *= targetCharacterOrientation;
            }
            else
            {
                var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
                transform.localRotation *= yRotation;
            }
        }

        //use shift to speed up flight
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            flySpeed *= accelerationRatio;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            flySpeed /= accelerationRatio;
        }

        //use ctrl to slow up flight
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            flySpeed *= slowDownRatio;
        }

        if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
        {
            flySpeed /= slowDownRatio;
        }

        //
        float adjustedSpeed = (flySpeed * Time.deltaTime) / Time.timeScale;
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            transform.Translate(Vector3.up * Input.GetAxisRaw("Vertical") * adjustedSpeed);
            followingAnt = false;
        }
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            transform.Translate(Vector3.right * Input.GetAxisRaw("Horizontal") * adjustedSpeed);
            followingAnt = false;
        }
        if (Input.GetAxisRaw("Up") != 0)
        {
            transform.Translate(Vector3.forward * adjustedSpeed);
        }
        else if (Input.GetAxisRaw("Down") != 0)
        {
            transform.Translate(-Vector3.forward * adjustedSpeed);
        }

        if (followingAnt)
        {
            FollowAntMovement();
        }
    }

    public void ResetView()
    {
        targetDirection = transform.localRotation.eulerAngles;
    }

    public void FollowAntMovement()
    {
        /*Vector3 antPos = targetAnt.position;
        Vector3 desiredPos = new Vector3(antPos.x, antPos.y + 1f, antPos.z) - (targetAnt.forward * 0.5f);
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime);

        Vector3 antAngles = targetAnt.rotation.eulerAngles;
        Quaternion desiredRotation = Quaternion.Euler(antAngles + new Vector3(45f, 0f, 0f));
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime);*/
        Vector3 requiredPosition = new Vector3(targetAnt.position.x, transform.position.y, targetAnt.position.z);
        transform.position = Vector3.Lerp(transform.position, requiredPosition, Time.deltaTime);
    }
}
