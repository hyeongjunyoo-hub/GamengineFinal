using UnityEngine;
using System.Collections;

public class PlayerKick : MonoBehaviour
{
    [Header("🎮 조작키 설정 (Inspector에서 설정하세요)")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.W;
    public KeyCode kickKey = KeyCode.S;
    // public KeyCode skillKey = KeyCode.LeftShift; // 나중에 스킬 구현 시 사용

    [Header("캐릭터 방향 설정")]
    [Tooltip("오른쪽을 보면 1, 왼쪽을 보면 -1")]
    public float facingDirection = 1f; 

    [Header("이동 설정")]
    public float moveSpeed = 20.0f;
    public float jumpForce = 18.0f;

    [Header("점프 판정 설정")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask whatIsGround;
    private bool isGrounded;

    [Header("발차기 설정")]
    public GameObject kickHitbox;
    public Animator anim;
    public float activeTime = 0.15f;
    public float startDelay = 0.3f;

    [Header("😵 스턴(기절) 설정")] // [추가됨]
    public int maxHitCount = 4; // 몇 대 맞으면 기절할지
    public float stunDuration = 3.0f; // 기절 지속 시간
    private int currentHitCount = 0; // 현재 맞은 횟수
    private bool isStunned = false; // 지금 기절 상태인가?
    private SpriteRenderer spriteRenderer; // 색깔 변화용

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (kickHitbox != null) kickHitbox.SetActive(false);

        // 시작할 때 방향에 맞춰서 캐릭터 뒤집기 (P2는 왼쪽을 봐야 함)
        // X축 스케일을 facingDirection에 맞춥니다.
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * facingDirection, transform.localScale.y, transform.localScale.z);
    }

    void Update()
    {
        if(isStunned)
        {
            // 혹시라도 밀리는 힘이 남아있을까봐 확실하게 0으로 고정 (선택 사항)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); 
            return;
        }
        
        // 1. 땅 감지
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        // 2. 이동 처리 (입력 키 변수 사용)
        float moveX = 0f;

        if (Input.GetKey(rightKey)) // 오른쪽 키 누름
        {
            moveX = 1f;
        }
        else if (Input.GetKey(leftKey)) // 왼쪽 키 누름
        {
            moveX = -1f;
        }

        // 플레이어 2(왼쪽 보는 애)는 좌우 키 입력에 따라 이동 방향이 반대가 되지 않도록
        // moveX 값 자체는 월드 좌표계 기준(오른쪽+, 왼쪽-)으로 적용합니다.
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
        
        // 3. 점프 처리 (점프 키 변수 사용)
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 4. 발차기 처리 (킥 키 변수 사용)
        if (Input.GetKeyDown(kickKey))
        {
            StartCoroutine(KickProcess());
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }

    IEnumerator KickProcess()
    {
        anim.SetTrigger("Kick");
        yield return new WaitForSeconds(startDelay);
        if (kickHitbox != null) kickHitbox.SetActive(true);
        yield return new WaitForSeconds(activeTime);
        if (kickHitbox != null) kickHitbox.SetActive(false);
    }

    // [추가됨] 외부(히트박스)에서 이 함수를 호출해서 때립니다.
    public void TakeHit()
    {
        if (isStunned) return; // 이미 기절했으면 더 안 맞음

        currentHitCount++;
        Debug.Log($"으악! 맞았다! ({currentHitCount}/{maxHitCount})");

        // 시각적 효과 (잠깐 빨개짐)
        StartCoroutine(HitColorEffect());

        // 4대 맞았으면 기절!
        if (currentHitCount >= maxHitCount)
        {
            StartCoroutine(StunRoutine());
        }
    }
    // [추가됨] 기절 처리 코루틴
    IEnumerator StunRoutine()
    {
        isStunned = true;
        currentHitCount = 0; // 카운트 초기화
        Debug.Log("😵 기절 상태! 3초간 움직일 수 없습니다.");
        // 움직임 멈춤
        rb.linearVelocity = Vector2.zero;
        spriteRenderer.color = Color.gray; // 기절하면 회색으로 변함

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
        spriteRenderer.color = Color.white; // 원상복구
        Debug.Log("😀 기절 풀림!");
    }

    // [추가됨] 맞았을 때 깜빡거리는 효과
    IEnumerator HitColorEffect()
    {
        spriteRenderer.color = new Color(1f, 0.5f, 0.5f); // 연한 빨강
        yield return new WaitForSeconds(0.1f);
        if (!isStunned) spriteRenderer.color = Color.white;
    }

}