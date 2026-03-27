using UnityEngine;
using System.Collections;

public class Dash : Item
{
    private int dashAmount = 2;
    private int dashLeft;
    private float dashCooldown = 0.8f;
    
    private MovementController mc;
    private PlayerMovement pm;
    

    void Start()
    {
        
        dashLeft = dashAmount;
        mc = player.GetComponent<MovementController>();
        pm = player.GetComponent<PlayerMovement>();
    } 

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.LeftShift))
        //{
        //    if(dashLeft > 0)
        //    {
        //        dashLeft--;
        //        ExecuteDash();
        //    }
        //    
        //    
        //}
    }

    void FixedUpdate()
    {
        if(dashLeft < dashAmount && !IsInvoking("ResetDash") && mc.GroundCheck())
        {
            
            Invoke("ResetDash", dashCooldown);
        }
    }

    private void ExecuteDash() 
    {
        Vector3 dir = pm.GetWishDir();
        SoundManager.PlaySound(SoundType.DASH, 1f);
        //StartCoroutine(pm.ChangeFOV(100, 200, 150));
        if(dir == Vector3.zero)
        {
            dir = Vector3.ProjectOnPlane(transform.forward, player.transform.up).normalized;
        }
        mc.resetVelocity();
        mc.SetDashDir(dir);
        mc.Dash(dir, 4f, 40, 10);
        
        mc.addVelocity(dir * 10);


    }

    private void ResetDash()
    {   
        if(dashLeft < dashAmount)
        {
            SoundManager.PlaySound(SoundType.DASH_RECHARGE, 0.1f);
            dashLeft++;
        }
    }

    
}
