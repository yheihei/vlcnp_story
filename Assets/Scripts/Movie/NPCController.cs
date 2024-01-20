using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VLCNP.Movie
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class NPCController : MonoBehaviour
    {
        [SerializeField] float speed = 4;
        Rigidbody2D rbody;
        Animator animator;
        float vx = 0;
        bool isLeft = true;

        private void Awake() {
            rbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        // 接地判定 Gwround tagがついたオブジェクトに接触しているか
        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Ground")
            {
                SetGround(true);
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Ground")
            {
                SetGround(false);
            }
        }

        public void SetGround(bool isGround)
        {
            animator.SetBool("isGround", isGround);
        }

        public void Jump(float _jumpPower = 2)
        {
            rbody.AddForce(new Vector2(0, _jumpPower), ForceMode2D.Impulse);
        }


        public void MoveToPositionEvent(Vector3 position, float timeout = 0)
        {
            StartCoroutine(MoveToPosition(position, timeout));
        }

        private IEnumerator MoveToPosition(Vector3 position, float timeout = 0)
        {
            // プレイヤーの位置が指定のx位置より左にある場合は左を向く
            if (position.x < transform.position.x)
            {
                isLeft = true;
            }
            else
            {
                isLeft = false;
            }
            UpdateCharacterDirection();
            // 経過時間を格納する変数
            float elapsedTime = 0;
            // プレイヤーの位置と指定のx位置が0.05以内になるまでループ
            while (Mathf.Abs(position.x - transform.position.x) > 0.05f)
            {
                // 経過時間加算
                elapsedTime += Time.deltaTime;
                // タイムアウト値になったらループを抜ける
                if (timeout > 0 && elapsedTime > timeout)
                {
                    break;
                }
                // 指定の位置に向かって移動
                UpdateMoveSpeed(position.x < transform.position.x ? -speed : speed);
                UpdateCharacterDirection();
                yield return null;
            }
            UpdateMoveSpeed(0);
        }

        private void UpdateMoveSpeed(float _vx)
        {
            vx = _vx;
            rbody.velocity = new Vector2(vx, rbody.velocity.y);
            animator.SetFloat("vx", Mathf.Abs(vx));
        }

        private void UpdateCharacterDirection()
        {
            if (isLeft)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-1 * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }    
}
