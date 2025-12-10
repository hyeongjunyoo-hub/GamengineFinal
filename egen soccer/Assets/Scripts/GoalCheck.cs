using UnityEngine;

public class GoalCheck : MonoBehaviour
{
    [Header("이 센서는 왼쪽 골대입니까?")]
    public bool isLeftGoal; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball")==false)
        {
            return;
        }
        Debug.Log("골인 감지됨! 들어온 물체: " + other.name);
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