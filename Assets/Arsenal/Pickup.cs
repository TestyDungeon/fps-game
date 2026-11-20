using UnityEngine;

[DisallowMultipleComponent]
public class Pickup : MonoBehaviour
{
    [Header("Visual")]
    public float rotationSpeed = 0;
    public float amplitude = 0;
    public float frequency = 0;

    [Header("Audio")]
    public bool playSound = false;
    public SoundType soundType;
    public float volume;

    [Header("Magnet")]
    public bool magnetic;          // pulled toward the player while they can use it
    bool collected;

    Transform visual;
    [SerializeField] PickupEffectData[] effects;

    void Update()
    {
        if (visual == null) return;
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        visual.localPosition = new Vector3(0, Mathf.Sin(Time.time * frequency) * amplitude, 0);
    }


    protected virtual void Awake()
    {
        if (transform.childCount > 0) visual = transform.GetChild(0);
    }

    public bool IsAttractedTo(PickupCollector c)
    {
        if (!magnetic) return false;
        if (effects == null) return false;
        foreach (var e in effects)
            if (e.CanCollect(c)) return true;     // pulled if ANY part is useful
        return false;
    }

    public void Collect(PickupCollector c)
    {
        if (collected) return;                    // Destroy is deferred; stops a double apply in one frame
        bool tookAny = false;
        if (effects == null) return;
        foreach (var e in effects)
        {
            if (!e.CanCollect(c)) continue;
            e.Apply(c);
            tookAny = true;
        }
        if (!tookAny) return;

        collected = true;
        if (playSound) SoundManager.PlaySound(soundType, volume);
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (effects == null || effects.Length == 0)
            Debug.LogWarning("Pickup has no PickupEffect and can never be collected", this);
    }
#endif
}