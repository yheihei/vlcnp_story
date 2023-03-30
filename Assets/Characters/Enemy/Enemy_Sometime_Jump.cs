using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Sometime_Jump : MonoBehaviour
{
    public float JumpPower = 5;
    public int Frequency = 120;

    private SpriteRenderer enemy;
    private Rigidbody2D enemyRbody;
    private Animator animator;
    private int count = 0;
    private string currentMode = "";
    public string groundAnime = "";
    public string jumpAnime = "";

    void Start()
    {
        enemy = GetComponent<SpriteRenderer>();
        enemyRbody = GetComponent<Rigidbody2D>();
        //enemyRbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        enemyRbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        animator = GetComponent<Animator>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        currentMode = groundAnime;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        currentMode = jumpAnime;
    }

    void FixedUpdate()
    {
        count++;
        if (count == Frequency)
        {
            enemyRbody.AddForce(new Vector2(0, JumpPower), ForceMode2D.Impulse);
            count = 0;
        }
        if (currentMode != "" && animator != null) {
            animator.Play(currentMode);
        }
    }
}
