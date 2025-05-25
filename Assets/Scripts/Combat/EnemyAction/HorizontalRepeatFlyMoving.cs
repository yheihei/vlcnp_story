using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    public class HorizontalRepeatFlyMoving : MonoBehaviour
    {
        // 移動速度
        [SerializeField]
        float speed = 0.0f;

        // 移動距離
        [SerializeField]
        float distance = 0.0f;

        // 累積移動距離
        float accumulatedDistance = 0.0f;

        bool isLeft = false;

        void FixedUpdate()
        {
            if (isLeft)
            {
                transform.position += Vector3.left * speed * Time.deltaTime;
            }
            else
            {
                transform.position += Vector3.right * speed * Time.deltaTime;
            }
            accumulatedDistance += speed * Time.deltaTime;
            if (accumulatedDistance >= distance)
            {
                isLeft = !isLeft;
                accumulatedDistance = 0.0f;
            }
        }
    }
}
