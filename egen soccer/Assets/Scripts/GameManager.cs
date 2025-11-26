using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 이미지 제어용
using TMPro; // 텍스트메쉬프로(타이머)용

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("시간 설정 (초 단위)")]
    public float regularTime = 120f; // 정규 2분
    public float overtime = 30f;     // 추가 30초

    [Header("UI 연결")]
    public TextMeshProUGUI timerText; // 타이머 텍스트
    public GameObject pausePanel;     // 일시정지 패널
    public Image soundButtonImage;    // 소리 버튼 (색깔 바꿀 대상)

    [Header("게임 상태 (수정 X)")]
    public int p1Score = 0;
    public int p2Score = 0;
    public GamePhase currentPhase = GamePhase.Regular;

    // 내부 변수
    private float currentTime;
    private bool isPaused = false;
    private bool isMuted = false;

    public enum GamePhase
    {
        Regular,    // 정규
        Overtime,   // 연장
        GoldenGoal, // 골든골
        GameOver    // 종료
    }

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        currentPhase = GamePhase.Regular;
        currentTime = regularTime;

        // 시작 시 패널 끄고, 시간 정상화, 소리 켜기
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        
        // 소리 초기화 (켜진 상태)
        isMuted = false;
        AudioListener.volume = 1f;
        soundButtonImage.color = Color.white;

        UpdateTimerUI();
    }

    void Update()
    {
        // 게임 오버가 아니고 일시정지가 아닐 때만 시간 흐름
        if (currentPhase != GamePhase.GameOver && !isPaused)
        {
            HandleTimer();
        }

        // ESC 키 기능
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) OnResumeClick();
            else OnPauseClick();
        }
    }

    // === 1. 시간 및 규칙 로직 ===
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
            if (p1Score == p2Score) // 비김 -> 연장전
            {
                currentPhase = GamePhase.Overtime;
                currentTime = overtime;
            }
            else EndGame(); // 승부 남 -> 종료
        }
        else if (currentPhase == GamePhase.Overtime)
        {
            if (p1Score == p2Score) // 또 비김 -> 골든골
            {
                currentPhase = GamePhase.GoldenGoal;
            }
            else EndGame(); // 승부 남 -> 종료
        }
    }

    void UpdateTimerUI()
    {
        if (currentPhase == GamePhase.GoldenGoal) return;

        int minutes = Mathf.FloorToInt(currentTime / 60F);
        int seconds = Mathf.FloorToInt(currentTime % 60F);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (currentPhase == GamePhase.Overtime) timerText.color = Color.red;
        else timerText.color = Color.white;
    }

    public void AddScore(int playerNum)
    {
        if (currentPhase == GamePhase.GameOver) return;

        if (playerNum == 1) p1Score++;
        else p2Score++;

        if (currentPhase == GamePhase.GoldenGoal) EndGame(); // 골든골이면 바로 종료
    }

    void EndGame()
    {
        currentPhase = GamePhase.GameOver;
        Debug.Log("게임 종료!");
        // 여기에 나중에 결과창 띄우는 코드 추가
    }


    // === 2. UI 버튼 기능 (질문하신 내용 포함) ===

    // 일시정지 버튼
    public void OnPauseClick()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // 시간 정지
    }

    // Resume(계속하기) 버튼
    public void OnResumeClick()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // 시간 재개
    }

    // Sound(소리) 버튼 (질문하신 코드 그대로 적용!)
    public void OnSoundClick()
    {
        isMuted = !isMuted; // 상태 반전

        if (isMuted)
        {
            AudioListener.volume = 0f; // 소리 끄기
            soundButtonImage.color = Color.gray; // 버튼 어둡게
        }
        else
        {
            AudioListener.volume = 1f; // 소리 켜기
            soundButtonImage.color = Color.white; // 버튼 밝게
        }
    }

    // Quit(나가기) 버튼
    public void OnQuitClick()
    {
        Time.timeScale = 1f; // (중요) 시간 다시 돌려놓기
        SceneManager.LoadScene("StartScene"); // 메뉴 씬 이름 확인하세요!
    }
}