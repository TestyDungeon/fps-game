using System.Collections.Generic;
using UnityEngine;

public class SoundOnParticleCollision : MonoBehaviour
{
    public AudioClip[] sounds;
    public float volume;
    private bool collided = false;
    public List<ParticleCollisionEvent> collisionEvents;
    [HideInInspector] public ParticleSystem part;

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        if(collided)
            return;

        ParticlePhysicsExtensions.GetCollisionEvents(part, other, collisionEvents);

        foreach (var item in collisionEvents)
        {
            collided = true;
            SoundManager.PlaySound(sounds, item.intersection, volume);
            Debug.Log("COLL");
        }
    }
}
