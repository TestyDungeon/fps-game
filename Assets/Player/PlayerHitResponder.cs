using System;
using UnityEngine;

public class PlayerHitResponder : MonoBehaviour, IDamageable
{
    //[SerializeField] private Transform playerPivot;
    private Health health;
    private GameManager gameManager;
    private Kick melee;

    public static PlayerHitResponder Instance { get; private set; }
    
    void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }


    void Start()
    {
        health = Player.Instance.Health;
        gameManager = FindAnyObjectByType<GameManager>();
        melee = FindAnyObjectByType<Kick>();
    }

    public void TakeDamage(Transform source, int damageAmount, Vector3 damagePoint = default, Vector3 normal = default)
    {
        if (health.GetHealth() <= 0)
            return;
        
        //if(melee.GetBlocking())
        //{
        //    bool isInFront = Vector3.Dot(playerPivot.forward, (damagePoint - transform.position).normalized) > 0;
        //    if(isInFront)
        //    {
        //        if (!melee.GetParrying())
        //            SoundManager.PlaySound(SoundType.SHIELD_BLOCK, 1);
        //        return;
        //    }
        //}

        Player.Instance.CameraRecoil.ApplyRecoil(5, 9, 9);
        SoundManager.PlaySound(SoundType.HURT, 0.3f);
        float armorAmount = health.GetArmor();
        float armorDamage = 0; 
            Debug.Log("Armor: " + health.GetArmor());
        if(armorAmount > 0)
        {
            armorDamage = armorAmount > 50 ? damageAmount / 2 : damageAmount / 3;
            health.SetArmor(health.GetArmor() - armorDamage);
        }

        float healthDamage = damageAmount - armorDamage;
        health.SetHealth((int)((float)health.GetHealth() - healthDamage));

        if (health.GetHealth() <= 0 && health.isAlive)
            Death();
    }

    

    protected void Death()
    {
        health.isAlive = false;
        Debug.Log("Player " + name + " died.");
        gameManager.GameOver();
    }

    public Health GetPlayerHealth()
    {
        return health;
    }
}
