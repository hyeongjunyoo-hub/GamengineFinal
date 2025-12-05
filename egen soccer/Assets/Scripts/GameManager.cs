using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic; // 리스트 사용을 위해 필수

// 캐릭터별 엔딩 이미지를 담을 '가방' 정의
[System.Serializable]
public class CharacterEndingData
{
    public string characterName; // 헷갈리지 않게 이름 적는 칸
    public Sprite winSprite;     // 이겼을 때 그림
    public Sprite loseSprite;    // 졌을 때 그림
}

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

    [Header("결과 화면 UI 연결")]
    public GameObject endPanel;       
    public Image p1PortraitImg;       
    public Image p1ResultTextImg;     
    public Image p2PortraitImg;       
    public Image p2ResultTextImg;     
    
    // [🔥 변경됨] 캐릭터별 승리/패배 이미지를 담을 리스트
    [Header("캐릭터 엔딩 이미지 (순서 중요! 0:진, 1:이, 2:전, 3:원)")]
    public List<CharacterEndingData> characterEndings; 

    [Header("결과 텍스트 이미지 에셋")]
    public Sprite winTextSprite;      // WIN 글자 이미지
    public Sprite loseTextSprite;     // LOSE 글자 이미지
    public Sprite drawTextSprite;     // Draw 글자 이미지

    [Header("사운드 아이콘 설정")]
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    [Header("게임 오브젝트 & 위치")]
    public GameObject ball; 
    public Transform p1SpawnPoint; 
    public Transform p2SpawnPoint; 

    [Header("골 센서 연결")]
    public Collider2D goalSensorL; 
    public Collider2D goalSensorR; 
    
    [Header("오디오 클립")] 
    public AudioClip kickoffSound; 
    public AudioClip goalNetSound; 
    public AudioClip crowdSound;   
    private AudioSource audioSource; 

    [Header("게임 상태")]
    public int p1Score = 0;
    public int p2Score = 0;
    public GamePhase currentPhase = GamePhase.Regular;
    
    [Header("골 이펙트")]
    public GameObject goalEffectObject; 

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
        audioSource = GetComponent<AudioSource>();

        pausePanel.SetActive(false);
        if(endPanel != null) endPanel.SetActive(false);

        Time.timeScale = 1f;
        
        isMuted = false;
        AudioListener.volume = 1f;
        
        if (soundButtonImage != null && soundOnSprite != null)
        {
            soundButtonImage.sprite = soundOnSprite;
        }

        UpdateTimerUI();
        UpdateScoreUI();
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

    // (사운드 및 타이머 로직 생략 없이 그대로 유지)
    void PlayKickoffSound()
    {
        if (audioSource != null && kickoffSound != null) audioSource.PlayOneShot(kickoffSound);
    }

    void PlayGoalSound()
    {
        if (audioSource == null) return;
        if (goalNetSound != null) audioSource.PlayOneShot(goalNetSound);
        if (crowdSound != null) audioSource.PlayOneShot(crowdSound);
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
        if (goalEffectObject != null) goalEffectObject.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        if (goalEffectObject != null) goalEffectObject.SetActive(false);
        
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        ball.transform.position = new Vector3(0, 2, 0); 
        ballRb.linearVelocity = Vector2.zero;
        ballRb.angularVelocity = 0f;

        ResetPlayers();
        
        WallSkill[] walls = FindObjectsOfType<WallSkill>();
        foreach (WallSkill wall in walls) Destroy(wall.gameObject);

        if(goalSensorL != null) goalSensorL.enabled = true;
        if(goalSensorR != null) goalSensorR.enabled = true;

        isGoalCeremony = false; 
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
            p1.GetComponent<PlayerKick>().ResetStatus();
        }

        if (p2 != null)
        {
            p2.transform.position = p2SpawnPoint.position;
            p2.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            p2.GetComponent<PlayerKick>().ResetStatus();
        }
    }

    // === [🔥 중요] 승패에 따라 캐릭터 이미지 교체 ===
    void EndGame()
    {
        currentPhase = GamePhase.GameOver;
        timerText.text = "GAME OVER";
        
        if (endPanel != null)
        {
            endPanel.SetActive(true);
            
            // 1. 누가 무슨 캐릭터를 골랐는지 데이터 가져오기
            int p1Idx = GameData.p1CharacterIdx;
            int p2Idx = GameData.p2CharacterIdx;

            // 2. 승패 판정 및 이미지 적용
            if (p1Score > p2Score) // P1 승리
            {
                // P1은 이긴 그림, P2는 진 그림
                if(p1PortraitImg != null) p1PortraitImg.sprite = characterEndings[p1Idx].winSprite;
                if(p2PortraitImg != null) p2PortraitImg.sprite = characterEndings[p2Idx].loseSprite;

                // 텍스트 설정
                if(p1ResultTextImg != null) p1ResultTextImg.sprite = winTextSprite;
                if(p2ResultTextImg != null) p2ResultTextImg.sprite = loseTextSprite;
            }
            else if (p2Score > p1Score) // P2 승리
            {
                // P1은 진 그림, P2는 이긴 그림
                if(p1PortraitImg != null) p1PortraitImg.sprite = characterEndings[p1Idx].loseSprite;
                if(p2PortraitImg != null) p2PortraitImg.sprite = characterEndings[p2Idx].winSprite;

                // 텍스트 설정
                if(p1ResultTextImg != null) p1ResultTextImg.sprite = loseTextSprite;
                if(p2ResultTextImg != null) p2ResultTextImg.sprite = winTextSprite;
            }
            else // 무승부 (둘 다 진 그림 혹은 무승부 그림)
            {
                if(p1PortraitImg != null) p1PortraitImg.sprite = characterEndings[p1Idx].loseSprite;
                if(p2PortraitImg != null) p2PortraitImg.sprite = characterEndings[p2Idx].loseSprite;

                if(p1ResultTextImg != null) p1ResultTextImg.sprite = drawTextSprite;
                if(p2ResultTextImg != null) p2ResultTextImg.sprite = drawTextSprite;
            }

            // 원본 비율로 맞추기 (이미지가 찌그러진다면 주석 해제하세요)
            // p1PortraitImg.SetNativeSize();
            // p2PortraitImg.SetNativeSize();
        }

        Time.timeScale = 0f;
    }

    public void OnRestartClick()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMenuClick()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene"); 
    }

    public void OnQuitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    // 일시정지 함수들
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
        AudioListener.volume = isMuted ? 0f : 1f;
        if (soundButtonImage != null)
            soundButtonImage.sprite = isMuted ? soundOffSprite : soundOnSprite;
    }
}