using UnityEngine;

public class GoalCheck : MonoBehaviour
{
    [Header("이 센서는 왼쪽 골대입니까?")]
    public bool isLeftGoal; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball"))
        {
            // BoxCollider2D 대신 모든 콜라이더의 부모인 Collider2D를 off
            GetComponent<Collider2D>().enabled = false;

            if (isLeftGoal)
            {
                Debug.Log("왼쪽 골대 골인! Player 2 득점!");
                GameManager.instance.AddScore(2); 
            }
            else
            {
                Debug.Log("오른쪽 골대 골인! Player 1 득점!");
                GameManager.instance.AddScore(1); 
            }
        }
    }
}