using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlighter : MonoBehaviour {

    private static Highlighter instance;

    public GameObject defaultPrefab;
    private Mesh defaultMesh;
    private Material defaultMaterial;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        defaultMesh = defaultPrefab.GetComponentInChildren<MeshFilter>().sharedMesh;
        defaultMaterial = defaultPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial;
    }

    public static void DrawHighlighter(Matrix4x4 matrix, HighlighterType type = HighlighterType.DEFAULT)
    {
        Graphics.DrawMesh(instance.defaultMesh, matrix, instance.defaultMaterial, SortingLayer.NameToID("Default"));
    }
}

public enum HighlighterType
{
    DEFAULT,
}
