using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Core
{
    public class BackGroundLoop : MonoBehaviour
    {
        [SerializeField, Header("視差効果"), Range(0f, 1f)]
        float parallaxEffect = 1f;
        float length;
        float startPosX;

        void Awake()
        {
            startPosX = transform.position.x;
            length = GetComponent<SpriteRenderer>().bounds.size.x;
        }

        void FixedUpdate()
        {
            Parallax();
        }

        void Parallax()
        {
            float temp = Camera.main.gameObject.transform.position.x * (1 - parallaxEffect);
            float dist = Camera.main.gameObject.transform.position.x * parallaxEffect;

            transform.position = new Vector3(startPosX + dist, transform.position.y, transform.position.z);

            if (temp > startPosX + length) startPosX += length;
            else if (temp < startPosX - length) startPosX -= length;
        }
    }

}
