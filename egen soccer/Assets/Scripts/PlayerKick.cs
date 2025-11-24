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
    public float jumpForce = 15.0f;

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

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (kickHitbox != null) kickHitbox.SetActive(false);

        // 시작할 때 방향에 맞춰서 캐릭터 뒤집기 (P2는 왼쪽을 봐야 함)
        // X축 스케일을 facingDirection에 맞춥니다.
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * facingDirection, transform.localScale.y, transform.localScale.z);
    }

    void Update()
    {
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
}