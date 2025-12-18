using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic instance;

    [SerializeField] private AudioClip[] musicTracks;
    [SerializeField] private float volume = 0.3f;
    
    private AudioSource audioSource;
    private int currentTrackIndex = 0;

    void Awake()
    {
        // Гарантируем только один экземпляр
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Настраиваем AudioSource
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.volume = volume;
            audioSource.loop = false; // Важно для перехода к следующему треку
            
            // Начинаем воспроизведение
            PlayNextTrack();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Когда трек заканчивается, играем следующий
        if (!audioSource.isPlaying && musicTracks.Length > 0)
        {
            PlayNextTrack();
        }
    }

    void PlayNextTrack()
    {
        if (musicTracks.Length == 0) return;
        
        audioSource.clip = musicTracks[currentTrackIndex];
        audioSource.Play();
        
        // Переходим к следующему треку
        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
}