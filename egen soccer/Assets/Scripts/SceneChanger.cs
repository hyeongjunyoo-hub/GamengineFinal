using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class SceneChanger : MonoBehaviour
{
    [Header("UI 연결")]
    public Image soundBtnImage;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    [Header("비디오 연결")]
    public VideoPlayer startVideo;

    // 내부 변수 삭제하고 GameData 사용

    void Start()
    {
        // 시작할 때 저장된 소리 설정 불러오기
        ApplySoundSetting();
    }

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
        // 상태 반전 후 저장
        GameData.isGlobalMuted = !GameData.isGlobalMuted;
        ApplySoundSetting();
    }

    // 소리 적용 함수 (중복 제거를 위해 분리)
    void ApplySoundSetting()
    {
        if (GameData.isGlobalMuted) // 소리 끄기
        {
            AudioListener.volume = 0f;
            if (startVideo != null) startVideo.SetDirectAudioVolume(0, 0f);
            if (soundBtnImage != null) soundBtnImage.sprite = soundOffSprite;
        }
        else // 소리 켜기
        {
            AudioListener.volume = 1f;
            if (startVideo != null) startVideo.SetDirectAudioVolume(0, 1f);
            if (soundBtnImage != null) soundBtnImage.sprite = soundOnSprite;
        }
    }
}