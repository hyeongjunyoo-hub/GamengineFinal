using UnityEngine;

public class KickHitboxController : MonoBehaviour
{
    [Header("발차기 설정")]
    public float kickForce = 30f; // 공을 찰 때의 힘
    public float kickUpwardForce = 8f; // 공을 위로 띄우는 힘

    private PlayerKick playerKick; // 플레이어 스크립트 참조

    void Start()
    {
        // 이 스크립트는 kickHitbox에 있으므로, 부모 오브젝트(플레이어)에서 PlayerKick 스크립트를 찾아옵니다.
        playerKick = GetComponentInParent<PlayerKick>();

        if (playerKick == null)
        {
            Debug.LogError("부모 오브젝트에서 PlayerKick 스크립트를 찾을 수 없습니다!");
        }
    }

    // 이 Hitbox가 다른 Collider와 만났을 때 호출됩니다. (Is Trigger가 켜져 있어야 함)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 이 히트박스가 활성화 상태이고, 부딪힌 오브젝트가 "Ball" 태그를 가지고 있는지 확인합니다.
        // gameObject.activeInHierarchy는 이 게임오브젝트가 씬에서 활성화 상태일 때만 true가 됩니다.
        if (gameObject.activeInHierarchy && other.CompareTag("Ball"))
        {
            Debug.Log("공과 충돌!");

            // 2. 부딪힌 공의 Rigidbody2D 컴포넌트를 가져옵니다.
            Rigidbody2D ballRb = other.GetComponent<Rigidbody2D>();
            if (ballRb != null)
            {
                // 3. PlayerKick 스크립트에서 플레이어가 바라보는 방향을 가져옵니다.
                float direction = playerKick.facingDirection;

                // 4. 공을 날려보낼 방향과 힘을 계산합니다.
                // 수평 방향(direction)과 약간의 수직 방향(kickUpwardForce)을 더합니다.
                Vector2 kickDirection = new Vector2(direction, 0).normalized;
                Vector2 force = (kickDirection * kickForce) + (Vector2.up * kickUpwardForce);

                // 5. 공에 순간적인 힘을 가합니다.
                ballRb.AddForce(force, ForceMode2D.Impulse);
            }
        }
    }
}
