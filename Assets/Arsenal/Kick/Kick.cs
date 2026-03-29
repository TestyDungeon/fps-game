using System;
using System.Collections;
using UnityEngine;
using Animancer;
using GravityGUN.Data;

public class Kick : Item, IAmmoHandler
{
    public event Action<int, int> OnAmmoChanged;
    [SerializeField] private int damage = 10;
    [SerializeField] private float meleeRechargeCooldown = 12f;
    [SerializeField] private float meleeCooldown = 0.25f;
    private bool canMelee = true;

    private BoxCollider shieldCollider;
    private Vector3 shieldSize;
    private BoxCollider swordCollider;
    private Vector3 swordSize;

    private AnimancerComponent animancer;
    [SerializeField] private ClipTransition attackAnimation;
    [SerializeField] private ClipTransition blockAnimation;

    private MovementController mc;
    private PlayerMovement pm;
    Vector3 targetPos = new Vector3(0.5f, 0, -0.18f);
    private int layerMask = 1 << 6;
    private int swordLayerMask = (1 << 8);
    private bool blocking = false;
    private bool canParry = false;
    private bool parried = false;

    
    private Transform transform_;

    void Start()
    {
        
        animancer = GetComponentInChildren<AnimancerComponent>();
        pm = player.GetComponent<PlayerMovement>();
        mc = player.GetComponent<MovementController>();
        transform_ = transform.parent;
        shieldCollider = transform.parent.GetComponentInParent<BoxCollider>();
        Debug.Log(shieldCollider.name);
        swordCollider = shieldCollider.transform.parent.GetComponent<BoxCollider>();
        Debug.Log(swordCollider.name);
        shieldSize = Vector3.Scale(shieldCollider.size, shieldCollider.transform.lossyScale);
        swordSize = Vector3.Scale(swordCollider.size, swordCollider.transform.lossyScale);
        startPos = transform.localPosition;
        inventory = FindAnyObjectByType<Inventory>();
        
    }

    

    void Update()
    {
        bool stagger = CheckStagger();

        if (Input.GetKeyDown(KeyCode.E) && ((canMelee && inventory.GetAmmo(LootType.MeleeAmmo) > 0) || stagger))
        {
            canMelee = false;
            StartCoroutine(ExecuteAttack(stagger));
            StartCoroutine(AttackReset());
            
            if(!IsInvoking("RechargeMelee"))
                Invoke("RechargeMelee", meleeRechargeCooldown);
            
            IEnumerator AttackReset()
            {
                yield return new WaitForSeconds(meleeCooldown);
                canMelee = true;
            }
        }

        /*
        if (Input.GetButtonDown("Fire2"))
        {
            shieldCollider.enabled = true;
            blocking = true;
            inventory.GetCurrent().gameObject.SetActive(false); 
            IEnumerator Parry()
            {
                canParry = true;
                yield return new WaitForSeconds(0.1f);
                canParry = false;
            }
            
            StartCoroutine(Parry());
            
            Debug.Log("SHIELD");
            var state = animancer.Play(blockAnimation, 0.1f, FadeMode.FromStart);
            state.Speed = Mathf.Abs(state.Speed);
            state.NormalizedTime = 0;
            //StartCoroutine(MoveShield());
            
            
        }
        if (Input.GetButtonUp("Fire2"))
        {
            inventory.GetCurrent().gameObject.SetActive(true); 
            shieldCollider.enabled = false;
            blocking = false;
            parried = false;

            var state = animancer.Play(blockAnimation, 0.1f, FadeMode.FromStart);
            state.Speed = -Mathf.Abs(state.Speed);
            state.NormalizedTime = 0.2f;
            Debug.Log("SHIELD");
            //StartCoroutine(MoveShieldOut());
            
        }
        */
    }

    void FixedUpdate()
    {
        //Time.timeScale = Mathf.MoveTowards(Time.timeScale, 1, Time.fixedUnscaledDeltaTime * 0.01f);
        if (blocking && canParry)
        {
            

            Collider[] overlap = Physics.OverlapBox(transform.TransformPoint(shieldCollider.center), shieldSize/2, Quaternion.identity, layerMask, QueryTriggerInteraction.Ignore);
            if(overlap.Length > 0)
            {
                if (!mc.GroundCheck() && !parried)
                {
                    
                    parried = true;
                    mc.resetVerticalVelocity();
                    mc.addVelocity(player.transform.up * 2);

                    pm.AddJump(1);
                }
                foreach(Collider col in overlap)
                {
                    Time.timeScale = 0.5f;
                    StartCoroutine(TimeReset());
                    
                    col.gameObject.GetComponent<EnemyProjectile>()?.Deflected(player.transform.position);
                    
                }   

            }
        }

    }
    


