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
    // [🔥 핵심] 단순 이펙트 프리팹 (전두콩, 원유대사)
    public GameObject simpleSkillEffectPrefab; 
    
    public KeyCode skillKey = KeyCode.R; 
    public float skillCooldown = 10.0f;
    private float nextSkillTime = 0f;

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

    void UseJeonSkill()
    {
        Debug.Log("전두콩 스킬: 랜덤 키 변경!");
        SpawnSimpleEffect(); // 이펙트
        PlayerKick[] allPlayers = FindObjectsOfType<PlayerKick>();
        foreach (PlayerKick player in allPlayers)
        {
            if (player != this) { player.ApplyBlind(5.0f); break; }
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
        Debug.Log("원유대사 스킬: 해골물!");
        SpawnSimpleEffect(); // 이펙트
        PlayerKick[] allPlayers = FindObjectsOfType<PlayerKick>();
        foreach (PlayerKick player in allPlayers)
        {
            if (player != this) { player.ApplySlow(5.0f); break; }
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
    public void ApplySlow(float duration) { slowDuration = duration; StartCoroutine(SlowRoutine()); }

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

    IEnumerator SlowRoutine()
    {
        if (isSlowed) yield break;
        isSlowed = true; moveSpeed = originSpeed * 0.3f; jumpForce = originJump * 0.5f; UpdateColor(); 
        yield return new WaitForSeconds(slowDuration);
        moveSpeed = originSpeed; jumpForce = originJump; isSlowed = false; UpdateColor();
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