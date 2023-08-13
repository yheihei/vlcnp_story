using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Combat;
using VLCNP.Movement;

namespace VLCNP.Control
{
    public class EnemyController : MonoBehaviour
    {
        Fighter fighter;
        [SerializeField] string attackTargetTagName = "Player";
        IMoveAction[] moveActions;

        private void Awake() {
            moveActions = GetComponents<IMoveAction>();
            fighter = GetComponent<Fighter>();
        }

        private void Update() {
            foreach (IMoveAction moveAction in moveActions)
            {
                moveAction.Move();
            }
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            AttackBehavior(other);
        }

        private void AttackBehavior(Collision2D other)
        {
            if (other.gameObject.tag != attackTargetTagName) return;
            fighter.DirectAttack(other.gameObject);
        }
    }
}
