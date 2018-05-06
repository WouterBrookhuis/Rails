using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class AbstractSerializer : MonoBehaviour
{
    protected BinaryWriter writer;
    protected BinaryReader reader;

    public abstract void SaveWorld(BinaryWriter writer);
    public abstract void LoadWorld(BinaryReader reader);


    protected void Serialize(Vector3 vector)
    {
        writer.Write(vector.x);
        writer.Write(vector.y);
        writer.Write(vector.z);
    }

    protected Vector3 DeserializeVector3()
    {
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        var z = reader.ReadSingle();
        return new Vector3(x, y, z);
    }

    protected void Serialize(Quaternion quaternion)
    {
        writer.Write(quaternion.w);
        writer.Write(quaternion.x);
        writer.Write(quaternion.y);
        writer.Write(quaternion.z);
    }

    protected Quaternion DeserializeQuaternion()
    {
        var w = reader.ReadSingle();
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        var z = reader.ReadSingle();
        return new Quaternion(x, y, z, w);
    }
}
