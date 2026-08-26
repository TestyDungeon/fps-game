using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField] private AudioClip[] tracks;
    [SerializeField] private AudioClip currentTrack;
    [SerializeField] private float volume = 1;
    [SerializeField] private float fadeDuration = 1;
    private AudioSource audioSource1;
    private AudioSource audioSource2;
    private bool isCrossfading;

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        audioSource1 = GetComponents<AudioSource>()[0];
        audioSource2 = GetComponents<AudioSource>()[1];
        foreach (var clip in tracks)
            clip.LoadAudioData();

        //currentTrack = tracks[Random.Range(0, tracks.Length)];
        PlayMusic(tracks[Random.Range(0, tracks.Length)], fadeDuration);
    }

    // Update is called once per frame
    void Update()
    {
       //if (audioSource1.isPlaying == false )
       //{
       //    currentTrack = tracks[Random.Range(0, tracks.Length)];
       //    Debug.Log("AUDIO");
       //    audioSource.clip = currentTrack;
       //    audioSource.volume = volume;
       //    audioSource.loop = false;
       //    audioSource.Play();
       //}

        if (isCrossfading || tracks == null || tracks.Length == 0)
            return;

        AudioSource currentSource = audioSource1.isPlaying ? audioSource1 : (audioSource2.isPlaying ? audioSource2 : null);

        if (currentSource == null || currentSource.clip == null)
            return;

        float timeRemaining = currentSource.clip.length - currentSource.time;
        if (timeRemaining <= fadeDuration)
        {
            PlayMusic(tracks[Random.Range(0, tracks.Length)], fadeDuration);
        }
    }


    public void PlayMusic(AudioClip track, float fadeDuration = 1f)
    {
        StartCoroutine(MusicCrossFade(track, fadeDuration));
    }

    private IEnumerator MusicCrossFade(AudioClip nextTrack, float fadeDuration)
    {
        isCrossfading = true;

        AudioSource fromSource;
        AudioSource toSource;

        if (audioSource1.isPlaying)
        {
            fromSource = audioSource1;
            toSource = audioSource2;
        }
        else
        {
            fromSource = audioSource2;
            toSource = audioSource1;
        }

        float percent = 0;

        toSource.clip = nextTrack;
        toSource.volume = 0;
        toSource.Play();

        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            float t = Mathf.Lerp(0f, 1f, percent); // your existing progress
            fromSource.volume = volume * Mathf.Pow(1f - t, 2f); // or use an AnimationCurve
            toSource.volume = volume * Mathf.Pow(t, 2f);
            yield return null;
        }
        
        fromSource.volume = 0;
        fromSource.Stop();

        isCrossfading = false;
    }
}
