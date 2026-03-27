using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    public float recoilAmount;
    public float recoilSpeed;
    public float returnSpeed;
    private Vector3 currentRecoil;
    private Vector3 targetRecoil;
    private Transform transform_;
    private MovementController mc;
    private Vector3 origin;
    private Vector3 prePos;

    void Start()
    {

        origin = transform.localPosition;
        mc = PlayerHitResponder.Instance.gameObject.GetComponent<MovementController>();    
        prePos = transform.localPosition;
    }


    void Update()
    {
        //Vector3.MoveTowards(transform.localPosition, origin, Time.deltaTime * 0.01f);
        //transform.localPosition = Vector3.Project(-mc.getVelocity(), PlayerHitResponder.Instance.transform.up) * 0.05f + origin;
        targetRecoil = Vector3.Lerp(targetRecoil, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRecoil = Vector3.Slerp(currentRecoil, targetRecoil, recoilSpeed * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRecoil);
    }

    void LateUpdate()
    {
        prePos = transform.localPosition;
    }

    public void ApplyRecoil()
    {
        targetRecoil += new Vector3(-recoilAmount, 0, 0);
    }
}
