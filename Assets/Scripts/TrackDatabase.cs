using RailLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackDatabase : Singleton<TrackDatabase> {

    public float zoneSize = 1000.0f;

    private Dictionary<Vector3, HashSet<TrackSection>> zones = new Dictionary<Vector3, HashSet<TrackSection>>();
    private Dictionary<uint, TrackSection> sectionMap = new Dictionary<uint, TrackSection>();
    private uint nextFreeId = 1;

    public void Clear()
    {
        zones = new Dictionary<Vector3, HashSet<TrackSection>>();
        sectionMap = new Dictionary<uint, TrackSection>();
        nextFreeId = 1;
    }

    public void BuildFromScene()
    {
        var components = FindObjectsOfType<TrackSectionComponent>();
        foreach(var component in components)
        {
            RegisterTrack(component.trackSection);
        }
    }

    public void RegisterTrack(TrackSection section)
    {
        var zoneStart = RoundToZone(section.Position);
        var zoneEnd = RoundToZone(section.EndPosition);
        if(!zones.ContainsKey(zoneStart))
        {
            zones[zoneStart] = new HashSet<TrackSection>();
        }
        zones[zoneStart].Add(section);
        if(zoneEnd != zoneStart)
        {
            if(!zones.ContainsKey(zoneEnd))
            {
                zones[zoneEnd] = new HashSet<TrackSection>();
            }
            zones[zoneEnd].Add(section);
        }

        if(section.UniqueID == 0)
        {
            section.UniqueID = nextFreeId++;
        }
        else if(nextFreeId <= section.UniqueID)
        {
            nextFreeId = section.UniqueID + 1;
        }

        sectionMap.Add(section.UniqueID, section);
    }

    public void DeregisterTrack(TrackSection section)
    {
        var zoneStart = RoundToZone(section.Position);
        var zoneEnd = RoundToZone(section.EndPosition);
        if(!zones.ContainsKey(zoneStart))
        {
            Debug.LogErrorFormat("Zone {0} does not exist, can not remove track from it", zoneStart);
        }
        else
        {
            zones[zoneStart].Remove(section);
        }
        
        if(zoneEnd != zoneStart)
        {
            if(!zones.ContainsKey(zoneEnd))
            {
                Debug.LogErrorFormat("Zone {0} does not exist, can not remove track from it", zoneStart);
            }
            else
            {
                zones[zoneEnd].Remove(section);
            }
        }

        sectionMap.Remove(section.UniqueID);
        section.UniqueID = 0;
    }

    public IEnumerable<TrackSection> GetZoneSections(Vector3 position)
    {
        var zone = RoundToZone(position);
        if(zones.ContainsKey(zone))
        {
            return zones[zone];
        }
        return new List<TrackSection>();
    }

    public TrackSection GetSection(uint id)
    {
        TrackSection s;
        sectionMap.TryGetValue(id, out s);
        return s;
    }

    private Vector3 RoundToZone(Vector3 position)
    {
        return new Vector3(
            Mathf.Floor(position.x / zoneSize) * zoneSize,
            Mathf.Floor(position.y / zoneSize) * zoneSize,
            Mathf.Floor(position.z / zoneSize) * zoneSize);
    }
}
