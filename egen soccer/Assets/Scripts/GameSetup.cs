using UnityEngine;

public class GameSetup : MonoBehaviour
{
    [Header("캐릭터 프리팹 (순서: 0진, 1이, 2전, 3원)")]
    public GameObject[] characterPrefabs; // 프리팹 4개 연결

    [Header("스폰 위치")]
    public Transform p1SpawnPoint; // 왼쪽
    public Transform p2SpawnPoint; // 오른쪽

    void Start()
    {
        // 1. 저장된 데이터 가져오기
        int p1Idx = GameData.p1CharacterIdx;
        int p2Idx = GameData.p2CharacterIdx;

        // 2. 플레이어 1 생성 (왼쪽)
        GameObject p1 = Instantiate(characterPrefabs[p1Idx], p1SpawnPoint.position, Quaternion.identity);
        
        // [중요!] 이름을 "Player1"로 바꿔야 ScoreManager가 찾을 수 있습니다.
        p1.name = "Player1"; 
        
        SettingPlayer(p1, 1); 

        // 3. 플레이어 2 생성 (오른쪽)
        GameObject p2 = Instantiate(characterPrefabs[p2Idx], p2SpawnPoint.position, Quaternion.identity);
        
        // [중요!] 이름을 "Player2"로 바꿉니다.
        p2.name = "Player2";
        
        SettingPlayer(p2, 2); 
    }

    void SettingPlayer(GameObject player, int playerNum)
    {
        PlayerKick script = player.GetComponent<PlayerKick>();
        
        if (script == null) return;

        // [🔥 추가됨] 플레이어 번호(1 or 2)를 스크립트에 입력해줍니다.
        // 이걸 해줘야 PlayerKick이 자기가 누구인지 알고 GameManager에게 스킬 횟수를 물어볼 수 있습니다.
        script.playerNum = playerNum; 

        if (playerNum == 1)
        {
            // P1 설정 (WASD, 오른쪽 보기)
            script.facingDirection = 1f;
            script.leftKey = KeyCode.A;
            script.rightKey = KeyCode.D;
            script.jumpKey = KeyCode.W;
            script.kickKey = KeyCode.S;
            script.skillKey = KeyCode.R;
        }
        else
        {
            // P2 설정 (화살표, 왼쪽 보기)
            script.facingDirection = -1f;
            
            // 생성 후 스케일 뒤집기
            Vector3 scale = player.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * -1f; 
            player.transform.localScale = scale;

            script.leftKey = KeyCode.LeftArrow;
            script.rightKey = KeyCode.RightArrow;
            script.jumpKey = KeyCode.UpArrow;
            script.kickKey = KeyCode.DownArrow;
            script.skillKey = KeyCode.RightShift;
        }
    }
}