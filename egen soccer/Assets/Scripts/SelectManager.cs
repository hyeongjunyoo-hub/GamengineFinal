using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic; 
using TMPro; // TextMeshPro

public class SelectManager : MonoBehaviour
{
    [Header("캐릭터 데이터 (순서 맞춰서 넣으세요!)")]
    public List<Sprite> characterSprites; // 이미지들
    public List<string> characterNames;   // [추가] 이름들 (예: 진지황, 이재묭...)

    [Header("UI 연결")]
    public Image p1Image;
    public Image p2Image;
    public TextMeshProUGUI p1NameText; // [추가] P1 이름 표시할 텍스트
    public TextMeshProUGUI p2NameText; // [추가] P2 이름 표시할 텍스트
    public GameObject p1ReadyText; 
    public GameObject p2ReadyText;
    // [추가됨] 오디오 설정
    [Header("오디오 설정")]
    public AudioClip passSound;   // 캐릭터 넘길 때 소리 (character_pass)
    public AudioClip selectSound; // 선택 확정 소리 (character_select)
    private AudioSource audioSource;

    // 내부 변수
    private int p1Idx = 0;
    private int p2Idx = 0;
    private bool isP1Ready = false;
    private bool isP2Ready = false;
    // [추가] 게임 시작 중인지 확인하는 변수 (중복 실행 방지)
    private bool isStarting = false;

    void Start()
    {
        // [추가] 오디오 소스 컴포넌트 가져오기 (없으면 자동 추가)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false; // 시작하자마자 소리나지 않게
        UpdateUI();
        p1ReadyText.SetActive(false);
        p2ReadyText.SetActive(false);
    }

    void Update()
    {
        // 게임이 시작 중이면 입력을 받지 않음
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

    void ChangeCharacter(int playerNum, int direction)
    {
        // [추가] 캐릭터 넘기는 소리 재생
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
        // [추가] 선택 확정(ready가 true)일 때만 선택 소리 재생
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

    // [수정됨] 이미지뿐만 아니라 이름도 같이 바꿈
    void UpdateUI()
    {
        // 1. 이미지 변경
        p1Image.sprite = characterSprites[p1Idx];
        p2Image.sprite = characterSprites[p2Idx];

        // 2. [추가] 이름 텍스트 변경
        // (리스트에 이름이 들어있을 때만 실행)
        if (characterNames.Count > 0)
        {
            p1NameText.text = characterNames[p1Idx];
            p2NameText.text = characterNames[p2Idx];
        }
    }

    IEnumerator StartGameRoutine()
    {
        isStarting = true; // 중복 실행 방지

        // 데이터 저장
        GameData.p1CharacterIdx = p1Idx;
        GameData.p2CharacterIdx = p2Idx;

        Debug.Log("캐릭터 선택 완료! 1초 뒤 게임 시작...");

        // [핵심] 1초 동안 대기 (소리가 재생될 시간을 줌)
        yield return new WaitForSeconds(1.0f);

        // 씬 이동
        SceneManager.LoadScene("PlayScene"); 
    }
}