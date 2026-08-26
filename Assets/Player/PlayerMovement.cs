using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    public event Action<int, int, float> OnDash;
    [SerializeField] private Transform playerPivot;

    private MovementController movementController;

    private Sway sway;

    [SerializeField] private PlayerMovementConfig playerMovementConfig;
    private Vector3 playerVelocity = Vector3.zero;
    private bool isPlayingFootsteps = false;
    private bool jumped = false;
    private int jumpsLeft;
    private bool wasGrounded = true;
    private float lastGroundedTime = 0;

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

    private float speedMultiplier = 1f;
    private float airControlMultiplier = 1f;

    private int dashInvokes = 2;

    private AudioSource fallingWindLoopAudioSource;
    private float windStartDelay = 0.5f;
    private float windStartMult = 0;

    private float lastVerticalSpeed = 0;

    Vector3 forward, right;

    float fmove, smove;


    public static PlayerMovement Instance { get; private set; }
    
    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    
        dashLeft = playerMovementConfig.dashAmount;
        defaultFOV = playerCamera.fieldOfView;
        targetFov = defaultFOV;
        defaultWeaponFOV = weaponCamera.fieldOfView;
        targetWeaponFov = defaultWeaponFOV;
        movementController = GetComponent<MovementController>();
        sway = GetComponent<Sway>();
        jumpsLeft = playerMovementConfig.jumpsAmount;
        //traceDistance = movementController.capsuleCollider.height/2 + slopeSticking;
    }

    private void Update()
    {
        if(!wasGrounded && movementController.GroundCheck())
        {
            sway.CameraLandBob(lastVerticalSpeed);
            //Debug.Log("Percent " + Mathf.Clamp01(Mathf.Max(playerVelocity.magnitude - 12, 0) / 14));
            SoundManager.PlaySound(SoundType.LANDING, 0.4f + Mathf.Clamp01(Mathf.Max(playerVelocity.magnitude - 12, 0) / 14) * 0.6f);
        }
        MoveInput();
        JumpButton();
        FOV();
        DashButton();
    }

    private void FixedUpdate()
    {
        if (movementController.GroundCheck())
        {
           dashInvokes = 1; 
        }
        //SlopeHandling();
        if (dashLeft < playerMovementConfig.dashAmount && !IsInvoking("ResetDash") && dashInvokes > 0)
        {
            Debug.Log("DashInvokes: " + dashInvokes);
            Invoke("ResetDash", playerMovementConfig.dashCooldown);
            dashInvokes--;
        }
        
        AirMove();
        playerVelocity = movementController.Move(playerVelocity);

        if (movementController.GroundCheck() && playerVelocity.sqrMagnitude > 25 && !isPlayingFootsteps)
            StartCoroutine(PlayFootStepsSound());

        WindSound();
    }

    void LateUpdate()
    {
        wasGrounded = movementController.GroundCheck();
        lastVerticalSpeed = movementController.GetVerticalSpeed();
    }

    private void MoveInput()
    {
        forward = playerPivot.forward;
        right = playerPivot.right;

        fmove = Input.GetAxisRaw("Horizontal");
        smove = Input.GetAxisRaw("Vertical");

        //Vector3.Normalize(forward);
        //Vector3.Normalize(right);
    }

    private void AirMove()
    {
        Vector3 wishvel = new Vector3();
        float wishspeed;

        for (int i = 0; i < 3; i++)
            wishvel[i] = forward[i] * smove + right[i] * fmove;

        wishdir = wishvel;
        wishdir = Vector3.Normalize(wishdir);
        wishspeed = wishdir.magnitude * (movementController.GroundCheck() ? playerMovementConfig.speed : playerMovementConfig.airSpeed);
        Debug.DrawRay(transform.position, wishdir, Color.yellow);

        if (wishspeed > playerMovementConfig.MAX_SPEED)
        {
            mathlib.VectorScale(wishvel, playerMovementConfig.MAX_SPEED / wishspeed, wishvel);
            wishspeed = playerMovementConfig.MAX_SPEED;
        }

        if (movementController.GroundCheck())
        {
            
            Friction();
            Accelerate(wishdir, wishspeed);
        }
        else
        {
            AirAccelerate(wishdir, wishspeed);
            //AirControl(wishdir, wishspeed);
        }
    }

    private void Accelerate(Vector3 wishDir, float wishSpeed)
    {
        float currentSpeed, addSpeed, accelSpeed;

        currentSpeed = Vector3.Dot(playerVelocity, wishDir);
        addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0)
            return;

        accelSpeed = playerMovementConfig.accel * Time.fixedDeltaTime * wishSpeed;

        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        for (int i = 0; i < 3; i++)
            playerVelocity[i] += wishDir[i] * accelSpeed;
    }

    private void AirAccelerate(Vector3 wishDir, float wishSpeed)
    {
        float wishSpd = wishSpeed;

        if (wishSpd > playerMovementConfig.airMaxSpeed)
            wishSpd = playerMovementConfig.airMaxSpeed;

        // PROJECT VELOCITY ONTO THE PLANE PERPENDICULAR TO TRANSFORM.UP (LOCAL GRAVITY UP)
        Vector3 currentHorizontalVel = Vector3.ProjectOnPlane(playerVelocity, transform.up);
        float currentSpeed = currentHorizontalVel.magnitude;

        float targetSpeed = wishSpd;

        if(currentSpeed > targetSpeed)
            targetSpeed = currentSpeed;

        if (targetSpeed > playerMovementConfig.airMaxSpeed)
        {
            targetSpeed = Mathf.Lerp(currentSpeed, playerMovementConfig.airMaxSpeed, Time.deltaTime * 2f);
        }

        if(wishSpd > 0)
        {
            playerVelocity = Vector3.Lerp(playerVelocity, wishDir * targetSpeed + (playerVelocity - currentHorizontalVel), playerMovementConfig.airAccel * airControlMultiplier * Time.fixedDeltaTime);
        }
    }
