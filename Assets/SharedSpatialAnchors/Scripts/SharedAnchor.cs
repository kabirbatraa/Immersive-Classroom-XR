/*
* Copyright (c) Meta Platforms, Inc. and affiliates.
* All rights reserved.
*
* Licensed under the Oculus SDK License Agreement (the "License");
* you may not use the Oculus SDK except in compliance with the License,
* which is provided at the time of installation or download, or which
* otherwise accompanies this software in either electronic or hard copy form.
*
* You may obtain a copy of the License at
*
* https://developer.oculus.com/licenses/oculussdk/
*
* Unless required by applicable law or agreed to in writing, the Oculus SDK
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PhotonPun = Photon.Pun;

/// <summary>
/// Controls anchor data and anchor control panel behavior.
/// </summary>
public class SharedAnchor : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI anchorName;

    [SerializeField]
    private Image saveIcon;

    [SerializeField]
    private GameObject shareButton;

    [SerializeField]
    private Image shareIcon;

    [SerializeField]
    private Image alignIcon;

    [SerializeField]
    private Color grayColor;

    [SerializeField]
    private Color greenColor;

    private OVRSpatialAnchor _spatialAnchor;

    public OVRSpatialAnchor GetSpatialAnchor(){
        return _spatialAnchor;
    }

    public bool IsSavedLocally
    {
        set
        {
            if (saveIcon != null)
            {
                saveIcon.color = value ? greenColor : grayColor;
            }
        }
    }

    private bool IsSelectedForShare
    {
        set
        {
            if (shareIcon != null)
            {
                shareIcon.color = value ? greenColor : grayColor;
            }
        }
    }

    public bool IsSelectedForAlign
    {
        set
        {
            if (alignIcon != null)
            {
                alignIcon.color = value ? greenColor : grayColor;
            }
        }
    }

    private void Awake()
    {
        _spatialAnchor = GetComponent<OVRSpatialAnchor>();
    }

    private IEnumerator Start()
    {
        while (_spatialAnchor && !_spatialAnchor.Created)
        {
            yield return null;
        }

        if (_spatialAnchor != null)
        {
            anchorName.text = _spatialAnchor.Uuid.ToString("D");
        }
        else
        {
            Destroy(gameObject);
        }

        if (SampleController.Instance.automaticCoLocation)
            transform.Find("Canvas").gameObject.SetActive(false);
    }

    public void OnSaveLocalButtonPressed()
    {
        SampleController.Instance.Log("OnSaveLocalButtonPressed: saving anchor");

        if (_spatialAnchor == null)
        {
            return;
        }

        _spatialAnchor.Save((_, isSuccessful) =>
        {
            if (isSuccessful)
            {
                IsSavedLocally = true;

                SampleController.Instance.GetComponent<SharedAnchorLoader>().AddLocallySavedAnchor(_spatialAnchor);
            }
        });
    }

    public void OnHideButtonPressed()
    {
        SampleController.Instance.Log("OnHideButtonPressed: hiding anchor");

        Destroy(gameObject);
    }

    public void OnEraseButtonPressed()
    {
        SampleController.Instance.Log("OnEraseButtonPressed: erasing anchor");

        if (_spatialAnchor == null)
        {
            return;
        }

        _spatialAnchor.Erase((_, isSuccessful) =>
        {
            if (isSuccessful)
            {
                SampleController.Instance.GetComponent<SharedAnchorLoader>().RemoveLocallySavedAnchor(_spatialAnchor);
                Destroy(gameObject);
            }
        });
    }

    public bool IsReadyToShare()
    {
        if (!Photon.Pun.PhotonNetwork.IsConnected)
        {
            SampleController.Instance.Log("Can't share - no users to share with because you are no longer connected to the Photon network");
            return false;
        }

        // var userIds = PhotonAnchorManager.GetUserList().Select(userId => userId.ToString()).ToArray();
        // if (userIds.Length == 0)
        // {
        //     SampleController.Instance.Log("Can't share - no users to share with or can't get the user ids through photon custom properties");
        //     return false;
        // }

        if (_spatialAnchor == null)
        {
            SampleController.Instance.Log("Can't share - no associated spatial anchor");
            return false;
        }
        return true;
    }

    public void OnShareButtonPressed()
    {
        SampleController.Instance.Log(nameof(OnShareButtonPressed));

        if (!IsReadyToShare())
        {
            return;
        }

        IsSelectedForShare = true;
        SaveToCloudThenShare();
    }

    private void SaveToCloudThenShare()
    {
        OVRSpatialAnchor.SaveOptions saveOptions;
        saveOptions.Storage = OVRSpace.StorageLocation.Cloud;
        _spatialAnchor.Save(saveOptions, (spatialAnchor, isSuccessful) =>
        {
            if (isSuccessful)
            {
                SampleController.Instance.Log("Successfully saved anchor(s) to the cloud");

                var userIds = PhotonAnchorManager.GetUserList().Select(userId => userId.ToString()).ToArray();
                ICollection<OVRSpaceUser> spaceUserList = new List<OVRSpaceUser>();
                
                foreach (string strUsername in userIds)
                {
                    ulong parsedId = ulong.Parse(strUsername);
                    if (parsedId == 0) continue; // do not share the anchor with a user with the id of 0 (e.g. the instructor gui)
                    spaceUserList.Add(new OVRSpaceUser(parsedId));
                }

                OVRSpatialAnchor.Share(new List<OVRSpatialAnchor> { spatialAnchor }, spaceUserList, OnShareComplete);

                SampleController.Instance.AddSharedAnchorToLocalPlayer(this);
            }
            else
            {
                SampleController.Instance.Log("Saving anchor(s) failed. Retrying...");
                SaveToCloudThenShare();
            }
        });
    }

    public void ReshareAnchor()
    {
        if (!IsReadyToShare())
        {
            return;
        }

        SampleController.Instance.Log("ReshareAnchor: re-sharing anchor with all users in the room");

        IsSelectedForShare = true;

        OVRSpatialAnchor.SaveOptions saveOptions;
        saveOptions.Storage = OVRSpace.StorageLocation.Cloud;
        ICollection<OVRSpaceUser> spaceUserList = new List<OVRSpaceUser>();
        foreach (string strUsername in PhotonAnchorManager.GetUsers())
        {
            ulong parsedId = ulong.Parse(strUsername);
            if (parsedId == 0) continue; // do not share the anchor with a user with the id of 0 (e.g. the instructor gui)
            spaceUserList.Add(new OVRSpaceUser(parsedId));
        }
        OVRSpatialAnchor.Share(new List<OVRSpatialAnchor> { _spatialAnchor }, spaceUserList, OnShareComplete);
    }

    private void OnShareComplete(ICollection<OVRSpatialAnchor> spatialAnchors, OVRSpatialAnchor.OperationResult result)
    {
        SampleController.Instance.Log(nameof(OnShareComplete) + " Result: " + result);

        if (result != OVRSpatialAnchor.OperationResult.Success)
        {
            foreach (var spatialAnchor in spatialAnchors)
            {
                spatialAnchor.GetComponent<SharedAnchor>().shareIcon.color = Color.red;
            }

            if (!PhotonPun.PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("userids"))
            {
                SampleController.Instance.Log("NO USERS.");
                return;
            }
            
            // Try to find the mole
            SampleController.Instance.Log("Try to find the mole.");
            OVRSpatialAnchor.SaveOptions saveOptions;
            saveOptions.Storage = OVRSpace.StorageLocation.Cloud;
            foreach (string strUsername in PhotonAnchorManager.GetUsers())
            {
                ulong parsedId = ulong.Parse(strUsername);
                if (parsedId == 0) continue; // do not share the anchor with a user with the id of 0 (e.g. the instructor gui)
                _spatialAnchor.Share(new OVRSpaceUser(parsedId), OnShareCompleteIndividual);
            }
            
            return;
        }

        var uuids = new Guid[spatialAnchors.Count];
        var uuidIndex = 0;

        foreach (var spatialAnchor in spatialAnchors)
        {
            SampleController.Instance.Log("OnShareComplete: space: " + spatialAnchor.Space.Handle + ", uuid: " + spatialAnchor.Uuid);

            uuids[uuidIndex] = spatialAnchor.Uuid;
            ++uuidIndex;
        }

        PhotonAnchorManager.Instance.PublishAnchorUuids(uuids, (uint)uuids.Length, true);
    }
    
    private void OnShareCompleteIndividual(OVRSpaceUser user, OVRSpatialAnchor.OperationResult result)
    {
        SampleController.Instance.Log(nameof(OnShareCompleteIndividual) + " " + user.Id + " Result: " + result);

        if (result != OVRSpatialAnchor.OperationResult.Success)
        {
            // kick the mole out
            SampleController.Instance.Log("Kick " +  user.Id + " out.");
            var userList = PhotonAnchorManager.GetUserList();
            var isUnknownUserId = !userList.Contains(user.Id);

            if (isUnknownUserId)
            {
                return;
            }

            userList.Remove(user.Id);
            PhotonAnchorManager.SaveUserList(userList);
        }
    }
    
    public void OnAlignButtonPressed()
    {
        SampleController.Instance.Log("OnAlignButtonPressed: aligning to anchor");

        AlignPlayer.Instance.SetAlignmentAnchor(this);
    }

    public void DisableShareIcon()
    {
        if (shareButton)
        {
            shareButton.SetActive(false);
        }
    }
}
