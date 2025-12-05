using UnityEngine;

public class BallSound : MonoBehaviour
{
    [Header("튀기는 소리")]
    public AudioClip bounceSound; // ball_bounce.mp3 연결
    
    private AudioSource audioSource;
    private Rigidbody2D rb;

    void Start()
    {
        // 오디오 소스 컴포넌트 가져오기 (없으면 추가)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 3D 입체 음향 끄기 (2D 게임이니까)
        audioSource.spatialBlend = 0f; 
        audioSource.playOnAwake = false;

        rb = GetComponent<Rigidbody2D>();
    }

    // [핵심] 물리적인 충돌이 일어났을 때 실행됨
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. 소리가 없으면 무시
        if (bounceSound == null) return;

        // 2. 너무 살살 부딪힌 건 소리 안 나게 하기 (옵션)
        // 상대속도가 1.0 이상일 때만 소리 재생 (수치는 조절 가능)
        if (collision.relativeVelocity.magnitude > 1.0f)
        {
            // 3. 충돌 세기에 따라 볼륨 조절 (0.5 ~ 1.0 사이)
            // 세게 부딪히면 크게, 살살 부딪히면 작게
            float volume = Mathf.Clamp(collision.relativeVelocity.magnitude / 10.0f, 0.3f, 1.0f);
            
            // 4. 소리 재생 (PlayOneShot은 소리가 겹쳐도 됨)
            audioSource.PlayOneShot(bounceSound, volume);
        }
    }
}