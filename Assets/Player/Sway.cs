using UnityEngine;

public class Sway : MonoBehaviour
{
    [SerializeField] private float cameraSway = 0.3f; 
    [SerializeField] private float velocitySway = 0.1f; 
    private Transform transform_;
    private Vector2 mouseInput = Vector2.zero;
    private Vector3 velocity = Vector3.zero;
    private Vector3 swayPos;
    private Vector3 swayCamPos;
    private Inventory inventory;


    void Start()
    {
        inventory = GetComponent<Inventory>();
    }

    void FixedUpdate()
    {
        if(inventory.GetCurrent() != null)
            transform_ = inventory.GetCurrent()?.gameObject.transform;

        if(transform_ != null)
        {
            GetInput();
            GetVelocity();
            SwayPosition();
            SwayApply();
        }
            
    }
    
    private void GetInput()
    {
        mouseInput.x = Input.GetAxisRaw("Mouse X");
        mouseInput.y = Input.GetAxisRaw("Mouse Y");
    }

    private void GetVelocity()
    {
        velocity = GetComponent<MovementController>().getVelocity();
    }

    private void SwayPosition()
    {
        swayCamPos.x = -mouseInput.x * cameraSway;
        swayCamPos.y = -mouseInput.y * cameraSway;
        swayPos = -transform_.InverseTransformDirection(velocity) * velocitySway;
    }

    private void SwayApply()
    {
        transform_.localPosition = Vector3.Lerp(transform_.localPosition, swayPos+swayCamPos, Time.deltaTime * 5);
    }
}
