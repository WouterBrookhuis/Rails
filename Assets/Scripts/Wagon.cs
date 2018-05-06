using RailLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Wagon : MonoBehaviour, IActivatable {

    public Coupler[] couplers;
    public WheelMarker[] wheels;
    public new Rigidbody rigidbody;
    [Tooltip("Indicates if physics calls affect this wagon's speed")]
    public bool isKinematic;
    [Tooltip("The wagon's forward speed in m/s")]
    public float speed;

    public float drag = 1.0f;

    /// <summary>
    /// Previous frame's speed
    /// </summary>
    private float previousSpeed;
    private bool placed;

    public Coupler frontCoupler;
    public Coupler rearCoupler;

    public bool Placed { get { return placed; } }

    public float LoC { get; private set; }

    public Vector3 PreviousSpeedVector
    {
        get { return transform.rotation * Vector3.forward * previousSpeed; }
    }

	// Use this for initialization
	void Start () {
        FindComponents();
    }

    private void FindComponents()
    {
        if(wheels == null || wheels.Length == 0)
        {
            wheels = transform.GetComponentsInChildren<WheelMarker>();
            if(wheels.Length != 2)
            {
                Debug.LogError("Wagon " + name + " does not have 2 wheel sets!");
            }
            couplers = transform.GetComponentsInChildren<Coupler>();
            if(couplers.Length != 2)
            {
                Debug.LogErrorFormat("Wagon {0} has {1} couplers, must be 2!", name, couplers.Length);
            }
            else
            {
                var relPosZero = transform.InverseTransformPoint(couplers[0].transform.position);
                var relPosOne = transform.InverseTransformPoint(couplers[1].transform.position);
                bool zeroIsForward = relPosZero.z > relPosOne.z;
                if(zeroIsForward)
                {
                    frontCoupler = couplers[0];
                    rearCoupler = couplers[1];
                }
                else
                {
                    frontCoupler = couplers[1];
                    rearCoupler = couplers[0];
                }
                LoC = Vector3.Distance(frontCoupler.transform.position, rearCoupler.transform.position);
            }
            rigidbody = GetComponent<Rigidbody>();
        }
    }

    private void Update()
    {
        if(wheels.Length != 2 || !placed)
        {
            return;
        }

        // Use previous frame's speed as basis for force calculation
        var velocity = PreviousSpeedVector;
        if(frontCoupler.Coupled)
        {
            var dv = frontCoupler.other.wagon.PreviousSpeedVector - velocity;
            //frontCoupler.ApplyForce(dv * rigidbody.mass, false);
        }
        if(rearCoupler.Coupled)
        {
            var dv = rearCoupler.other.wagon.PreviousSpeedVector - velocity;
            //rearCoupler.ApplyForce(dv * rigidbody.mass, false);
        }
    }

    // Update is called once per frame
    void LateUpdate () {
		if(wheels.Length != 2 || !placed)
        {
            return;
        }

        // Apply some drag to the final speed
        ApplyForce(rigidbody.mass * -speed * drag);

        // Apply the speed calculated in Update()
        foreach(var wheel in wheels)
        {
            if(!wheel.Follower.Move(speed * Time.deltaTime))
            {
                Derail();
                return;
            }
        }

        var delta = Mathf.Abs(wheels[0].transform.localPosition.z) + Mathf.Abs(wheels[1].transform.localPosition.z);
        delta = Mathf.Abs(wheels[0].transform.localPosition.z / delta);
        var position = Vector3.Lerp(wheels[0].Follower.Position, wheels[1].Follower.Position, delta);
        var rotation = Quaternion.LookRotation(wheels[0].Follower.Position - wheels[1].Follower.Position);
        transform.SetPositionAndRotation(position, rotation);

        previousSpeed = speed;
    }

    public void Place(TrackFollower[] followers)
    {
        FindComponents();
        if(wheels.Length != followers.Length)
        {
            Debug.LogErrorFormat("Follower count {0} does not match wheel count {1} for wagon {2}", followers.Length, wheels.Length, name);
            return;
        }
        for(int i = 0; i < wheels.Length; i++)
        {
            wheels[i].Place(followers[i]);
            wheels[i].lockRotation = false;
        }
        OnPlaced();
    }

    public void Place(TrackSection trackSection, float centerDistance)
    {
        FindComponents();
        foreach(var wheel in wheels)
        {
            wheel.Place(trackSection, centerDistance + wheel.transform.localPosition.z);
            wheel.lockRotation = false;
        }
        OnPlaced();
    }

    public void Derail()
    {
        Debug.LogWarning("Train Derailed!");
        Decouple();
        placed = false;
        rigidbody.isKinematic = false;
        foreach(var wheel in wheels)
        {
            wheel.lockRotation = true;
        }

        speed = previousSpeed = 0;
    }

    public void Decouple()
    {
        foreach(var coupler in couplers)
        {
            coupler.Decouple();
        }
    }

    private void OnPlaced()
    {
        speed = 0;
        previousSpeed = 0;
        placed = true;
        rigidbody.isKinematic = true;
    }

    public void Hover(ActivateInfo hit)
    {
        ToolController.Instance.ActiveTool.OnWagonHover(this, hit);
    }

    public void Activate(ActivateInfo hit)
    {
        ToolController.Instance.ActiveTool.OnWagonActivate(this, hit);
    }

    public void ApplyForce(Vector3 force)
    {
        var alongForward = transform.InverseTransformDirection(force);
        ApplyForce(alongForward.z);
    }

    public void ApplyForce(float forceForward)
    {
        if(isKinematic) return;
        var acceleration = forceForward / rigidbody.mass;
        speed += acceleration * Time.deltaTime;
    }
}
