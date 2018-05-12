using RailLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlexTrackTool : Tool
{
    public Material trackMaterial;
    private TrackLayer trackLayer;
    public Text statusText;


    private enum State
    {
        Idle,
        SelectedStart,
    }

    private State state = State.Idle;
    private TrackSection fromSection;
    private bool fromEnd;
    private Vector3 fromPoint;
    private Vector3 toPoint;
    private Quaternion fromRotation;
    private Quaternion toRotation;
    private Vector3 curveCenter;

    private void Start()
    {
        trackLayer = new TrackLayer();
        ToState(State.Idle);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if(fromSection != null && fromSection.Component != null)
        {
            fromSection.Component.GetComponent<GlowObject>().ActivateGlow();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if(fromSection != null && fromSection.Component != null)
        {
            fromSection.Component.GetComponent<GlowObject>().DeactivateGlow();
        }
    }

    public override void OnTrackActivate(TrackSectionComponent track, ActivateInfo info)
    {
        bool selectStart;
        if(info.button == ActivateButton.LeftClick && HighlightClosestEnd(track.trackSection, true, info.hit.point, out selectStart))
        {
            if(state == State.Idle)
            {
                fromSection = track.trackSection;
                fromEnd = !selectStart;
                ToState(State.SelectedStart);
            }
            else if(state == State.SelectedStart)
            {
                // Try to place the track
                var toSection = track.trackSection;
                var toEnd = !selectStart;
                if(!TryPlaceTrack(fromSection, fromEnd, toSection, toEnd))
                {
                    Debug.Log("Unable to place flex section!");
                }

                ToState(State.Idle);
            }
        }
        else if(info.button == ActivateButton.RightClick)
        {
            ToState(State.Idle);
        }
    }

    private void ToState(State newState)
    {
        switch(newState)
        {
            case State.Idle:
                if(fromSection != null && fromSection.Component != null)
                {
                    fromSection.Component.GetComponent<GlowObject>().DeactivateGlow();
                }
                fromSection = null;
                statusText.text = "Click a track piece to start from";
                break;
            case State.SelectedStart:
                if(fromSection != null && fromSection.Component != null)
                {
                    fromSection.Component.GetComponent<GlowObject>().ActivateGlow();
                }
                statusText.text = "Click a track piece to go to";
                break;
        }

        state = newState;
    }

    private bool TryPlaceTrack(TrackSection from, bool fromEnd, TrackSection to, bool toEnd)
    {
        Vector3 intersection;
        fromPoint = fromEnd ? from.EndPosition : from.Position;
        toPoint = toEnd ? to.EndPosition : to.Position;
        fromRotation = fromEnd ? from.EndRotation : Helper.InvertTrackRotation(from.Rotation);
        // Note that this is pointing towards the section being build
        toRotation = toEnd ? to.EndRotation : Helper.InvertTrackRotation(to.Rotation);

        if(Helper.LineLineIntersection(out intersection,
            fromPoint,
            fromRotation * Vector3.forward,
            toPoint,
            toRotation * Vector3.forward))
        {
            float d1, d2, shortestDistance;
            d1 = shortestDistance = Vector3.Distance(intersection, fromPoint);
            d2 = Vector3.Distance(intersection, toPoint);
            if(d2 < shortestDistance)
            {
                shortestDistance = d2;
            }

            // Calculate curvature
            float curve = 180.0f - Quaternion.Angle(fromRotation, toRotation);

            // Check if the curve should go left or right
            var cross = Vector3.Cross(fromRotation * Vector3.forward, (toPoint - fromPoint));
            if(cross.y < 0)
            {
                // Go left
                curve = -curve;
            }


            // Calculate straight length
            var straightLength = Mathf.Max(d1, d2) - shortestDistance;

            trackLayer.Reposition(from, !fromEnd);
            if(d1 == d2)
            {
                // Place only curve
                trackLayer.PlaceTrack(CalculateCurveLength(curve, 0, 0), curve);
                TrackDatabase.Instance.RegisterTrack(trackLayer.CurrentSection);
                BasicTrackLayerTool.PlaceTrackGO(trackLayer.CurrentSection, trackMaterial);
                BasicTrackLayerTool.TryAutoConnect(trackLayer.CurrentSection);
            }
            else if(shortestDistance == d1)
            {
                // Place curve first, then straight
                trackLayer.PlaceTrack(CalculateCurveLength(curve, 0, straightLength), curve);
                TrackDatabase.Instance.RegisterTrack(trackLayer.CurrentSection);
                BasicTrackLayerTool.PlaceTrackGO(trackLayer.CurrentSection, trackMaterial);
                trackLayer.PlaceTrack(straightLength);
                TrackDatabase.Instance.RegisterTrack(trackLayer.CurrentSection);
                BasicTrackLayerTool.PlaceTrackGO(trackLayer.CurrentSection, trackMaterial);
                BasicTrackLayerTool.TryAutoConnect(trackLayer.CurrentSection);
            }
            else if(shortestDistance == d2)
            {
                // Place straight first, then curve
                trackLayer.PlaceTrack(straightLength);
                TrackDatabase.Instance.RegisterTrack(trackLayer.CurrentSection);
                BasicTrackLayerTool.PlaceTrackGO(trackLayer.CurrentSection, trackMaterial);
                trackLayer.PlaceTrack(CalculateCurveLength(curve, straightLength, 0), curve);
                TrackDatabase.Instance.RegisterTrack(trackLayer.CurrentSection);
                BasicTrackLayerTool.PlaceTrackGO(trackLayer.CurrentSection, trackMaterial);
                BasicTrackLayerTool.TryAutoConnect(trackLayer.CurrentSection);
            }

            trackLayer.Reposition(null, false);
            return true;
        }
        // TODO: Add in-line check
        else if(Quaternion.Angle(fromRotation, Helper.InvertTrackRotation(toRotation)) < 0.5f)
        {
            trackLayer.Reposition(fromSection, !fromEnd);
            trackLayer.PlaceTrack(Vector3.Distance(fromPoint, toPoint));
            TrackDatabase.Instance.RegisterTrack(trackLayer.CurrentSection);
            BasicTrackLayerTool.PlaceTrackGO(trackLayer.CurrentSection, trackMaterial);
            BasicTrackLayerTool.TryAutoConnect(trackLayer.CurrentSection);
            return true;
        }
        return false;
    }

    public float CalculateCurveLength(float curve, float startOffset, float endOffset)
    {
        // Curve length = radius * curve angle (rad)
        var startPoint = fromPoint + fromRotation * Vector3.forward * startOffset;
        var endPoint = toPoint + toRotation * Vector3.forward * endOffset;
        Helper.LineLineIntersection(out curveCenter,
            startPoint, fromRotation * (Vector3.right * Mathf.Sign(curve)),
            endPoint, toRotation * (Vector3.left * Mathf.Sign(curve)));
        var radius = Vector3.Distance(endPoint, curveCenter);
        return radius * Mathf.Deg2Rad * Mathf.Abs(curve);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(fromPoint, fromPoint + fromRotation * Vector3.forward * 100.0f);
        Gizmos.DrawLine(toPoint, toPoint + toRotation * Vector3.forward * 100.0f);
        Gizmos.DrawLine(fromPoint, curveCenter);
        Gizmos.DrawLine(toPoint, curveCenter);
        Gizmos.DrawCube(curveCenter, Vector3.one * 2);
    }
}
