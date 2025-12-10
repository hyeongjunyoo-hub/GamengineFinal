using UnityEngine;

public class KickHitboxController : MonoBehaviour
{
    [Header("발차기 설정")]
    public float kickForce = 30f; 
    public float kickUpwardForce = 8f;

    private PlayerKick myPlayer; // 나 자신(공격자)의 스크립트

    void Start()
    {
        // 내 부모(공격자)가 누구인지 찾기
        myPlayer = GetComponentInParent<PlayerKick>();

        if (myPlayer == null)
        {
            Debug.LogError("부모 오브젝트에서 PlayerKick 스크립트를 찾을 수 없습니다!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!gameObject.activeInHierarchy) return;

        // 1. 공과 충돌했을 때 (기존 코드)
        if (other.CompareTag("Ball"))
        {
            // ... (기존 공 날리기 로직과 동일) ...
            Rigidbody2D ballRb = other.GetComponent<Rigidbody2D>();
            if (ballRb != null)
            {
                float direction = myPlayer.facingDirection;
                Vector2 kickDirection = new Vector2(direction, 0).normalized;
                Vector2 force = (kickDirection * kickForce) + (Vector2.up * kickUpwardForce);
                ballRb.AddForce(force, ForceMode2D.Impulse);
                Debug.Log("공을 찼습니다!");
                myPlayer.PlayKickSoundEffect();
            }
        }
        // 2. 플레이어(상대방)와 충돌했을 때
        else if (other.CompareTag("Player"))
        {
            // 부딪힌 대상에게서 PlayerKick 스크립트를 가져옴
            PlayerKick enemy = other.GetComponent<PlayerKick>();

            // 중요! 부딪힌 사람이 '나 자신'이면 무시
            if (enemy != null && enemy != myPlayer)
            {
                Debug.Log("상대방을 발로 찼습니다!");
                enemy.TakeHit(); // 상대방의 '맞음' 함수 실행
            }
        }
    }
}