using UnityEngine;

public class SlideDoorLeaf : DoorLeaf
{
    [SerializeField] Vector3 openOffset;
    Vector3 closedPos;
    void Awake()
    {
        closedPos = transform.localPosition;
    }
    public override void SetProgress(float t)
    {
        transform.localPosition = closedPos + openOffset * t;
    }
}
