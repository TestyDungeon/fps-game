using System;
using UnityEngine;

// Sits on the same GameObject as PlayerMovement/Health/Inventory. Its only job
// is being "the current player" — holding references to its parts and telling
// the persistent world (HUD, GameManager, etc.) whenever a new one spawns.
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    public static event Action OnPlayerSpawned;

    public Health Health { get; private set; }
    public Inventory Inventory { get; private set; }
    public PlayerMovement Movement { get; private set; }

    void Awake()
    {
        Instance = this;

        Health = GetComponent<Health>();
        Inventory = GetComponent<Inventory>();
        Movement = GetComponent<PlayerMovement>();

        OnPlayerSpawned?.Invoke();
    }
}
