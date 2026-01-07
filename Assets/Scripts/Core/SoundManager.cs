using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource MusicSource;
    public AudioSource SFXSource;

    [Header("Clips")]
    public AudioClip MenuMusic;
    public AudioClip ShellGameMusic;
    public AudioClip CodeDuelMusic;
    public AudioClip WinSound;
    public AudioClip LoseSound;
    public AudioClip ClickSound;
    public AudioClip AlertSound;
    public AudioClip FootstepSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (MusicSource.clip == clip) return;
        MusicSource.clip = clip;
        MusicSource.loop = true;
        MusicSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            SFXSource.PlayOneShot(clip, volume);
        }
    }

    public void StopSFX()
    {
        SFXSource.Stop();
    }

    public void PlayFootsteps()
    {
        if (FootstepSound != null && !SFXSource.isPlaying)
        {
            SFXSource.clip = FootstepSound;
            SFXSource.loop = true;
            SFXSource.Play();
        }
    }

    public void StopFootsteps()
    {
        if (SFXSource.clip == FootstepSound && SFXSource.isPlaying)
        {
            SFXSource.Stop();
            SFXSource.loop = false;
        }
    }

    public void PlayClick() => PlaySFX(ClickSound);
    public void PlayAlert() => PlaySFX(AlertSound);
    public void PlayWin() => PlaySFX(WinSound);
    public void PlayLose() => PlaySFX(LoseSound);
}
