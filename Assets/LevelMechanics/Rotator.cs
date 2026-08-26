using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotationVector;
    [SerializeField] private float speed;

    void Update()
    {
        transform.Rotate(rotationVector * speed * Time.deltaTime);
    }
}
