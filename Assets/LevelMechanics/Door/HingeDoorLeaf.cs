using UnityEngine;

public class HingeDoorLeaf : DoorLeaf
{
    [SerializeField] float openAngle = 90f;
    Quaternion closedRot;
    void Awake()
    {
        closedRot = transform.localRotation;
    }
    
    public override void SetProgress(float t)
    {
        transform.localRotation = closedRot * Quaternion.Euler(0f, openAngle * t, 0f);
    }
}
