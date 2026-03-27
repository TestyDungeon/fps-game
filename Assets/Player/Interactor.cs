using UnityEngine;

public class Interactor : MonoBehaviour
{
    private LayerMask layerMask = 1 << 14;
    private float range = 1.5f;
    private Transform origin;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        origin = GetComponentInChildren<Camera>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if(Physics.Raycast(origin.position, origin.forward, out RaycastHit hit, range, layerMask))
            {
                hit.transform.GetComponent<IInteractable>().Interact();
            }
        }
    }
}
