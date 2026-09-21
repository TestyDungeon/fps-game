using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Sits on the same GameObject as PlayerMovement/Health/Inventory. Its only job
// is being "the current player" — holding references to its parts and telling
// the persistent world (HUD, GameManager, etc.) whenever a new one spawns.
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    public static event Action OnPlayerSpawned;

    public Health Health { get; private set; }
    public Inventory Inventory { get; private set; }
    public PlayerMovement PlayerMovement { get; private set; }
    public MovementController MovementController { get; private set; }

    public CameraRecoil CameraRecoil { get; private set; }

    void Awake()
    {
        Instance = this;

        Health = GetComponent<Health>();
        Inventory = GetComponent<Inventory>();
        PlayerMovement = GetComponent<PlayerMovement>();
        MovementController = GetComponent<MovementController>();
        CameraRecoil = GetComponentInChildren<CameraRecoil>();
        OnPlayerSpawned?.Invoke();
    }

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Health.HealthChanged();
        Health.ArmorChanged();
    }
}
