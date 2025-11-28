using UnityEngine;

public class GoalCheck : MonoBehaviour
{
    [Header("이 골대는 누구 겁니까?")]
    public bool isLeftGoal; // 왼쪽 골대면 체크(True), 오른쪽이면 해제(False)

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 게 '공'이라면
        if (other.CompareTag("Ball"))
        {
            // 왼쪽 골대에 들어감 -> 오른쪽 플레이어(P2) 득점!
            if (isLeftGoal)
            {
                Debug.Log("Player 2 득점!");
                ScoreManager.instance.AddScore(2);
            }
            // 오른쪽 골대에 들어감 -> 왼쪽 플레이어(P1) 득점!
            else
            {
                Debug.Log("Player 1 득점!");
                ScoreManager.instance.AddScore(1);
            }
        }
    }
}
