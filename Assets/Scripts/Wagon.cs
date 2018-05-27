using RailLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    private float wheelBase;

    /// <summary>
    /// Previous frame's speed
    /// </summary>
    private float previousSpeed;
    private bool placed;

    public Coupler frontCoupler;
    public Coupler rearCoupler;

    public Train Train { get; set; }

    public Wagon Next
    {
        get
        {
            if(rearCoupler.Coupled)
            {
                return rearCoupler.other.wagon;
            }
            return null;
        }
    }

    public Wagon Previous
    {
        get
        {
            if(frontCoupler.Coupled)
            {
                return frontCoupler.other.wagon;
            }
            return null;
        }
    }

    public bool Placed { get { return placed; } }

    public float LoC { get; private set; }

    public Vector3 PreviousSpeedVector
    {
        get { return transform.rotation * Vector3.forward * previousSpeed; }
    }

    private void Awake()
    {
        // Ensure a train is set when awoken
        if(Train == null)
        {
            Train = new Train(this);
        }
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
            else
            {
                wheelBase = Vector3.Distance(wheels[0].transform.position, wheels[1].transform.position);
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

        if(Vector3.Distance(wheels[0].Follower.Position, wheels[1].Follower.Position) > wheelBase + 1.0f)
        {
            Derail();
        }

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

    public void OnCoupled(Wagon to)
    {
        Debug.LogFormat("Wagon {0} coupled to {1}", name, to.name);

        Train.LogConsist();
    }
}

public class TrainIterator
{
    private Wagon wagon;
    private bool reverse;

    public TrainIterator(Wagon start, bool reverse = false)
    {
        wagon = start;
        this.reverse = reverse;
    }

    public void Flip()
    {
        reverse = !reverse;
    }

    public bool HasNext()
    {
        return reverse ? wagon.Previous != null : wagon.Next != null;
    }

    public Wagon Next()
    {
        var nextWagon = reverse ? wagon.Previous : wagon.Next;
        // Check if we need to invert on the next wagon
        if(IsRelativeInverted(wagon, nextWagon))
        {
            reverse = !reverse;
        }
        wagon = nextWagon;
        return wagon;
    }

    private bool IsRelativeInverted(Wagon from, Wagon to)
    {
        if(from.Next == to && to.Next == from ||
            from.Previous == to && to.Previous == from)
        {
            return true;
        }
        return false;
    }
}
