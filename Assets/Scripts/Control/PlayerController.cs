using System.Collections.Generic;
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
        // 触れている話しかけ先ごとの、重なっている当たり判定の数。
        // 操作キャラは当たり判定を複数(体・足・壁)持つので、1つが出ただけでは離れたことにしない
        readonly Dictionary<ICollisionAction, int> touchingActions = new Dictionary<ICollisionAction, int>();
        bool isStopped = false;
        bool reportedStopped = false;
        bool canAimDown = true;

        public bool IsStopped
        {
            get => isStopped;
            set
            {
                if (isStopped == value) return;
                isStopped = value;
                reportedStopped = false;
                PerfLog.Log($"[PlayerController] {name} IsStopped -> {value} at t={Time.time:F3}");
            }
        }

        public string attackButton = "x";

        [SerializeField]
        Leg leg;

        private void Awake()
        {
            mover = GetComponent<Mover>();
            fighter = GetComponent<Fighter>();
            canAimDown = CanAimDown(GetComponent<BaseStats>());
        }

        // 下方向の攻撃はオロチの役割なので、Akim(StatClass.Player)は下入力で照準を下げない
        static bool CanAimDown(BaseStats baseStats)
        {
            return baseStats == null || baseStats.GetStatClass() != StatClass.Player;
        }

        private void OnEnable()
        {
            PerfLog.Log($"[PlayerController] {name} enabled at t={Time.time:F3}");
        }

        private void OnDisable()
        {
            PerfLog.Log($"[PlayerController] {name} disabled at t={Time.time:F3}");
            // キャラ切り替えなどで消えるときは、触れていた記録を残さない(戻ったときに入り直しで数え直す)
            touchingActions.Clear();
            collisionAction = null;
        }

        void Update()
        {
            if (LoadCompleteManager.Instance != null && !LoadCompleteManager.Instance.IsLoaded)
                return;
            if (isStopped)
            {
                if (!reportedStopped)
                {
                    PerfLog.Log($"[PlayerController] {name} Update skipped because IsStopped at t={Time.time:F3}");
                    reportedStopped = true;
                }
                mover.Stop();
                return;
            }
            if (reportedStopped)
            {
                PerfLog.Log($"[PlayerController] {name} Update resumed at t={Time.time:F3}");
                reportedStopped = false;
            }
            mover.Move();
            AttackBehaviour();
            InteractWithCollisionActions();
        }

        // 出入りの瞬間だけ数え直す(触れている間の毎フレームの処理はしない)
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out ICollisionAction _collisionAction))
                return;
            touchingActions.TryGetValue(_collisionAction, out int count);
            touchingActions[_collisionAction] = count + 1;
            if (collisionAction != null)
                return;
            collisionAction = _collisionAction;
            collisionAction.ShowInformation();
            if (collisionAction.IsCollisionStart())
                collisionAction.ExecuteCollisionStart();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.TryGetComponent(out ICollisionAction _collisionAction))
                return;
            if (!touchingActions.TryGetValue(_collisionAction, out int count))
                return;
            // ほかの当たり判定がまだ触れていれば、話しかけられるままにする
            if (count > 1)
            {
                touchingActions[_collisionAction] = count - 1;
                return;
            }
            touchingActions.Remove(_collisionAction);
            if (collisionAction != _collisionAction)
                return;
            collisionAction.HideInformation();
            collisionAction = null;
            // 重なっていた別の話しかけ先があれば、そちらへ切り替える(触れたときに始まるイベントは始めない)
            foreach (ICollisionAction touching in touchingActions.Keys)
            {
                if (touching is Object touchingObject && touchingObject == null)
                    continue;
                collisionAction = touching;
                collisionAction.ShowInformation();
                break;
            }
        }

        private void AttackBehaviour()
        {
            if (PlayerInputAdapter.IsAimUpPressed())
            {
                fighter.WeaponUp();
            }
            else if (canAimDown && PlayerInputAdapter.IsAimDownPressed())
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
