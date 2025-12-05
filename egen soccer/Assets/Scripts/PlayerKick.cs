using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public CharacterType myType;

    [Header("🎮 조작키 설정")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.W;
    public KeyCode kickKey = KeyCode.S;

    private KeyCode currentLeftKey;
    private KeyCode currentRightKey;
    private KeyCode currentJumpKey;
    private KeyCode currentKickKey;

    [Header("캐릭터 방향 설정")]
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
    public float startDelay = 0.0f; // (수정됨: 0으로 설정 권장)
    // [추가됨] 킥 사운드 설정
    [Header("🔊 오디오 설정")]
    public AudioClip kickSound; // 발차기 소리 파일
    public AudioClip skillSound;
    private AudioSource audioSource; // 재생기

    [Header("😵 상태이상 설정")]
    public int maxHitCount = 4; 
    public float stunDuration = 3.0f; 
    private int currentHitCount = 0; 
    private bool isStunned = false; 
    private bool isBlinded = false; 
    private bool isSlowed = false;  

    private float blindDuration = 5.0f;
    private float slowDuration = 5.0f;

    private float originSpeed;
    private float originJump;

    private SpriteRenderer spriteRenderer; 

    [Header("⚡ 스킬 설정")]
    public bool canUseSkill = false; 
    public GameObject drumPrefab; 
    public GameObject wallPrefab;
    public float wallSpawnX = 8.0f; 

    public int maxSkillCount = 5; 
    public KeyCode skillKey = KeyCode.R; 
    
    public float skillCooldown = 10.0f;
    private float nextSkillTime = 0f;
    private int currentSkillCount = 0;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (kickHitbox != null) kickHitbox.SetActive(false);

        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * facingDirection, transform.localScale.y, transform.localScale.z);

        originSpeed = moveSpeed;
        originJump = jumpForce;

        ResetControls();
    }

    void Update()
    {
        if (isStunned) 
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); 
            return; 
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        float moveX = 0f;
        if (Input.GetKey(currentRightKey)) moveX = 1f;
        else if (Input.GetKey(currentLeftKey)) moveX = -1f;

        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
        
        if (Input.GetKeyDown(currentJumpKey) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (Input.GetKeyDown(currentKickKey))
        {
            StartCoroutine(KickProcess());
        }

        // [수정된 스킬 사용 로직]
        if (canUseSkill && Input.GetKeyDown(skillKey))
        {
            if (currentSkillCount >= maxSkillCount)
            {
                Debug.Log("스킬 횟수를 모두 소진했습니다!");
            }
            else if (Time.time < nextSkillTime)
            {
                float remainingTime = nextSkillTime - Time.time;
                Debug.Log($"쿨타임 중입니다! 남은 시간: {remainingTime:F1}초");
            }
            else 
            {
                // [🔥 핵심 추가] 스킬 발동 소리 재생!
                if (audioSource != null && skillSound != null)
                {
                    audioSource.PlayOneShot(skillSound);
                }
                // 이번에 사용할 스킬의 지속시간을 저장할 변수
                float currentDuration = 0f;

                if (myType == CharacterType.Lee) 
                { 
                    UseDrumSkill(); 
                    currentDuration = 4.0f; // 드럼통 기절 시간 (4초)
                }
                else if (myType == CharacterType.Jeon) 
                { 
                    UseJeonSkill(); 
                    currentDuration = 5.0f; // 실명 시간 (5초)
                }
                else if (myType == CharacterType.Jin) 
                { 
                    UseJinSkill(); 
                    currentDuration = 5.0f; // 성벽 유지 시간 (5초)
                }
                else if (myType == CharacterType.Won) 
                { 
                    UseWonSkill(); 
                    currentDuration = 5.0f; // 둔화 시간 (5초)
                }
                
                // [핵심] 다음 사용 가능 시간 = 현재 시간 + 스킬 지속시간 + 쿨타임(10초)
                // 즉, 스킬 효과가 다 끝나고 나서부터 10초를 셉니다.
                nextSkillTime = Time.time + currentDuration + skillCooldown;
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

    // --- 스킬 함수들 ---

    void UseDrumSkill()
    {
        currentSkillCount++;
        Debug.Log("이재묭 스킬: 드럼통!");
        PlayerKick[] allPlayers = FindObjectsOfType<PlayerKick>();
        foreach (PlayerKick player in allPlayers)
        {
            if (player != this) 
            {
                Vector3 spawnPos = new Vector3(player.transform.position.x, 6.0f, 0);
                GameObject drum = Instantiate(drumPrefab, spawnPos, Quaternion.identity);
                drum.GetComponent<DrumSkill>().caster = this.gameObject;
                break; 
            }
        }
    }

    void UseJeonSkill()
    {
        currentSkillCount++;
        Debug.Log("전두콩 스킬: 랜덤 키 변경!");
        PlayerKick[] allPlayers = FindObjectsOfType<PlayerKick>();
        foreach (PlayerKick player in allPlayers)
        {
            if (player != this) { player.ApplyBlind(5.0f); break; }
        }
    }

    void UseJinSkill()
    {
        currentSkillCount++;
        Debug.Log("진지황 스킬: 만리장성!");
        float spawnX = 32.5f; 
        float spawnY = -16.0f; 
        Vector3 spawnPos;
        Vector3 spawnScale = new Vector3(1, 1, 1);

        if (facingDirection == 1f) spawnPos = new Vector3(-spawnX, spawnY, 0); 
        else 
        {
            spawnPos = new Vector3(spawnX, spawnY, 0); 
            spawnScale = new Vector3(-1, 1, 1);
        }

        GameObject wall = Instantiate(wallPrefab, spawnPos, Quaternion.identity);
        wall.transform.localScale = spawnScale;
    }

    void UseWonSkill()
    {
        currentSkillCount++;
        Debug.Log("원유대사 스킬: 해골물!");
        PlayerKick[] allPlayers = FindObjectsOfType<PlayerKick>();
        foreach (PlayerKick player in allPlayers)
        {
            if (player != this) { player.ApplySlow(5.0f); break; }
        }
    }

    // --- 피격/상태이상 함수들 ---

    public void TakeHit()
    {
        if (isStunned) return; 
        currentHitCount++;
        
        if (currentHitCount >= maxHitCount) 
        {
            StartCoroutine(StunRoutine());
        }
        else 
        {
            StartCoroutine(HitColorEffect());
        }
    }

    public void ApplyDirectStun(float duration)
    {
        stunDuration = duration;
        StartCoroutine(StunRoutine());
    }

    public void ApplyBlind(float duration)
    {
        blindDuration = duration;
        StartCoroutine(BlindRoutine());
    }

    public void ApplySlow(float duration)
    {
        slowDuration = duration;
        StartCoroutine(SlowRoutine());
    }

    // --- 상태이상 코루틴 ---

    IEnumerator StunRoutine()
    {
        isStunned = true;
        currentHitCount = 0; 
        rb.linearVelocity = Vector2.zero; 
        UpdateColor(); 
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
        UpdateColor(); 
    }

    IEnumerator BlindRoutine()
    {
        isBlinded = true;
        Debug.Log("😵 조작키 섞임!");
        UpdateColor(); 

        List<KeyCode> keysToShuffle = new List<KeyCode> { leftKey, rightKey, jumpKey, kickKey };
        for (int i = 0; i < keysToShuffle.Count; i++)
        {
            KeyCode temp = keysToShuffle[i];
            int randomIndex = Random.Range(i, keysToShuffle.Count);
            keysToShuffle[i] = keysToShuffle[randomIndex];
            keysToShuffle[randomIndex] = temp;
        }

        currentLeftKey = keysToShuffle[0];
        currentRightKey = keysToShuffle[1];
        currentJumpKey = keysToShuffle[2];
        currentKickKey = keysToShuffle[3];

        yield return new WaitForSeconds(blindDuration);

        isBlinded = false;
        ResetControls();
        UpdateColor();
    }

    IEnumerator SlowRoutine()
    {
        if (isSlowed) yield break;
        isSlowed = true;
        moveSpeed = originSpeed * 0.3f;
        jumpForce = originJump * 0.5f;
        UpdateColor(); 
        yield return new WaitForSeconds(slowDuration);
        moveSpeed = originSpeed;
        jumpForce = originJump;
        isSlowed = false;
        UpdateColor();
    }

    IEnumerator HitColorEffect()
    {
        if (!isStunned && !isBlinded && !isSlowed) 
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f); 
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }

    void UpdateColor()
    {
        if (isStunned) spriteRenderer.color = Color.gray;       
        else if (isBlinded) spriteRenderer.color = Color.black; 
        else if (isSlowed) spriteRenderer.color = Color.blue;   
        else spriteRenderer.color = Color.white;                
    }

    void ResetControls()
    {
        currentLeftKey = leftKey;
        currentRightKey = rightKey;
        currentJumpKey = jumpKey;
        currentKickKey = kickKey;
    }

    // 상태 완전 초기화 함수
    public void ResetStatus()
    {
        StopAllCoroutines();

        isStunned = false;
        isBlinded = false;
        isSlowed = false;

        if(spriteRenderer != null) spriteRenderer.color = Color.white;

        // 크기 복구 (1f로 강제 설정하여 찌그러짐 방지)
        transform.localScale = new Vector3(1f * facingDirection, 1f, 1f);

        moveSpeed = originSpeed;
        jumpForce = originJump;

        ResetControls();

        DrumSkill attachedDrum = GetComponentInChildren<DrumSkill>();
        if (attachedDrum != null)
        {
            Destroy(attachedDrum.gameObject);
        }
    }
    // [추가] 외부(히트박스)에서 킥 소리를 재생하라고 시킬 때 쓰는 함수
    public void PlayKickSoundEffect()
    {
        if (audioSource != null && kickSound != null)
        {
            audioSource.PlayOneShot(kickSound);
        }
    }
}