using RailLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTrackLayerTool : Tool
{

    private TrackLayer trackLayer;
    public float length = 5.0f;
    public float angle = 15.0f;
    public LayerMask terrainMask;
    private TrackSection lastHighlightedSection;
    private bool shouldInvertOnReposition;
    private TrackSection lastSection;

    public TrackLayer TrackLayer { get { return trackLayer; } }

    private void Start()
    {
        trackLayer = new TrackLayer();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if(trackLayer != null)
        {
            // Other tools may have connected our track piece
            DeselectIfConnected();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if(trackLayer != null && trackLayer.CurrentSection != null && trackLayer.CurrentSection.Component != null)
        {
            trackLayer.CurrentSection.Component.GetComponent<GlowObject>().DeactivateGlow();
        }
    }

    private void Update()
    {
        if(lastSection != trackLayer.CurrentSection)
        {
            if(lastSection != null && lastSection.Component != null)
            {
                lastSection.Component.GetComponent<GlowObject>().DeactivateGlow();
            }
            if(trackLayer.CurrentSection != null && trackLayer.CurrentSection.Component != null)
            {
                trackLayer.CurrentSection.Component.GetComponent<GlowObject>().ActivateGlow();
            }
        }
        lastSection = trackLayer.CurrentSection;
    }

    public override void OnTrackActivate(TrackSectionComponent track, ActivateInfo info)
    {
        if(lastHighlightedSection == track.trackSection && info.button == ActivateButton.LeftClick)
        {
            // Reposition on track section, or deselect
            trackLayer.Reposition(lastHighlightedSection != trackLayer.CurrentSection ? lastHighlightedSection : null, shouldInvertOnReposition);
        }
        else if(info.button == ActivateButton.RightClick)
        {
            // Delete track
            if(track.trackSection == trackLayer.CurrentSection)
            {
                trackLayer.CurrentSection = null;
            }
            RemoveTrackSection(track);
        }
    }

    public override void OnTerrainHit(RaycastHit hit)
    {
        trackLayer.Reposition(null, false);
    }

    public override void OnTrackHover(TrackSectionComponent track, ActivateInfo info)
    {
        if(HighlightClosestEnd(track.trackSection, true, info.hit.point, out shouldInvertOnReposition))
        {
            lastHighlightedSection = track.trackSection;
        }
        else
        {
            lastHighlightedSection = null;
        }
    }

    public void PlaceLeft()
    {
        TrackSection section = null;
        if(trackLayer.CurrentSection != null)
        {
            section = trackLayer.PlaceTrack(length, -angle);
        }
        else
        {
            RaycastHit hit;
            if(FirstPersonController.Main.RaycastCameraForward(out hit, 1000.0f, terrainMask))
            {
                section = trackLayer.StartTrack(hit.point + Vector3.up * 0.01f, GetNewTrackRotation(hit),
                    length, -angle);
            }
        }
        if(section != null)
        {
            TrackFactory.Instance.PlaceAndRegisterSection(section, true);
            DeselectIfConnected();
        }
    }

    public void PlaceRight()
    {
        TrackSection section = null;
        if(trackLayer.CurrentSection != null)
        {
            section = trackLayer.PlaceTrack(length, angle);
        }
        else
        {
            RaycastHit hit;
            if(FirstPersonController.Main.RaycastCameraForward(out hit, 1000.0f, terrainMask))
            {
                section = trackLayer.StartTrack(hit.point + Vector3.up * 0.01f, GetNewTrackRotation(hit),
                    length, angle);
            }
        }
        if(section != null)
        {
            TrackFactory.Instance.PlaceAndRegisterSection(section, true);
            DeselectIfConnected();
        }
    }

    public void PlaceStraight()
    {
        TrackSection section = null;
        if(trackLayer.CurrentSection != null)
        {
            section = trackLayer.PlaceTrack(length);
        }
        else
        {
            RaycastHit hit;
            if(FirstPersonController.Main.RaycastCameraForward(out hit, 1000.0f, terrainMask))
            {
                section = trackLayer.StartTrack(hit.point + Vector3.up * 0.01f, GetNewTrackRotation(hit), length);
            }
        }
        if(section != null)
        {
            TrackFactory.Instance.PlaceAndRegisterSection(section, true);
            DeselectIfConnected();
        }
    }

    public void PlaceStraightShort()
    {
        var tmp = length;
        length = 2.0f;
        PlaceStraight();
        length = tmp;
    }

    public void PlaceJunctionRight()
    {
        TrackSection section = null;
        if(trackLayer.CurrentSection != null)
        {
            section = trackLayer.PlaceTrack(2.0f);
        }
        else
        {
            RaycastHit hit;
            if(FirstPersonController.Main.RaycastCameraForward(out hit, 1000.0f, terrainMask))
            {
                section = trackLayer.StartTrack(hit.point + Vector3.up * 0.01f, GetNewTrackRotation(hit), 2.0f);
            }
        }
        if(section != null)
        {
            var straight = trackLayer.PlaceTrack(length);
            trackLayer.MoveBack();
            var right = trackLayer.PlaceTrack(length, angle);
            var junction = new Junction(section, straight, right);

            // Create game objects and group them
            var group = new List<TrackSectionComponent>();
            TrackFactory.Instance.PlaceAndRegisterSection(section, true).AddToGroup(group);
            TrackFactory.Instance.PlaceAndRegisterSection(straight, true).AddToGroup(group);
            TrackFactory.Instance.PlaceAndRegisterSection(right, true).AddToGroup(group);

            // Add switch
            TrackFactory.Instance.AddSwitchToJunction(junction);

            DeselectIfConnected();
        }
    }

    public void PlaceJunctionLeft()
    {
        TrackSection section = null;
        if(trackLayer.CurrentSection != null)
        {
            section = trackLayer.PlaceTrack(2.0f);
        }
        else
        {
            RaycastHit hit;
            if(FirstPersonController.Main.RaycastCameraForward(out hit, 1000.0f, terrainMask))
            {
                section = trackLayer.StartTrack(hit.point + Vector3.up * 0.01f, GetNewTrackRotation(hit), 2.0f);
            }
        }
        if(section != null)
        {
            var straight = trackLayer.PlaceTrack(length);
            trackLayer.MoveBack();
            var left = trackLayer.PlaceTrack(length, -angle);
            var junction = new Junction(section, left, straight);

            // Create game objects and group them
            var group = new List<TrackSectionComponent>();
            TrackFactory.Instance.PlaceAndRegisterSection(section, true).AddToGroup(group);
            TrackFactory.Instance.PlaceAndRegisterSection(straight, true).AddToGroup(group);
            TrackFactory.Instance.PlaceAndRegisterSection(left, true).AddToGroup(group);

            // Add switch
            junction.Toggle();  // Select straight
            TrackFactory.Instance.AddSwitchToJunction(junction);

            DeselectIfConnected();
        }
    }

    public static void TryAutoConnect(TrackSection section)
    {
        const float maxConnectAngle = 5.0f;
        const float maxConnectDistance = 0.05f;
        if(section.Next == null)
        {
            var sections = TrackDatabase.Instance.GetZoneSections(section.EndPosition);
            foreach(var other in sections)
            {
                if(other == section)
                {
                    continue;
                }
                if(other.Next == null)
                {
                    if(Vector3.Distance(other.EndPosition, section.EndPosition) < maxConnectDistance &&
                        Quaternion.Angle(other.EndRotation, Helper.InvertTrackRotation(section.EndRotation)) < maxConnectAngle)
                    {
                        other.Next = section;
                        section.Next = other;
                        break;
                    }
                }
                if(other.Previous == null)
                {
                    if(Vector3.Distance(other.Position, section.EndPosition) < maxConnectDistance &&
                        Quaternion.Angle(other.Rotation, section.EndRotation) < maxConnectAngle)
                    {
                        other.Previous = section;
                        section.Next = other;
                        break;
                    }
                }
            }
        }

        if(section.Previous == null)
        {
            var sections = TrackDatabase.Instance.GetZoneSections(section.Position);
            foreach(var other in sections)
            {
                if(other == section)
                {
                    continue;
                }
                if(other.Next == null)
                {
                    if(Vector3.Distance(other.EndPosition, section.Position) < maxConnectDistance &&
                        Quaternion.Angle(other.EndRotation, section.Rotation) < maxConnectAngle)
                    {
                        other.Next = section;
                        section.Previous = other;
                        break;
                    }
                }
                if(other.Previous == null)
                {
                    if(Vector3.Distance(other.Position, section.Position) < maxConnectDistance &&
                        Quaternion.Angle(other.Rotation, Helper.InvertTrackRotation(section.Rotation)) < maxConnectAngle)
                    {
                        other.Previous = section;
                        section.Previous = other;
                        break;
                    }
                }
            }
        }
    }

    private void DeselectIfConnected()
    {
        if(trackLayer.CurrentSection != null && 
            ((trackLayer.CurrentSection.Next != null && !trackLayer.Inverted) ||
            (trackLayer.CurrentSection.Previous != null && trackLayer.Inverted)))
        {
            trackLayer.CurrentSection = null;
        }
    }

    private Quaternion GetNewTrackRotation(RaycastHit hit)
    {
        var normalizedForward = Vector3.ProjectOnPlane(FirstPersonController.Main.pitchTransform.forward, hit.normal);
        return Quaternion.LookRotation(normalizedForward, hit.normal);
    }
    
    public static void RemoveTrackSection(TrackSectionComponent component)
    {
        List<TrackSectionComponent> toRemove = new List<TrackSectionComponent> { component };
        if(component.InGroup)
        {
            toRemove = component.group;
        }

        foreach(var track in toRemove)
        {
            // Unlink track sections
            if(track.trackSection.Next != null)
            {
                if(track.trackSection.Next.Previous == track.trackSection)
                {
                    track.trackSection.Next.Previous = null;
                }
                else if(track.trackSection.Next.Next == track.trackSection)
                {
                    track.trackSection.Next.Next = null;
                }
            }
            if(track.trackSection.Previous != null)
            {
                if(track.trackSection.Previous.Previous == track.trackSection)
                {
                    track.trackSection.Previous.Previous = null;
                }
                else if(track.trackSection.Previous.Next == track.trackSection)
                {
                    track.trackSection.Previous.Next = null;
                }
            }
            TrackDatabase.Instance.DeregisterTrack(track.trackSection);
            GameObject.Destroy(track.gameObject);
        }
    }
}
