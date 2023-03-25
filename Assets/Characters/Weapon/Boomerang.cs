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

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        count += 1;
        float speed = IsLeft ? Speed : (-1) * Speed;

        if (isTurn) {
            targetObject = GameObject.FindGameObjectWithTag("Player");
            Vector3 dir = (targetObject.transform.position - this.transform.position).normalized;
            float vx = dir.x * speed / 50;
            float vy = dir.y * speed / 50;
            transform.Translate(vx, vy, 0);
        } else
        {
            transform.Translate(speed / 50, 0, 0);
        }
        if (count == TurnCount)
        {
            Speed *= -1;
            count = 0;
            isTurn = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("ブーメランあたり");
        Debug.Log(collision);
    }
}
