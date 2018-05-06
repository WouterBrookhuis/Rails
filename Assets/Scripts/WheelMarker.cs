using RailLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelMarker : MonoBehaviour {

    public TrackFollower Follower { get { return follower; } }
    public bool autoUpdateRotation = true;
    public bool lockRotation = false;

    private TrackFollower follower;

    public void Place(TrackFollower follower)
    {
        this.follower = follower;
    }

    public void Place(TrackSection section, float distance)
    {
        follower = new TrackFollower(section, 0, false);
        Debug.Log("Placing follower");
        follower.Move(distance);
    }

    void LateUpdate()
    {
        if(autoUpdateRotation && follower != null && !lockRotation)
        {
            transform.rotation = follower.Rotation;
        }
    }
}
