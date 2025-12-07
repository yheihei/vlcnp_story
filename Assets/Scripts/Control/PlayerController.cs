using Newtonsoft.Json.Linq;
using UnityEngine;
using VLCNP.Actions;
using VLCNP.Attributes;
using VLCNP.Combat;
using VLCNP.Core;
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
        bool reportedStopped = false;

        public bool IsStopped
        {
            get => isStopped;
            set
            {
                if (isStopped == value) return;
                isStopped = value;
                reportedStopped = false;
                Debug.Log($"[PlayerController] {name} IsStopped -> {value} at t={Time.time:F3}");
            }
        }

        public string attackButton = "x";

        [SerializeField]
        Leg leg;

        private void Awake()
        {
            mover = GetComponent<Mover>();
            fighter = GetComponent<Fighter>();
        }

        private void OnEnable()
        {
            Debug.Log($"[PlayerController] {name} enabled at t={Time.time:F3}");
        }

        private void OnDisable()
        {
            Debug.Log($"[PlayerController] {name} disabled at t={Time.time:F3}");
        }

        void Update()
        {
            if (LoadCompleteManager.Instance != null && !LoadCompleteManager.Instance.IsLoaded)
                return;
            if (isStopped)
            {
                if (!reportedStopped)
                {
                    Debug.Log($"[PlayerController] {name} Update skipped because IsStopped at t={Time.time:F3}");
                    reportedStopped = true;
                }
                mover.Stop();
                return;
            }
            if (reportedStopped)
            {
                Debug.Log($"[PlayerController] {name} Update resumed at t={Time.time:F3}");
                reportedStopped = false;
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
            if (PlayerInputAdapter.IsAimUpPressed())
            {
                fighter.WeaponUp();
            }
            else if (PlayerInputAdapter.IsAimDownPressed())
            {
                fighter.WeaponDown();
            }
            else
            {
                fighter.WeaponHorizontal();
            }
            if (PlayerInputAdapter.WasAttackPressed(attackButton))
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
            return PlayerInputAdapter.WasInteractPressed()
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
