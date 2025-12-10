using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("버튼 클릭 소리")]
    public AudioClip clickSound; 
    private AudioSource audioSource;

    void Awake()
    {
        // 싱글톤(중복 방지) 코드 삭제! 
        // 이제 각 씬마다 있는 SoundManager가 각자 일

        audioSource = GetComponent<AudioSource>();
        
        // 안전장치: 오디오 소스가 없으면 추가
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}