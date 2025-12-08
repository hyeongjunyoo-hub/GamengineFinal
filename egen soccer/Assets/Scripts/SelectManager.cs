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

    [Header("시스템 버튼 설정")]
    public Image soundBtnImage;       
    public Sprite soundOnSprite;      
    public Sprite soundOffSprite;     

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

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false; 
        
        // [🔥 핵심] 시작할 때 저장된 소리 설정 불러오기
        ApplySoundSetting();
        
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

    // === 버튼 기능 ===

    public void ToggleSound()
    {
        GameData.isGlobalMuted = !GameData.isGlobalMuted; // 상태 반전
        ApplySoundSetting(); // 적용
    }

    void ApplySoundSetting()
    {
        if (GameData.isGlobalMuted)
        {
            AudioListener.volume = 0f; 
            if (soundBtnImage != null) soundBtnImage.sprite = soundOffSprite;
        }
        else
        {
            AudioListener.volume = 1f; 
            if (soundBtnImage != null) soundBtnImage.sprite = soundOnSprite;
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    // === 내부 로직 ===

    void ChangeCharacter(int playerNum, int direction)
    {
        if (audioSource != null && passSound != null) audioSource.PlayOneShot(passSound);
        
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
        if (ready && audioSource != null && selectSound != null) audioSource.PlayOneShot(selectSound);

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
        
        // [🔥 핵심] 게임 씬 넘어가기 전에 로비 BGM 파괴 -> PlayScene은 조용하게 시작
        GameObject bgm = GameObject.Find("BGM_Player");
        if (bgm != null) Destroy(bgm); 
        
        SceneManager.LoadScene("PlayScene"); 
    }
}