using UnityEngine;

public class Breakable : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject content;
    [SerializeField] private AudioClip sound;
    [SerializeField] private float volume = 1;
    [SerializeField] private GameObject particles;
    private Health health;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    public void TakeDamage(Transform source, int damageAmount, Vector3 damagePoint, Vector3 normal)
    {
        if (health != null && health.isAlive)
            health.TakeDamage(damageAmount, damagePoint, normal);
    }

    void OnEnable()
    {
        if (health != null)
            health.OnDeath += OnDeath;
    }

    void OnDisable()
    {
        if (health != null)
            health.OnDeath -= OnDeath;
    }

    private void OnDeath()
    {
        SoundManager.PlaySound(sound, transform.position, volume, 1);

        if(particles != null)
            Instantiate(particles, transform.position, transform.rotation);
        if(content != null)
            Instantiate(content, transform.position, transform.rotation);
        
        Destroy(gameObject);
    }
}
