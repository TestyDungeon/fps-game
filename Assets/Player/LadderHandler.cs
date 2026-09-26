using UnityEngine;

public class LadderHandler : MonoBehaviour, ICustomTriggerReceiver
{
    private MovementController mc;
    private PlayerMovement pm;

    void Start()
    {
        mc = GetComponent<MovementController>();
        pm = GetComponent<PlayerMovement>();
    }

    public void OnCustomTriggerEnter(Collider other)
    {
        
    }

    public void OnCustomTriggerStay(Collider other)
    {
        if (other.TryGetComponent<Ladder>(out Ladder lad))
        {
            if(pm.GetWishDir().sqrMagnitude > 0)
            {
                mc.resetNegativeVerticalVelocity();
                if(Vector3.Project(mc.getVelocity(), mc.transform.up).sqrMagnitude < 7 * 7)
                {
                    mc.addVelocity(mc.transform.up * 2);

                }
            }
            else
            {
                if(mc.GetVerticalSpeed() < -3)
                {
                    if(!mc.isGrounded)
                        mc.addVelocity(mc.transform.up * 0.5f);

                }
                
            }
        }
    }

    public void OnCustomTriggerExit(Collider other)
    {
        
    }
}
