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
        // 1. 저장된 데이터 가져오기 (GameData에서)
        int p1Idx = GameData.p1CharacterIdx;
        int p2Idx = GameData.p2CharacterIdx;

        // 2. 플레이어 1 생성 (왼쪽)
        GameObject p1 = Instantiate(characterPrefabs[p1Idx], p1SpawnPoint.position, Quaternion.identity);
        SettingPlayer(p1, 1); // P1 설정 적용

        // 3. 플레이어 2 생성 (오른쪽)
        GameObject p2 = Instantiate(characterPrefabs[p2Idx], p2SpawnPoint.position, Quaternion.identity);
        SettingPlayer(p2, 2); // P2 설정 적용
    }

    void SettingPlayer(GameObject player, int playerNum)
    {
        PlayerKick script = player.GetComponent<PlayerKick>();
        
        if (script ==null) return;
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
