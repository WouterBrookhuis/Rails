using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coupler : MonoBehaviour, IActivatable
{
    public float maxSlack = 0.5f;
    public float slack = 0.0f;
    public Coupler other;
    public Transform yawTransform;
    public bool restBackwards = false;
    public Wagon wagon;

    public bool Coupled { get { return other != null; } }

    private void Awake()
    {
        wagon = GetComponentInParent<Wagon>();
    }

    public void Activate(ActivateInfo hit)
    {
        Decouple();
    }

    public void Couple(Coupler other)
    {
        Decouple();
        if(wagon.Placed == false || other.wagon.Placed == false) return;
        this.other = other;
        other.other = this;
    }

    public void Decouple()
    {
        if(other != null)
        {
            other.other = null;
            other.yawTransform.localRotation = other.restBackwards ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
            yawTransform.localRotation = restBackwards ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
            other = null;
        }
    }

    public void Hover(ActivateInfo hit)
    {
        // TODO: Highlight in some way?
    }

    private void Update()
    {
        if(other != null)
        {
            yawTransform.LookAt(other.yawTransform);
            // Apply impact forces at slack extremes
            var distanceBetween = Vector3.Distance(yawTransform.position, other.transform.position) - transform.localPosition.z;
            var factor = other.wagon.isKinematic ? 1.0f : 2.0f;
            if(distanceBetween < 0)
            {
                // Push wagons away from each other
                // forward is towards the other wagon, however distancebetween is negative, so the force goes backwards
                wagon.ApplyForce(yawTransform.forward * (distanceBetween / (Time.deltaTime * factor)) * wagon.rigidbody.mass);
            }
            else if(distanceBetween > maxSlack)
            {
                // Pull wagons towards each other
                wagon.ApplyForce(yawTransform.forward * ((distanceBetween - maxSlack) / (Time.deltaTime * factor)) * wagon.rigidbody.mass);
            }
        }
    }

    private void LateUpdate()
    {
        if(other != null)
        {
            // Update slack
            var distanceBetween = Vector3.Distance(yawTransform.position, other.transform.position) - transform.localPosition.z;
            slack = Mathf.Clamp(maxSlack - distanceBetween, 0, maxSlack);
        }
    }

    public void ApplyForce(Vector3 force, bool ignoreSlack)
    {
        bool allowForce = false;
        var localizedForce = transform.InverseTransformDirection(force);
        if(slack == 0.0f)
        {
            // Tense coupler, only allow pulling forces
            
            allowForce = localizedForce.z < 0;
        }
        else if(slack == maxSlack)
        {
            // Relaxed coupler, only allow pushing forces
            allowForce = localizedForce.z > 0;
        }
        else if(ignoreSlack)
        {
            allowForce = true;
        }

        if(allowForce)
        {
            other.wagon.ApplyForce(force);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(Coupled)
        {
            return;
        }

        var otherCoupler = other.transform.GetComponent<Coupler>();
        if(otherCoupler != null && otherCoupler != this)
        {
            if(otherCoupler.Coupled)
            {
                Debug.LogWarning("Collided with other coupler that was already connected...");
            }
            else
            {
                Couple(otherCoupler);
            }
        }
    }
}
