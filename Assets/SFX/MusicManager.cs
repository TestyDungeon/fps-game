using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private AudioClip[] tracks;
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
            Debug.Log("AUDIO");
            audioSource.clip = currentTrack;
            audioSource.volume = volume;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
