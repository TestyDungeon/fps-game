using UnityEngine;

public class Sway : MonoBehaviour
{
    [SerializeField] private float cameraSway = 0.3f; 
    [SerializeField] private float itemRotation = 0.1f; 
    [SerializeField] private float rotRate = 5f; 
    [SerializeField] private float velocitySway = 0.1f; 
    private Transform transform_;
    private Vector2 mouseInput = Vector2.zero;
    private Vector3 velocity = Vector3.zero;
    private Vector3 swayPos;
    private Vector3 swayCamPos;
    private Vector3 rot;
    private Vector3 bobPos;
    private Inventory inventory;
    private MovementController mc;

    [SerializeField] private float bobFrequency = 5f;
    [SerializeField] private float bobVerticalAmount = 0.05f;
    [SerializeField] private float bobHorizontalAmount = 0.03f;
    private float bobTimer = 0f;


    [SerializeField] RectTransform uiTransform;

    void Start()
    {
        inventory = GetComponent<Inventory>();
        mc = GetComponent<MovementController>();
    }

    void Update()
    {
        if(inventory.GetCurrent() != null)
            transform_ = inventory.GetCurrent()?.gameObject.transform;

        if(transform_ != null)
        {
            GetInput();
            SwayPosition();
            GetVelocity();
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
        velocity = mc.getVelocity();
    }

    private void SwayPosition()
    {
        swayCamPos.x = -mouseInput.x * cameraSway;
        swayCamPos.y = -mouseInput.y * cameraSway;

        rot.x = Mathf.Clamp(-mouseInput.y * itemRotation, -5, 5);
        rot.y = Mathf.Clamp(mouseInput.x * itemRotation, -5, 5);

        swayPos.x = Mathf.Clamp(-transform_.InverseTransformDirection(velocity).x, -5, 5) * velocitySway;

        float horizontalVelocity = new Vector3(velocity.x, 0, velocity.z).magnitude;
        if (horizontalVelocity > 5f && mc.GroundCheck())
        {
            bobTimer = Time.time * bobFrequency;
            bobPos.y = Mathf.Sin(bobTimer) * bobVerticalAmount;
            bobPos.x = Mathf.Cos(bobTimer * 0.5f) * bobHorizontalAmount;
        }
        else
        {
            bobTimer = Time.time * bobFrequency / 10;
            bobPos.y = Mathf.Sin(bobTimer) * bobVerticalAmount  / 5;
            bobPos.x = 0; 
            //bobTimer = 0f;  // Reset when stationary
            //bobPos = Vector3.Lerp(bobPos, Vector3.zero, 10 * Time.deltaTime);
        }
    }

    private void SwayApply()
    {

        transform_.localPosition = Vector3.Lerp(transform_.localPosition, swayPos+swayCamPos+bobPos, Time.deltaTime * 5);
        transform_.localRotation = Quaternion.Lerp(transform_.localRotation, Quaternion.Euler(rot), Time.deltaTime * rotRate);

        uiTransform.localRotation = Quaternion.Lerp(uiTransform.localRotation, Quaternion.Euler(rot * 1.5f), Time.deltaTime * 5);
        foreach(Item i in inventory.GetAlwaysOn())
        {
            i.transform.localPosition = Vector3.Lerp(i.transform.localPosition, swayPos+swayCamPos+bobPos, Time.deltaTime * 10);
            i.transform.localRotation = Quaternion.Lerp(i.transform.localRotation, Quaternion.Euler(rot), Time.deltaTime * 15);
        }
    }
}
