using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CameraMovement : NetworkBehaviour {

    private Transform lookat;
    private Transform camTransform;

    private const float Y_ANGLE_MIN = 33.0f;
    private const float Y_ANGLE_MAX = 50.0f;
    private const float X_ANGLE_MIN = -45.0f;
    private const float X_ANGLE_MAX = 45.0f;

    private Camera cam;

    private float distance = 13.0f;
    private float currentX = 0.0f;
    private float currentY = 33.0f;
    private float sensitivityX = 8.0f;
    private float sensitivityY = 8.0f;

    private void Start()
    {
        camTransform = transform;
        cam = Camera.main;
        lookat = null;
    }
    private void Update()
    {
        if(lookat == null)
        {
            if(GameObject.Find("ThirdPersonController(Clone)"))
            {
                lookat = GameObject.Find("ThirdPersonController(Clone)").transform;
            }
        }
        else
        {
            currentX += Input.GetAxis("Mouse X");
            currentY += Input.GetAxis("Mouse Y");

            //currentY = Mathf.Clamp(currentY, Y_ANGLE_MIN, Y_ANGLE_MAX);
            //currentX = Mathf.Clamp(currentX, X_ANGLE_MIN, X_ANGLE_MAX);
        }
    }

    private void LateUpdate()
    {
        Vector3 direction = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        camTransform.position = lookat.position + rotation * direction;
        camTransform.LookAt(lookat.position);
    }
}
