using RailLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackFactory : Singleton<TrackFactory>
{
    public Material trackMaterial;
    public float trackWidth = 4.0f;
    public float trackLength = 4.0f;
    public Switch switchPrefab;

    public TrackSectionComponent PlaceSectionGameObject(TrackSection section)
    {
        return PlaceTrackGO(section);
    }

    public TrackSectionComponent PlaceAndRegisterSection(TrackSection section, bool autoconnect = false)
    {
        TrackDatabase.Instance.RegisterTrack(section);
        var component = PlaceSectionGameObject(section);
        if(autoconnect)
        {
            BasicTrackLayerTool.TryAutoConnect(section);
        }
        return component;
    }

    public Switch AddSwitchToJunction(Junction junction, bool leftSide = false)
    {
        var sw = Instantiate(switchPrefab);
        sw.transform.SetParent(junction.Entry.Component.transform);
        sw.transform.localPosition = Vector3.forward * junction.Entry.Length * 0.5f + (leftSide ? Vector3.left * 2 : Vector3.right * 2);
        sw.transform.localRotation = Quaternion.identity;
        sw.junction = junction;
        sw.Initialize();
        return sw;
    }

    private TrackSectionComponent PlaceTrackGO(TrackSection section)
    {
        var go = new GameObject("Track Section");
        var tsc = go.AddComponent<TrackSectionComponent>();
        tsc.SetTrackSection(section, trackMaterial);
        var glow = go.AddComponent<GlowObject>();
        glow.glowColor = Color.red;
        return tsc;
    }
}
