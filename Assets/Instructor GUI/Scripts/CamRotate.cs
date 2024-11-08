using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class CamRotate : MonoBehaviour
{

    public Transform currentTarget;
    public float rotationSpeed = 300f;
    public float zoomSpeed = 0.5f;
    public float minZoomDistance = 0.02f;
    public float maxZoomDistance = 1f;
    private float distanceFromTarget;
    private Vector3 currentRotation;
    public bool lockOn = true;
    public GameObject mainObjectContainer;

    public static CamRotate Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }


    void Start()
    {
        currentRotation = transform.eulerAngles;
        currentTarget = null;
        // distanceFromTarget = Vector3.Distance(transform.position, currentTarget.position);
    }

    void Update()
    {
        string currentActiveObjectName = (string)GetRoomCustomProperty("mainObjectCurrentModelName");
        for (int i = 0; i < mainObjectContainer.transform.childCount; i++)
        {
            if (mainObjectContainer.transform.GetChild(i).gameObject.activeSelf)
            {
                if (mainObjectContainer.transform.GetChild(i).name == currentActiveObjectName)
                {
                    currentTarget = mainObjectContainer.transform.GetChild(i);
                }
            }
        }

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