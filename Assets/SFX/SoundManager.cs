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
    DEFLECT,
    DASH_RECHARGE,
    KICK,
    AIR_WHOOSH,
    DOOR_OPEN,
    UI_BUTTON,
    JUMP_PAD,
    FALLING_WIND_START,
    FALLING_WIND_LOOP,
    GRAPPLE_END,
    GRAPPLE_POINT,
    
    CORPSE_EXPLOSION
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

    public static AudioSource PlayLoop(SoundType sound, float volume = 1, float start = 0)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        GameObject tempAudio = new GameObject($"LoopAudio_{sound}");
        tempAudio.transform.position = instance.transform.position;
        AudioSource source = tempAudio.AddComponent<AudioSource>();
        
        source.clip = randomClip;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.loop = true;
        source.playOnAwake = false;
        source.time = Mathf.Clamp(start, 0f, Mathf.Max(0f, randomClip.length - 0.01f));

        source.Play();

        return source;
    }

    public static void StopLoop(AudioSource source, float fadeOut)
    {
        if (source == null)
            return;

        source.Stop();
        Destroy(source.gameObject);
    }

    public static void PlaySound(AudioClip sound, float volume = 1)
    {

        GameObject tempAudio = new GameObject("TempAudio");
        tempAudio.transform.position = instance.transform.position;
        AudioSource source = tempAudio.AddComponent<AudioSource>();

        source.clip = sound;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.playOnAwake = false;

        source.Play();
        Destroy(tempAudio, sound.length);
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
