using RailLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TrainSerializer : AbstractSerializer
{
    public override void LoadWorld(BinaryReader reader)
    {
        this.reader = reader;
        var wagonCount = reader.ReadInt32();
        for(int i = 0; i < wagonCount; i++)
        {
            DeserializeWagon();
        }
    }

    public override void SaveWorld(BinaryWriter writer)
    {
        this.writer = writer;
        var wagons = FindObjectsOfType<Wagon>().Where(x => x.Placed).ToArray();
        writer.Write(wagons.Length);
        for(int i = 0; i < wagons.Length; i++)
        {
            Serialize(wagons[i]);
        }
    }

    private void Serialize(Wagon wagon)
    {
        writer.Write(wagon.name);
        writer.Write(wagon.wheels.Length);
        for(int i = 0; i < wagon.wheels.Length; i++)
        {
            Serialize(wagon.wheels[i].Follower);
        }
    }

    private Wagon DeserializeWagon()
    {
        var prefabName = reader.ReadString();
        var prefab = WagonCollection.Instance.GetPrefab(prefabName);
        var instance = Instantiate(prefab);
        instance.name = prefab.name;
        var followers = new List<TrackFollower>();
        var wheelCount = reader.ReadInt32();
        for(int i = 0; i < wheelCount; i++)
        {
            followers.Add(DeserializeTrackFollower());
        }
        instance.Place(followers.ToArray());

        return instance;
    }

    private void Serialize(TrackFollower follower)
    {
        writer.Write(follower.TrackSection.UniqueID);
        writer.Write(follower.Distance);
        writer.Write(follower.Inverted);
    }

    private TrackFollower DeserializeTrackFollower()
    {
        var trackId = reader.ReadUInt32();
        var distance = reader.ReadSingle();
        var inverted = reader.ReadBoolean();
        return new TrackFollower(TrackDatabase.Instance.GetSection(trackId), distance, inverted);
    }
}
