using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 이미지 제어용
using UnityEngine.Video; // 비디오 제어용

public class SceneChanger : MonoBehaviour
{
    [Header("UI 연결")]
    public Image soundBtnImage;       // 버튼에 있는 Image 컴포넌트
    public Sprite soundOnSprite;      // 🔊 소리 켜진 아이콘 (초록색/기본)
    public Sprite soundOffSprite;     // 🔇 소리 꺼진 아이콘 (빨간색)

    [Header("비디오 연결")]
    public VideoPlayer startVideo;    // 배경 비디오

    private bool isMuted = false;

    public void GoToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ToggleSound()
    {
        isMuted = !isMuted; // 상태 반전

        if (isMuted) // 소리 끄기 (MUTE)
        {
            AudioListener.volume = 0f;
            if (startVideo != null) startVideo.SetDirectAudioVolume(0, 0f);

            // ✨ 이미지 교체: 꺼진 그림(빨간색)으로
            if (soundBtnImage != null) 
            {
                soundBtnImage.sprite = soundOffSprite;
                // 혹시 색깔이 회색으로 남아있을까봐 흰색으로 초기화
                soundBtnImage.color = Color.white; 
            }
        }
        else // 소리 켜기 (UNMUTE)
        {
            AudioListener.volume = 1f;
            if (startVideo != null) startVideo.SetDirectAudioVolume(0, 1f);

            // ✨ 이미지 교체: 켜진 그림(원래대로)으로
            if (soundBtnImage != null) 
            {
                soundBtnImage.sprite = soundOnSprite;
                soundBtnImage.color = Color.white;
            }
        }
    }
}