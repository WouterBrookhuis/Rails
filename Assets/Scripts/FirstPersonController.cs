using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FirstPersonController : MonoBehaviour
{
    public static FirstPersonController Main { get; private set; }

    public float scale = 1.0f;
    public float height = 2.0f;
    public float minHeight = 1.0f;
    public float maxHeight = 2.5f;
    public float scaleSpeed = 10f;
    public float runSpeed = 20.0f;
    public float walkSpeed = 10.0f;
    public float stepSpeed = 5.0f;
    public float mouseSensitivity = 1.0f;
    public Transform pitchTransform;
    public bool invertMouseY = false;
    public bool disableLookWhenMouseUnlocked = true;
    private float pitchAngle = 0;
    public bool lockedMouse = false;

    public LayerMask activatableMask;
    private EventSystem eventSystem;

    private void Awake()
    {
        if(Main == null)
        {
            Main = this;
        }
    }

    // Use this for initialization
    void Start () {
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        pitchAngle = pitchTransform.rotation.eulerAngles.x;
	}
	
	// Update is called once per frame
	void Update () {
        // Get input parameters
        var mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), invertMouseY ? Input.GetAxisRaw("Mouse Y") : -Input.GetAxisRaw("Mouse Y"));
        var walkInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        var scaleInput = Input.GetAxisRaw("Mouse ScrollWheel");
        var speed = walkSpeed;
        if(walkInput.sqrMagnitude > 1)
        {
            walkInput.Normalize();
        }
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            speed = runSpeed;
        }
        else if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            speed = stepSpeed;
        }
        // Handle mouse lock
        if(Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            SetMouseLock(!lockedMouse);
        }

        // Do scale
        height = Mathf.Clamp(height + scaleInput * scaleSpeed, minHeight, maxHeight);

        if(!disableLookWhenMouseUnlocked || lockedMouse)
        {
            // Do yaw rotation
            transform.Rotate(Vector3.up, mouseInput.x * mouseSensitivity * Time.deltaTime);

            // Do pitch rotation
            var pitchDelta = mouseInput.y * mouseSensitivity * Time.deltaTime;
            if(pitchAngle + pitchDelta < -85)
            {
                pitchDelta = -85 - pitchAngle;
            }
            else if(pitchAngle + pitchDelta > 85)
            {
                pitchDelta = 85 - pitchAngle;
            }
            pitchAngle += pitchDelta;
            pitchTransform.Rotate(Vector3.right, pitchDelta, Space.Self);
        }

        // Do movement
        transform.Translate(walkInput * speed * Time.deltaTime * scale, Space.Self);

        // Update camera height
        var pos = pitchTransform.localPosition;
        pos.y = height * scale;
        pitchTransform.localPosition = pos;

        // Do activation
        RaycastHit hit;
        Ray ray = lockedMouse ?
            new Ray(pitchTransform.position, pitchTransform.forward) :
            Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out hit, 1000.0f, activatableMask))
        {
            var activatable = hit.collider.GetComponent<IActivatable>();
            var info = new ActivateInfo(hit, ActivateButton.None);
            if(activatable != null)
            {
                activatable.Hover(info);
                if(Clicked())
                {
                    info.button = Input.GetButtonDown("Fire1") ? ActivateButton.LeftClick : ActivateButton.RightClick;
                    activatable.Activate(info);
                }
            }
            else if(Clicked())
            {
                ToolController.Instance.ActiveTool.OnTerrainHit(hit);
            }
        }
        else if(Clicked())
        {
            ToolController.Instance.ActiveTool.OnNothingHit();
        }
	}

    private bool Clicked()
    {
        return (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2")) && !eventSystem.IsPointerOverGameObject();
    }

    public bool RaycastCameraForward(out RaycastHit hit, float range, int mask)
    {
        return Physics.Raycast(pitchTransform.position, pitchTransform.forward, out hit, range, mask);
    }

    public void SetMouseLock(bool locked)
    {
        if(locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        lockedMouse = locked;
    }
}
