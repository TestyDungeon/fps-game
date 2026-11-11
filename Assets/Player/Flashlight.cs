using UnityEngine;

public class Flashlight : MonoBehaviour
{
    private Light flashLight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        flashLight = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            flashLight.enabled = !flashLight.enabled;
        }
    }
}
