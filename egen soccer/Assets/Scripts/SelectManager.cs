using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic; 
using TMPro; 

public class SelectManager : MonoBehaviour
{
    [Header("캐릭터 데이터 (순서: 0진, 1이, 2전, 3원)")]
    public List<Sprite> characterSprites; 
    public List<string> characterNames;   
    
    [Header("스킬 데이터")]
    public List<Sprite> skillIcons;       
    [TextArea(2, 3)]                      
    public List<string> skillDescriptions; 

    [Header("UI 연결 - P1")]
    public Image p1Image;
    public TextMeshProUGUI p1NameText;
    public GameObject p1ReadyText; 
    public Image p1SkillImage;            
    public TextMeshProUGUI p1SkillDescText; 

    [Header("UI 연결 - P2")]
    public Image p2Image;
    public TextMeshProUGUI p2NameText;
    public GameObject p2ReadyText;
    public Image p2SkillImage;            
    public TextMeshProUGUI p2SkillDescText; 

    // [🔥 추가됨] 시스템 버튼 UI (소리/종료)
    [Header("시스템 버튼 설정")]
    public Image soundBtnImage;       // 소리 버튼의 아이콘(Image)
    public Sprite soundOnSprite;      // 🔊 켜짐 그림
    public Sprite soundOffSprite;     // 🔇 꺼짐 그림

    [Header("오디오 설정")]
    public AudioClip passSound;   
    public AudioClip selectSound; 
    private AudioSource audioSource;

    // 내부 변수
    private int p1Idx = 0;
    private int p2Idx = 0;
    private bool isP1Ready = false;
    private bool isP2Ready = false;
    private bool isStarting = false;
    private bool isMuted = false; // 소리 상태 변수

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false; 
        
        // [🔥 추가됨] 소리 버튼 초기화
        isMuted = false;
        AudioListener.volume = 1f;
        if (soundBtnImage != null && soundOnSprite != null)
        {
            soundBtnImage.sprite = soundOnSprite;
            soundBtnImage.color = Color.white;
        }
        
        UpdateUI();
        p1ReadyText.SetActive(false);
        p2ReadyText.SetActive(false);
    }

    void Update()
    {
        if (isStarting) return;

        // === Player 1 (WASD) ===
        if (!isP1Ready)
        {
            if (Input.GetKeyDown(KeyCode.A)) { ChangeCharacter(1, -1); }
            if (Input.GetKeyDown(KeyCode.D)) { ChangeCharacter(1, 1); }
            if (Input.GetKeyDown(KeyCode.S)) { SetReady(1, true); }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.W)) { SetReady(1, false); }
        }

        // === Player 2 (방향키) ===
        if (!isP2Ready)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) { ChangeCharacter(2, -1); }
            if (Input.GetKeyDown(KeyCode.RightArrow)) { ChangeCharacter(2, 1); }
            if (Input.GetKeyDown(KeyCode.DownArrow)) { SetReady(2, true); }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) { SetReady(2, false); }
        }

        if (isP1Ready && isP2Ready)
        {
            StartCoroutine(StartGameRoutine());
        }
    }

    // === [🔥 추가됨] 버튼 기능 구현 (StartScene과 동일) ===

    // 1. 소리 껐다 켜기 (이미지 교체 포함)
    public void ToggleSound()
    {
        isMuted = !isMuted; // 상태 반전

        if (isMuted) // 소리 끄기
        {
            AudioListener.volume = 0f; 
            if (soundBtnImage != null && soundOffSprite != null) 
            {
                soundBtnImage.sprite = soundOffSprite; // 빨간 아이콘
            }
        }
        else // 소리 켜기
        {
            AudioListener.volume = 1f; 
            if (soundBtnImage != null && soundOnSprite != null) 
            {
                soundBtnImage.sprite = soundOnSprite; // 초록 아이콘
            }
        }
    }

    // 2. 게임 종료 (Quit)
    public void QuitGame()
    {
        Debug.Log("게임 종료!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    // 3. (선택사항) 다시 타이틀 화면으로 돌아가고 싶다면?
    public void GoToTitle()
    {
        SceneManager.LoadScene("StartScene");
    }

    // ================================================

    void ChangeCharacter(int playerNum, int direction)
    {
        if (audioSource != null && passSound != null)
        {
            audioSource.PlayOneShot(passSound);
        }
        if (playerNum == 1)
        {
            p1Idx += direction;
            if (p1Idx < 0) p1Idx = characterSprites.Count - 1;
            if (p1Idx >= characterSprites.Count) p1Idx = 0;
        }
        else
        {
            p2Idx += direction;
            if (p2Idx < 0) p2Idx = characterSprites.Count - 1;
            if (p2Idx >= characterSprites.Count) p2Idx = 0;
        }
        UpdateUI();
    }

    void SetReady(int playerNum, bool ready)
    {
        if (ready && audioSource != null && selectSound != null)
        {
            audioSource.PlayOneShot(selectSound);
        }
        if (playerNum == 1)
        {
            isP1Ready = ready;
            p1ReadyText.SetActive(ready);
            p1Image.color = ready ? Color.gray : Color.white; 
        }
        else
        {
            isP2Ready = ready;
            p2ReadyText.SetActive(ready);
            p2Image.color = ready ? Color.gray : Color.white;
        }
    }

    void UpdateUI()
    {
        p1Image.sprite = characterSprites[p1Idx];
        p2Image.sprite = characterSprites[p2Idx];

        if (characterNames.Count > 0)
        {
            p1NameText.text = characterNames[p1Idx];
            p2NameText.text = characterNames[p2Idx];
        }

        if (skillIcons.Count > 0)
        {
            p1SkillImage.sprite = skillIcons[p1Idx];
            p2SkillImage.sprite = skillIcons[p2Idx];
            p1SkillImage.preserveAspect = true;
            p2SkillImage.preserveAspect = true;
        }

        if (skillDescriptions.Count > 0)
        {
            p1SkillDescText.text = skillDescriptions[p1Idx];
            p2SkillDescText.text = skillDescriptions[p2Idx];
        }
    }

    IEnumerator StartGameRoutine()
    {
        isStarting = true; 
        GameData.p1CharacterIdx = p1Idx;
        GameData.p2CharacterIdx = p2Idx;
        Debug.Log("캐릭터 선택 완료! 1초 뒤 게임 시작...");
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("PlayScene"); 
    }
}