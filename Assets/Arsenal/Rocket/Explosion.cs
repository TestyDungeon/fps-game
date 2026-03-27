using UnityEngine;

public class Explosion : MonoBehaviour
{
    public GunConfig gunConfig;

    void Awake()
    {
        Destroy(gameObject, 2);
    }
    void Start()
    {

        var surrounding_objects = Physics.OverlapSphere(transform.position, gunConfig.explosionRadius, ~0, QueryTriggerInteraction.Ignore);
        SoundManager.PlaySound(SoundType.ROCKETEXPLODE, transform.position, 0.1f);
        foreach (var obj in surrounding_objects)
        {
            MovementController mc = obj.GetComponent<MovementController>();
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            //if (mc != null)
            //{
            //    mc.addVelocity(explosionForce * (obj.transform.position - transform.position).normalized);
            //}
            IDamageable victim = obj.GetComponent<IDamageable>();
            float dist = (transform.position - obj.transform.position).magnitude;
            if (victim != null && dist <= gunConfig.explosionRadius)
            {
                if(obj.tag == "Player")
                    victim.TakeDamage(PlayerHitResponder.Instance.transform, Mathf.FloorToInt(CalculateDamage(gunConfig.explosionRadius, dist) * 0.3f));
                else
                    victim.TakeDamage(PlayerHitResponder.Instance.transform, CalculateDamage(gunConfig.explosionRadius, dist));
            }
            //if (rb != null)
            //    rb.AddExplosionForce(explosionForceRB, transform.position, explosionRadius, 3, ForceMode.Impulse);
        }
    }

    private int CalculateDamage(float radius, float distance)
    {
        //float logBase = Mathf.Log(radius + 1);
        //float logDist = Mathf.Log(distance + 1);
        float actualDamage = ((radius - distance)/radius) * 1;
        if(actualDamage > 0.8)
            actualDamage = gunConfig.damage;
        if(actualDamage <= 0.8)
            actualDamage = gunConfig.damage * 0.2f;
        Debug.Log("Actual Damage: " + actualDamage);
        if(actualDamage < 0)
            actualDamage = 0;
        return gunConfig.damage;
        //return Mathf.FloorToInt(actualDamage);
    }
}
