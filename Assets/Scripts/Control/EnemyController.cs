using System.Collections;
using System.Collections.Generic;
using Nethereum.Quorum.RPC.DTOs;
using UnityEngine;
using VLCNP.Combat;
using VLCNP.Core;
using VLCNP.Movement;

namespace VLCNP.Control
{
    public class EnemyController : MonoBehaviour, IStoppable
    {
        Fighter fighter;
        [SerializeField] string attackTargetTagName = "Player";
        IMoveAction[] moveActions;

        bool isStopped = false;
        public bool IsStopped { get => isStopped; set => isStopped = value; }

        private void Awake() {
            moveActions = GetComponents<IMoveAction>();
            fighter = GetComponent<Fighter>();
        }

        private void Update() {
            if (isStopped) return;
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
