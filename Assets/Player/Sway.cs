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
    private Vector3 rot;
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
    private Vector3 cameraBobPos;
    private float cameraBobTimer = 0f;
    private Vector3 landBobVector;

    private float lastLanded = -1;

    private float landItemTimingOffset = 0;
    private float landCameraTimingOffset = 0;

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
        if(inventory.GetCurrent() != null)
        {
            transform_ = inventory.GetCurrent()?.gameObject.transform;
        }

        GetInput();
        GetVelocity();

        if(transform_ != null && inventory.GetCurrent().GetCanUse())
        {
            if(mc.isGrounded)
                CalculateSway();
            else
            {
                swayPos = Vector3.Lerp(swayPos, Vector3.zero, Time.deltaTime * 10);
            }
        }
        CalculateRotation();
        CalculateBob();

        if(transform_ != null && inventory.GetCurrent().GetCanUse())
        {
            ApplySwayRotation();
        }

        ApplyCameraBob();
            
        OnRotationChanged?.Invoke(rot);

        //if(uiTransforms.Count > 0)
        //{
        //    ApplyUIRotation();
        //}

        //if(hudTransforms.Count > 0)
        //{
        //    ApplyHUDRotation();
        //}
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

    private void CalculateSway()
    {
        swayCamPos.x = -mouseInput.x * cameraSway;
        swayCamPos.y = -mouseInput.y * cameraSway;

        swayPos.x = Mathf.Clamp(-transform_.InverseTransformDirection(velocity).x, -5, 5) * horizontalVelocitySway;
        swayPos.y = Mathf.Lerp(swayPos.y, Mathf.Clamp(-transform_.InverseTransformDirection(velocity).y, -2, 5) * verticalVelocitySway, Time.deltaTime * 5);
    }

    private void CalculateRotation()
    {
        rot.x = Mathf.Clamp(-mouseInput.y * itemRotation, -5, 5);
        rot.y = Mathf.Clamp(mouseInput.x * itemRotation, -5, 5);
    }

    private void CalculateBob()
    {
        float horizontalVelocity = Vector3.ProjectOnPlane(velocity, transform.up).magnitude;
        if (horizontalVelocity > 3f && mc.isGrounded)
        {
            bobTimer = Time.time * bobFrequency;
            bobPos.y = Mathf.Sin(bobTimer + landItemTimingOffset) * bobVerticalAmount;
            bobPos.x = Mathf.Cos(bobTimer * 0.5f) * bobHorizontalAmount;

            cameraBobTimer = Time.time * cameraBobFrequency;
            cameraBobPos.y = Mathf.Sin(cameraBobTimer + landCameraTimingOffset) * cameraBobVerticalAmount;
        }
        else
        {
            bobTimer = Time.time * bobFrequency / 10;
            bobPos.y = Mathf.Sin(bobTimer + landItemTimingOffset) * bobVerticalAmount  / 5;
            bobPos.x = 0; 

            //cameraBobTimer = Time.time * cameraBobFrequency / 10;
            cameraBobPos.y = 0 /*Mathf.Sin(cameraBobTimer) * cameraBobVerticalAmount  / 5*/;
            //bobTimer = 0f;  // Reset when stationary
            //bobPos = Vector3.Lerp(bobPos, Vector3.zero, 10 * Time.deltaTime);
        }
    }

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

    private void ApplyCameraBob()
    {
        if(cameraTransform != null)
        {
            
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, (Time.time - lastLanded) > 0.1f ? cameraBobPos : cameraBobPos /*landBobVector*/, Time.deltaTime * 5);
        }
    }

    public void CameraLandBob(float verticalSpeed)
    {
        landCameraTimingOffset = -cameraBobTimer + 135;
        landItemTimingOffset = -bobTimer + 135;
        //landBobVector = new Vector3(0, -0.5f * Mathf.Clamp01(Mathf.Abs(verticalSpeed) / 10), 0);
        Debug.Log("Land: " + Mathf.Clamp01(Mathf.Abs(Mathf.Min(verticalSpeed, 0)) / 10));
        lastLanded = Time.time;
    }


    public void OnJump()
    {
        swayPos += new Vector3(0, 0.2f, 0);
    }

    public void OnLand()
    {
        //StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(0.01f);
            if(Player.Instance.MovementController.isGrounded)
                swayPos += new Vector3(0, -0.1f, 0);
        }
    }

    //public void ItemLandBob(float verticalSpeed)
    //{
    //    landBobVector = new Vector3(0, -0.5f * Mathf.Clamp01(Mathf.Abs(verticalSpeed) / 10), 0);
    //    Debug.Log("Land: " + Mathf.Clamp01(Mathf.Abs(Mathf.Min(verticalSpeed, 0)) / 10));
    //    lastLanded = Time.time;
    //}

   //private void ApplyUIRotation()
   //{
   //    foreach(Transform uiTransform in uiTransforms)
   //    {
   //        uiTransform.localRotation = Quaternion.Lerp(uiTransform.localRotation, Quaternion.Euler(rot * 2.5f), Time.unscaledDeltaTime * 5);
   //    }
   //}
   //private void ApplyHUDRotation()
   //{
   //    foreach(Transform hudTransform in hudTransforms)
   //    {
   //        hudTransform.localRotation = Quaternion.Lerp(hudTransform.localRotation, Quaternion.Euler(rot * 1.5f), Time.deltaTime * 5);
   //    }
   //}
}
