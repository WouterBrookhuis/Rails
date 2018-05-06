using RailLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : MonoBehaviour {

    public UISlidingPanel uiPanel;

    protected virtual void OnEnable()
    {
        if(uiPanel != null)
        {
            uiPanel.Near();
        }
    }

    protected virtual void OnDisable()
    {
        if(uiPanel != null)
        {
            uiPanel.Far();
        }
    }

    public virtual void OnTrackHover(TrackSectionComponent track, ActivateInfo info)
    {
    }

    public virtual void OnTrackActivate(TrackSectionComponent track, ActivateInfo info)
    {   
    }

    public virtual void OnWagonHover(Wagon wagon, ActivateInfo info)
    {
    }

    public virtual void OnWagonActivate(Wagon wagon, ActivateInfo info)
    {
    }

    public virtual void OnTerrainHit(RaycastHit hit)
    {
    }

    public virtual void OnNothingHit()
    {
    }

    protected bool HighlightClosestEnd(TrackSection trackSection, bool onlyUnconnected, Vector3 point, out bool startIsClosest)
    {
        var toEnd = Vector3.Distance(point, trackSection.EndPosition);
        var toStart = Vector3.Distance(point, trackSection.Position);
        startIsClosest = false;

        if(toEnd < toStart)
        {
            if(trackSection.Next == null || !onlyUnconnected)
            {
                Highlighter.DrawHighlighter(Matrix4x4.TRS(trackSection.EndPosition, trackSection.GetRotationOnTrack(trackSection.Length), Vector3.one));
                return true;
            }
        }
        else
        {
            if(trackSection.Previous == null || !onlyUnconnected)
            {
                Highlighter.DrawHighlighter(Matrix4x4.TRS(trackSection.Position, trackSection.Rotation, Vector3.one));
                startIsClosest = true;
                return true;
            }
        }
        return false;
    }

    public void Select()
    {
        ToolController.Instance.SetTool(GetType());
    }
}
