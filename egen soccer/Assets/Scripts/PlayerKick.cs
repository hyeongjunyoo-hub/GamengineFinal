using UnityEngine;
using System.Collections;

public enum CharacterType 
{ 
    Jin,    // 진지황
    Lee,    // 이재묭
    Jeon,   // 전두콩
    Won     // 원유대사
}
public class PlayerKick : MonoBehaviour
{
    [Header("🆔 캐릭터 설정 (중요!)")]
    public CharacterType myType; // 여기서 내 정체를 설정합니다.
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

    [Header("🛢️ 스킬 설정 (이재묭 전용)")]
    public bool canUseSkill = false; // 이 캐릭터가 스킬을 쓸 수 있는지 (Inspector에서 체크)
    public GameObject drumPrefab; // 드럼통 프리팹
    public int maxSkillCount = 5; // 최대 사용 횟수
    public KeyCode skillKey = KeyCode.R; // 스킬 키 (R)
    private int currentSkillCount = 0; // 현재 사용한 횟수

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
        // [추가] 5. 스킬 사용 (R키)
        if (canUseSkill && Input.GetKeyDown(skillKey) && currentSkillCount < maxSkillCount)
        {
            // 조건 3가지가 모두 맞아야 발동!
            // 1. canUseSkill: 스킬을 쓸 수 있는 캐릭터인가? (이재묭인가?)
            // 2. Input.GetKeyDown: 지금 스킬 키(R)를 눌렀는가?
            // 3. 횟수 제한: 아직 5번을 다 안 썼는가?

            if (myType == CharacterType.Lee) // (아까 추가한 안전장치)
            {
                UseDrumSkill(); // -> 드럼통 떨구러 가자!
            }
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
    public void ApplyDirectStun(float duration)
    {
        // 1. 기절 시간을 받아옴 (예: 4초)
        stunDuration = duration; 
        
        // 2. 이미 만들어뒀던 '기절 코루틴(StunRoutine)'을 강제로 실행!
        StartCoroutine(StunRoutine());
    }
    

    void UseDrumSkill()
    {
        currentSkillCount++; // 1. 횟수 차감 (이제 1번 쓴 거임)
        Debug.Log($"스킬 사용! 남은 횟수: {maxSkillCount - currentSkillCount}");

        // 2. "이 게임에 있는 모든 플레이어 다 나와봐!"
        PlayerKick[] allPlayers = FindObjectsOfType<PlayerKick>();

        // 3. "그중에서 나 말고 다른 놈(적)을 찾아!"
        foreach (PlayerKick player in allPlayers)
        {
            if (player != this) // 'this'는 나 자신(이재묭)
            {
                // 4. 적을 찾았다! 적의 머리 위(Y + 6.0f) 좌표 계산
                Vector3 spawnPos = new Vector3(player.transform.position.x, 6.0f, 0);
                
                // 5. 드럼통 소환 (Instantiate)
                GameObject drum = Instantiate(drumPrefab, spawnPos, Quaternion.identity);
                
                // 6. [중요] 드럼통아, 주인님은 나(this)야. 나한테는 터지지 마.
                drum.GetComponent<DrumSkill>().caster = this.gameObject;
                
                break; // 적을 찾았으니 더 찾지 말고 끝냄
            }
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