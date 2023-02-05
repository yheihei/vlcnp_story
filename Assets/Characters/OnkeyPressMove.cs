using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// キーを押すと、スプライトが移動する
public class OnkeyPressMove : MonoBehaviour
{

    public float speed = 4; // スピード：Inspectorで指定
    public float jumpPower = 6;

    float vx = 0;
    bool leftFlag = true;
    bool pushFlag = false;
    bool jumpFlag = false;
    bool groundFlag = false;
    Rigidbody2D rbody;
    Animator animator;
    SpriteRenderer player;
    string currentMode = "";
    string groundAnime = "akim";
    string jumpAnime = "akim_jump";

    private void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        rbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        animator = GetComponent<Animator>();
        player = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        groundFlag = true;
        currentMode = groundAnime;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        groundFlag = false;
        currentMode = jumpAnime;
    }

    void Update()
    { // ずっと行う
        vx = 0;
        if (Input.GetKey("right"))
        { // もし、右キーが押されたら
            vx = speed; // 右に進む移動量を入れる
            leftFlag = false;
        }
        if (Input.GetKey("left"))
        { // もし、左キーが押されたら
            vx = -speed; // 左に進む移動量を入れる
            leftFlag = true;
        }
        if (Input.GetKey("space") && groundFlag && rbody.velocity.y < 2)
        {
            if (pushFlag == false)
            {
                jumpFlag = true;
                pushFlag = true;
            }
        }
        else
        {
            pushFlag = false;
        }
    }

    void FixedUpdate()
    { // ずっと行う（一定時間ごとに）
      // 移動する
        rbody.velocity = new Vector2(vx, rbody.velocity.y);
        // 左右の向きを変える
        if (leftFlag)
        {
            player.transform.localScale = new Vector3(Mathf.Abs(player.transform.localScale.x), player.transform.localScale.y, player.transform.localScale.z);
        }
        else
        {
            player.transform.localScale = new Vector3(-1 * Mathf.Abs(player.transform.localScale.x), player.transform.localScale.y, player.transform.localScale.z);
        }
        //this.GetComponent<SpriteRenderer>().flipX = !leftFlag;
        if (groundFlag && Mathf.Abs(rbody.velocity.x) < 0.01 && groundFlag && Mathf.Abs(rbody.velocity.y) < 0.01)
        {
            animator.Play(currentMode, 0, 0);
        } else
        {
            animator.Play(currentMode);
        }
        if (jumpFlag)
        {
            jumpFlag = false;
            rbody.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
        }
    }
}
