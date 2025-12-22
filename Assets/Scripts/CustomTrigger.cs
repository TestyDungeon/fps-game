using UnityEngine;
using System.Collections.Generic;

public class CustomTrigger : MonoBehaviour
{
    [SerializeField] private MonoBehaviour receiver;
    private HashSet<Collider> _currentTriggers = new HashSet<Collider>();
    private ICustomTriggerReceiver _receiver;
    private CapsuleCollider capsuleCollider;
    private float capsuleHalfHeight;

    void Awake() 
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleHalfHeight = capsuleCollider.height / 2 - capsuleCollider.radius;

        if (receiver != null)
            _receiver = receiver as ICustomTriggerReceiver;
    }

    void Update()
    {
        CustomTriggerCheck();
    }

    private void CustomTriggerCheck()
    {
        _currentTriggers.RemoveWhere(c => c == null);

        Collider[] hits = Physics.OverlapCapsule(
            transform.position + transform.up * capsuleHalfHeight, 
            transform.position - transform.up * capsuleHalfHeight, 
            capsuleCollider.radius);
        // Detect enters
        foreach (var hit in hits)
        {
            if (!hit.isTrigger)
                continue;

            if (_currentTriggers.Add(hit))
            {
                _receiver?.OnCustomTriggerEnter(hit);
                //OnCustomTriggerEnter(hit);
            }
            else
            {
                // Was already in set → staying
                _receiver?.OnCustomTriggerStay(hit);
                //OnCustomTriggerStay(hit);
            }
        }

        // Detect exits
        _currentTriggers.RemoveWhere(c =>
        {
            if (!System.Array.Exists(hits, h => h == c))
            {
                _receiver?.OnCustomTriggerExit(c);
                //OnCustomTriggerExit(c);
                return true;
            }
            return false;
        });
    }

    
}
