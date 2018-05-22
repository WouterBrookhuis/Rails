using RailLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TrackSectionComponent : MonoBehaviour, IActivatable {

    public TrackSection trackSection;
    public List<TrackSectionComponent> group;

    public bool InGroup { get { return group != null && group.Count > 0; } }

    public void Activate(ActivateInfo hit)
    {
        ToolController.Instance.ActiveTool.OnTrackActivate(this, hit);
    }

    public void Hover(ActivateInfo hit)
    {
        ToolController.Instance.ActiveTool.OnTrackHover(this, hit);
    }

    void Update()
    {
        if(trackSection.Next == null)
        {
            Highlighter.DrawHighlighter(Matrix4x4.TRS(trackSection.EndPosition + Vector3.up, trackSection.EndRotation, Vector3.one));
        }
        if(trackSection.Previous == null)
        {
            Highlighter.DrawHighlighter(Matrix4x4.TRS(trackSection.Position + Vector3.up, trackSection.Rotation, Vector3.one));
        }
    }

    public void AddToGroup(List<TrackSectionComponent> group)
    {
        RemoveFromGroup();
        this.group = group;
        this.group.Add(this);
    }

    public void RemoveFromGroup()
    {
        if(group != null)
        {
            group.Remove(this);
            group = null;
        }
    }

    public void SetTrackSection(TrackSection section, Material trackMaterial)
    {
        var mesh = CreateSectionMesh(section);
        GetComponent<Renderer>().sharedMaterial = trackMaterial;
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        transform.SetPositionAndRotation(section.Position, section.Rotation);
        trackSection = section;
        trackSection.Component = this;
    }

    private Mesh CreateSectionMesh(TrackSection section)
    {
        float subLength = TrackFactory.Instance.trackLength;
        float subWidth = TrackFactory.Instance.trackWidth;

        int subdivisions = Mathf.CeilToInt(section.Length / subLength);
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[2 + subdivisions * 2];
        Vector2[] uvs = new Vector2[2 + subdivisions * 2];
        int[] triangles = new int[subdivisions * 2 * 6];

        int vertexIndex = 0;
        int triangleIndex = 0;

        float distance = 0.0f;
        for(int i = 0; i < subdivisions + 1; i++)
        {
            if(i == subdivisions)
                distance = section.Length;

            Vector3 center = section.GetLocalPositionOnTrack(distance);
            Quaternion angle = section.GetLocalRotationOnTrack(distance);
            Vector3 tangent = angle * Vector3.forward;

            Vector3 right = -Vector3.Cross(tangent, Vector3.up) * subWidth * 0.5f;
            Vector3 left = -right;

            vertices[vertexIndex] = left + center;
            vertices[vertexIndex + 1] = right + center;

            uvs[vertexIndex] = new Vector2(0, i);
            uvs[vertexIndex + 1] = new Vector2(1, i);

            //Debug.Log("i: " + i + " center: " + center.ToString() + " left: " + left.ToString() + " right " + right.ToString());
            if(i > 0.0f)
            {
                triangles[triangleIndex] = vertexIndex - 2;
                triangles[triangleIndex + 1] = vertexIndex + 1;
                triangles[triangleIndex + 2] = vertexIndex - 1;

                triangles[triangleIndex + 3] = vertexIndex - 2;
                triangles[triangleIndex + 4] = vertexIndex;
                triangles[triangleIndex + 5] = vertexIndex + 1;
                triangleIndex += 6;
            }
            vertexIndex += 2;

            distance += section.Length / (float)subdivisions;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }
}
