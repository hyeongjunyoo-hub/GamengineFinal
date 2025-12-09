using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public enum CharacterType 
{ 
    Jin,    // 진지황
    Lee,    // 이재묭
    Jeon,   // 전두콩
    Won     // 원유대사
}

public class PlayerKick : MonoBehaviour
{
    [Header("🆔 캐릭터 설정")]
    public CharacterType myType;
    public int playerNum; 

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
    public float startDelay = 0.0f;

    [Header("🔊 오디오 설정")]
    public AudioClip kickSound; 
    public AudioClip skillSound;
    public AudioClip hurtSound;
    // [🔥 핵심] 스킬 목소리
    public AudioClip skillVoiceClip; 
    private AudioSource audioSource; 

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
    public GameObject jeonSkillEffect;
    public GameObject wonSkillEffectPrefab;
    // [🔥 핵심] 단순 이펙트 프리팹 (전두콩, 원유대사)
    public GameObject simpleSkillEffectPrefab; 
    
    public KeyCode skillKey = KeyCode.R; 
    public float skillCooldown = 10.0f;
    private float nextSkillTime = 0f;
    private int currentSkillCount = 0; // 스킬 사용 횟수 추적 변수

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (kickHitbox != null) kickHitbox.SetActive(false);

        if (playerNum == 0)
        {
            if (gameObject.name.Contains("Player1")) playerNum = 1;
            else if (gameObject.name.Contains("Player2")) playerNum = 2;
        }

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

