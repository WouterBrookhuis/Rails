using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Wagon))]
public class LocomotiveController : MonoBehaviour {

    public Wagon wagon;
    public float acceleration = 1.0f;
    public float targetSpeed;

    public float maxSpeed;
    public float maxTractiveEffort;
    public AnimationCurve tractiveEffortCurve;

    // Use this for initialization
    void Start () {
        wagon = GetComponent<Wagon>();
        wagon.isKinematic = true;
	}
	
	// Update is called once per frame
	void Update ()
    {
        /*float curvePoint = Mathf.Clamp01(Mathf.Abs(wagon.speed) / maxSpeed);
        var tractiveEffort = tractiveEffortCurve.Evaluate(curvePoint) * maxTractiveEffort;
        if(wagon.speed > targetSpeed)
        {
            tractiveEffort = -tractiveEffort;
        }
        wagon.ApplyForce(tractiveEffort);*/
        wagon.speed = Mathf.MoveTowards(wagon.speed, targetSpeed, acceleration * Time.deltaTime);
    }
}
