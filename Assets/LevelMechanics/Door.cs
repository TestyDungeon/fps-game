using UnityEngine;
using DG.Tweening;

public class Door : MonoBehaviour, IInteractable
{
    public bool hasKey;
    public Pickup key = null;
    public bool locked;
    private BoxCollider boxCollider;
    private Vector3 size;
    private Vector3 closedPos;
    private bool closed = true;

    [SerializeField] private bool rotate = false;
    [SerializeField] private Transform rotationAxis;
    [SerializeField] private float animSpeed = 0.3f;
    [SerializeField] private float openRange = 4;
    [SerializeField] private Vector3 direction = Vector3.up;
    [SerializeField] private bool automatic = false;
    [SerializeField] private float activationRange = 4;

    private Tween movementTween;
    private float rotationAngle;

    void Awake()
    {
        if(key != null)
            locked = true;
        //Renderer renderer = GetComponent<Renderer>();
        //size = renderer.bounds.size; 
        //size = Vector3.Scale(boxCollider.size, transform.lossyScale);
        closedPos = transform.position;
    }

    void FixedUpdate()
    {
        if (automatic)
        {
            if (Vector3.Distance(PlayerMovement.Instance.transform.position, closedPos) < activationRange)
            {
                Interact();
            }
            else
            {
                Open(false);
            }
        }
        
    }

    public void Interact()  
    {
        if(key == null && hasKey)
            locked = false;
        if(!locked)
            Open(true);
    }

    

    public void Open(bool state)
    {
        if (state == true && state == closed)
        {
            if (rotate && rotationAxis == null)
            {
                Debug.LogError($"{nameof(Door)} on {name} requires a rotation axis when rotation is enabled.", this);
                return;
            }

            SoundManager.PlaySound(SoundType.DOOR_OPEN, transform.position, 0.2f, 0.9f);
            AnimateDoor(openRange);
            closed = false;
        }
        else if(!state)
        {
            AnimateDoor(0);
            closed = true;
        }
    }

    private void AnimateDoor(float target)
    {
        movementTween?.Kill();

        if (rotate)
        {
            movementTween = DOTween.To(
                () => rotationAngle,
                angle =>
                {
                    transform.RotateAround(
                        rotationAxis.position,
                        rotationAxis.forward,
                        angle - rotationAngle);
                    rotationAngle = angle;
                },
                target,
                animSpeed)
                .SetEase(Ease.OutBack);
            return;
        }

        movementTween = transform.DOMove(closedPos + direction * target, animSpeed)
            .SetEase(Ease.Linear);
    }
}
