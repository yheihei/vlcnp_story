using UnityEngine;
using VLCNP.Combat;
using VLCNP.Movement;
using VLCNP.Actions;
using VLCNP.Core;

namespace VLCNP.Control
{
    public class PlayerController : MonoBehaviour, IStoppable
    {
        Mover mover;
        Fighter fighter;
        ICollisionAction collisionAction;
        bool isStopped = false;

        public bool IsStopped { get => isStopped; set => isStopped = value; }

        private void Awake()
        {
            mover = GetComponent<Mover>();
            fighter = GetComponent<Fighter>();
        }

        void Update()
        {
            if (isStopped)
            {
                mover.Stop();
                return;
            }
            mover.Move();
            AttackBehaviour();
            InteractWithCollisionActions();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            ICollisionAction _collisionAction = other.GetComponent<ICollisionAction>();
            if (_collisionAction == null) return;
            if (collisionAction != null) return;
            collisionAction = _collisionAction;
            collisionAction.ShowInformation();
            if (collisionAction.IsCollisionStart()) collisionAction.ExecuteCollisionStart();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            ICollisionAction _collisionAction = other.GetComponent<ICollisionAction>();
            if (_collisionAction == null) return;
            if (collisionAction == other.GetComponent<ICollisionAction>())
            {
                collisionAction.HideInformation();
                collisionAction = null;
            }
        }

        private void AttackBehaviour()
        {
            if (Input.GetKey("up"))
            {
                fighter.WeaponUp();
            }
            else if (Input.GetKey("down"))
            {
                fighter.WeaponDown();
            }
            else
            {
                fighter.WeaponHorizontal();
            }
            if (Input.GetKeyUp("z"))
            {
                fighter.Attack();
            }
        }

        private void InteractWithCollisionActions()
        {
            if (Input.GetKey("up") && collisionAction != null && collisionAction.IsAction)
            {
                collisionAction.Execute();
            }
        }
    }
}
