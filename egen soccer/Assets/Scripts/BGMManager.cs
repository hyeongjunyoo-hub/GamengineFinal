using UnityEngine;

public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;

    void Awake()
    {
        // 1. 이미 BGM 관리자가 있다면? (중복 방지)
        if (instance != null)
        {
            Destroy(gameObject); // 새로 생긴 나는 필요 없으니 삭제
            return;
        }

        // 2. 내가 첫 번째라면? (유일한 관리자)
        instance = this;
        DontDestroyOnLoad(gameObject); // 씬이 넘어가도 나를 파괴하지 마라!
    }
}