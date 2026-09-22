using System.Collections;
using UnityEngine;

public class EnemyHitResponder : MonoBehaviour, IDamageable
{
    private Health enemyHealth;
    private EnemyStateManager state;
    private float lastFalter;
    private Material originalMaterial;
    private MaterialPropertyBlock _mpb;
    private static readonly int ColorID = Shader.PropertyToID("_BaseColor");
    private Collider hurtBox;
    private Collider hurtBoxDead;
    private bool isCorpse = false;
    private float lastDamage = 0;
    private Vector3 deathKnockbackVector = Vector3.zero;

    void Awake()
    {
        
        lastFalter = -10f;
        _mpb = new MaterialPropertyBlock();
    }
    void Start()
    {
        hurtBox = GetComponent<CapsuleCollider>();
        hurtBoxDead = GetComponent<BoxCollider>();
        enemyHealth = GetComponentInParent<Health>();
        state = GetComponentInParent<EnemyStateManager>();
        //originalMaterial = state.smrs[0].materials[0]; 
        
    }

    void FixedUpdate()
    {
        deathKnockbackVector = Vector3.MoveTowards(deathKnockbackVector, Vector3.zero, Time.fixedDeltaTime * 100);
        //lastDamage = Mathf.MoveTowards(lastDamage, 0, Time.fixedDeltaTime * 200);
        if(enemyHealth.GetArmor() < enemyHealth.GetMaxArmor())
        {
            //Debug.Log("Armor " + enemyHealth.GetArmor());
            enemyHealth.SetArmor(enemyHealth.GetArmor() + Time.fixedDeltaTime * 120f);
        }
    }

    public void TakeDamage(Transform source, int damageAmount, Vector3 damagePoint, Vector3 normal)
    {
        state.lastDamageVector = (damagePoint - source.position).normalized * damageAmount;
        deathKnockbackVector += (transform.position - source.position).normalized * damageAmount;
        //state.SetTarget(source);
        SoundManager.PlaySound(state.enemyConfig.hurtSFX, transform.position, 0.01f, 0.7f);
        //if(state.GetCurrentState() is EnemyFalterState && !state.movementController.GroundCheck())
        //{
//
        //    Debug.Log("AIR JUGGLE");
        //    //state.movementController.addVelocity(transform.up * 0.15f * damageAmount);
        //    //StartCoroutine(state.AirJuggle(transform.up * 0.1f * damageAmount));
        //}
        GameObject particles = Instantiate(GameManager.Instance.bloodParticles, damagePoint, Quaternion.LookRotation(normal));
        Destroy(particles, 2f);
        //SoundManager.PlaySound(SoundType.HURTENEMY, transform.position, 0.5f);
        //int preHealth = enemyHealth.GetHealth();
        //if(state.GetCurrentState() is EnemyAirFalterState)
        //{
        //    state.AirJuggle(damageAmount * 0.2f);
        //    damageAmount *= 2;
        //}

        if((damagePoint.y - transform.position.y) > 0.6)
            enemyHealth.TakeDamage(Mathf.RoundToInt(damageAmount * 1.25f));
        else
            enemyHealth.TakeDamage(damageAmount);

        if(enemyHealth.GetHealth() < -enemyHealth.GetMaxHealth() && !isCorpse)
        {
            isCorpse = true;
            state.SpawnCorpse();
            SoundManager.PlaySound(SoundType.CORPSE_EXPLOSION, state.transform.position, 0.2f, 0.5f);
            Object.Destroy(state.gameObject);
        }
        
        if(hurtBox.enabled == true && enemyHealth.GetHealth() <= 0)
        {
            Vector3 up = Vector3.zero;
            if(state.movementController.GroundCheck() && Vector3.Angle(transform.up, deathKnockbackVector) >= 90)
                up = transform.up * deathKnockbackVector.magnitude * 0.25f;

            state.DeathKnockback((Vector3.ProjectOnPlane(deathKnockbackVector, transform.up).normalized * deathKnockbackVector.magnitude) + up);
            
        }
        //StartCoroutine(WhiteMaterialChange());


        if(Time.time - lastFalter > 1)
            enemyHealth.SetArmor(enemyHealth.GetArmor() - damageAmount * 5f);
        

        //if(enemyHealth.GetHealth() < enemyHealth.GetMaxHealth() * 0.4)
        //{
        //    state.SwitchState(state.StaggerState);
        //}

        


        if(enemyHealth.GetArmor() <= 0)
        {
            if(state.GetCurrentState() is not EnemyStaggerState)
            {
                lastFalter = Time.time;
            enemyHealth.SetArmor(enemyHealth.GetMaxArmor());
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
            GameObject orb = Instantiate(GameManager.Instance.healthOrb, transform.position, Quaternion.identity);
            orb.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.5f, ForceMode.Force);
            Destroy(orb, 8);
            enemyHealth.TakeDamage(enemyHealth.GetMaxHealth());
        }
    }

    private IEnumerator WhiteMaterialChange()
    {
        SetColor(Color.white);
        yield return new WaitForSeconds(10f);
        SetColor(Color.white * 0f); // or restore original via GetPropertyBlock first
        foreach (SkinnedMeshRenderer smr in state.smrs)
        {
            smr.SetPropertyBlock(null, 0);

        }
        
        

        

        //foreach (SkinnedMeshRenderer smr in state.smrs)
        //{
        //    Material[] mats = smr.materials;
        //    mats[0] = GameManager.Instance.whiteMaterial;
        //    smr.materials = mats;
        //}
        //yield return new WaitForSeconds(0.1f);
        //foreach (SkinnedMeshRenderer smr in state.smrs)
        //{
        //    Material[] mats = smr.materials;
        //    mats[0] = originalMaterial;
        //    smr.materials = mats;
        //}
    }
    private void SetColor(Color color)
    {
        foreach (SkinnedMeshRenderer smr in state.smrs)
        {
            smr.GetPropertyBlock(_mpb, 0);
            _mpb.SetColor(ColorID, color);
            smr.SetPropertyBlock(_mpb, 0);
        }

    }

    public IEnumerator SetHurtBoxToDead()
    {
        yield return new WaitForSeconds(0.1f);
        hurtBox.enabled = false;
        hurtBoxDead.enabled = true;

    }
}
