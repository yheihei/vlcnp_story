using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour
{
    public bool IsLeft = false;
    public float Speed = 15;
    public float deleteTime = 0.5f;
    private Rigidbody2D RBody;

    private void Start()
    {
        RBody = GetComponent<Rigidbody2D>();
        RBody.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
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
        float speed = IsLeft ? Speed : (-1) * Speed;
        transform.Translate(speed / 50, 0, 0);
    }
}
