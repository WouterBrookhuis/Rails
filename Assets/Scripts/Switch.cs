using RailLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour, IActivatable {

    public float leverAngle = 45.0f;
    public Transform lever;
    public Junction junction;

    public void Initialize()
    {
        if(lever == null)
        {
            lever = transform.GetChild(0);
        }
        UpdateFromJunction();
    }

    public void Activate(ActivateInfo info)
    {
        junction.Toggle();
        UpdateFromJunction();
    }

    public void UpdateFromJunction()
    {
        lever.localEulerAngles = new Vector3(0, 0, junction.GoLeft ? leverAngle : -leverAngle);
    }

    public void Hover(ActivateInfo info)
    {
    }
}
