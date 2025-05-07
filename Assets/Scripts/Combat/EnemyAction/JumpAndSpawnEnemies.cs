using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TNRD;
using Unity.VisualScripting;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class JumpAndSpawnEnemies : EnemyAction
    {
        /*
        ジャンプして着地時に敵をスポーンさせるアクション。
        スポーンにはISpawnクラスをリストとして持ち、着地時に全部にExecute()を呼び出す。
        着地時、CinemachineImpulseSourceがあればカメラを揺らす
        */
        [SerializeField]
        public List<SerializableInterface<ISpawn>> spawns;

        [SerializeField]
        CinemachineImpulseSource impulseSource;

        [SerializeField]
        float jumpForce = 150f;

        [SerializeField]
        float jumpHorizontalForce = 300f;

        [SerializeField]
        int maxSpawnCount = 2;
        List<GameObject> spawnedObjects = new List<GameObject>();

        bool isGround = false;
        bool isLeft = false;

        Rigidbody2D rBody;

        private void Awake()
        {
            rBody = GetComponent<Rigidbody2D>();
            if (rBody == null)
                Debug.LogError(
                    $"Rigidbody2D is not set in the inspector for JumpAndSpawnEnemies on {gameObject.name}"
                );
        }

        public override void Execute()
        {
            if (IsExecuting)
                return;
            if (IsDone)
                return;
            IsExecuting = true;
            StartCoroutine(JumpAndSpawn());
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!collision.gameObject.CompareTag("Ground"))
            {
                return;
            }
            isGround = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.gameObject.CompareTag("Ground"))
            {
                return;
            }
            isGround = false;
        }

        private void LookAtPlayer()
        {
            // playerをタグから取得
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }
            if (transform.position.x < player.transform.position.x)
            {
                isLeft = false;
            }
            else
            {
                isLeft = true;
            }

            transform.localScale = new Vector3(
                isLeft
                    ? 1 * Mathf.Abs(transform.localScale.x)
                    : -1 * Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }

        private IEnumerator JumpAndSpawn()
        {
            LookAtPlayer();
            // ジャンプ
            float xForce = isLeft ? -jumpHorizontalForce : jumpHorizontalForce;
            rBody.AddForce(new Vector2(xForce, jumpForce), ForceMode2D.Impulse);
            yield return new WaitForSeconds(0.5f);

            // 着地まで待機
            while (!isGround)
            {
                yield return null;
            }
            rBody.velocity = new Vector2(0, 0);
            // カメラを揺らす
            if (impulseSource != null)
            {
                impulseSource.GenerateImpulseWithVelocity(new Vector3(0.25f, 0.2f, 0));
            }

            // 着地したらスポーン
            // 現在のspawnedObjects内をチェックし、nullになっているものを削除
            spawnedObjects.RemoveAll(obj => obj == null);
            if (spawnedObjects.Count < maxSpawnCount)
            {
                foreach (var spawn in spawns)
                {
                    GameObject spawnObject = spawn.Value.Execute();
                    spawnedObjects.Add(spawnObject);
                }
            }
            IsDone = true;
        }
    }
}
