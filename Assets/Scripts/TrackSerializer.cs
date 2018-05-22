using RailLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TrackSerializer : AbstractSerializer
{
    private Dictionary<List<TrackSectionComponent>, ushort> trackGroupIds;
    private Dictionary<ushort, List<TrackSectionComponent>> sectionGroupIds;
    private Dictionary<uint, TrackSection> loadedSections;

    public override void SaveWorld(BinaryWriter writer)
    {
        trackGroupIds = new Dictionary<List<TrackSectionComponent>, ushort>();
        this.writer = writer;

        var trackComponents = FindObjectsOfType<TrackSectionComponent>();

        writer.Write(trackComponents.Length);
        foreach(var component in trackComponents)
        {
            Serialize(component);
        }

        var switches = FindObjectsOfType<Switch>();

        writer.Write(switches.Length);
        foreach(var sw in switches)
        {
            Serialize(sw);
        }
    }

    public override void LoadWorld(BinaryReader reader)
    {
        TrackDatabase.Instance.Clear();
        // TODO: Destroy all existing objects?

        trackGroupIds = new Dictionary<List<TrackSectionComponent>, ushort>();
        sectionGroupIds = new Dictionary<ushort, List<TrackSectionComponent>>();
        loadedSections = new Dictionary<uint, TrackSection>();
        this.reader = reader;

        var trackComponentCount = reader.ReadInt32();
        for(int i = 0; i < trackComponentCount; i++)
        {
            DeserializeTrackSectionComponent();
        }

        var switchCount = reader.ReadInt32();
        for(int i = 0; i < switchCount; i++)
        {
            DeserializeSwitch();
        }
    }

    private void Serialize(TrackSectionComponent component)
    {
        Serialize(component.trackSection);
        ushort id = 0;
        if(component.InGroup)
        {
            if(!trackGroupIds.ContainsKey(component.group))
            {
                trackGroupIds.Add(component.group, (ushort)(trackGroupIds.Count + 1));
            }
            id = trackGroupIds[component.group];
        }
        writer.Write(id);
    }

    private TrackSectionComponent DeserializeTrackSectionComponent()
    {
        var section = DeserializeTrackSection();
        var groupId = reader.ReadUInt16();

        var component = TrackFactory.Instance.PlaceSectionGameObject(section);
        if(groupId != 0)
        {
            if(!sectionGroupIds.ContainsKey(groupId))
            {
                sectionGroupIds.Add(groupId, new List<TrackSectionComponent>());
            }
            component.AddToGroup(sectionGroupIds[groupId]);
        }
        return component;
    }

    private void Serialize(Switch sw)
    {
        Serialize(sw.transform.position);
        Serialize(sw.transform.rotation);
        Serialize(sw.junction);
    }

    private Switch DeserializeSwitch()
    {
        var pos = DeserializeVector3();
        var rot = DeserializeQuaternion();
        var junction = DeserializeJunction();
        var sw = TrackFactory.Instance.AddSwitchToJunction(junction);
        return sw;
    }

    private void Serialize(Junction junction)
    {
        writer.Write(junction.Entry.UniqueID);
        writer.Write(junction.Left.UniqueID);
        writer.Write(junction.Right.UniqueID);
        writer.Write(junction.GoLeft);
    }

    private Junction DeserializeJunction()
    {
        var entry = reader.ReadUInt32();
        var left = reader.ReadUInt32();
        var right = reader.ReadUInt32();
        var goLeft = reader.ReadBoolean();
        var junction = new Junction(loadedSections[entry], loadedSections[left], loadedSections[right]);
        if(!goLeft)
        {
            junction.Toggle();
        }
        return junction;
    }

    private void Serialize(TrackSection section)
    {
        if(section.UniqueID == 0)
        {
            throw new System.Exception("Invalid ID 0 for track section!");
        }
        writer.Write(section.UniqueID);
        Serialize(section.Position);
        Serialize(section.Rotation);
        writer.Write(section.Length);
        writer.Write(section.Curved ? section.Curve : 0);
    }

    private TrackSection DeserializeTrackSection()
    {
        var id = reader.ReadUInt32();
        if(id == 0)
        {
            throw new System.Exception("Invalid ID 0 for track section!");
        }
        var pos = DeserializeVector3();
        var rot = DeserializeQuaternion();
        var length = reader.ReadSingle();
        var curve = reader.ReadSingle();
        var section = new TrackSection()
        {
            UniqueID = id,
            Position = pos,
            Rotation = rot,
            Length = length,
            Curved = curve != 0,
            Curve = curve,
        };
        if(loadedSections.ContainsKey(id))
        {
            throw new System.Exception("Duplicate track id " + id);
        }
        loadedSections[id] = section;
        TrackDatabase.Instance.RegisterTrack(section);
        // TODO: This may be bad
        BasicTrackLayerTool.TryAutoConnect(section);
        return section;
    }
}
