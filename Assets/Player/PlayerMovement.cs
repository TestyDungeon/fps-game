using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public event Action<int, int, float> OnDash;
    [SerializeField] private Transform playerPivot;

    private MovementController movementController;

    [SerializeField] private PlayerConfig playerConfig;
    private Vector3 playerVelocity = Vector3.zero;
    private bool isPlayingFootsteps = false;
    private bool jumped = false;
    private int jumpsLeft;
    private bool wasGrounded = true;

    private int dashLeft;

    private float slopeSticking = 0.9f;
    float traceDistance;
    private float frictionMult = 1;
    private Vector3 wishdir;
    private bool coyoteUsed = false;

    [SerializeField] private Camera playerCamera;
    [HideInInspector] public float defaultFOV;
    [HideInInspector] public float targetFov;
    [SerializeField] private Camera weaponCamera;
    [HideInInspector] public float defaultWeaponFOV;
    [HideInInspector] public float targetWeaponFov;

    private void Awake()
    {
        dashLeft = playerConfig.dashAmount;
        defaultFOV = playerCamera.fieldOfView;
        targetFov = defaultFOV;
        defaultWeaponFOV = weaponCamera.fieldOfView;
        targetWeaponFov = defaultWeaponFOV;
        movementController = GetComponent<MovementController>();
        jumpsLeft = playerConfig.jumpsAmount;
        //traceDistance = movementController.capsuleCollider.height/2 + slopeSticking;
    }

    private void Update()
    {
        
        if(!wasGrounded && movementController.GroundCheck())
            SoundManager.PlaySound(SoundType.LANDING, 0.6f);
        AirMove();
        JumpButton();
        FOV();
        DashButton();
    }

    private void FixedUpdate()
    {
        SlopeHandling();
        if(dashLeft < playerConfig.dashAmount && !IsInvoking("ResetDash")/* && movementController.GroundCheck()*/)
        {
            Invoke("ResetDash", playerConfig.dashCooldown);
            
        }
        
        playerVelocity = movementController.Move(playerVelocity);
        if (movementController.GroundCheck() && playerVelocity.sqrMagnitude > 25)
            StartCoroutine(PlayFootStepsSound());
    }

    void LateUpdate()
    {
        wasGrounded = movementController.GroundCheck();
    }


    private void GroundMove()
    {
    
        if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, traceDistance, movementController.layerMask))
        {
            if (hit.normal.y >= 0.7f)
            {
                float dropDistance = hit.distance;
                transform.position += -transform.up * dropDistance;
                //playerVelocity += -transform.up * dropDistance;
            }
        }

    }

    private void AirMove()
    {
        
        Vector3 wishvel = new Vector3();
        float wishspeed;

        Vector3 forward;
        Vector3 right;

        float fmove, smove;

        forward = playerPivot.forward;
        right = playerPivot.right;

        fmove = Input.GetAxisRaw("Horizontal");
        smove = Input.GetAxisRaw("Vertical");

        Vector3.Normalize(forward);
        Vector3.Normalize(right);

        for (int i = 0; i < 3; i++)
            wishvel[i] = forward[i] * smove + right[i] * fmove;

        wishdir = wishvel;
        wishdir = Vector3.Normalize(wishdir);
        wishspeed = wishdir.magnitude * (movementController.GroundCheck() ? playerConfig.speed : playerConfig.airMaxSpeed);
        Debug.DrawRay(transform.position, wishdir, Color.yellow);

        if (wishspeed > playerConfig.MAX_SPEED)
        {
            mathlib.VectorScale(wishvel, playerConfig.MAX_SPEED / wishspeed, wishvel);
            wishspeed = playerConfig.MAX_SPEED;
        }

        if (movementController.GroundCheck())
        {
            
            Friction();
            Accelerate(wishdir, wishspeed);
            //GroundMove();
        }
        else
        {
            AirAccelerate(wishdir, wishspeed);
        }
    }

    private void Accelerate(Vector3 wishDir, float wishSpeed)
    {
        float currentSpeed, addSpeed, accelSpeed;

        currentSpeed = Vector3.Dot(playerVelocity, wishDir);
        addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0)
            return;

        accelSpeed = playerConfig.accel * Time.deltaTime * wishSpeed;

        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        for (int i = 0; i < 3; i++)
            playerVelocity[i] += wishDir[i] * accelSpeed;
    }


    private void AirAccelerate(Vector3 wishDir, float wishSpeed)
    {
        
        float wishSpd = wishSpeed;

        if (wishSpd > playerConfig.airMaxSpeed)
            wishSpd = playerConfig.airMaxSpeed;

        Vector3 currentHorizontalVel = new Vector3(playerVelocity[0], 0, playerVelocity[2]);
        float currentSpeed = currentHorizontalVel.magnitude;

        float targetSpeed = wishSpd;

        if(currentSpeed > targetSpeed)
            targetSpeed = currentSpeed;

        if (targetSpeed > playerConfig.airMaxSpeed)
        {
            targetSpeed = Mathf.Lerp(currentSpeed, playerConfig.airMaxSpeed, Time.deltaTime * 2f);
        }

        if(wishSpd > 0)
        {
            playerVelocity[0] = Mathf.Lerp(playerVelocity[0], (wishDir * targetSpeed)[0], playerConfig.airAccel * Time.deltaTime);
            playerVelocity[2] = Mathf.Lerp(playerVelocity[2], (wishDir * targetSpeed)[2], playerConfig.airAccel * Time.deltaTime);
        }
    }

    private void Friction()
    {
        //ref float vel;
        float control, drop, newspeed;

        float speed = playerVelocity.magnitude;

        if (speed < 0.01)
        {
            playerVelocity = Vector3.zero;
            return;
        }

        drop = 0;

        if (movementController.GroundCheck())
        {
            control = speed < playerConfig.stopSpeed ? playerConfig.stopSpeed : speed;
            drop += control * playerConfig.friction * Time.deltaTime;
        }

        newspeed = speed - drop;
        if (newspeed < 0)
            newspeed = 0;
        newspeed /= speed;


        playerVelocity[0] *= newspeed;
        playerVelocity[2] *= newspeed;
    }

    private void JumpButton()
    {
        if (!movementController.GroundCheck() && jumpsLeft <= 0)
        {
            
            return;
        }
        else if(movementController.GroundCheck() && !jumped)
        {
            StopCoroutine(CoyoteJump());
            jumpsLeft = playerConfig.jumpsAmount;
        }

        if (!movementController.GroundCheck() && !jumped && !coyoteUsed)
            StartCoroutine(CoyoteJump());

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumped = true;
            jumpsLeft--;
            
            movementController.resetVerticalVelocity();
            playerVelocity += transform.up * playerConfig.jumpStrength;
            //movementController.addVelocityContextual(transform.up * playerConfig.jumpStrength);
            //if(movementController.GroundCheck())
                SoundManager.PlaySound(SoundType.JUMP, 0.4f);
        }

        // Reset jumped flag when we're grounded and not pressing jump
        if (movementController.GroundCheck() && !Input.GetKey(KeyCode.Space))
        {
            coyoteUsed = false;
            jumped = false;
        }
    }

    private void SlopeHandling()
    {
        if (movementController.GroundCheck() && !jumped)
        {
            movementController.addVelocity(-transform.up * 0.25f);
        }
    }

    private IEnumerator CoyoteJump()
    {
        coyoteUsed = true;
        yield return new WaitForSeconds(0.1f);
        jumpsLeft = playerConfig.jumpsAmount - 1;
    }

    public void AddJump(int j)
    {
        jumpsLeft = j;
    }

    private void DashButton()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(dashLeft > 0)
            {
                SetDash(dashLeft - 1);
                ExecuteDash();
            }
        }
    }

    private void ExecuteDash() 
    {
        
        Vector3 dir = GetWishDir();
        SoundManager.PlaySound(SoundType.DASH, 1f);
        if(dir == Vector3.zero)
        {
            dir = Vector3.ProjectOnPlane(playerCamera.transform.forward, transform.up).normalized;
        }
        movementController.resetVelocity();
        movementController.SetDashDir(dir);
        movementController.Dash(dir, 4f, 40, 10);
        
        movementController.addVelocity(dir * 10);


    }

    private void ResetDash()
    {   
        if(dashLeft < playerConfig.dashAmount)
        {
            SoundManager.PlaySound(SoundType.DASH_RECHARGE, 0.1f);
            SetDash(dashLeft + 1);
        }
    }

    private void SetDash(int x)
    {
        dashLeft = x;
        OnDash?.Invoke(playerConfig.dashAmount, dashLeft, playerConfig.dashCooldown);
    }

    private void OnGUI()
    {
        //GUI.color = Color.green;
        //var ups = playerVelocity;
        //GUI.Label(new Rect(0, 15, 400, 100),
        //"Speed: " + Mathf.Round(ups.magnitude * 100) / 100 + "ups\n" +
        //"Velocity: " + ups + "\n" +
        //"Grounded: " + movementController.GroundCheck() + "\n" +
        //"Jumps: " + jumpsLeft + "\n" +
        //"Jumped?: " + jumped + "\n" +
        //"Time Scale: " + Time.timeScale);
    }

    IEnumerator PlayFootStepsSound()
    {
        if (isPlayingFootsteps)
            yield break;
            
        isPlayingFootsteps = true;
        SoundManager.PlaySound(SoundType.FOOTSTEP, 0.8f);
        yield return new WaitForSeconds(0.5f);
        isPlayingFootsteps = false;
    }

    public Vector3 GetWishDir()
    {
        return wishdir;
    }

    public IEnumerator ChangeFriction(float mult, float dur)
    {
        frictionMult = mult;
        yield return new WaitForSeconds(dur);
        frictionMult = 1;
    }

    private void FOV()
    {
        float rate = 200;
        float weaponRate = rate * (defaultWeaponFOV / defaultFOV);
        if(playerCamera.fieldOfView != targetFov)
        {
            playerCamera.fieldOfView = Mathf.MoveTowards(playerCamera.fieldOfView, targetFov, Time.deltaTime * rate);
        }
        if(weaponCamera.fieldOfView != targetWeaponFov)
        {
            weaponCamera.fieldOfView = Mathf.MoveTowards(weaponCamera.fieldOfView, targetWeaponFov, Time.deltaTime * weaponRate);
        }
    } 

    public void SetFOV(float fov)
    {

        targetFov = defaultFOV * fov;
        targetWeaponFov = defaultWeaponFOV * fov;
        
    }

}