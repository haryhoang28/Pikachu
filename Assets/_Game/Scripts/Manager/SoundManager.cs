using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource _backgroundAudioSource;
    [SerializeField] private AudioSource _connectEffectAudioSource;
    
    [SerializeField] private AudioClip _backgroundAudioClip;
    [SerializeField] private AudioClip _connectEffectAudioClip;
    public void PlayBackgroundMusic()
    {
        _backgroundAudioSource.clip = _backgroundAudioClip; 
        if (!_backgroundAudioSource.isPlaying)
        {
            _backgroundAudioSource.Play();
        }
    }
}
