using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // 리스트 사용을 위해 필수
using TMPro; // TextMeshPro 사용 시 필수

public class SelectManager : MonoBehaviour
{
    [Header("캐릭터 이미지들 (4개 순서대로 넣으세요)")]
    public List<Sprite> characterSprites; 

    [Header("UI 연결")]
    public Image p1Image;
    public Image p2Image;
    public GameObject p1ReadyText; // P1 READY 글자
    public GameObject p2ReadyText; // P2 READY 글자

    // 내부 변수
    private int p1Idx = 0;
    private int p2Idx = 0;
    private bool isP1Ready = false;
    private bool isP2Ready = false;

    void Start()
    {
        // 시작 시 첫 번째 캐릭터 보여주기
        UpdateUI();
        p1ReadyText.SetActive(false);
        p2ReadyText.SetActive(false);
    }

    void Update()
    {
        // === Player 1 (WASD) ===
        if (!isP1Ready) // 준비 안 된 상태에서만 변경 가능
        {
            if (Input.GetKeyDown(KeyCode.A)) { ChangeCharacter(1, -1); }
            if (Input.GetKeyDown(KeyCode.D)) { ChangeCharacter(1, 1); }
            if (Input.GetKeyDown(KeyCode.S)) { SetReady(1, true); } // S키로 선택 완료
        }
        else // 준비 상태에서 취소하려면
        {
            if (Input.GetKeyDown(KeyCode.W)) { SetReady(1, false); } // W키로 취소
        }

        // === Player 2 (방향키) ===
        if (!isP2Ready)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) { ChangeCharacter(2, -1); }
            if (Input.GetKeyDown(KeyCode.RightArrow)) { ChangeCharacter(2, 1); }
            if (Input.GetKeyDown(KeyCode.DownArrow)) { SetReady(2, true); } // 아래키로 선택 완료
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) { SetReady(2, false); } // 위키로 취소
        }

        // === 게임 시작 체크 ===
        // 둘 다 준비 완료되면 게임 씬으로 이동
        if (isP1Ready && isP2Ready)
        {
            StartGame();
        }
    }

    // 캐릭터 변경 함수 (playerNum: 1 or 2, direction: -1(이전) or 1(다음))
    void ChangeCharacter(int playerNum, int direction)
    {
        if (playerNum == 1)
        {
            p1Idx += direction;
            // 인덱스가 0보다 작아지면 마지막으로, 리스트 길이보다 커지면 0으로 (무한 롤링)
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
        if (playerNum == 1)
        {
            isP1Ready = ready;
            p1ReadyText.SetActive(ready); // READY 글자 켜기/끄기
            // 선택 완료 시 약간 어둡게 처리 (시각적 효과)
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
    }

    void StartGame()
    {
        // 선택한 정보 저장
        GameData.p1CharacterIdx = p1Idx;
        GameData.p2CharacterIdx = p2Idx;

        // 게임 씬으로 이동 (씬 이름 확인!)
        SceneManager.LoadScene("PlayScene"); 
    }
}