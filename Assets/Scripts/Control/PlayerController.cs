using Newtonsoft.Json.Linq;
using UnityEngine;
using VLCNP.Actions;
using VLCNP.Attributes;
using VLCNP.Combat;
using VLCNP.Core;
using VLCNP.Input;
using VLCNP.Movement;
using VLCNP.Saving;
using VLCNP.Stats;

namespace VLCNP.Control
{
    public class PlayerController : MonoBehaviour, IStoppable, IJsonSaveable
    {
        Mover mover;
        Fighter fighter;
        ICollisionAction collisionAction;
        bool isStopped = false;

        public bool IsStopped
        {
            get => isStopped;
            set => isStopped = value;
        }


        [SerializeField]
        Leg leg;

        private void Awake()
        {
            mover = GetComponent<Mover>();
            fighter = GetComponent<Fighter>();
        }

        void Update()
        {
            if (LoadCompleteManager.Instance != null && !LoadCompleteManager.Instance.IsLoaded)
                return;
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
            if (_collisionAction == null)
                return;
            if (collisionAction != null)
                return;
            collisionAction = _collisionAction;
            collisionAction.ShowInformation();
            if (collisionAction.IsCollisionStart())
                collisionAction.ExecuteCollisionStart();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            ICollisionAction _collisionAction = other.GetComponent<ICollisionAction>();
            if (_collisionAction == null)
                return;
            if (collisionAction == other.GetComponent<ICollisionAction>())
            {
                collisionAction.HideInformation();
                collisionAction = null;
            }
        }

        private void AttackBehaviour()
        {
            if (InputManager.Instance.IsMovingUp())
            {
                fighter.WeaponUp();
            }
            else if (InputManager.Instance.IsMovingDown())
            {
                fighter.WeaponDown();
            }
            else
            {
                fighter.WeaponHorizontal();
            }
            if (InputManager.Instance.IsAttackPressed())
            {
                fighter.Attack();
            }
        }

        private void InteractWithCollisionActions()
        {
            if (canInteractAction())
            {
                collisionAction.Execute();
            }
        }

        private bool canInteractAction()
        {
            return InputManager.Instance.IsInteractPressed()
                && collisionAction != null
                && collisionAction.IsAction
                && leg.IsGround;
        }

        [System.Serializable]
        struct StatusSaveData
        {
            public float healthPoints;
            public float experiencePoints;
        }

        public JToken CaptureAsJToken()
        {
            // HP, Experienceを保存
            StatusSaveData statusSaveData = new StatusSaveData();
            statusSaveData.healthPoints = GetComponent<Health>().GetHealthPoints();
            statusSaveData.experiencePoints = GetComponent<Experience>().GetExperiencePoints();
            return JToken.FromObject(statusSaveData);
        }

        public void RestoreFromJToken(JToken state)
        {
            // HP, Experienceを復元 TODO: 復元できなかった場合の処理を記述
            StatusSaveData statusSaveData = state.ToObject<StatusSaveData>();
            GetComponent<Health>().SetHealthPoints(statusSaveData.healthPoints);
            GetComponent<Experience>().SetExperiencePoints(statusSaveData.experiencePoints);
        }
    }
}
