using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultTool : Tool {
    public float trainImpulseSpeed = 5.0f;
    public override void OnWagonActivate(Wagon wagon, ActivateInfo info)
    {
        var localHit = wagon.transform.InverseTransformPoint(info.hit.point);
        var mag = wagon.rigidbody.mass * trainImpulseSpeed * (1.0f / Time.deltaTime);
        if(localHit.z < 0)
        {
            mag = -mag;
        }
        // Push wagon
        wagon.ApplyForce(mag);
    }
}
