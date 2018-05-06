using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainDriveTool : Tool
{
    public LocomotiveController locomotive;
    public UnityEngine.UI.Text speedText;
    public UnityEngine.UI.Text locoText;
    public UnityEngine.UI.Slider speedSlider;
    public int multiplier = 1;

    private void Update()
    {
        speedText.text = string.Format("Speed: {0}", locomotive != null ? locomotive.wagon.speed.ToString("0.00") : "-.--");
        locoText.text = string.Format("Driving {0}", locomotive != null ? locomotive.name : "Nothing");
    }

    public override void OnWagonActivate(Wagon wagon, ActivateInfo info)
    {
        var loco = wagon.GetComponent<LocomotiveController>();
        if(loco != null)
        {
            locomotive = loco;
            speedSlider.value = Mathf.Round(locomotive.targetSpeed) / multiplier;
        }
    }

    public override void OnNothingHit()
    {
        locomotive = null;
    }

    public override void OnTerrainHit(RaycastHit hit)
    {
        locomotive = null;
    }

    public void SpeedChanged()
    {
        if(locomotive != null)
        {
            locomotive.targetSpeed = speedSlider.value * multiplier;
        }
    }
}
