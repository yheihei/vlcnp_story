using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Movement
{
    public class Leg : MonoBehaviour
    {
        bool isGround = false;

        public bool IsGround { get => isGround; set => isGround = value; }

        // 着地したときのイベント
        public event Action OnLanded;

        // tag プロパティは呼ぶたびに文字列を生成するので CompareTag で比べる
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!collision.CompareTag("Ground") && !collision.CompareTag("Enemy"))
            {
                return;
            }
            IsGround = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.CompareTag("Ground") && !collision.CompareTag("Enemy"))
            {
                return;
            }
            IsGround = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ((collision.CompareTag("Ground") || collision.CompareTag("Enemy")) && !IsGround)
            {
                OnLanded?.Invoke();
            }
        }

        public void NotifiedLanded(bool _isGround)
        {
            IsGround = _isGround;
            if (IsGround)
            {
                OnLanded?.Invoke();
            }
        }
    }
}