/*

    private void AirAccelerate(Vector3 wishDir, float wishSpeed)
    {
        float wishSpd = wishSpeed;

        if (wishSpd > playerMovementConfig.airMaxSpeed)
            wishSpd = playerMovementConfig.airMaxSpeed;

        float currentSpeed = Vector3.Dot(playerVelocity, wishDir);
        float addSpeed = wishSpd - currentSpeed;

        if (addSpeed <= 0)
            return;

        float accelSpeed = playerMovementConfig.airAccel * Time.deltaTime * wishSpeed;

        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        for (int i = 0; i < 3; i++)
            playerVelocity[i] += wishDir[i] * accelSpeed;
    }


    private void AirControl(Vector3 wishDir, float wishSpeed)
    {
        if (wishSpeed <= 0f) return;

        Vector3 verticalVel = Vector3.Project(playerVelocity, transform.up);
        Vector3 horizontalVel = playerVelocity - verticalVel;

        float speed = horizontalVel.magnitude;
        if (speed < 0.001f) return;

        Vector3 velDir = horizontalVel / speed;
        float dot = Vector3.Dot(velDir, wishDir);

        // only steer while wishdir roughly agrees with current velocity -
        // without this gate, air control lets you cancel/reverse momentum for free
        if (dot > 0f)
        {
            float k = playerMovementConfig.airControl * dot * dot * Time.deltaTime;
            velDir = (velDir + wishDir * k).normalized;
        }

        playerVelocity = velDir * speed + verticalVel; // same speed, new direction
    }
*/
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
            control = speed < playerMovementConfig.stopSpeed ? playerMovementConfig.stopSpeed : speed;
            drop += control * playerMovementConfig.friction * Time.deltaTime;
        }

        newspeed = speed - drop;
        if (newspeed < 0)
            newspeed = 0;
        newspeed /= speed;

        // Separate vertical and horizontal velocity relative to local up
        Vector3 verticalVel = Vector3.Project(playerVelocity, transform.up);
        Vector3 horizontalVel = playerVelocity - verticalVel;

        // Apply friction only to horizontal components
        playerVelocity = (horizontalVel * newspeed) + verticalVel;
    }

    private void JumpButton()
    {
        if ((!movementController.GroundCheck(out RaycastHit hit) || Vector3.Angle(hit.normal, transform.up) > 65) && jumpsLeft <= 0)
        {
            
            return;
        }
        else if(movementController.GroundCheck() && !jumped)
        {
            StopCoroutine(CoyoteJump());
            jumpsLeft = playerMovementConfig.jumpsAmount;
        }

        if (!movementController.GroundCheck() && !jumped && !coyoteUsed)
            StartCoroutine(CoyoteJump());

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumped = true;
            jumpsLeft--;
            
            movementController.resetVerticalVelocity();
            playerVelocity += transform.up * playerMovementConfig.jumpStrength;
            //movementController.addVelocityContextual(transform.up * playerConfig.jumpStrength);
            //if(movementController.GroundCheck())
                SoundManager.PlaySound(SoundType.JUMP, 0.4f);
        }

        if (movementController.GroundCheck() && !Input.GetKey(KeyCode.Space))
        {
            coyoteUsed = false;
            jumped = false;
        }
    }

    private void WindSound()
    {
        float volume = 0.25f;
        if (movementController.GroundCheck())
        {
            windStartMult = 0;
            lastGroundedTime = Time.time;
            if (fallingWindLoopAudioSource != null)
            {
                SoundManager.StopLoop(fallingWindLoopAudioSource, 0);
            }
        }
        else
        {
            if(fallingWindLoopAudioSource == null)
            {
                //Debug.Log("Falling");
                fallingWindLoopAudioSource = SoundManager.PlayLoop(SoundType.FALLING_WIND_LOOP, 0, UnityEngine.Random.Range(0, 3));

            }

            if(fallingWindLoopAudioSource != null)
            {
                if(movementController.GetIsDashing() == false)
                    fallingWindLoopAudioSource.volume = Mathf.Clamp01(Mathf.Max(playerVelocity.magnitude, 0) / 25) * volume * windStartMult;
                windStartMult = Mathf.MoveTowards(windStartMult, 1, windStartDelay * Time.fixedDeltaTime);
                //else
                //    fallingWindLoopAudioSource.volume = Mathf.Clamp01(movementController.GetDashSpeed() / 40) * volume;
            }
        }
    }

    private IEnumerator CoyoteJump()
    {
        coyoteUsed = true;
        yield return new WaitForSeconds(0.1f);
        jumpsLeft = playerMovementConfig.jumpsAmount - 1;
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
                dashInvokes--;
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
        movementController.Dash(dir, 4f, 40, 10);
        
        //movementController.addVelocity(dir * 10);


    }

    private void ResetDash()
    {   
        if(dashLeft < playerMovementConfig.dashAmount)
        {
            SoundManager.PlaySound(SoundType.DASH_RECHARGE, 0.1f);
            SetDash(dashLeft + 1);
        }
    }

    private void SetDash(int x)
    {
        dashLeft = x;
        OnDash?.Invoke(playerMovementConfig.dashAmount, dashLeft, playerMovementConfig.dashCooldown);
    }

    private void OnGUI()
    {
        GUI.color = Color.green;
        var ups = playerVelocity;
        GUI.Label(new Rect(0, 15, 400, 200),
        "Speed: " + Mathf.Round(ups.magnitude * 100) / 100 + "ups\n" +
        "Velocity: " + ups + "\n" +
        "Grounded: " + movementController.GroundCheck() + "\n" +
        "Jumps: " + jumpsLeft + "\n" +
        "Jumped?: " + jumped + "\n" +
        "Gravity Align Speed: " + movementController.GetGravityAlignSpeed() + "\n" +
        "Gravity Align Step: " + movementController.GetGravityAlignStep() + "\n" +
        "Time Scale: " + Time.timeScale);
    }

    IEnumerator PlayFootStepsSound()
    {
        //if (isPlayingFootsteps)
        //    yield break;
            
        isPlayingFootsteps = true;
        SoundManager.PlaySound(SoundType.FOOTSTEP, 0.35f);
        yield return new WaitForSeconds(0.35f);
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

    public void SetSpeedMultiplier(float value)
    {
        speedMultiplier = value;
    }

    public void SetAirControlMultiplier(float value)
    {
        airControlMultiplier = value;
    }
}