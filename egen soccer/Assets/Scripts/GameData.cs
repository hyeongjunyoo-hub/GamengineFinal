using UnityEngine;

public class GameData : MonoBehaviour
{
    // static 변수는 씬이 바뀌어도 값이 유지됩니다.
    public static int p1CharacterIdx = 0;
    public static int p2CharacterIdx = 0;
}
//씬이 넘어가도 "누가 뭘 골랐는지" 기억해야 하므로, 이전에 말씀드린 GameData 스크립트가 필요합니다. (이미 만드셨다면 내용만 확인하세요