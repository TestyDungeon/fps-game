using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform playerPivot;

    private MovementController movementController;

    //[SerializeField] private float MAX_SPEED = 0.5f;
    //[SerializeField] private float speed = 0.35f;
    //[SerializeField] private float accel = 11f;
    //[SerializeField] private float airMaxSpeed = 0.1f;
    //[SerializeField] private float airAccel = 11f;
    //[SerializeField] private float friction = 5f;
    //[SerializeField] private float stopSpeed = 0.1f;
    //[SerializeField] private float jumpStrength = 0.35f;
    [SerializeField] private PlayerConfig playerConfig;
    private Vector3 playerVelocity = Vector3.zero;
    private bool isPlayingFootsteps = false;

    private void Awake()
    {
        movementController = GetComponent<MovementController>();
    }

    private void Update()
    {
        AirMove();
        JumpButton();
    }

    private void FixedUpdate()
    {
        playerVelocity = movementController.Move(playerVelocity);
        if (movementController.GroundCheck() && playerVelocity.sqrMagnitude > 25)
            StartCoroutine(PlayFootStepsSound());
    }

    private void AirMove()
    {
        Vector3 wishdir;
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
        wishspeed = wishdir.magnitude * playerConfig.speed;
        Vector3.Normalize(wishdir);

        if (wishspeed > playerConfig.MAX_SPEED)
        {
            mathlib.VectorScale(wishvel, playerConfig.MAX_SPEED / wishspeed, wishvel);
            wishspeed = playerConfig.MAX_SPEED;
        }

        if (movementController.GroundCheck())
        {
            Friction();
            Accelerate(wishdir, wishspeed);
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

        float currentSpeed = Vector3.Dot(playerVelocity, wishDir);
        float addSpeed = wishSpd - currentSpeed;

        if (addSpeed <= 0)
            return;

        float accelSpeed = playerConfig.airAccel * Time.deltaTime * wishSpeed;

        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        for (int i = 0; i < 3; i++)
            playerVelocity[i] += wishDir[i] * accelSpeed;
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
        playerVelocity[1] *= newspeed;
        playerVelocity[2] *= newspeed;
    }

    private void JumpButton()
    {
        if (!movementController.GroundCheck())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerVelocity -= Vector3.Project(playerVelocity, transform.up);
            playerVelocity += transform.up * playerConfig.jumpStrength;
            SoundManager.PlaySound(SoundType.JUMP, 0.4f);
        }
    }

    //private void OnGUI()
    //{
    //    GUI.color = Color.green;
    //    var ups = playerVelocity;
    //    GUI.Label(new Rect(0, 15, 400, 100),
    //    "Speed: " + Mathf.Round(ups.magnitude * 100) / 100 + "ups\n" +
    //    "Velocity: " + ups + "\n" +
    //    "Grounded: " + movementController.GroundCheck());
    //}

    IEnumerator PlayFootStepsSound()
    {
        if (isPlayingFootsteps)
            yield break;
            
        isPlayingFootsteps = true;
        SoundManager.PlaySound(SoundType.FOOTSTEP, 0.2f);
        yield return new WaitForSeconds(0.25f);
        isPlayingFootsteps = false;
    }
}