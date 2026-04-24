using UnityEngine;
using DG.Tweening;

public class MovingPlatform : MonoBehaviour, IInteractable
{
    Vector3 start;
    public Transform end;
    Vector3 to;
    [SerializeField] float duration = 1;

    private bool dir = true;

    void Start()
    {
        start = transform.position;
    }


    public void Interact()
    {
        Move(dir);
        
        dir = !dir;
    }

    private void Move(bool x)
    {
        if(x)
            transform.DOMove(end.position, duration);
        else
            transform.DOMove(start, duration);
    }
}
