using System;
using System.Collections;
using System.Collections.Generic;
using Nethereum.BlockchainProcessing.BlockProcessing;
using Nethereum.ENS.EthRegistrarSubdomainRegistrar.ContractDefinition;
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
        public enum Direction
        {
            Left,
            Right
        }

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

        public void Defeated(float rotation = 90)
        {
            // デフォルトはうつ伏せ
            transform.Rotate(new Vector3(0, 0, rotation));
        }

        public void SetDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    isLeft = true;
                    break;
                case Direction.Right:
                    isLeft = false;
                    break;
            }
            UpdateCharacterDirection();
        }

        public void SetSpeed(float _speed)
        {
            speed = _speed;
        }

        public void MoveToPositionEvent(Vector3 position, float timeout = 0, bool keepDirection = false)
        {
            StartCoroutine(MoveToPosition(position, timeout, keepDirection));
        }

        public void MoveToRelativePositionEvent(Vector3 position, float timeout = 0, bool keepDirection = false)
        {
            StartCoroutine(MoveToPosition(transform.position + position, timeout, keepDirection));
        }

        private IEnumerator MoveToPosition(Vector3 position, float timeout = 0, bool keepDirection = false)
        {
            // プレイヤーの位置が指定のx位置より左にある場合は左を向く
            if (!keepDirection)
            {
                if (position.x < transform.position.x)
                {
                    isLeft = true;
                }
                else
                {
                    isLeft = false;
                }
            }
            UpdateCharacterDirection();
            // 経過時間を格納する変数
            float elapsedTime = 0;
            // プレイヤーの位置と指定のx位置が特定の値以内になるまでループ
            while (Mathf.Abs(position.x - transform.position.x) > 0.1f)
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

        // 震える
        public void Shake(float duration = 1, float magnitude = 0.1f)
        {
            StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            Vector3 originalPosition = transform.position;
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                transform.position = originalPosition + new Vector3(UnityEngine.Random.Range(-1f, 1f) * magnitude, UnityEngine.Random.Range(-1f, 1f) * magnitude, 0);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.position = originalPosition;
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
