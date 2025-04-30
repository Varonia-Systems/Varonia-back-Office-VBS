//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: For controlling in-game objects with tracked devices.
//
//=============================================================================

using UnityEngine;
using Valve.VR;

public class Wrist : MonoBehaviour
{


    public enum WristType
    {
        Left = 0,
        Right = 1,
    }

    public WristType WristType_;

  

    [HideInInspector]
    public int index = -100;


    public TrackingState Trakingstate;

    public bool isValid { get; private set; }

    private void OnNewPoses(TrackedDevicePose_t[] poses)
    {
        if (index < 0)
        { Trakingstate = TrackingState.Lost; return; }


        if (!poses[index].bDeviceIsConnected)
        { Trakingstate = TrackingState.Lost; return; }

        if (!poses[index].bPoseIsValid)
        { Trakingstate = TrackingState.Lost; return; }

        if (poses[index].eTrackingResult != ETrackingResult.Running_OK || transform.position == new Vector3(0, 0, 0))
            Trakingstate = TrackingState.Lost;
        else
            Trakingstate = TrackingState.Ok;


        isValid = true;

        var pose = new SteamVR_Utils.RigidTransform(poses[index].mDeviceToAbsoluteTracking);

        transform.localPosition = pose.pos;
        transform.localRotation = pose.rot;

    }

    SteamVR_Events.Action newPosesAction;

    Wrist()
    {
        newPosesAction = SteamVR_Events.NewPosesAction(OnNewPoses);
    }

    void OnEnable()
    {
        newPosesAction.enabled = true;
    }

    void OnDisable()
    {
        newPosesAction.enabled = false;
        isValid = false;
    }



}

