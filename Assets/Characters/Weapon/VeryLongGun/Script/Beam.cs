using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour, IWeapon
{
    private bool isLeft = false;
    public float Speed = 30;
    public float deleteTime = 0.18f;
    private Rigidbody2D RBody;

    bool IWeapon.IsLeft { get => isLeft; set => isLeft = value; }
    private ParticleSystem particle;

    private void Start()
    {
        RBody = GetComponent<Rigidbody2D>();
        RBody.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        particle = GetComponent<ParticleSystem>();
        if (particle != null)
        {
            // 進行方向の逆にパーティクルを伸ばす
            ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particle.velocityOverLifetime;
            velocityOverLifetime.x = isLeft? 100f : -100f;
        }
        Destroy(this.gameObject, deleteTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy")) {
            BeamStatus beamStatus = GetComponent<BeamStatus>();
            EnemyStatus enemyStatus = collision.gameObject.GetComponent<EnemyStatus>();
            enemyStatus.AddDamage(beamStatus);
            Destroy(this.gameObject);
        }
    }

    private void FixedUpdate()
    {
        float speed = isLeft ? (-1) * Speed : Speed;
        transform.Translate(speed / 50, 0, 0);
    }
}
