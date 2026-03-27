using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyHitResponder : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject bloodParticlesPrefab;
    private Health enemyHealth;
    private EnemyStateManager state;
    private float lastFalter;
    


    void Awake()
    {
        enemyHealth = GetComponent<Health>();
        state = GetComponent<EnemyStateManager>();
        lastFalter = -10f;
    }

    void FixedUpdate()
    {
        if(enemyHealth.GetPosture() < enemyHealth.GetMaxPosture())
        {
            //Debug.Log("Posture " + enemyHealth.GetPosture());
            enemyHealth.SetPosture(enemyHealth.GetPosture() + Time.fixedDeltaTime * 60f);
        }
    }

    public void TakeDamage(Transform source, int damageAmount, Vector3 damagePoint, Vector3 normal)
    {
        state.lastDamageVector = (damagePoint - source.position).normalized * damageAmount * 2f;
        //state.SetTarget(source);
        SoundManager.PlaySound(state.enemyConfig.hurtSFX, transform.position, 0.01f, 0.7f);
        //if(state.GetCurrentState() is EnemyFalterState && !state.movementController.GroundCheck())
        //{
//
        //    Debug.Log("Current state " + state.GetCurrentState());
        //    StartCoroutine(state.AirJuggle(transform.up * 0.1f * damageAmount));
        //}
        GameObject particles = Instantiate(bloodParticlesPrefab, damagePoint, Quaternion.LookRotation(normal));
        Destroy(particles, 2f);
        //SoundManager.PlaySound(SoundType.HURTENEMY, transform.position, 0.5f);
        int preHealth = enemyHealth.GetHealth();
        if(state.GetCurrentState() is EnemyAirFalterState)
        {
            state.AirJuggle(damageAmount * 0.2f);
            damageAmount *= 2;
        }

        if((damagePoint.y - transform.position.y) > 0.6)
            enemyHealth.TakeDamage(Mathf.RoundToInt(damageAmount * 1.25f));
        else
            enemyHealth.TakeDamage(damageAmount);

        //if(enemyHealth.GetHealth() <= 0)
        //    Destroy(gameObject);

        if(Time.time - lastFalter > 4)
            enemyHealth.SetPosture(enemyHealth.GetPosture() - damageAmount * 2);
        
        //if(damageAmount >= enemyHealth.GetMaxHealth() * 0.33f ||
        //enemyHealth.GetMaxHealth() * 0.33f - (preHealth % (enemyHealth.GetMaxHealth() * 0.33f)) < damageAmount)
        //{
        //    state.SwitchState(state.FalterState);
        //    
        //}

        //if(enemyHealth.GetHealth() < enemyHealth.GetMaxHealth() * 0.4)
        //{
        //    state.SwitchState(state.StaggerState);
        //}

        


        if(enemyHealth.GetPosture() <= 0)
        {
            if(state.GetCurrentState() is not EnemyStaggerState)
            {
                lastFalter = Time.time;
                state.SwitchState(state.FalterState);
            }
        }

        if(state.GetCurrentState() is EnemyIdleState)
            state.SwitchState(state.ChaseState);
    }

    

    public void CheckStaggerKill()
    {
        if(state.GetCurrentState() is EnemyStaggerState)
        {
            GameObject orb = Instantiate(state.enemyConfig.healthOrb, transform.position, Quaternion.identity);
            orb.GetComponent<Rigidbody>().AddForce(Vector3.up * 1, ForceMode.Force);
            Destroy(orb, 8);
            enemyHealth.TakeDamage(enemyHealth.GetMaxHealth());
        }
    }
}
