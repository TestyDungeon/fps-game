using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sway : MonoBehaviour
{
    [SerializeField] private float cameraSway = 0.3f; 
    [SerializeField] private float itemRotation = 0.1f; 
    [SerializeField] private float rotRate = 5f; 
    [SerializeField] private float horizontalVelocitySway = 0.01f; 
    [SerializeField] private float verticalVelocitySway = 0.01f; 
    private Transform transform_;

    private Vector2 mouseInput = Vector2.zero;
    private Vector3 velocity = Vector3.zero;
    private Vector3 swayPos;
    private Vector3 swayCamPos;
    private Vector3 itemRot;
    private Inventory inventory;
    private MovementController mc;

    [SerializeField] private float bobFrequency = 5f;
    [SerializeField] private float bobVerticalAmount = 0.05f;
    [SerializeField] private float bobHorizontalAmount = 0.03f;
    private Vector3 bobPos;
    private float bobTimer = 0f;


    [SerializeField] private float cameraBobFrequency = 5f;
    [SerializeField] private float cameraBobVerticalAmount = 0.05f;
    [SerializeField] private Transform cameraTransform;
    private Vector3 camBobPos;
    private float camBobTimer = 0f;
    private Vector3 landBobVector;

    private float lastLanded = -1;

    private float landItemTimingOffset = 0;
    private float landCameraTimingOffset = 0;

    private Vector3 jumpBobPos = Vector3.zero;
    private Vector3 tJumpBobPos = Vector3.zero;
    private Vector3 landCamBobPos = Vector3.zero;
    private Vector3 tLandCamBobPos = Vector3.zero;

    public static event Action<Vector3> OnRotationChanged;
    //[SerializeField] List<RectTransform> uiTransforms;
    //[SerializeField] List<RectTransform> hudTransforms;

    void Start()
    {
        inventory = GetComponent<Inventory>();
        mc = GetComponent<MovementController>();
    }

    void Update()
    {
        GetInput();
        GetVelocity();

        if(inventory.GetCurrent() != null)
        {
            transform_ = inventory.GetCurrent()?.gameObject.transform;
        }

        ItemVelocitySway();
        ItemLookSway();
        ItemRotation();
        ItemBob();

        CameraBob();

        JumpBob();
        LandBob();
        //if(transform_ != null && inventory.GetCurrent().GetCanUse())
        //{
        //    ApplySwayRotation();
        //}

        //ApplyCameraBob();
            
        OnRotationChanged?.Invoke(itemRot);


        if(transform_ != null)
        {
            transform_.localPosition = swayPos + bobPos + swayCamPos + jumpBobPos;
            transform_.localRotation = Quaternion.Euler(itemRot);
        }
        
        
        
        foreach(Item i in inventory.GetAlwaysOn())
        {
            i.transform.localPosition = swayPos + bobPos + swayCamPos + jumpBobPos;
            i.transform.localRotation = Quaternion.Euler(itemRot);
        }

        cameraTransform.localPosition = camBobPos + landCamBobPos;
    }
    
    private void GetInput()
    {
        mouseInput.x = Input.GetAxisRaw("Mouse X");
        mouseInput.y = Input.GetAxisRaw("Mouse Y");
    }

    private void GetVelocity()
    {
        velocity = mc.getVelocity();
    }

    private void ItemVelocitySway()
    {
        Vector3 tSwayPos = swayPos;
        tSwayPos.x = Mathf.Clamp(-transform_.InverseTransformDirection(velocity).x, -5, 5) * horizontalVelocitySway;
        tSwayPos.y = Mathf.Lerp(swayPos.y, Mathf.Clamp(-transform_.InverseTransformDirection(velocity).y, -2, 5) * verticalVelocitySway, Time.deltaTime * 5);
        swayPos = Vector3.Lerp(swayPos, tSwayPos, Time.deltaTime * 5);
    }

    private void ItemLookSway()
    {
        Vector3 tSwayCamPos = swayCamPos;
        tSwayCamPos.x = -mouseInput.x * cameraSway;
        tSwayCamPos.y = -mouseInput.y * cameraSway;
        swayCamPos = Vector3.Lerp(swayCamPos, tSwayCamPos, Time.deltaTime * 5);
    }

    private void ItemRotation()
    {
        Vector3 tItemRot = Vector3.zero;
        tItemRot.x = Mathf.Clamp(-mouseInput.y * itemRotation, -5, 5);
        tItemRot.y = Mathf.Clamp(mouseInput.x * itemRotation, -5, 5);
        itemRot = Vector3.Lerp(itemRot, tItemRot, Time.deltaTime * rotRate);
    }

    private void ItemBob()
    {
        float horizontalVelocity = Vector3.ProjectOnPlane(velocity, transform.up).magnitude;
        Vector3 tBobPos = bobPos;
        if (horizontalVelocity > 3f && (mc.isGrounded || (Time.time - mc.lastTimeGrounded) < 0.1f))
        {
            bobTimer = Time.time * bobFrequency;
            tBobPos.y = Mathf.Sin(bobTimer + landItemTimingOffset) * bobVerticalAmount;
            tBobPos.x = Mathf.Cos(bobTimer * 0.5f) * bobHorizontalAmount;

            //cameraBobTimer = Time.time * cameraBobFrequency;
            //cameraBobPos.y = Mathf.Sin(cameraBobTimer + landCameraTimingOffset) * cameraBobVerticalAmount;
        }
        else
        {
            bobTimer = Time.time * bobFrequency / 10;
            tBobPos.y = Mathf.Sin(bobTimer + landItemTimingOffset) * bobVerticalAmount  / 5;
            tBobPos.x = 0; 

            //cameraBobTimer = Time.time * cameraBobFrequency / 10;
            //cameraBobPos.y = 0 /*Mathf.Sin(cameraBobTimer) * cameraBobVerticalAmount  / 5*/;
            //bobTimer = 0f;  // Reset when stationary
            //bobPos = Vector3.Lerp(bobPos, Vector3.zero, 10 * Time.deltaTime);
        }

        bobPos = Vector3.Lerp(bobPos, tBobPos, Time.deltaTime * 5);
    }

    private void CameraBob()
    {
        float horizontalVelocity = Vector3.ProjectOnPlane(velocity, transform.up).magnitude;
        Vector3 tCamBobPos = camBobPos;
        if (horizontalVelocity > 3f && (mc.isGrounded || (Time.time - mc.lastTimeGrounded) < 0.1f))
        {
            camBobTimer = Time.time * cameraBobFrequency;
            tCamBobPos.y = Mathf.Sin(camBobTimer + landCameraTimingOffset) * cameraBobVerticalAmount;
        }
        else
        {
            tCamBobPos.y = 0;
        }

        camBobPos = Vector3.Lerp(camBobPos, tCamBobPos, Time.deltaTime * 5);
    }
    /*
    private void ApplySwayRotation()
    {
        if(transform_ != null)
        {
            transform_.localPosition = Vector3.Lerp(transform_.localPosition, swayPos+swayCamPos + bobPos, Time.deltaTime * 5);
            transform_.localRotation = Quaternion.Lerp(transform_.localRotation, Quaternion.Euler(rot), Time.deltaTime * rotRate);
        }
        
        
        
        foreach(Item i in inventory.GetAlwaysOn())
        {
            i.transform.localPosition = Vector3.Lerp(i.transform.localPosition, swayPos+swayCamPos + bobPos, Time.deltaTime * 10);
            i.transform.localRotation = Quaternion.Lerp(i.transform.localRotation, Quaternion.Euler(rot), Time.deltaTime * 15);
        }
    }
    */
    
    //private void ApplyCameraBob()
    //{
    //    if(cameraTransform != null)
    //    {
    //        
    //        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, (Time.time - lastLanded) > 0.1f ? cameraBobPos : cameraBobPos /*landBobVector*/, Time.deltaTime * 5);
    //    }
    //}
    
    public void CameraLandBob(float verticalSpeed)
    {
        landCameraTimingOffset = -camBobTimer + 135;
        landItemTimingOffset = -bobTimer + 135;
        //landBobVector = new Vector3(0, -0.5f * Mathf.Clamp01(Mathf.Abs(verticalSpeed) / 10), 0);
        Debug.Log("Land: " + Mathf.Clamp01(Mathf.Abs(Mathf.Min(verticalSpeed, 0)) / 10));
        lastLanded = Time.time;
    }


    public void OnJump()
    {
        tJumpBobPos = new Vector3(0, 0.2f, 0);
    }

    private void JumpBob()
    {
        jumpBobPos = Vector3.Lerp(jumpBobPos, tJumpBobPos, Time.deltaTime * 5);
        tJumpBobPos = Vector3.Lerp(tJumpBobPos, Vector3.zero, Time.deltaTime * 10);
    }

    public void OnLand(float verticalSpeed)
    {
        float amp = Mathf.Max(Mathf.Min(verticalSpeed, 0) / 10, -1);
        tLandCamBobPos = new Vector3(0, amp, 0);
    }

    private void LandBob()
    {
        if((landCamBobPos - tLandCamBobPos).magnitude > 0.01)
            landCamBobPos = Vector3.Lerp(landCamBobPos, tLandCamBobPos, Time.deltaTime * 5);

        tLandCamBobPos = Vector3.Lerp(tLandCamBobPos, Vector3.zero, Time.deltaTime * 10);
    }

    //public void ItemLandBob(float verticalSpeed)
    //{
    //    landBobVector = new Vector3(0, -0.5f * Mathf.Clamp01(Mathf.Abs(verticalSpeed) / 10), 0);
    //    Debug.Log("Land: " + Mathf.Clamp01(Mathf.Abs(Mathf.Min(verticalSpeed, 0)) / 10));
    //    lastLanded = Time.time;
    //}
}
