using UnityEngine;
using System;

public enum SoundType
{
    FOOTSTEP,
    JUMP,
    HURT,
    HURTENEMY,
    SMGSHOT,
    ROCKETSHOT,
    ROCKETEXPLODE,
    SHOTGUNSHOT,
    GRAPPLE,
    PICKUP_GRAPPLE,
    PICKUP_GUNS,
    PICKUP_MEDKIT,
    PICKUP_AMMO,
    ENEMY_ATTACK,
    FLYING_ENEMY_ATTACK
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    private static SoundManager instance;
    private AudioSource audioSource;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        instance.audioSource.PlayOneShot(randomClip, volume);
    }

    public static void PlaySound(SoundType sound, Vector3 position, float volume = 1)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        AudioSource.PlayClipAtPoint(randomClip, position, volume);
    }
#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for(int i = 0; i < soundList.Length; i++)
            soundList[i].name = names[i];
    }
#endif
}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}