        // [🔥 핵심] 스킬 사용 로직 (GameManager 연동)
        if (canUseSkill && Input.GetKeyDown(skillKey))
        {
            if (Time.time < nextSkillTime)
            {
                // 쿨타임 중
            }
            else if (GameManager.instance != null && GameManager.instance.TryUseSkill(playerNum))
            {
                // 효과음 & 목소리 재생
                if (audioSource != null)
                {
                    if (skillSound != null) audioSource.PlayOneShot(skillSound);
                    if (skillVoiceClip != null) audioSource.PlayOneShot(skillVoiceClip);
                }

                float currentDuration = 0f;
                if (myType == CharacterType.Lee) { UseDrumSkill(); currentDuration = 4.0f; }
                else if (myType == CharacterType.Jeon) { UseJeonSkill(); currentDuration = 5.0f; }
                else if (myType == CharacterType.Jin) { UseJinSkill(); currentDuration = 5.0f; }
                else if (myType == CharacterType.Won) { UseWonSkill(); currentDuration = 5.0f; }
                
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

    // [PlayerKick.cs] 내부의 UseJeonSkill 함수를 아래 코드로 교체하세요.

void UseJeonSkill()
{
    currentSkillCount++;
    Debug.Log("전두콩 스킬: 실명! 내 앞에 이펙트 소환!");

    // 1. 스킬 이펙트 프리팹이 연결되어 있다면 실행
    if (jeonSkillEffect != null)
    {
        // [위치 계산 핵심 로직]
        // transform.position: 현재 내 캐릭터가 서 있는 실시간 위치
        // facingDirection: 내가 바라보는 방향 (1이면 오른쪽, -1이면 왼쪽)
        // new Vector3(facingDirection * 1.5f, 0.5f, 0): 내 위치 기준 앞쪽으로 1.5만큼, 위로 0.5만큼 이동
        
        // 거리(1.5f)와 높이(0.5f)는 게임 화면을 보면서 입맛에 맞게 숫자만 고치면 됩니다.
        Vector3 spawnOffset = new Vector3(facingDirection * 1.5f, 0.5f, 0); 
        Vector3 spawnPos = transform.position + spawnOffset;
        
        // 계산된 위치(spawnPos)에 이펙트 생성
        GameObject effect = Instantiate(jeonSkillEffect, spawnPos, Quaternion.identity);
        
        // [방향 맞추기 로직]
        // 원본 스프라이트가 "왼쪽"을 보고 있다고 하셨으므로:
        Vector3 scale = effect.transform.localScale;

        if (facingDirection == 1f) // Player 1 (오른쪽 보는 중)
        {
            // 이펙트도 오른쪽을 보게 하려면 -> 좌우 반전 필요 (X를 음수로)
            scale.x = -Mathf.Abs(scale.x);
        }
        else // Player 2 (왼쪽 보는 중)
        {
            // 이펙트도 왼쪽을 보게 하려면 -> 원본 그대로 유지 (X를 양수로)
            scale.x = Mathf.Abs(scale.x);
        }

        // 설정한 스케일 적용
        effect.transform.localScale = scale;

        // 2초 뒤에 이펙트 삭제 (지속 시간 조절 가능)
        Destroy(effect, 2.0f);
    }

    // 2. 기존 기능: 상대방 찾아서 실명 상태이상 걸기
    PlayerKick[] allPlayers = FindObjectsOfType<PlayerKick>();
    foreach (PlayerKick player in allPlayers)
    {
        // 나 자신이 아닌 다른 플레이어에게 효과 적용
        if (player != this) 
        { 
            player.ApplyBlind(5.0f); // 5초간 실명
            break; 
        }
    }
}

    void UseJinSkill()
    {
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
        Debug.Log("원유대사 스킬: 해골물! (상대 머리 위 부처 소환)");
        
        // 내 스킬 사용 횟수 차감 등은 Update에서 이미 처리됨
        
        PlayerKick[] allPlayers = FindObjectsOfType<PlayerKick>();
        foreach (PlayerKick player in allPlayers)
        {
            // 내가 아닌 다른 플레이어(상대방)를 찾음
            if (player != this) 
            { 
                // [🔥 핵심 변경] 상대방에게 슬로우를 걸면서 '내 스킬 이펙트(부처)'를 넘겨줌
                player.ApplySlow(5.0f, wonSkillEffectPrefab); 
                break; 
            }
        }
    }

    void SpawnSimpleEffect()
    {
        if (simpleSkillEffectPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, 2.0f, 0);
            GameObject effect = Instantiate(simpleSkillEffectPrefab, spawnPos, Quaternion.identity);
            Destroy(effect, 2.0f);
        }
    }

    // 2. 슬로우 적용 함수 (피해자 입장) - 파라미터 추가됨
    public void ApplySlow(float duration, GameObject effectPrefab = null) 
    { 
        slowDuration = duration; 
        // 코루틴 시작할 때 이펙트 프리팹도 같이 넘김
        StartCoroutine(SlowRoutine(effectPrefab)); 
    }
    // --- 피격 및 상태이상 ---
    public void TakeHit()
    {
        if (isStunned) return;
        if (audioSource != null && hurtSound != null) audioSource.PlayOneShot(hurtSound);
        currentHitCount++;
        if (currentHitCount >= maxHitCount) StartCoroutine(StunRoutine());
        else StartCoroutine(HitColorEffect());
    }

    public void ApplyDirectStun(float duration) { stunDuration = duration; StartCoroutine(StunRoutine()); }
    public void ApplyBlind(float duration) { blindDuration = duration; StartCoroutine(BlindRoutine()); }
    

    IEnumerator StunRoutine()
    {
        isStunned = true; currentHitCount = 0; rb.linearVelocity = Vector2.zero; UpdateColor(); 
        yield return new WaitForSeconds(stunDuration); isStunned = false; UpdateColor(); 
    }

    IEnumerator BlindRoutine()
    {
        isBlinded = true; Debug.Log("😵 조작키 섞임!"); UpdateColor(); 
        List<KeyCode> keysToShuffle = new List<KeyCode> { leftKey, rightKey, jumpKey, kickKey };
        for (int i = 0; i < keysToShuffle.Count; i++) { KeyCode temp = keysToShuffle[i]; int randomIndex = Random.Range(i, keysToShuffle.Count); keysToShuffle[i] = keysToShuffle[randomIndex]; keysToShuffle[randomIndex] = temp; }
        currentLeftKey = keysToShuffle[0]; currentRightKey = keysToShuffle[1]; currentJumpKey = keysToShuffle[2]; currentKickKey = keysToShuffle[3];
        yield return new WaitForSeconds(blindDuration);
        isBlinded = false; ResetControls(); UpdateColor();
    }

    // 3. 슬로우 코루틴 (상태이상 지속 시간 담당)
    IEnumerator SlowRoutine(GameObject effectPrefab)
    {
        if (isSlowed) yield break; // 이미 느려진 상태면 중복 적용 X

        isSlowed = true; 
        moveSpeed = originSpeed * 0.3f; // 속도 감소
        jumpForce = originJump * 0.5f;  // 점프 감소
        UpdateColor(); 

        GameObject myBuddha = null; // 소환된 부처님을 담을 변수

        // [🔥 핵심 로직] 이펙트(부처)가 존재하면 내 머리 위에 소환
        if (effectPrefab != null)
        {
            // 1. 내 위치(transform)에 생성
            myBuddha = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            
            // 2. 나(플레이어)를 부모로 설정 -> 내가 움직이면 같이 따라다님
            myBuddha.transform.SetParent(this.transform);

            // 3. 머리 위로 위치 조정 (Y값 2.5f 정도면 머리 위, 필요시 조절)
            myBuddha.transform.localPosition = new Vector3(0, 6.0f, 0);

            // 4. [중요] 스케일 보정 (내가 뒤집혀 있어도 부처님은 찌그러지지 않게)
            // 부모 스케일의 영향을 받지 않도록 1,1,1로 초기화하되, 방향 고려
            // (부처님 이미지가 좌우 대칭이라면 그냥 1,1,1로 둬도 무방)
            myBuddha.transform.localScale = new Vector3(0.7f,0.7f,0.7f);
        }

        // --- 상태이상 지속 시간 대기 ---
        yield return new WaitForSeconds(slowDuration);

        // --- 상태이상 종료 ---
        moveSpeed = originSpeed; 
        jumpForce = originJump; 
        isSlowed = false; 
        UpdateColor();

        // [🔥 종료 처리] 상태이상이 풀렸으니 머리 위 부처님도 삭제
        if (myBuddha != null)
        {
            Destroy(myBuddha);
        }
    }

    IEnumerator HitColorEffect()
    {
        if (!isStunned && !isBlinded && !isSlowed) 
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f); yield return new WaitForSeconds(0.1f); spriteRenderer.color = Color.white;
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
        currentLeftKey = leftKey; currentRightKey = rightKey; currentJumpKey = jumpKey; currentKickKey = kickKey;
    }

    public void ResetStatus()
    {
        StopAllCoroutines(); isStunned = false; isBlinded = false; isSlowed = false;
        if(spriteRenderer != null) spriteRenderer.color = Color.white;
        transform.localScale = new Vector3(1f * facingDirection, 1f, 1f);
        moveSpeed = originSpeed; jumpForce = originJump; ResetControls();
        DrumSkill attachedDrum = GetComponentInChildren<DrumSkill>(); if (attachedDrum != null) Destroy(attachedDrum.gameObject);
    }

    public void PlayKickSoundEffect()
    {
        if (audioSource != null && kickSound != null) audioSource.PlayOneShot(kickSound);
    }
}