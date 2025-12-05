using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("시간 설정 (초 단위)")]
    public float regularTime = 120f; 
    public float overtime = 30f;     

    [Header("UI 연결")]
    public TextMeshProUGUI timerText; 
    public GameObject pausePanel;     
    public Image soundButtonImage;    
    public TextMeshProUGUI p1ScoreText; 
    public TextMeshProUGUI p2ScoreText; 

    [Header("사운드 아이콘 설정")]
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    [Header("게임 오브젝트 & 위치")]
    public GameObject ball; 
    public Transform p1SpawnPoint; 
    public Transform p2SpawnPoint; 

    [Header("골 센서 연결 (중복골 방지용)")]
    public Collider2D goalSensorL; // 왼쪽 센서
    public Collider2D goalSensorR; // 오른쪽 센서
    
    [Header("오디오 클립 설정")] // [추가됨]
    public AudioClip kickoffSound; // 킥오프 소리 (호루라기 등)
    public AudioClip goalNetSound; // [추가] 골 그물 소리 (hit_goal_1)
    public AudioClip crowdSound;   // [추가] 관중 함성 (crowd_sound)
    private AudioSource audioSource; // 소리 재생기

    [Header("게임 상태")]
    public int p1Score = 0;
    public int p2Score = 0;
    public GamePhase currentPhase = GamePhase.Regular;
    [Header("골 이펙트")]
    public GameObject goalEffectObject; // GOAL 글자 오브젝트

    private float currentTime;
    private bool isPaused = false;
    private bool isMuted = false;
    private bool isGoalCeremony = false; 
    private Color defaultColor; 

    public enum GamePhase
    {
        Regular, Overtime, GoldenGoal, GameOver
    }

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        currentPhase = GamePhase.Regular;
        currentTime = regularTime;

        if (timerText != null) defaultColor = timerText.color;
        // [추가] 오디오 소스 가져오기
        audioSource = GetComponent<AudioSource>();

        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        
        isMuted = false;
        AudioListener.volume = 1f;
        
        if (soundButtonImage != null && soundOnSprite != null)
        {
            soundButtonImage.sprite = soundOnSprite;
        }

        UpdateTimerUI();
        UpdateScoreUI();
        // [추가] 게임 시작 시 킥오프 소리 재생!
        PlayKickoffSound();
    }

    void Update()
    {
        if (currentPhase != GamePhase.GameOver && !isPaused && !isGoalCeremony)
        {
            HandleTimer();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) OnResumeClick();
            else OnPauseClick();
        }
    }

    // [추가] 소리 재생 함수
    void PlayKickoffSound()
    {
        if (audioSource != null && kickoffSound != null)
        {
            audioSource.PlayOneShot(kickoffSound);
        }
    }
    // [추가] 골인 사운드 재생 (그물 소리 + 함성)
    void PlayGoalSound()
    {
        if (audioSource == null) return;

        // 1. 그물 소리는 짧고 강하게 한 번 재생 (PlayOneShot)
        if (goalNetSound != null)
        {
            audioSource.PlayOneShot(goalNetSound);
        }
       // 2. 관중 함성 소리 재생 (PlayOneShot은 파일 길이만큼 재생되고 알아서 끝납니다)
        if (crowdSound != null)
        {
            audioSource.PlayOneShot(crowdSound);
        }
    }
    void HandleTimer()
    {
        if (currentPhase == GamePhase.GoldenGoal)
        {
            timerText.text = "GOLDEN GOAL";
            timerText.color = Color.yellow;
            return;
        }
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            currentTime = 0;
            CheckPhaseChange();
        }
        UpdateTimerUI();
    }

    void CheckPhaseChange()
    {
        if (currentPhase == GamePhase.Regular)
        {
            if (p1Score == p2Score) 
            {
                currentPhase = GamePhase.Overtime;
                currentTime = overtime;
            }
            else EndGame(); 
        }
        else if (currentPhase == GamePhase.Overtime)
        {
            if (p1Score == p2Score) currentPhase = GamePhase.GoldenGoal;
            else EndGame(); 
        }
    }

    void UpdateTimerUI()
    {
        if (currentPhase == GamePhase.GoldenGoal) return;
        int minutes = Mathf.FloorToInt(currentTime / 60F);
        int seconds = Mathf.FloorToInt(currentTime % 60F);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (currentPhase == GamePhase.Overtime || (currentPhase == GamePhase.Regular && currentTime <= 10.0f)) 
            timerText.color = Color.red;
        else 
            timerText.color = defaultColor; 
    }

    public void AddScore(int playerNum)
    {
        if (currentPhase == GamePhase.GameOver) return;
        if (playerNum == 1) p1Score++; else p2Score++;
        UpdateScoreUI();
        PlayGoalSound();
        if (currentPhase == GamePhase.GoldenGoal) EndGame();
        else StartCoroutine(ResetRound());
    }

    void UpdateScoreUI()
    {
        p1ScoreText.text = p1Score.ToString();
        p2ScoreText.text = p2Score.ToString();
    }

    IEnumerator ResetRound()
    {
        isGoalCeremony = true; 
        Debug.Log("골인! 세레머니...");
        
        // [추가] 1. GOAL 이펙트 켜기 (애니메이션 자동 재생됨)
       // [추가] 1. GOAL 이펙트 켜기 (애니메이션 자동 재생됨)
        if (goalEffectObject != null)
        {
            goalEffectObject.SetActive(true);
        }
        // 2초간 대기 (세레머니 시간)
        yield return new WaitForSeconds(2.0f);
        
        if (goalEffectObject != null)
        {
        goalEffectObject.SetActive(false);
        }
        // 공 리셋
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        ball.transform.position = new Vector3(0, 2, 0); 
        ballRb.linearVelocity = Vector2.zero;
        ballRb.angularVelocity = 0f;

        // 플레이어 리셋 (상태이상 해제 포함)
        ResetPlayers();
        
        // [🔥 추가됨] 맵에 남아있는 모든 성벽(WallSkill) 찾아서 철거!
        WallSkill[] walls = FindObjectsOfType<WallSkill>();
        foreach (WallSkill wall in walls)
        {
            Destroy(wall.gameObject);
        }

        // 골 센서 다시 켜기
        if(goalSensorL != null) goalSensorL.enabled = true;
        if(goalSensorR != null) goalSensorR.enabled = true;

        isGoalCeremony = false; 
        Debug.Log("경기 재개!");
        // [추가] 재시작 시 킥오프 소리 재생!
        PlayKickoffSound();
    }

    void ResetPlayers()
    {
        GameObject p1 = GameObject.Find("Player1");
        GameObject p2 = GameObject.Find("Player2");

        if (p1 != null)
        {
            p1.transform.position = p1SpawnPoint.position;
            p1.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            
            // 상태 이상 초기화
            p1.GetComponent<PlayerKick>().ResetStatus();
        }

        if (p2 != null)
        {
            p2.transform.position = p2SpawnPoint.position;
            p2.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

            // 상태 이상 초기화
            p2.GetComponent<PlayerKick>().ResetStatus();
        }
    }

    void EndGame()
    {
        currentPhase = GamePhase.GameOver;
        timerText.text = "GAME OVER";
        Debug.Log($"게임 종료! 승자: {(p1Score > p2Score ? "P1" : "P2")}");
    }

    public void OnPauseClick()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnResumeClick()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnSoundClick()
    {
        isMuted = !isMuted;
        if (isMuted)
        {
            AudioListener.volume = 0f;
            if (soundButtonImage != null && soundOffSprite != null)
                soundButtonImage.sprite = soundOffSprite;
        }
        else
        {
            AudioListener.volume = 1f;
            if (soundButtonImage != null && soundOnSprite != null)
                soundButtonImage.sprite = soundOnSprite;
        }
    }

    public void OnQuitClick()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene"); 
    }
    
}