using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sys_Camera_Orbit : MonoBehaviour
{
    [SerializeField] private Vector3 lookAt = new Vector3(-0.1f, 0.15f, 0.15f);
    [SerializeField] private float speed = 1.5f;

    private Camera cam = null;
    private GameObject targetObject = null;
    
    float angle;
    float distance;

    private void Start()
    {
        Screen.SetResolution(640,360,Screen.fullScreen);
        cam = GetComponent<Camera>();

        targetObject = new GameObject();
        targetObject.transform.position = lookAt;

        distance = Vector3.Distance(targetObject.transform.position,transform.position);

        angle = -90f;
    }
    
    private void Update()
    {
        rotateAround();
    }

    private void rotateAround()
    {
        targetObject.transform.position = lookAt;
        if (Input.GetKey("left")) { angle += speed; }
        if (Input.GetKey("right")) { angle -= speed; }
        Vector3 location = transform.position;
        Vector3 newLocation;
        Vector3 target = targetObject.transform.position;

        newLocation.x = target.x + Mathf.Cos(angle * Mathf.Deg2Rad) * distance;
        newLocation.z = target.z + Mathf.Sin(angle * Mathf.Deg2Rad) * distance;
        newLocation.y = location.y;

        transform.position = newLocation;
        transform.LookAt(targetObject.transform);
        transform.Rotate(new Vector3(0, 0, 1), 3);
    }
}
