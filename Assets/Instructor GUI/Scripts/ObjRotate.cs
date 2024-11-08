using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class ObjRotate : MonoBehaviour
{

    public Transform currentTarget;
    public float rotationSpeed = 300f;
    public float zoomSpeed = 0.5f;
    public float minZoomDistance = 1f;
    public float maxZoomDistance = 1f;
    private float distanceFromTarget;
    private Vector3 currentRotation;
    public bool lockOn = true;


    void Start()
    {
        currentRotation = transform.eulerAngles;
        currentTarget = null;
        distanceFromTarget = Vector3.Distance(transform.position, currentTarget.position);
    }

    void Update()
    {
        minZoomDistance = (float)GetRoomCustomProperty("LaserLength");
        maxZoomDistance = (float)GetRoomCustomProperty("LaserLength");

        currentTarget = CamRotate.Instance.currentTarget;

        if (currentTarget != null && lockOn)
        {
            if (Input.GetMouseButton(1))
            {
                currentRotation.x += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
                currentRotation.y -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
                currentRotation.y = Mathf.Clamp(currentRotation.y, -90f, 90f);
            }

            distanceFromTarget -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            distanceFromTarget = Mathf.Clamp(distanceFromTarget, minZoomDistance, maxZoomDistance);

            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distanceFromTarget);
            Quaternion rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
            transform.position = currentTarget.position + rotation * negDistance;
            transform.LookAt(currentTarget.position);
        }
    }

    private Transform FindTargetByName(string targetName)
    {
        // find it in the main object container
        GameObject mainObjectContainer = GameObject.FindWithTag("MainObjectContainer");
        if (mainObjectContainer != null)
        {
            foreach (Transform child in mainObjectContainer.transform)
            {
                if (child.name == targetName)
                {
                    return child;
                }
            }
        }
        return null;
    }

    private object GetRoomCustomProperty(string key)
    {
        return PhotonNetwork.CurrentRoom.CustomProperties[key];
    }

    private bool RoomHasCustomProperty(string key)
    {
        return PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key);
    }
}