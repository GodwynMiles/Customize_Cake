using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    public GameObject target;
    float speed = 5;

    float minFov = 35f;
    float maxFov = 100f;
    float sensitivity = 17f;


    void Update()
    {
 
        if (Input.GetMouseButton(0))
        {
            transform.RotateAround(target.transform.position, transform.up, Input.GetAxis("Mouse X") * speed);
            transform.RotateAround(target.transform.position, transform.right, Input.GetAxis("Mouse Y") * speed);
        }
        //ZOOM

        float fov = Camera.main.fieldOfView;
        fov += Input.GetAxis("Mouse ScrollWheel") * -sensitivity;
        fov = Mathf.Clamp(fov, minFov, maxFov);
        Camera.main.fieldOfView = fov;

    }
}