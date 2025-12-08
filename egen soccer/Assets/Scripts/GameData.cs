using UnityEngine;

public class GameData : MonoBehaviour
{
    // 캐릭터 선택 정보 (기존)
    public static int p1CharacterIdx = 0;
    public static int p2CharacterIdx = 0;

    // [🔥 추가됨] 소리 설정 저장 (true면 음소거, false면 소리 켬)
    // static으로 선언해서 씬이 바뀌어도 값이 유지됩니다.
    public static bool isGlobalMuted = false; 
}