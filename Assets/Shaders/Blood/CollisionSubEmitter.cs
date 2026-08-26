using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class ParticleDetectCollision : MonoBehaviour
{
    public GameObject DecalPrefab;
    [HideInInspector] public ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;
    public int decalIndex = 0;

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        ParticlePhysicsExtensions.GetCollisionEvents(part, other, collisionEvents);

        foreach (var item in collisionEvents)
        {
            SpawnDecal(item);
        }

    }

    void SpawnDecal(ParticleCollisionEvent item)
    {
        Quaternion qua = Quaternion.LookRotation(-item.normal, Vector3.up);
        DecalManager.Instance.PositionDecal(decalIndex, item.intersection, qua, Vector3.one * Random.Range(1f, 2f));

    }

    void DisplayCollisionPoint(ParticleCollisionEvent item)
    {
        Debug.DrawRay(item.intersection, Vector3.up, Color.green, 1);
    }
}