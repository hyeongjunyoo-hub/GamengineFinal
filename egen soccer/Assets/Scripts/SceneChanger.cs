using UnityEngine;
using UnityEngine.SceneManagement; // ✨ 씬 이동을 위해 이게 꼭 있어야 해요!

public class SceneChanger : MonoBehaviour
{
    public void GoToMenu()
    {
        Debug.Log("씬 이동!");
        // "MenuScene"은 이동하려는 씬 파일 이름과 토씨 하나 안 틀리고 똑같아야 합니다.
        SceneManager.LoadScene("MenuScene");
    }
    
    // 나중에 Quit 버튼에도 쓰세요
    public void QuitGame()
    {
        Debug.Log("게임 종료!");
        Application.Quit();
    }
}