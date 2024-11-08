using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PhotonPun = Photon.Pun;
using PhotonRealtime = Photon.Realtime;

public class MainObjectManagerAndCommunicator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // on start, send to the cloud the number of models there are under a main object
        // child count contains the number of children on this current transform (this = instance of main object)
        SetRoomCustomProperty("totalNumberOfModels", transform.childCount);
    }

    void FixedUpdate() {
        

        // read from the server which model should be active

        // GameObject mainObjectContainer = GameObject.FindWithTag("MainObjectContainer");
        // if (mainObjectContainer != null) {
        //     if (RoomHasCustomProperty("mainObjectCurrentModelName")) {
        //         string currentActiveObjectName = (string)GetRoomCustomProperty("mainObjectCurrentModelName");
                
        //         // for every potential model (child of container), disable unless name = currentActiveObject
        //         foreach (Transform child in mainObjectContainer.transform) {
        //             GameObject potentialModel = child.gameObject;
        //             potentialModel.SetActive(potentialModel.name == currentActiveObjectName);
        //         }
        //     }
        // }

        if (RoomHasCustomProperty("mainObjectCurrentModelName")) {
            string currentActiveObjectName = (string)GetRoomCustomProperty("mainObjectCurrentModelName");
            
            // for every potential model (child of container), disable unless name = currentActiveObject
            foreach (Transform child in transform) {
                GameObject potentialModel = child.gameObject;
                potentialModel.SetActive(potentialModel.name == currentActiveObjectName);
            }
        }

        // // index based main object selection (too much work to make everything work with this)
        // if (RoomHasCustomProperty("mainObjectCurrentModelIndex")) {
        //     int activeObjectIndex = (int)GetRoomCustomProperty("mainObjectCurrentModelIndex");

        //     for (int i = 0; i < transform.childCount; i++) {
        //         Transform child = transform.GetChild(i);
        //         GameObject potentialModel = child.gameObject;
        //         potentialModel.SetActive(i == activeObjectIndex);
        //     }
        // }

    }




    // room has custom property?
    public bool RoomHasCustomProperty(string key) {
        return PhotonPun.PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key);
    }

    // get room custom property
    public object GetRoomCustomProperty(string key) {

        // string key = "groupNum" + photonObject.GetComponent<PhotonPun.PhotonView>().ViewID;
        // int objectGroupNumber = (int)PhotonPun.PhotonNetwork.CurrentRoom.CustomProperties[key];

        // return objectGroupNumber;
        return PhotonPun.PhotonNetwork.CurrentRoom.CustomProperties[key];
    }


    public void SetRoomCustomProperty(string key, object value) {

        var newCustomProperty = new ExitGames.Client.Photon.Hashtable { { key, value } };
        // update on server
        PhotonPun.PhotonNetwork.CurrentRoom.SetCustomProperties(newCustomProperty);

        // update locally because server will update local cached hashmap with delay
        PhotonPun.PhotonNetwork.CurrentRoom.CustomProperties[key] = value;

    }
}
