using RailLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainPlaceTool : Tool {

    public Button firstButton;

    public Wagon selectedWagon;

    private void Start()
    {
        bool makeCopy = false;
        foreach(var wagon in WagonCollection.Instance.prefabs)
        {
            Button button = firstButton;
            if(makeCopy)
            {
                button = Instantiate(button);
                button.transform.SetParent(firstButton.transform.parent);
            }
            makeCopy = true;
            button.GetComponentInChildren<Text>().text = wagon.name;
            button.onClick.AddListener(() => {
                selectedWagon = wagon;
            });
        }
    }

    public override void OnTrackHover(TrackSectionComponent track, ActivateInfo info)
    {
        if(selectedWagon != null)
        {
            float distance;
            // TODO: Draw ghost instead
            var matrix = Matrix4x4.TRS(GetSnappedTrackPosition(track.trackSection, info.hit.point, out distance),
                track.trackSection.GetRotationOnTrack(distance),
                Vector3.one);

            Highlighter.DrawHighlighter(matrix);

            Graphics.DrawMesh(selectedWagon.GetComponent<MeshFilter>().sharedMesh, matrix, selectedWagon.GetComponent<MeshRenderer>().sharedMaterial, gameObject.layer);
        }

    }

    public override void OnTrackActivate(TrackSectionComponent track, ActivateInfo info)
    {
        if(info.button == ActivateButton.LeftClick && selectedWagon != null)
        {
            // Place the selected wagon on the track
            float distance;
            GetSnappedTrackPosition(track.trackSection, info.hit.point, out distance);
            PlaceSelected(track.trackSection, distance);
        }
    }

    public override void OnWagonActivate(Wagon wagon, ActivateInfo info)
    {
        if(info.button == ActivateButton.RightClick)
        {
            Destroy(wagon.gameObject);
        }
    }

    private Vector3 GetSnappedTrackPosition(TrackSection section, Vector3 point, out float distance)
    {
        // TODO: This doesn't find tracks with both endpoints in a different sector than the hit...
        const float testResolution = 3.0f;
        distance = 0;
        float distanceFromPoint = float.MaxValue;
        float delta = Mathf.Min(testResolution, section.Length / 2);
        while(distance < section.Length)
        {
            var newDistance = distance + delta;
            var newDistanceFromPoint = Vector3.Distance(point, section.GetPositionOnTrack(newDistance));
            if(newDistanceFromPoint > distanceFromPoint)
            {
                break;
            }
            distance = newDistance;
            distanceFromPoint = newDistanceFromPoint;
        }

        distance = Mathf.Clamp(distance, 0, section.Length);
        return section.GetPositionOnTrack(distance);
    }

    private Wagon PlaceSelected(TrackSection section, float distance)
    {
        var wagon = Instantiate(selectedWagon);
        wagon.name = selectedWagon.name;
        selectedWagon = null;
        wagon.Place(section, distance);
        /*wagon.isKinematic = false;

        var wagon2 = Instantiate(wagon);
        wagon2.name = wagon.name;
        wagon2.Place(section, distance + wagon.LoC);
        wagon2.isKinematic = false;
        wagon2.GetComponent<LocomotiveController>().enabled = false;

        var wagon3 = Instantiate(wagon);
        wagon3.name = wagon.name;
        wagon3.Place(section, distance + wagon.LoC * 2);

        wagon3.isKinematic = false;
        wagon3.GetComponent<LocomotiveController>().enabled = false;

        wagon2.rearCoupler.Couple(wagon.frontCoupler);
        wagon3.rearCoupler.Couple(wagon2.frontCoupler);*/

        return wagon;
    }
}
