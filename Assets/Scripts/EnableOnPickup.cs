using UnityEngine;
using UnityEngine.Events;

public class EnableOnPickup : MonoBehaviour
{
    [SerializeField] private GameObject pickup;
    public UnityEvent onPickup;

    void FixedUpdate()
    {
        if(pickup == null)
        {
            onPickup?.Invoke();
        }
    }
}
