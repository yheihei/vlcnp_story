using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TNRD;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace VLCNP.Combat.EnemyAction
{
    public class HipDropAndSpawnEnemies : EnemyAction
    {
        /*
        重力加速度を従来の2倍にして強力な下方向の加速度を与え　着地時に敵をスポーンさせるアクション。
        スポーンにはISpawnクラスをリストとして持ち、着地時に全部にExecute()を呼び出す。
        着地時、CinemachineImpulseSourceがあればカメラを揺らす
        着地時、WeaponConfigの衝撃波を SyougekihaSpawnPosition に指定された位置から左右に生成する
        */
        float originalGravityScale;

        [SerializeField]
        public List<SerializableInterface<ISpawn>> spawns;

        [SerializeField]
        CinemachineImpulseSource impulseSource;

        [SerializeField]
        float verticalForce = 300f;

        [SerializeField]
        int maxSpawnCount = 2;
        List<GameObject> spawnedObjects = new List<GameObject>();

        [SerializeField]
        WeaponConfig syougekihaWeaponConfig;

        [SerializeField]
        Transform syougekihaSpawnPosition;

        [SerializeField]
        float minXPosition = -10f; // x座標の最小値

        [SerializeField]
        float maxXPosition = 10f; // x座標の最大値

        bool isGround = false;
        bool isLeft = false;

        Rigidbody2D rBody;
        private GameObject player;

        [SerializeField]
        List<string> groundTags = new List<string> { "Ground", "Player" };

        private void Awake()
        {
            rBody = GetComponent<Rigidbody2D>();
            if (rBody == null)
                throw new Exception($"Rigidbody2D is null on {gameObject.name}");
            originalGravityScale = rBody.gravityScale;
        }

        public override void Execute()
        {
            if (IsExecuting)
                return;
            if (IsDone)
                return;
            IsExecuting = true;
            StartCoroutine(HipDropAndSpawnExecute());
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!groundTags.Contains(collision.gameObject.tag))
            {
                return;
            }
            isGround = true;
            rBody.gravityScale = originalGravityScale;
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!groundTags.Contains(collision.gameObject.tag))
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
            this.player = GameObject.FindGameObjectWithTag("Player");
            if (this.player == null)
            {
                return;
            }
            if (transform.position.x < this.player.transform.position.x)
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

        private IEnumerator HipDropAndSpawnExecute()
        {
            LookAtPlayer();
            // プレイヤーの真上に移動（x座標を制限付きで）
            Vector3 playerPosition =
                this.player != null ? this.player.transform.position : transform.position; // プレイヤーが見つからない場合は現在位置を使用
            // x座標を指定範囲内に制限
            float clampedX = Mathf.Clamp(playerPosition.x, minXPosition, maxXPosition);
            Vector3 targetPosition = new Vector3(
                clampedX,
                transform.position.y,
                transform.position.z
            );
            transform.position = targetPosition;
            // 落ちる前に一瞬カメラを揺らす
            if (impulseSource != null)
            {
                impulseSource.GenerateImpulseWithVelocity(new Vector3(-0.3f, -0.3f, 0));
            }
            // ちょっと待つ
            yield return new WaitForSeconds(0.5f);
            // 重力加速度を2倍にして強力な下方向の加速度を与える
            rBody.gravityScale = originalGravityScale * 2;
            rBody.velocity = new Vector2(0, -verticalForce);
            // 着地まで待機
            float timeout = 5.0f; // 5秒のタイムアウト
            float startTime = Time.time;
            while (!isGround)
            {
                // タイムアウト処理
                if (Time.time - startTime > timeout)
                {
                    Debug.LogWarning($"Ground detection timed out for {gameObject.name}");
                    break;
                }
                yield return null;
            }
            rBody.velocity = new Vector2(0, 0);
            // カメラを揺らす
            if (impulseSource != null)
            {
                impulseSource.GenerateImpulseWithVelocity(new Vector3(0.3f, 0.3f, 0));
            }

            // 左右に衝撃波を生成
            if (syougekihaWeaponConfig != null && syougekihaSpawnPosition != null)
            {
                syougekihaWeaponConfig?.LaunchProjectile(syougekihaSpawnPosition, 1, true);
                // 右に衝撃波
                syougekihaWeaponConfig?.LaunchProjectile(syougekihaSpawnPosition, 1, false);
            }
            else
            {
                Debug.LogWarning(
                    $"Cannot spawn shockwave: WeaponConfig or SpawnPosition is null on {gameObject.name}"
                );
            }

            // 着地したらスポーン
            // 現在のspawnedObjects内をチェックし、nullになっているものを削除
            spawnedObjects.RemoveAll(obj => obj == null);
            if (spawnedObjects.Count < maxSpawnCount)
            {
                if (spawns != null)
                {
                    foreach (var spawn in spawns)
                    {
                        if (spawn != null && spawn.Value != null)
                        {
                            GameObject spawnObject = spawn.Value.Execute();
                            if (spawnObject != null)
                            {
                                spawnedObjects.Add(spawnObject);
                            }
                        }
                    }
                }
            }

            IsDone = true;
        }
    }
}
