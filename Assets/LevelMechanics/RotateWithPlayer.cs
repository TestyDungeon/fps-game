using UnityEngine;

public class RotateWithPlayer : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;
    [SerializeField] private bool followZ = true;

    private void Awake()
    {
        if (cameraPivot == null)
        {
            Debug.LogError($"{nameof(RotateWithPlayer)} on {name} requires a camera pivot reference.", this);
        }
    }

    private void LateUpdate()
    {
        if (cameraPivot != null)
        {
            Vector3 currentRotation = transform.eulerAngles;
            Vector3 pivotRotation = cameraPivot.eulerAngles;

            transform.rotation = Quaternion.Euler(
                followX ? pivotRotation.x : currentRotation.x,
                followY ? pivotRotation.y : currentRotation.y,
                followZ ? pivotRotation.z : currentRotation.z);
        }
    }
}
