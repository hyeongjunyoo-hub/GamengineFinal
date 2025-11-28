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
    private bool isStunned = false;
    private bool isBlinded = false;
    private bool isSlowed = false;
    private float blindDuration = 5.0f;
    private float slowDuration = 5.0f;

    private float originSpeed;
    private float originJump;

    private SpriteRenderer spriteRenderer; // 색깔 변화용

    [Header("🛢️ 스킬 설정 ")]
    public bool canUseSkill = false; // 이 캐릭터가 스킬을 쓸 수 있는지 (Inspector에서 체크)
    public GameObject drumPrefab; // 드럼통 프리팹
    public GameObject wallPrefab; // 성벽 프리팹
    public float wallSpawnX = 8.0f; // 골대 앞 X좌표 거리 (절대값)
    public int maxSkillCount = 5; // 최대 사용 횟수
    public KeyCode skillKey = KeyCode.R; // 스킬 키 (R)
    public float skillCooldown = 10.0f;
    private float nextSkillTime = 0f;
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
        originSpeed = moveSpeed;
        originJump = jumpForce;
    }

    void Update()
    {
        if(isStunned)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); 
            return;
        }
        
        // 1. 땅 감지
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        // [핵심 로직] 실명 상태면 키를 서로 바꿔치기!
        KeyCode targetRight = isBlinded ? leftKey : rightKey;
        KeyCode targetLeft  = isBlinded ? rightKey : leftKey;
        KeyCode targetJump  = isBlinded ? kickKey : jumpKey;
        KeyCode targetKick  = isBlinded ? jumpKey : kickKey;

        // 2. 이동 처리 (수정됨: rightKey -> targetRight 사용!)
        float moveX = 0f;

        if (Input.GetKey(targetRight)) // 👈 여기가 바뀌었습니다!
        {
            moveX = 1f;
        }
        else if (Input.GetKey(targetLeft)) // 👈 여기가 바뀌었습니다!
        {
            moveX = -1f;
        }

        // 플레이어 2(왼쪽 보는 애)는 좌우 키 입력에 따라 이동 방향이 반대가 되지 않도록
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
        
        // 3. 점프 처리 (수정됨: jumpKey -> targetJump 사용!)
        if (Input.GetKeyDown(targetJump) && isGrounded) // 👈 여기가 바뀌었습니다!
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 4. 발차기 처리 (수정됨: kickKey -> targetKick 사용!)
        if (Input.GetKeyDown(targetKick)) // 👈 여기가 바뀌었습니다!
        {
            StartCoroutine(KickProcess());
        }

        // 5. 스킬 사용 (R키는 안 바꿈 - 헷갈리니까)
        if (canUseSkill && Input.GetKeyDown(skillKey))
        {
            if(currentSkillCount >= maxSkillCount)
            {
                Debug.Log("스킬 횟수를 모두 소진했습니다!");
            }
            else if(Time.time < nextSkillTime)
            {
                float remainingTime = nextSkillTime - Time.time;
                Debug.Log($"쿨타임 중입니다 남은시간: {remainingTime:F1}초");
            }
            else 
            {
                if (myType == CharacterType.Lee)
                {
                    UseDrumSkill();
                }
                else if (myType == CharacterType.Jeon)
                {
                    UseJeonSkill();
                }
                else if (myType == CharacterType.Jin) // [추가] 진지황 스킬
                {
                    UseJinSkill(); 
                }
                else if (myType == CharacterType.Won)
                {
                    UseWonSkill();
                }
                nextSkillTime = Time.time + skillCooldown;
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
    public void ApplyBlind(float duration)
    {
        blindDuration = duration;
        StartCoroutine(BlindRoutine());
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
    void UseJeonSkill()
    {
        currentSkillCount++;
        Debug.Log("전두콩 스킬 발동! 상대방 조작 반전!");

        PlayerKick[] allPlayers = FindObjectsOfType<PlayerKick>();
        foreach (PlayerKick player in allPlayers)
        {
            if (player != this) 
            {
                player.ApplyBlind(5.0f);
                break; 
            }
        }
    }
    // [진지황] 만리장성 소환 (위치 고정 & 반전)
    void UseJinSkill()
    {
        currentSkillCount++;
        Debug.Log("진지황 스킬 발동! 만리장성!");

        // 1. 소환 위치 설정 (골대 앞 고정 위치)
        // 왼쪽 골대 앞: (-32.5, -13.6) / 오른쪽 골대 앞: (33.2, -13.6)
        // 맵 크기에 따라 X값(32.5)은 조금 조절이 필요할 수 있습니다.
        float spawnX = 32.5f; 
        float spawnY = -16.0f; // 땅 밑에서 올라와야 하니 시작점은 낮게 잡습니다.

        Vector3 spawnPos;
        Vector3 spawnScale = new Vector3(1,1,1);

        // P1(왼쪽 팀) -> 왼쪽 골대 앞 (-X)
        if (facingDirection == 1f) 
        {
            spawnPos = new Vector3(-spawnX, spawnY, 0);
            // 회전 없음 (그대로)
        }
        // P2(오른쪽 팀) -> 오른쪽 골대 앞 (+X)
        else 
        {
            spawnPos = new Vector3(spawnX, spawnY, 0);
            // 좌우 반전 (Y축으로 180도 회전)
            spawnScale = new Vector3(-1, 1, 1);
        }
        // 1. 회전 없이 생성 (Quaternion.identity)
        GameObject wall = Instantiate(wallPrefab, spawnPos, Quaternion.identity);
        
        // 2. 크기를 조절해서 반전시킴 (이렇게 해야 안 잘림!)
        wall.transform.localScale = spawnScale;
       
    }
    void UseWonSkill()
    {
        currentSkillCount++;
        Debug.Log("원유대사 스킬: 해골물(둔화)!");

        PlayerKick[] allPlayers = FindObjectsOfType<PlayerKick>();
        foreach (PlayerKick player in allPlayers)
        {
            if (player != this) 
            {
                // 적에게 둔화(Slow) 적용 (5초)
                player.ApplySlow(5.0f);
                break; 
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
        if(isBlinded) spriteRenderer.color = Color.black;
        Debug.Log("😀 기절 풀림!");
    }
    IEnumerator BlindRoutine()
    {
        isBlinded = true;
        Debug.Log("😵 앞이 안 보여! (조작 반전됨)");
        spriteRenderer.color = Color.black; 

        yield return new WaitForSeconds(blindDuration);

        isBlinded = false;
        Debug.Log("👀 시야 회복!");
        
        if (isStunned) spriteRenderer.color = Color.gray;
        else spriteRenderer.color = Color.white;
    }
    public void ApplySlow(float duration)
    {
        slowDuration = duration;
        StartCoroutine(SlowRoutine());
    }
    // [추가됨] 맞았을 때 깜빡거리는 효과
    IEnumerator SlowRoutine()
    {
        // 이미 둔화 상태라면 시간만 연장하거나 무시 (여기선 중복 실행 방지)
        if (isSlowed) yield break;

        isSlowed = true;
        Debug.Log("느려짐! (해골물 마심)");

        // 1. 능력치 대폭 깎기 (원래 속도의 30% 수준으로)
        moveSpeed = originSpeed * 0.3f;
        jumpForce = originJump * 0.5f;

        UpdateColor(); // 파란색으로 변함

        yield return new WaitForSeconds(slowDuration);

        // 2. 능력치 원상복구
        moveSpeed = originSpeed;
        jumpForce = originJump;

        isSlowed = false;
        Debug.Log("속도 회복!");
        UpdateColor();
    }
    IEnumerator HitColorEffect()
    {
        spriteRenderer.color = new Color(1f, 0.5f, 0.5f); // 연한 빨강
        yield return new WaitForSeconds(0.1f);
        if (!isStunned) spriteRenderer.color = Color.white;
    }
    void UpdateColor()
    {
        if (isStunned) spriteRenderer.color = Color.gray;       // 기절: 회색 (1순위)
        else if (isBlinded) spriteRenderer.color = Color.black; // 실명: 검정 (2순위)
        else if (isSlowed) spriteRenderer.color = Color.blue;   // 둔화: 파랑 (3순위)
        else spriteRenderer.color = Color.white;                // 정상: 흰색
    }

}