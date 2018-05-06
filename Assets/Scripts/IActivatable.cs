using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActivatable
{
    void Hover(ActivateInfo hit);
    void Activate(ActivateInfo hit);
}

public struct ActivateInfo
{
    public RaycastHit hit;
    public ActivateButton button;

    public ActivateInfo(RaycastHit hit, ActivateButton button)
    {
        this.hit = hit;
        this.button = button;
    }
}

public enum ActivateButton
{
    None,
    LeftClick,
    RightClick
}