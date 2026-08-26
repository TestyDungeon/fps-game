using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    private Light pointLight;
    [SerializeField] private float minIntensity = 0;
    [SerializeField] private float maxIntensity = 1;
    [SerializeField] private float flickerSpeed = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pointLight = GetComponent<Light>();
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        pointLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}