    private IEnumerator ExecuteAttack(bool stagger = false)
    {
        var state = animancer.Play(attackAnimation, 0.1f, FadeMode.FromStart);
        state.Speed = 1;
        state.NormalizedTime = 0;
        SoundManager.PlaySound(SoundType.AIR_WHOOSH, 1);
        bool groundCheckPreDash;
        
        Collider[] cols;
        

        
        Vector3 worldCenter = swordCollider.transform.TransformPoint(swordCollider.center);
        cols = Physics.OverlapBox(worldCenter, swordSize / 2, Quaternion.identity, swordLayerMask);
        

        if(cols.Length > 0)
        {
            groundCheckPreDash = mc.GroundCheck();
            //mc.Dash(!groundCheckPreDash ? transform.forward : Vector3.ProjectOnPlane(transform.forward, player.transform.up).normalized, Mathf.Clamp((cols[0].transform.position - transform.position).magnitude - 0.5f, 0, 100), 30, 0);
            if (!stagger)
            {
                inventory.ConsumeAmmo(LootType.MeleeAmmo, 1);
            }
            yield return new WaitForSeconds(0.15f);
            SoundManager.PlaySound(SoundType.KICK, 0.25f);
            foreach(Collider col in cols)
            {
                if(col.tag == "Enemy")
                {
                    EnemyStateManager esm = col.GetComponent<EnemyStateManager>();
                    
                    
                    Time.timeScale = 0.5f;
                    StartCoroutine(TimeReset());
                    EnemyHitResponder ehr = col.GetComponent<EnemyHitResponder>();
                    ehr.TakeDamage(player.transform, damage, col.transform.position, col.transform.up);
                    ehr.CheckStaggerKill();
                    esm.SwitchState(esm.FalterState);
                    
                    Destroy(Instantiate(GameManager.Instance.ammoOrb, col.transform.position, Quaternion.identity), 8);
                }
                if(col.tag == "Bullet")
                {
                    Debug.Log("Deflect");
                    float timeScale = 0.1f;
                    EnemyProjectile proj = col.GetComponent<EnemyProjectile>();
                    proj.Deflected(transform.position, transform.forward);
                    if (!mc.GroundCheck())
                    {
                        mc.resetVerticalVelocity();
                        mc.addVelocity(player.transform.up * 5);
                        
                        pm.AddJump(1);
                    }
                    yield return new WaitForSeconds(0.15f * timeScale);
                }
            }
            mc.resetVerticalVelocity();
            mc.addVelocity(!groundCheckPreDash ? -transform.forward * 10 : -Vector3.ProjectOnPlane(transform.forward, player.transform.up).normalized * 5);
        }
        yield return new WaitForSeconds(0.1f);
    }

    private bool CheckStagger()
    {
        Vector3 worldCenter = swordCollider.transform.TransformPoint(swordCollider.center);
        Collider[] cols = Physics.OverlapBox(worldCenter, swordSize / 2);
        if(cols.Length > 0)
        {
            foreach(Collider col in cols)
            {
                if(col.tag == "Enemy")
                {
                    EnemyStateManager esm = col.GetComponent<EnemyStateManager>();
                    if(esm.GetCurrentState() == esm.StaggerState)
                        return true;
                }
            }
        }
        return false;
    }

    private void RechargeMelee()
    {   
        if(inventory.GetAmmo(LootType.MeleeAmmo) < inventory.GetMaxAmmo(LootType.MeleeAmmo))
        {
            inventory.AddAmmo(LootType.MeleeAmmo, 1);
        }
        if(inventory.GetAmmo(LootType.MeleeAmmo) < inventory.GetMaxAmmo(LootType.MeleeAmmo))
        {
            if(!IsInvoking("RechargeMelee"))
                Invoke("RechargeMelee", meleeRechargeCooldown);
        }
    }

    public bool GetBlocking()
    {
        return blocking;
    }

    public bool GetParrying()
    {
        return canParry;
    }

    public int GetAmmo()
    {
        if(inventory == null)
            Debug.Log("INV IS NULL");
        return inventory != null ? inventory.GetAmmo(LootType.MeleeAmmo) : 0;
    }

    public LootType GetAmmoType()
    {
        return LootType.MeleeAmmo;
    }


    public void AmmoChanged(int maxAmmo, int amount)
    {
        OnAmmoChanged?.Invoke(maxAmmo, amount);
    }

    IEnumerator TimeReset()
    {
        while(Time.timeScale != 1)
        {
            Time.timeScale = Mathf.MoveTowards(Time.timeScale, 1, Time.fixedUnscaledDeltaTime * 1f);
            yield return new WaitForFixedUpdate();
        }
    }
}
