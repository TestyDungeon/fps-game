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

    void Awake()
    {
        if(key != null)
            locked = true;

        boxCollider = GetComponent<BoxCollider>();
        size = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale);
        closedPos = transform.position;
    }

    void FixedUpdate()
    {
        if (Vector3.Distance(PlayerHitResponder.Instance.transform.position, closedPos) < 4)
        {
            Interact();
        }
        else
        {
            Open(false);
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
        if (state && state == closed)
        {
            
            SoundManager.PlaySound(SoundType.DOOR_OPEN, transform.position, 0.2f, 0.9f);
            transform.DOMove(closedPos + Vector3.up * size.y, 0.3f);
            closed = false;
        }
        else if(!state)
        {
            
            transform.DOMove(closedPos, 0.3f);
            closed = true;
        }
    }
}
