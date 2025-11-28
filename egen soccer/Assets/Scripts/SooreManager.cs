using UnityEngine;
using TMPro; // (TextMeshPro 안 쓰시면 using UnityEngine.UI; 로 바꾸세요)
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    // 어디서든 이 스크립트를 부를 수 있게 함 (싱글톤 패턴)
    public static ScoreManager instance;

    [Header("UI 연결")]
    public TextMeshProUGUI p1ScoreText; // P1 점수판
    public TextMeshProUGUI p2ScoreText; // P2 점수판

    [Header("게임 오브젝트")]
    public GameObject ball; // 축구공

    // 내부 점수 변수
    private int scoreP1 = 0;
    private int scoreP2 = 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UpdateScoreUI();
    }

    // 골이 들어갔을 때 부르는 함수 (whoScored: 1이면 P1득점, 2면 P2득점)
    public void AddScore(int whoScored)
    {
        if (whoScored == 1) scoreP1++;
        else scoreP2++;

        UpdateScoreUI();
        
        // 골 세레머니 후 공 리셋 (코루틴 실행)
        StartCoroutine(ResetRound());
    }

    void UpdateScoreUI()
    {
        // 텍스트 갱신
        p1ScoreText.text = scoreP1.ToString();
        p2ScoreText.text = scoreP2.ToString();
    }

    IEnumerator ResetRound()
    {
        Debug.Log("골인! 잠시 대기...");
        
        // 1. 공을 잠깐 멈춤 & 안 보이게 (선택사항)
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        ballRb.linearVelocity = Vector2.zero; // (Unity 6)
        ballRb.angularVelocity = 0f;
        
        // 2. 2초 정도 대기 (환호성 시간)
        yield return new WaitForSeconds(0.5f);

        // 3. 공을 중앙으로 이동 및 초기화
        ball.transform.position = new Vector3(0, 2, 0); // 공중에서 떨어지게 Y값 2
        ballRb.linearVelocity = Vector2.zero;
        
        Debug.Log("경기 재개!");
    }
}
