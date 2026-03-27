using UnityEngine;

[CreateAssetMenu(fileName = "Player Config", menuName = "Player/Player Config")]
public class PlayerConfig : ScriptableObject
{
    [Header("Movement")]
    public float MAX_SPEED = 30f;
    public float speed = 10f;
    public float accel = 11f;
    public float airMaxSpeed = 2f;
    public float airAccel = 11f;
    public float friction = 7f;
    public float stopSpeed = 0.1f;
    public int jumpsAmount = 1;
    public float jumpStrength = 10f;
    public int dashAmount = 2;
    public float dashCooldown = 0.8f;
}
