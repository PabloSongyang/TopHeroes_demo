using UnityEngine;

[System.Serializable]
public class SoundInfo
{
    public string Label => this.m_Label;
    public AudioClip AudioClip => this.m_AudioClip;

    [SerializeField]
    private string m_Label;
    [SerializeField]
    private AudioClip m_AudioClip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField]
    private AudioSource m_EffectAudio;
    [SerializeField]
    private AudioSource m_BGMAudio;

    [SerializeField]
    private AudioClip m_BGM;
    private void Awake()
    {
        Instance = this;
    }

    public void PlaySound(AudioClip audioClip)
    {
        this.m_EffectAudio.PlayOneShot(audioClip);
    }

    public void PlaySound(SoundInfo soundInfo)
    {
        if (soundInfo == null)
            return;

        if (soundInfo.AudioClip == null)
            return;

        this.m_EffectAudio.PlayOneShot(soundInfo.AudioClip);
    }

    public void PlayBGM()
    {
        this.m_BGMAudio.clip = m_BGM;
        this.m_BGMAudio.Play();
    }

    public void StopBGM()
    {
        this.m_BGMAudio.Stop();
    }
}