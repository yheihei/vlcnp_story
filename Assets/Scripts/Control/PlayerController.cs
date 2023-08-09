using UnityEngine;
using VLCNP.Combat;
using VLCNP.Movement;

namespace VLCNP.Control
{
    public class PlayerController : MonoBehaviour
    {
        Mover mover;
        Fighter fighter;

        private void Awake() {
            mover = GetComponent<Mover>();
            fighter = GetComponent<Fighter>();
        }

        void Update()
        {
            mover.Move();
            AttackBehaviour();
        }

        private void AttackBehaviour()
        {
            if (Input.GetKeyUp("x"))
            {
                fighter.Attack();
            }
        }
    }    
}
