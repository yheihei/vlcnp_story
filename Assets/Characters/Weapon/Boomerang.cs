using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boomerang : MonoBehaviour
{
    public bool IsLeft = false;
    public float Speed = 7;
    public int TurnCount = 50;
    private int count = 0;
    private bool isTurn = false;
    private GameObject targetObject;
    private float vx;
    private float vy;

    private void FixedUpdate()
    {
        count += 1;
        float speed = IsLeft ? Speed : (-1) * Speed;

        if (isTurn) {
            transform.Translate(vx, vy, 0);
        } else
        {
            transform.Translate(speed / 50, 0, 0);
        }
        if (count == TurnCount)
        {
            if (!isTurn) {
                TurnCount = TurnCount * 2;
            }
            count = 0;
            targetObject = GameObject.FindGameObjectWithTag("Player");
            Vector3 dir = (targetObject.transform.position - this.transform.position).normalized;
            vx = (-1) * dir.x * speed / 50;
            vy = (-1) * dir.y * speed / 50;
            isTurn = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("Player"))
        {
            Destroy(this.gameObject);
        }
    }
}
