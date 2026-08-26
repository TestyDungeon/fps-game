using System.Collections.Generic;
using UnityEngine;

public class DecalManager : MonoBehaviour
{
    public static DecalManager Instance { get; private set; }

    [System.Serializable]
    public class DecalPool
    {
        public GameObject decalPrefab;
        public int maxAmount;
        [System.NonSerialized] public Queue<GameObject> decals = new Queue<GameObject>();
    }

    [SerializeField] private DecalPool[] decalPools;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (decalPools == null)
        {
            return;
        }

        foreach (DecalPool decalPool in decalPools)
        {
            if (decalPool == null || decalPool.decalPrefab == null || decalPool.maxAmount <= 0)
            {
                continue;
            }

            decalPool.decals = new Queue<GameObject>(decalPool.maxAmount);

            for (int i = 0; i < decalPool.maxAmount; i++)
            {
                decalPool.decals.Enqueue(Instantiate(decalPool.decalPrefab, new Vector3(500, 500, 500), Quaternion.identity, transform));
            }
        }
        
    }

    public void PositionDecal(int ind, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (decalPools == null || decalPools.Length == 0)
        {
            return;
        }

        DecalPool decalPool = decalPools[0];
        if (decalPool == null || decalPool.decals == null || decalPool.decals.Count == 0)
        {
            return;
        }

        GameObject decal = decalPools[ind].decals.Dequeue();
        decal.transform.position = position;
        decal.transform.rotation = rotation;
        decal.transform.localScale = scale;
        decalPool.decals.Enqueue(decal);
    }
}
