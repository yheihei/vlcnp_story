using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Movement;

namespace VLCNP.Control
{
    public class EnemyController : MonoBehaviour
    {
        IMoveAction[] moveActions;

        private void Awake() {
            moveActions = GetComponents<IMoveAction>();
        }

        private void Update() {
            foreach (IMoveAction moveAction in moveActions)
            {
                moveAction.Move();
            }
        }
    }
}
