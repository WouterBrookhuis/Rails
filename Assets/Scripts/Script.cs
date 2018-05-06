using RailLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Script : MonoBehaviour {
    public LocomotiveController testWagon;
    public float speedDelta = 1.0f;
    private Text speedText;

    private BasicTrackLayerTool trackTool;

	// Use this for initialization
	void Start () {
        trackTool = FindObjectOfType<BasicTrackLayerTool>();
        speedText = GameObject.Find("SpeedText").GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        speedText.text = string.Format("Speed {0}/{1}", testWagon.wagon.speed.ToString("0.00"), testWagon.targetSpeed.ToString("0.00"));
    }

    public void PlaceTrain()
    {
        testWagon.wagon.Place(trackTool.TrackLayer.CurrentSection, -trackTool.TrackLayer.CurrentSection.Length / 2);
    }

    public void ForwardTrain()
    {
        testWagon.targetSpeed += speedDelta;
    }

    public void BackwardTrain()
    {
        testWagon.targetSpeed -= speedDelta;
    }
}
