using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat
{
    public class ArrowAlignToVelocity2D : MonoBehaviour
    {
        [SerializeField] float minSpeed = 0.1f;
        Rigidbody2D _rb;

        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        void FixedUpdate()
        {
            if (_rb == null) return;
            Vector2 v = _rb.velocity;
            if (v.sqrMagnitude < minSpeed * minSpeed) return;

            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
    }
}
