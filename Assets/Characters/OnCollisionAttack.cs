using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisionAttack : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "akim1")
        {
            GameObject player = collision.gameObject;
            PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
            EnemyStatus enemyStatus = this.GetComponent<EnemyStatus>();
            playerStatus.addDamage(enemyStatus);
        }
    }
}
