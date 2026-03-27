using UnityEngine;
using System;

public enum SoundType
{
    FOOTSTEP,
    JUMP,
    HURT,
    ENEMY_SPAWN,
    LANDING,
    FALLING,
    ROCKETEXPLODE,
    PICKUP_KEY,
    GRAPPLE,
    PICKUP_GRAPPLE,
    PICKUP_GUNS,
    PICKUP_MEDKIT,
    PICKUP_AMMO,
    SHIELD_BLOCK,
    DASH,
    REVOLVERSHOT,
    DASH_RECHARGE,
    KICK,
    AIR_WHOOSH,
    RIFLE_SHOT,
    MELEE_ENEMY_ATTACK,
    SUPER_SHOTGUN_SHOT,
    DEFLECT,
    SHOTGUN_PUMP,
    GREATSWORD_SWOOSH,
    DOOR_OPEN
}

[System.Serializable]
public struct Sounds
{
    [SerializeField] private AudioClip[] sounds;
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

    public static void PlaySound(AudioClip sound, float volume = 1)
    {
        instance.audioSource.PlayOneShot(sound, volume);
    }

    public static void PlaySound(AudioClip[] sounds, float volume = 1)
    {
        if(sounds.Length <= 0)
            return;
        AudioClip[] clips = sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        instance.audioSource.PlayOneShot(randomClip, volume);
    }

    public static void PlaySound(SoundType sound, Vector3 position, float volume = 1, float spatialBlend = 1f)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        //AudioSource.PlayClipAtPoint(randomClip, position, volume);

        GameObject tempAudio = new GameObject("TempAudio");
        tempAudio.transform.position = position;
        AudioSource source = tempAudio.AddComponent<AudioSource>();

        source.clip = randomClip;
        source.volume = volume;
        source.spatialBlend = spatialBlend; // fully 3D
        source.minDistance = 3f;
        source.maxDistance = 50f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;

        source.Play();
        Destroy(tempAudio, randomClip.length);
    }

    public static void PlaySound(AudioClip sound, Vector3 position, float volume = 1, float spatialBlend = 1f)
    {
        GameObject tempAudio = new GameObject("TempAudio");
        tempAudio.transform.position = position;
        AudioSource source = tempAudio.AddComponent<AudioSource>();

        source.clip = sound;
        source.volume = volume;
        source.spatialBlend = spatialBlend; // fully 3D
        source.minDistance = 3f;
        source.maxDistance = 50f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;

        source.Play();
        Destroy(tempAudio, sound.length);
    }

    public static void PlaySound(AudioClip[] sounds, Vector3 position, float volume = 1, float spatialBlend = 1f)
    {
        if(sounds.Length <= 0)
            return;
        AudioClip[] clips = sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        //AudioSource.PlayClipAtPoint(randomClip, position, volume);

        GameObject tempAudio = new GameObject("TempAudio");
        tempAudio.transform.position = position;
        AudioSource source = tempAudio.AddComponent<AudioSource>();

        source.clip = randomClip;
        source.volume = volume;
        source.spatialBlend = spatialBlend; // fully 3D
        source.minDistance = 3f;
        source.maxDistance = 50f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;

        source.Play();
        Destroy(tempAudio, randomClip.length);
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
