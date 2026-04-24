using UnityEngine;

[CreateAssetMenu(fileName = "Player Stats Config", menuName = "Player/Player Stats Config")]
public class PlayerStatsConfig : ScriptableObject
{
    //[Header("Movement")]
    public int maxHealth = 100;
    public float speedMultiplier = 1;
    public float airControlMultiplier = 1;
}
