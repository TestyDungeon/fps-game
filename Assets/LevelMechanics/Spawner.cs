using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private float spawnCD = 5;
    [SerializeField] private GameObject gameObjectToSpawn;
    private float lastTime = -1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.time - lastTime > spawnCD)
        {
            lastTime = Time.time;
            Object.Instantiate(gameObjectToSpawn, transform.position, Quaternion.identity);
        }
    }
}
