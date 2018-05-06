using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Cameras;

public class MoveTarget : MonoBehaviour {

    new private Transform camera;
    public float speed = 10.0f;

	// Use this for initialization
	void Start () {
        camera = Camera.main.transform;
    }
	
	// Update is called once per frame
	void Update () {
        var input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        var fwd = camera.forward;
        fwd.y = 0;
        fwd.Normalize();

        transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
        transform.Translate(input * speed * Time.deltaTime, Space.Self);
	}
}
