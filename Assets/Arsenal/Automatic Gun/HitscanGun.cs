using UnityEngine;
using System.Collections;

public class HitscanGun : Gun
{
    private int layerMask = ~(1 << 3);


    protected override void Shoot(Vector3 dir)
    {
        float distance = gunConfig.range;
        Vector3 target = transform.position + dir * distance;

        if (Physics.Raycast(cameraPivot.position, dir, out RaycastHit hit, gunConfig.range, layerMask, QueryTriggerInteraction.Ignore))
        {
            target = hit.point;
            distance = hit.distance;
            if (hit.rigidbody)
            {
                hit.rigidbody.AddForceAtPosition(transform.forward * gunConfig.force, hit.point, ForceMode.Impulse);
            }
            if (hit.transform.gameObject.GetComponent<IDamageable>() != null)
            {
                hit.transform.gameObject.GetComponent<IDamageable>().TakeDamage(gunConfig.damage, hit.point, hit.normal);
            }
        }
        
        StartCoroutine(SpawnBulletTrail(target));
    }

    

    
}
