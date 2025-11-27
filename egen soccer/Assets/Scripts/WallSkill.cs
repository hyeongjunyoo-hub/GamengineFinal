using UnityEngine;
using System.Collections;

public class WallSkill : MonoBehaviour
{
    public float riseSpeed = 10.0f; // 이동 속도
    public float targetY = -13.6f;  // 땅 위로 올라왔을 때 Y좌표 (목표 높이)
    public float lifeTime = 5.0f;   // 버티는 시간

    private Vector3 startPos; // 처음 위치 (땅 밑)
    private Vector3 endPos;   // 목표 위치 (땅 위)

    void Start()
    {
        // 1. 현재 위치(땅 밑)를 기억해둠 (나중에 돌아가야 하니까)
        startPos = transform.position;
        
        // 2. 올라갈 목표 위치 계산 (X, Z는 그대로, Y만 변경)
        endPos = new Vector3(transform.position.x, targetY, transform.position.z);

        // 3. 움직임 시작!
        StartCoroutine(WallRoutine());
    }

    IEnumerator WallRoutine()
    {
        // [1단계] 위로 솟아오름 (Rise)
        while (Vector3.Distance(transform.position, endPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos, riseSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = endPos; // 위치 딱 맞추기

        // [2단계] 버티기 (Wait)
        yield return new WaitForSeconds(lifeTime);

        // [3단계] 다시 땅 밑으로 꺼짐 (Sink)
        while (Vector3.Distance(transform.position, startPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, riseSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = startPos; // 위치 딱 맞추기

        // [4단계] 임무 완료 후 삭제
        Destroy(gameObject);
    }
}