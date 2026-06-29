using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] tracks;
    [SerializeField] private AudioClip currentTrack;
    [SerializeField] private float volume = 1;
    private AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (audioSource.isPlaying == false)
        {
            currentTrack = tracks[Random.Range(0, tracks.Length)];
            Debug.Log("AUDIO");
            audioSource.clip = currentTrack;
            audioSource.volume = volume;
            audioSource.loop = false;
            audioSource.Play();
        }
    }
}
