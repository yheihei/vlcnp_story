using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Combat;

namespace VLCNP.Control
{
    public class TrapController : MonoBehaviour
    {
        Fighter fighter;
        [SerializeField] string targetTagName = "Player";

        private void Awake() {
            fighter = GetComponent<Fighter>();
        }

        // tag プロパティは呼ぶたびに文字列を生成するので CompareTag で比べる
        private void OnCollisionStay2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag(targetTagName)) return;
            fighter.DirectAttack(other.gameObject);
        }
    }
}
