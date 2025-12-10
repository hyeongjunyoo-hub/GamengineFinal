using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhaseDisplay : MonoBehaviour
{
    [Header("이미지 UI 연결")]
    public GameObject kickoffObj;     
    public GameObject injuryTimeObj;  
    public GameObject goldenGoalObj;  

    [Header("연출 설정")]
    public float popUpDuration = 0.5f; // 커지는 시간
    public float stayDuration = 1.0f;  // 유지되는 시간
    public float popDownDuration = 0.5f; // 작아지는 시간

    // 최종 크기 조절 변수
    [Range(0.1f, 2.0f)] 
    public float finalSize = 0.6f; 

    public void ShowKickoff() { StartCoroutine(PopUpRoutine(kickoffObj)); }
    public void ShowInjuryTime() { StartCoroutine(PopUpRoutine(injuryTimeObj)); }
    public void ShowGoldenGoal() { StartCoroutine(PopUpRoutine(goldenGoalObj)); }

    IEnumerator PopUpRoutine(GameObject targetObj)
    {
        if (targetObj == null) yield break;

        targetObj.SetActive(true);
        targetObj.transform.localScale = Vector3.zero; 

        // 1. 등장 (커지기) - 0에서 목표 크기까지 부드럽게
        float timer = 0f;
        while (timer < popUpDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / popUpDuration;
        
            t = t * t * (3f - 2f * t); 
            
            float scale = Mathf.Lerp(0f, finalSize, t);
            targetObj.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        targetObj.transform.localScale = Vector3.one * finalSize; // 크기 확정

        // 2. 대기하기
        yield return new WaitForSecondsRealtime(stayDuration);

        // 3. 퇴장 (작아지기) - 목표 크기에서 0까지 부드럽게
        timer = 0f;
        while (timer < popDownDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / popDownDuration;
            
            // 여기도 SmoothStep 적용
            t = t * t * (3f - 2f * t);

            float scale = Mathf.Lerp(finalSize, 0f, t);
            targetObj.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        targetObj.transform.localScale = Vector3.zero; 
        targetObj.SetActive(false); 
    }
}