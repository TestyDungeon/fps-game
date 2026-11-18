using UnityEngine;

public class Projectile : MonoBehaviour
{
    public ProjectileConfig cfg;
    [SerializeField] private ParticleSystem trailParticles = null;
    Transform owner, target;
    Vector3 velocity;
    Vector3 gravityDir = Vector3.down;   // your old gravityVec, if gravity direction changes in your game
    LayerMask hitMask;
    int bouncesLeft;
    float homing, travelled, age, damageMult = 1f;
    bool dead;
    const float Skin = 0.02f;
    const int MaxSteps = 4;

    public void Launch(Transform owner, Vector3 dir,
                       LayerMask mask, Transform target = null)
    {
        this.owner = owner; this.target = target; hitMask = mask;
        velocity = dir.normalized * cfg.speed;
        bouncesLeft = cfg.bounces;
        homing = cfg.homingTurnRate;
    }

    void Update()
    {
        if (dead) return;
        float dt = Time.deltaTime;
        age += dt;

        velocity += gravityDir * cfg.gravity * dt;

        if (target != null && homing > 0f)
        {
            Vector3 want = (target.position - transform.position).normalized;
            Vector3 dir = Vector3.RotateTowards(velocity.normalized, want, homing * dt, 0f);
            velocity = dir * velocity.magnitude;
            homing = Mathf.Lerp(homing, 0f, cfg.homingDecay * dt);
        }

        float remaining = velocity.magnitude * dt;
        for (int i = 0; i < MaxSteps && remaining > 0.0001f && !dead; i++)
        {
            Vector3 dir = velocity.normalized;
            if (Physics.SphereCast(transform.position, cfg.radius, dir, out RaycastHit hit,
                                   remaining + Skin, hitMask, QueryTriggerInteraction.Collide))
            {
                float travel = Mathf.Max(0f, hit.distance - Skin);   // stop short of the surface
                transform.position += dir * travel;
                remaining -= travel;
                travelled += travel;
                HandleHit(hit);                                      // may change velocity
            }
            else
            {
                transform.position += dir * remaining;
                travelled += remaining;
                remaining = 0f;
            }
        }
        if (travelled >= cfg.maxRange) Detonate();
    }

    void HandleHit(RaycastHit hit)
    {
        var victim = hit.collider.GetComponentInParent<IDamageable>();
        if (victim != null)
        {
            victim.TakeDamage(owner, Mathf.RoundToInt(cfg.damage * damageMult), hit.point, hit.normal);
            Detonate();                      // creatures stop it, walls can bounce it
        }
        else if (bouncesLeft > 0)
        {
            float impact = -Vector3.Dot(velocity, hit.normal);        // speed into the surface
            if (impact < 2)                               // add restSpeed (~2) to the config
                velocity = Vector3.ProjectOnPlane(velocity, hit.normal)
                           * Mathf.Max(0f, 1f - 8f * Time.deltaTime); // slide with a bit of friction
            else
            {
                bouncesLeft--;
                velocity = Vector3.Reflect(velocity, hit.normal) * cfg.bounceSpeedKeep;
            }
        }
        else Detonate();
    }

    void Detonate()
    {
        if (dead) return;                    // fixes your multiple-explosion bug
        dead = true;
        if (cfg.explosionPrefab != null)
        {
            var e = Instantiate(cfg.explosionPrefab, transform.position, Quaternion.identity);
            e.transform.localScale = Vector3.one * cfg.explosionRadius;
        }
        if(trailParticles != null)
        {
            trailParticles.transform.SetParent(null);
            trailParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        Destroy(gameObject);                 // or keep your particle-stop-then-destroy logic here
    }

    public void Deflect(Transform newOwner, Vector3 newDir, float speedMult, float newDamageMult)
    {
        if (!cfg.parryable) return;
        target = owner;                      // fly back at whoever fired it
        owner = newOwner;
        velocity = newDir.normalized * velocity.magnitude * speedMult;
        damageMult = newDamageMult;
    }

    public void SetGravityDir(Vector3 dir)
    {
        gravityDir = dir;
    }
}