using UnityEngine;
using GravityGUN.Data;

public class Pickup : MonoBehaviour
{
    [Header("Loot")]
    public LootType lootType;
    public int amount;
    public GameObject item = null;
    [Header("Visual")]
    public float rotationSpeed = 0;

    public float amplitude = 0;
    public float frequency = 0;

    private Transform transform_;
    void Awake()
    {
        if(transform.childCount > 0)
            transform_ = transform.GetChild(0).transform;
        
    }

    void Update()
    {
        if(transform_ != null)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            float y = Mathf.Sin(Time.time * frequency) * amplitude;
            transform_.localPosition = new Vector3(0, y, 0);
        }
    }
    public void Collect(PickupCollector collector)
    {
        if(item != null)
        {
            if (collector.ApplyCollected(item))
            {
                Destroy(gameObject);
                
            }
        }
        else if(collector.ApplyCollected(lootType, amount))
            Destroy(gameObject);
    }

}
