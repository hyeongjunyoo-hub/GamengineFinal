using UnityEngine;
using System.Collections;

public class DrumSkill : MonoBehaviour
{
    public float stunTime = 4.0f; // 기절 시간
    public GameObject caster; // 스킬 쓴 사람

    private bool isTrapped = false; 
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // 땅에 안 맞고 허공으로 떨어질 경우 대비
        Destroy(gameObject, 5.0f); 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTrapped) return;

        if (other.CompareTag("Player"))
        {
            if (other.gameObject != caster)
            {
                PlayerKick enemy = other.GetComponent<PlayerKick>();
                if (enemy != null)
                {
                    enemy.ApplyDirectStun(stunTime);
                    StartCoroutine(CrushRoutine(enemy.gameObject));

                    if(anim !=null)
                    {
                        anim.SetTrigger("Hit");
                    }
                }
            }
        }
        else if (other.CompareTag("Ground") || other.name.Contains("Ground")) 
        {
            Destroy(gameObject);
        }
    }

    // "깔렸다!" 연출 코루틴
    IEnumerator CrushRoutine(GameObject target)
    {
        isTrapped = true;
        Debug.Log("으악! 드럼통에 깔렸다!");

        // 1. 드럼통 물리 끄기
        // (Unity 6는 linearVelocity, 구버전은 velocity 사용)
        rb.linearVelocity = Vector2.zero; 
        rb.isKinematic = true; 
        col.enabled = false; 

        // 2. 타겟(캐릭터)의 원래 크기 저장
        Vector3 originalScale = target.transform.localScale;

        // 3. 캐릭터를 호떡처럼 납작하게 만들기 (Squash)
        // X는 1.3배 넓게, Y는 0.3배 납작하게
        float direction = Mathf.Sign(originalScale.x); // 캐릭터가 보는 방향 유지
        float squashX = 1.3f;
        float squashY = 0.3f;
        target.transform.localScale = new Vector3(squashX * direction, squashY, 1f);

        // 4. 드럼통을 캐릭터 자식으로 넣기
        transform.SetParent(target.transform);
        
        // 5. [중요!] 드럼통이 같이 찌그러지는 것 방지 (역보정)
        // 부모가 0.3배 됐으니, 자식은 1/0.3배 해서 원래 크기 유지
        transform.localScale = new Vector3(1f / squashX, 1f / squashY, 1f);

        // 6. 위치 조정 (발 밑으로 깔리게)
        // 캐릭터가 납작해졌으므로 Y위치를 잘 맞춰야 함
        transform.localPosition = new Vector3(0, -4.5f, 0); 

        // 7. 드럼통이 캐릭터를 가리도록 순서 변경
        SpriteRenderer targetSr = target.GetComponent<SpriteRenderer>();
        if (targetSr != null)
        {
            sr.sortingOrder = targetSr.sortingOrder + 1;
        }

        // 8. 기절 시간만큼 대기
        yield return new WaitForSeconds(stunTime);

        // 9. 캐릭터 크기 원상복구
        if (target != null)
        {
            target.transform.localScale = originalScale;
        }

        // 10. 드럼통 삭제
        Destroy(gameObject);
    }
}