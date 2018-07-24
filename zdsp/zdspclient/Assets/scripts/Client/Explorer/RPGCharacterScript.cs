using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RPGCharacterScript : MonoBehaviour
{
	public Animation m_Animator;
	public int moveSpeed = 8;
	public int health = 100;
	public string PlayerName = "Test";

    public float attackDuration = 1.0f;
	public float currentAttackCD = 0.0f;

	void Start ()
	{
		m_Animator = GetComponent<Animation>();
		PlayEffect("standby");
        GUI.color = Color.cyan;
    }

	//void OnGUI()
	//{
	//	Vector2 targetPos;
	//	targetPos = Camera.main.WorldToScreenPoint (transform.position);
	//	GUI.Box(new Rect(targetPos.x-30,  targetPos.y-60, 60, 20), PlayerName);	
	//}

    void Update()
    {
        if (currentAttackCD > 0)
            currentAttackCD -= Time.deltaTime; 
    }

    public void AttackButtonPressed()
    {
        if (currentAttackCD <= 0f)
        {
            PlayEffect("atk1");
            currentAttackCD = attackDuration;
        }
    }

    public void OnSpeedAdjusted(bool speedUp)
    {
        if(speedUp)
        {
            moveSpeed *= 2;
            attackDuration /= 2;
        }
        else
        {
            moveSpeed /= 2;
            attackDuration *= 2;
        }
    }

    public bool IsAttacking()
    {
        return currentAttackCD > 0;
    }

    public void PlayEffect(string name)
    {
        m_Animator.Play(name);
    }
}
