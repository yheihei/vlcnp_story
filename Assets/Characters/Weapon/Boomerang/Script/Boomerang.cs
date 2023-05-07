using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boomerang : MonoBehaviour, IWeapon
{
    private bool isLeft = false;
    public float Speed = 7;
    private float speed;
    public int TurnCount = 30;
    private int count = 0;
    private bool isTurn = false;
    private GameObject targetObject;
    private float vx;
    private float vy;
    private float beforeX;
    private List<GameObject> hittingEnemies = new List<GameObject>();
    private ParticleSystem particle;

    bool IWeapon.IsLeft { get => isLeft; set => isLeft = value; }

    private void Start()
    {
        speed = isLeft ? (-1) * Speed : Speed;
        beforeX = transform.position.x;
        particle = GetComponent<ParticleSystem>();
    }

    private void FixedUpdate()
    {
        count += 1;

        if (isTurn) {
            transform.Translate(vx, vy, 0);
        } else
        {
            transform.Translate(speed / 50, 0, 0);
        }
        if (count == TurnCount)
        {
            if (!isTurn) {
                // 反対方向に2倍すすむようにする
                TurnCount = TurnCount * 2;
            }
            count = 0;
            targetObject = GameObject.FindGameObjectWithTag("Player");
            Vector3 dir = speed < 0 ? (targetObject.transform.position - this.transform.position).normalized : (this.transform.position - targetObject.transform.position).normalized;
            vx = (-1) * dir.x * speed / 50;
            vy = (-1) * dir.y * speed / 50;
            isTurn = true;
        }
        // 現在の進行方向を取得
        float nowX = transform.position.x - beforeX;
        print(nowX);
        beforeX = transform.position.x;
        if (particle) {
            // X方向の速度をパーティクルのlinearXに反映
            ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particle.velocityOverLifetime;
            velocityOverLifetime.x = nowX > 0 ? -50f : 50f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("Player"))
        {
            Destroy(this.gameObject);
        }

        // 敵との接触開始で、その敵が接触中でなければ接触中の敵リストに入れる
        if (collision.gameObject.CompareTag("Enemy") && !hittingEnemies.Contains(collision.gameObject))
        {
            hittingEnemies.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 接触判定した敵のみにダメージを与え多段ヒットを防ぐ
        if (hittingEnemies.Contains(collision.gameObject))
        {
            WeaponStatus status = GetComponent<WeaponStatus>();
            EnemyStatus enemyStatus = collision.gameObject.GetComponent<EnemyStatus>();
            enemyStatus.AddDamage(status);
            hittingEnemies.Remove(collision.gameObject);
        }
    }
}
