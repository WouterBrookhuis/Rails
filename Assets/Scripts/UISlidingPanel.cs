using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISlidingPanel : MonoBehaviour {

    public Vector3 moveDelta;
    public float speed = 15;
    public float targetPosition;
    public float position;
    public bool startAtFar;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
        if(startAtFar)
        {
            position = targetPosition = 1.0f;
        }
    }

    private void Update()
    {
        position = Mathf.MoveTowards(position, targetPosition, speed * Time.deltaTime);
        transform.position = Vector3.Lerp(startPos, startPos + moveDelta, position);
    }

    public void Toggle()
    {
        if(targetPosition < 0.5f)
        {
            targetPosition = 1.0f;
        }
        else
        {
            targetPosition = 0.0f;
        }
    }

    public void Far()
    {
        targetPosition = 1.0f;
    }

    public void Near()
    {
        targetPosition = 0.0f;
    }
}
