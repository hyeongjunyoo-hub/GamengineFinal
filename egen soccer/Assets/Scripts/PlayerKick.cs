using UnityEngine;
using System.Collections;

public class PlayerKick : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 100.0f;

    [Header("발차기 설정")]
    public GameObject kickHitbox;   // 아까 만든 발차기 공격 범위 오브젝트 (KickHitbox)
    public Animator anim;           // 캐릭터 애니메이터
    public float activeTime = 0.2f; // 공격 판정이 켜져있는 시간 (초)
    public float startDelay = 10f; // 발을 뻗기까지 걸리는 선딜레이 (필요하면 사용)

    // 컴포넌트 참조
    private Rigidbody2D rb;

    // 플레이어가 바라보는 방향 (1: 오른쪽, -1: 왼쪽)
    public float facingDirection { get; private set; } = 1f;

    void Start()
    {
        // 이동 및 스프라이트 제어를 위해 컴포넌트를 가져옵니다.
        rb = GetComponent<Rigidbody2D>();

        // 게임 시작 시 히트박스가 켜져있을 경우를 대비해 비활성화합니다.
        if (kickHitbox != null)
        {
            kickHitbox.SetActive(false);
        }
    }
    
    void Update()
    {
        // --- 이동 처리 ---
        float moveX = 0f;

        // D키를 누르면 오른쪽으로 이동
        if (Input.GetKey(KeyCode.D))
        {
            moveX = 1f;
        }
        // A키를 누르면 왼쪽으로 이동
        else if (Input.GetKey(KeyCode.A))
        {
            moveX = -1f;
        }

        // 계산된 이동 방향으로 캐릭터의 속도를 설정합니다.
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
          // 속도 계산: 이동 중이면 moveSpeed, 아니면 0
        float currentSpeed = Mathf.Abs(rb.linearVelocity.x);

        // --- 발차기 처리 ---
        // 예: S키를 누르면 발차기
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(KickProcess());
        }
    }

    // 발차기 과정을 처리하는 코루틴 함수
    IEnumerator KickProcess()
    {
        // 1. 발차기 애니메이션 실행
        anim.SetTrigger("Kick");

        // 2. 발이 뻗어질 때까지 아주 살짝 기다립니다 (선딜레이).
        yield return new WaitForSeconds(startDelay);

        // 3. 공격 판정(Hitbox)을 켭니다.
        if (kickHitbox != null) kickHitbox.SetActive(true);

        // 4. 공격 판정이 유효한 시간만큼 기다립니다.
        yield return new WaitForSeconds(activeTime);

        // 5. 공격 판정(Hitbox)을 다시 끕니다.
        if (kickHitbox != null) kickHitbox.SetActive(false);
    }
}