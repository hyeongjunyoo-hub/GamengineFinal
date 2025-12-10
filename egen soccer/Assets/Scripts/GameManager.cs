using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CharacterEndingData
{
    public string characterName;
    public Sprite winSprite;
    public Sprite loseSprite;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("연출 스크립트 연결")] 
    public PhaseDisplay phaseDisplay;
    [Header("시간 설정")]
    public float regularTime = 120f; 
    public float overtime = 30f;     

    [Header("스킬 설정")]
    public int maxSkillCount = 5; 

    [Header("스킬 아이콘 UI")]
    public GameObject[] p1SkillIcons; 
    public GameObject[] p2SkillIcons; 

    public int p1CurrentSkillCount;
    public int p2CurrentSkillCount;

    [Header("UI 연결")]
    public TextMeshProUGUI timerText; 
    public GameObject pausePanel;     
    public Image soundButtonImage;    
    public TextMeshProUGUI p1ScoreText; 
    public TextMeshProUGUI p2ScoreText;

    [Header("결과 화면 UI")]
    public GameObject endPanel;       
    public Image p1PortraitImg;       
    public Image p1ResultTextImg;     
    public Image p2PortraitImg;       
    public Image p2ResultTextImg;     
    
    [Header("캐릭터 엔딩 이미지")]
    public List<CharacterEndingData> characterEndings; 

    [Header("결과 텍스트 에셋")]
    public Sprite winTextSprite;      
    public Sprite loseTextSprite;     
    public Sprite drawTextSprite;     

    [Header("사운드 아이콘")]
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    [Header("게임 오브젝트")]
    public GameObject ball; 
    public Transform p1SpawnPoint; 
    public Transform p2SpawnPoint; 

    [Header("골 센서")]
    public Collider2D goalSensorL; 
    public Collider2D goalSensorR;
    
    [Header("오디오")] 
    public AudioClip kickoffSound; 
    public AudioClip goalNetSound; 
    public AudioClip crowdSound;
    public AudioClip winSound;   
    private AudioSource audioSource; 

    [Header("상태 변수")]
    public int p1Score = 0;
    public int p2Score = 0;
    public GamePhase currentPhase = GamePhase.Regular;
    public GameObject goalEffectObject; 

    private float currentTime;
    private bool isPaused = false;
    private bool isGoalCeremony = false; 
    private Color defaultColor; 

    // PlayScene에서만 쓰는 독립적인 소리 변수
    private bool isLocalMuted = false; 

    public enum GamePhase { Regular, Overtime, GoldenGoal, GameOver }

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
        
        // [소리 설정] 시작할 때 무조건 소리 켜기 (다른 씬 설정 무시)
        isLocalMuted = false;
        AudioListener.volume = 1f;
        if (soundButtonImage != null && soundOnSprite != null)
            soundButtonImage.sprite = soundOnSprite;

        p1CurrentSkillCount = maxSkillCount;
        p2CurrentSkillCount = maxSkillCount;
        UpdateSkillUI();

        UpdateTimerUI();
        UpdateScoreUI();
        PlayKickoffSound();
        if (phaseDisplay != null) phaseDisplay.ShowKickoff();
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

    public bool TryUseSkill(int playerNum)
    {
        if (playerNum == 1)
        {
            if (p1CurrentSkillCount > 0)
            {
                p1CurrentSkillCount--; 
                UpdateSkillUI();       
                return true;
            }
        }
        else 
        {
            if (p2CurrentSkillCount > 0)
            {
                p2CurrentSkillCount--;
                UpdateSkillUI();
                return true;
            }
        }
        return false;
    }


    void UpdateSkillUI()
    {
        if (p1SkillIcons != null)
        {
            for (int i = 0; i < p1SkillIcons.Length; i++)
            {
                if (i < p1CurrentSkillCount) p1SkillIcons[i].SetActive(true);
                else p1SkillIcons[i].SetActive(false);
            }
        }

        if (p2SkillIcons != null)
        {
            for (int i = 0; i < p2SkillIcons.Length; i++)
            {
                if (i < p2CurrentSkillCount) p2SkillIcons[i].SetActive(true);
                else p2SkillIcons[i].SetActive(false);
            }
        }
    }

    // === 소리 독립 제어 ===
    public void OnSoundClick()
    {
        isLocalMuted = !isLocalMuted;
        ApplyLocalSoundSetting();
    }

    void ApplyLocalSoundSetting()
    {
        if (isLocalMuted)
        {
            AudioListener.volume = 0f;
            if (soundButtonImage != null) soundButtonImage.sprite = soundOffSprite;
        }
        else
        {
            AudioListener.volume = 1f;
            if (soundButtonImage != null) soundButtonImage.sprite = soundOnSprite;
        }
    }

    // === 메뉴로 나가기 ===
    public void OnMenuClick() 
    { 
        Time.timeScale = 1f; 
        
        // 나가기 전에 소리를 무조건 다시 켜놓기
        // MenuScene에 도착했을 때 그쪽 소리 설정
        AudioListener.volume = 1f; 

        // StartScene이 아니라 MenuScene으로 이동
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

    // ... (나머지 로직 동일) ...
    void PlayKickoffSound() { if (audioSource != null && kickoffSound != null) audioSource.PlayOneShot(kickoffSound); }
    void PlayGoalSound() { if (audioSource == null) return; if (goalNetSound != null) audioSource.PlayOneShot(goalNetSound); if (crowdSound != null) audioSource.PlayOneShot(crowdSound); }
    void HandleTimer() { if (currentPhase == GamePhase.GoldenGoal) { timerText.text = "GOLDEN GOAL"; timerText.color = Color.yellow; return; } currentTime -= Time.deltaTime; if (currentTime <= 0) { currentTime = 0; CheckPhaseChange(); } UpdateTimerUI(); }
    void CheckPhaseChange()
    {
        if (currentPhase == GamePhase.Regular)
        {
            if (p1Score == p2Score)
            {
                currentPhase = GamePhase.Overtime;
                currentTime = overtime;
                // [🔥 추가 3] 연장전(Injury Time) 문구 띄우기
                if (phaseDisplay != null) phaseDisplay.ShowInjuryTime();
            }
            else
            {
                EndGame();
            }
        }
        else if (currentPhase == GamePhase.Overtime)
        {
            if (p1Score == p2Score)
            {
                currentPhase = GamePhase.GoldenGoal;
                // 골든골(Golden Goal) 문구 띄우기
                if (phaseDisplay != null) phaseDisplay.ShowGoldenGoal();
            }
            else
            {
                EndGame();
            }
        }
    }
    void UpdateTimerUI() { if (currentPhase == GamePhase.GoldenGoal) return; int minutes = Mathf.FloorToInt(currentTime / 60F); int seconds = Mathf.FloorToInt(currentTime % 60F); timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds); if (currentPhase == GamePhase.Overtime || (currentPhase == GamePhase.Regular && currentTime <= 10.0f)) timerText.color = Color.red; else timerText.color = defaultColor; }
    public void AddScore(int playerNum) { if (currentPhase == GamePhase.GameOver) return; if (playerNum == 1) p1Score++; else p2Score++; UpdateScoreUI(); PlayGoalSound(); if (currentPhase == GamePhase.GoldenGoal) EndGame(); else StartCoroutine(ResetRound()); }
    void UpdateScoreUI() { p1ScoreText.text = p1Score.ToString(); p2ScoreText.text = p2Score.ToString(); }
    IEnumerator ResetRound() { isGoalCeremony = true; if (goalEffectObject != null) goalEffectObject.SetActive(true); yield return new WaitForSeconds(2.0f); if (goalEffectObject != null) goalEffectObject.SetActive(false); Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>(); ball.transform.position = new Vector3(0, 2, 0); ballRb.linearVelocity = Vector2.zero; ballRb.angularVelocity = 0f; ResetPlayers(); WallSkill[] walls = FindObjectsOfType<WallSkill>(); foreach (WallSkill wall in walls) Destroy(wall.gameObject); if(goalSensorL != null) goalSensorL.enabled = true; if(goalSensorR != null) goalSensorR.enabled = true; isGoalCeremony = false; PlayKickoffSound();}
    void ResetPlayers() { GameObject p1 = GameObject.Find("Player1"); GameObject p2 = GameObject.Find("Player2"); if (p1 != null) { p1.transform.position = p1SpawnPoint.position; p1.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; p1.GetComponent<PlayerKick>().ResetStatus(); } if (p2 != null) { p2.transform.position = p2SpawnPoint.position; p2.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; p2.GetComponent<PlayerKick>().ResetStatus();  } }
    void EndGame() { currentPhase = GamePhase.GameOver; timerText.text = "GAME OVER"; if (endPanel != null) { endPanel.SetActive(true); if (audioSource != null && winSound != null) audioSource.PlayOneShot(winSound); int p1Idx = GameData.p1CharacterIdx; int p2Idx = GameData.p2CharacterIdx; if (p1Score > p2Score) { if(p1PortraitImg != null) p1PortraitImg.sprite = characterEndings[p1Idx].winSprite; if(p2PortraitImg != null) p2PortraitImg.sprite = characterEndings[p2Idx].loseSprite; if(p1ResultTextImg != null) p1ResultTextImg.sprite = winTextSprite; if(p2ResultTextImg != null) p2ResultTextImg.sprite = loseTextSprite; } else if (p2Score > p1Score) { if(p1PortraitImg != null) p1PortraitImg.sprite = characterEndings[p1Idx].loseSprite; if(p2PortraitImg != null) p2PortraitImg.sprite = characterEndings[p2Idx].winSprite; if(p1ResultTextImg != null) p1ResultTextImg.sprite = loseTextSprite; if(p2ResultTextImg != null) p2ResultTextImg.sprite = winTextSprite; } else { if(p1PortraitImg != null) p1PortraitImg.sprite = characterEndings[p1Idx].loseSprite; if(p2PortraitImg != null) p2PortraitImg.sprite = characterEndings[p2Idx].loseSprite; if(p1ResultTextImg != null) p1ResultTextImg.sprite = drawTextSprite; if(p2ResultTextImg != null) p2ResultTextImg.sprite = drawTextSprite; } } Time.timeScale = 0f; }
    public void OnRestartClick() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void OnPauseClick() { isPaused = true; pausePanel.SetActive(true); Time.timeScale = 0f; }
    public void OnResumeClick() { isPaused = false; pausePanel.SetActive(false); Time.timeScale = 1f; }
}