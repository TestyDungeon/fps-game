using UnityEngine;
using System.Collections.Generic;

public class CustomTrigger : MonoBehaviour
{
    [SerializeField] private List<MonoBehaviour> receivers;
    [SerializeField] private LayerMask layermsk = 1 << 9 | 1 << 13;
    private HashSet<Collider> _currentTriggers = new HashSet<Collider>();
    private List<ICustomTriggerReceiver> receivers_ = new List<ICustomTriggerReceiver>();
    private CapsuleCollider capsuleCollider;
    private float capsuleHalfHeight;

    private readonly Collider[] overlapBuffer = new Collider[16];

    void Awake() 
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        if(tag != "Enemy")
            capsuleHalfHeight = capsuleCollider.height / 2 - capsuleCollider.radius;
        else
            capsuleHalfHeight = 0.2f;

        receivers_.Clear();

        if (receivers == null)
            return;

        foreach (var behaviour in receivers)
        {
            if (behaviour is ICustomTriggerReceiver customTriggerReceiver)
            {
                receivers_.Add(customTriggerReceiver);
            }
        }
    }

    void FixedUpdate()
    {
        CustomTriggerCheck();
    }

    private void CustomTriggerCheck()
    {
        _currentTriggers.RemoveWhere(c => c == null);
        int hitCount = Physics.OverlapCapsuleNonAlloc(
            transform.position + transform.up * capsuleHalfHeight, 
            transform.position - transform.up * capsuleHalfHeight, 
            capsuleCollider.radius, overlapBuffer, layermsk);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = overlapBuffer[i];
            if (!hit.isTrigger)
                continue;

            if (_currentTriggers.Add(hit))
            {
                foreach(ICustomTriggerReceiver receiver in receivers_)
                {
                    receiver?.OnCustomTriggerEnter(hit);
                }
                if(tag != "Enemy" && hit.TryGetComponent<LevelTrigger>(out LevelTrigger levelTrigger))
                {
                    levelTrigger.OnEnter();
                }
            }
            else
            {
                foreach(ICustomTriggerReceiver receiver in receivers_)
                {
                    receiver?.OnCustomTriggerStay(hit);
                }
            }
        }

        _currentTriggers.RemoveWhere(c =>
        {
            if (System.Array.IndexOf(overlapBuffer, c, 0, hitCount) < 0)
            {
                foreach(ICustomTriggerReceiver receiver in receivers_)
                {
                    receiver?.OnCustomTriggerExit(c);
                }
                if(tag != "Enemy" && c.TryGetComponent<LevelTrigger>(out LevelTrigger levelTrigger))
                {
                    levelTrigger.OnExit();
                }
                return true;
            }
            return false;
        });
    }

    
}
