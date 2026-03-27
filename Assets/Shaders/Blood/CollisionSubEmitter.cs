using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class ParticleDetectCollision : MonoBehaviour
{
    public GameObject DecalPrefab;
    public ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;

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
        //We give negative normal as forward, so the decal is rotated like we wanted
        Quaternion qua = Quaternion.LookRotation(-item.normal, Vector3.up);

        GameObject spawnedDecal = Instantiate(DecalPrefab, item.intersection, qua);
        spawnedDecal.transform.localScale *= Random.Range(1f, 2f);


    }

    void DisplayCollisionPoint(ParticleCollisionEvent item)
    {
        Debug.DrawRay(item.intersection, Vector3.up, Color.green, 1);
    }
}