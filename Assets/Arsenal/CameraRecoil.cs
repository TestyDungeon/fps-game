using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    private float recoilAmount;
    private float recoilSpeed;
    private float returnSpeed;
    private Vector3 currentRecoil;
    private Vector3 targetRecoil;
    private Vector3 origin;
    private float stepOffset;

    private Vector2 screenShake = new Vector2(0, 0);

    void Awake()
    {
        origin = transform.localPosition;  
    }


    void Update()
    {
        float dt = Time.deltaTime;
        //Vector3.MoveTowards(transform.localPosition, origin, Time.deltaTime * 0.01f);
        //transform.localPosition = Vector3.Project(-mc.getVelocity(), PlayerHitResponder.Instance.transform.up) * 0.05f + origin;
        screenShake = new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.x, 0) * screenShake;
        targetRecoil = Vector3.Lerp(targetRecoil, Vector3.zero, ExpStep(returnSpeed, dt));
        currentRecoil = Vector3.Lerp(currentRecoil, targetRecoil, ExpStep(recoilSpeed, dt));
 
        
        transform.localPosition = Vector3.Lerp(transform.localPosition, origin, Time.deltaTime * 4);
        transform.localRotation = Quaternion.Euler(currentRecoil/* + new Vector3(screenShake.x, screenShake.y, 0)*/);
        screenShake = Vector2.MoveTowards(screenShake, Vector2.zero, Time.deltaTime * 0.1f);
    }

    void LateUpdate()
    {
        //StepSmooth();
        //transform.localPosition = Vector3.Lerp(transform.localPosition, origin, Time.deltaTime * 6f);
    }


    private static float ExpStep(float speed, float dt)
    {
        return 1f - Mathf.Exp(-speed * dt);
    }


    public void ApplyRecoil(float recoilAmount_, float recoilSpeed_, float returnSpeed_, bool additive = true)
    {
        recoilAmount = recoilAmount_;
        recoilSpeed = recoilSpeed_;
        returnSpeed = returnSpeed_;
        if(additive)
            targetRecoil += new Vector3(-recoilAmount, 0, 0);
        else
            targetRecoil = new Vector3(-recoilAmount, 0, 0);
        //ApplyScreenShake(recoilAmount_);
        
    }

    public void ResetRecoil()
    {
        targetRecoil = Vector3.zero;
        currentRecoil = targetRecoil;
        transform.localRotation = Quaternion.Euler(currentRecoil);
    }


    public void ApplyScreenShake(float screenShakeAmount_)
    {
        screenShake = Vector2.one * screenShakeAmount_ * 0.5f;
        targetRecoil += new Vector3(0, 0, Random.Range(-1f, 1f) * Mathf.Abs(screenShakeAmount_));
    }

    public void StepSmooth()
    {
        stepOffset += Player.Instance.MovementController.ConsumeStepSmooth();                         // adds pendingStepSmooth, clears it
        stepOffset = Mathf.MoveTowards(stepOffset, 0f, 6f * Time.deltaTime);
        transform.localPosition = origin - transform.up * stepOffset;
    }

    public void StepSmooth(float amount)
    {
        stepOffset += Player.Instance.MovementController.ConsumeStepSmooth();                         // adds pendingStepSmooth, clears it
        stepOffset = Mathf.MoveTowards(stepOffset, 0f, 6f * Time.deltaTime);
        transform.localPosition -= Vector3.up * Mathf.Clamp(amount, 0, 0.05f);
    }
}
