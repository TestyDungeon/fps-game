using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Attack Config", menuName = "AI/Enemy Attack Config")] 
public class AttackPattern : ScriptableObject {
    public float duration = 1f, cooldown = 2f;
    public AttackStep[] steps;               // sorted by time
}
public enum StepType { FaceTarget, Lunge, Shoot, MeleeHit }
[System.Serializable] public struct AttackStep {
    public float time; public StepType type; public float value; public int muzzle;
}