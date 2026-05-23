using System;
using Cinemachine;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Combat;
using VLCNP.Core;
using VLCNP.Movement;
using VLCNP.Saving;
using VLCNP.Stats;
using VLCNP.UI;

namespace VLCNP.Control
{
    public class PartyCongroller : MonoBehaviour, IJsonSaveable, IStoppable
    {
        public const string TagName = "Party";

        [SerializeField]
        GameObject currentPlayer;

        [SerializeField]
        GameObject[] members;

        [SerializeField]
        HPDisplay hpDisplay;

        [SerializeField]
        HPBar hpBar;

        [SerializeField]
        ExperienceBar experienceBar;

        [SerializeField]
        LevelDisplay levelDisplay;

        [SerializeField]
        FlagManager flagManager;

        [SerializeField]
        CinemachineVirtualCamera virtualCamera;

        AudioSource audioSource;

        [SerializeField]
        AudioClip switchCharacterSound;

        [SerializeField]
        float switchCharacterVolume = 0.3f;

        PartyHealthLevel partyHealthLevel;
        MemberRuntimeCache[] memberCaches;
        MemberRuntimeCache currentMemberCache;

        bool isStopped = false;
        public bool IsStopped
        {
            get => isStopped;
            set
            {
                if (isStopped == value) return;
                isStopped = value;
                PerfLog.Log($"[PartyCongroller] IsStopped -> {value} at t={Time.time:F3}");
            }
        }

        public event Action<GameObject> OnChangeCharacter;

        KeyCode swithCharacterButton = KeyCode.Z;

        public static PartyCongroller FindInScene()
        {
            GameObject taggedObject = GameObject.FindWithTag(TagName);
            if (taggedObject != null &&
                taggedObject.TryGetComponent(out PartyCongroller controller))
            {
                return controller;
            }

            PartyCongroller[] controllers = FindObjectsOfType<PartyCongroller>(true);
            foreach (PartyCongroller candidate in controllers)
            {
                if (candidate != null &&
                    candidate.CompareTag(TagName))
                {
                    return candidate;
                }
            }

            return controllers.Length > 0 ? controllers[0] : null;
        }

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            partyHealthLevel = GetComponent<PartyHealthLevel>();
            BuildMemberCaches();
            RefreshCurrentMemberCache();
            flagManager.OnChangeFlag += OnChangeFlag;
        }

        private void Start()
        {
            SetCurrentPlayerActive();
            ChangeHud();
            OnChangeCharacter?.Invoke(currentPlayer);
            PerfLog.Log($"[PartyCongroller] Start currentPlayer={currentPlayer?.name} at t={Time.time:F3}");
        }

        private void SetCurrentPlayerActive()
        {
            foreach (GameObject member in members)
            {
                if (member.gameObject != currentPlayer)
                {
                    member.gameObject.SetActive(false);
                    PerfLog.Log($"[PartyCongroller] deactivate member={member.name}");
                    continue;
                }
                member.gameObject.SetActive(true);
                PerfLog.Log($"[PartyCongroller] activate current member={member.name}");
                // player配下のIStoppableへ現在の停止状態を伝播する
                MemberRuntimeCache cache = GetMemberCache(member);
                if (cache == null)
                    continue;
                foreach (IStoppable stoppable in cache.stoppables)
                {
                    stoppable.IsStopped = isStopped;
                }
            }
        }

        private void Update()
        {
            if (isStopped)
            {
                return;
            }
            // 現在のキャラが死んでいれば受け付けない
            if (currentMemberCache.health.IsDead)
            {
                return;
            }
            if (PlayerInputAdapter.WasCharacterSwitchPressed(swithCharacterButton))
            {
                if (SwitchNextPlayer())
                    playChangeSe();
            }
        }

        public void SetCurrentPlayerByName(string name = "Akim")
        {
            GameObject player = Array.Find(members, member => member.name == name);
            if (player == null || player == currentPlayer)
                return;
            if (!IsJoined(player.name))
                return;
            SwitchToPlayer(player);
        }

        private bool SwitchNextPlayer()
        {
            GameObject nextPlayer = GetNextPlayer();
            // 現在のキャラクターと同じであれば何もしない
            if (nextPlayer == currentPlayer)
                return false;

            SwitchToPlayer(nextPlayer);
            return true;
        }

        private void SwitchToPlayer(GameObject nextPlayer)
        {
            GameObject previousPlayer = currentPlayer;
            MemberRuntimeCache previousCache = currentMemberCache;
            MemberRuntimeCache nextCache = GetMemberCache(nextPlayer);
            Vector2 previousVelocity =
                previousCache?.rigidbody2D?.velocity ?? Vector2.zero;

            SetNextPlayerPosition(nextCache, previousCache);
            nextCache.health.InheritInvincible(previousCache.health);

            currentPlayer = nextPlayer;
            currentMemberCache = nextCache;
            SetCurrentPlayerActive();

            ChangeHud();

            EquipWeapon();

            ApplyVelocityToCurrentPlayer(previousVelocity);

            // プレイヤーのStun状態を引き継ぐ
            PlayerStun previousPlayerStun = previousCache.playerStun;
            if (previousPlayerStun != null)
            {
                PlayerStun currentPlayerStun = currentMemberCache.playerStun;
                if (currentPlayerStun != null)
                {
                    currentPlayerStun.Set(previousPlayerStun);
                }
            }

            // legの接地状態を切り替え前のプレイヤーから引き継ぐ
            Leg leg = currentMemberCache.leg;
            if (leg != null && previousCache.leg != null)
            {
                leg.NotifiedLanded(previousCache.leg.IsGround);
            }

            // 切り替え前のプレイヤーのジャンプ状態を解除
            previousCache.jump?.EndJump();

            OnChangeCharacter?.Invoke(currentPlayer);
        }

        private void EquipWeapon()
        {
            if (currentPlayer.name != "Akim")
                return;
            if (flagManager.GetFlag(Flag.VeryLongGunEquipped))
            {
                currentMemberCache.fighter.EquipWeapon(Resources.Load<WeaponConfig>("VeryLongGunConfig"));
            }
        }

        private void TransferPlayerStats(GameObject from, GameObject to)
        {
            MemberRuntimeCache fromCache = GetMemberCache(from);
            MemberRuntimeCache toCache = GetMemberCache(to);
            toCache.health.SetHealthPoints(fromCache.health.GetHealthPoints());
            BaseStats toBaseStats = toCache.baseStats;
            if (toBaseStats != null && partyHealthLevel != null)
            {
                partyHealthLevel.SetLevel(partyHealthLevel.GetCurrentLevel(), toBaseStats);
            }
            toCache.experience.SetExperiencePoints(fromCache.experience.GetExperiencePoints());
        }

        private void ApplyVelocityToCurrentPlayer(Vector2 velocity)
        {
            Rigidbody2D currentRigitBody = currentMemberCache.rigidbody2D;
            if (currentRigitBody != null)
            {
                currentRigitBody.velocity = velocity;
            }
        }

        private void SynchronizePartyMembersHealthLevel()
        {
            if (partyHealthLevel == null)
            {
                Debug.LogWarning("PartyHealthLevel component not found");
                return;
            }
            foreach (MemberRuntimeCache memberCache in memberCaches)
            {
                BaseStats memberBaseStats = memberCache.baseStats;
                if (memberBaseStats == null)
                {
                    Debug.LogWarning($"BaseStats component not found on member: {memberCache.gameObject.name}");
                    continue;
                }
                partyHealthLevel.SetLevel(partyHealthLevel.GetCurrentLevel(), memberBaseStats);
            }
        }

        private void ChangeHud()
        {
            virtualCamera.Follow = currentMemberCache.transform;

            // HP表示のプレイヤーの切り替え
            hpDisplay.SetPlayer(currentPlayer);
            hpBar.SetPlayer(currentPlayer);
            // Experience表示のプレイヤーの切り替え
            experienceBar.SetPlayerExperience(currentPlayer);
            levelDisplay.SetBaseStats(currentMemberCache.baseStats);
        }

        private GameObject GetNextPlayer()
        {
            int index = System.Array.IndexOf(members, currentPlayer);
            index = (index + 1) % members.Length;
            GameObject nextPlayer;
            while (true)
            {
                // 次のキャラクター取得
                nextPlayer = members[index];
                if (IsJoined(nextPlayer.name))
                    break;
                // 見つからなければ次のキャラクターを選択
                index = (index + 1) % members.Length;
            }
            return nextPlayer;
        }

        // 指定のキャラクターが仲間になっているかどうか
        public bool IsJoined(string name)
        {
            // Akimであれば常に仲間
            if (name == "Akim")
                return true;
            // LeeleeであればJoinedLeeleeフラグが立っていれば仲間
            if (name == "Leelee")
                return flagManager.GetFlag(Flag.JoinedLeelee);
            // OrochiであればVLOrochiJoinedフラグが立っていれば仲間
            if (name == "Orochi")
                return flagManager.GetFlag(Flag.VLOrochiJoined);
            // それ以外は仲間でない
            return false;
        }

        void OnChangeFlag(Flag flag, bool value)
        {
            // 離脱したキャラクターがcurrentPlayerであれば次のキャラクターに切り替える
            // リーリー使用時にリーリーが離脱したときは次のキャラクターに切り替える
            if (flag == Flag.JoinedLeelee && !value && currentPlayer.name == "Leelee")
            {
                SwitchNextPlayer();
            }
        }

        private void SetNextPlayerPosition(MemberRuntimeCache nextCache, MemberRuntimeCache previousCache)
        {
            // 位置を前のキャラに合わせる
            nextCache.transform.position = previousCache.transform.position;
            // 向きを前のキャラに合わせる
            nextCache.mover.IsLeft = previousCache.mover.IsLeft;
            // 前のキャラの足の位置の高さ取得
            float previousFootPositionY = previousCache.legTransform.localPosition.y;
            // 次のキャラの足の位置の高さ取得
            float nextFootPositionY = nextCache.legTransform.localPosition.y;
            // 高さの差分を足す
            nextCache.transform.position += new Vector3(
                0,
                previousFootPositionY - nextFootPositionY,
                0
            );
        }

        public GameObject GetCurrentPlayer()
        {
            return currentPlayer;
        }

        public void IncrementHealthLevel()
        {
            // PartyHealthLevelを1あげる
            if (partyHealthLevel == null)
                return;
            int nextLevel = partyHealthLevel.GetCurrentLevel() + 1;
            // member全員のHealthLevelを1あげる
            foreach (MemberRuntimeCache memberCache in memberCaches)
            {
                BaseStats memberBaseStats = memberCache.baseStats;
                if (memberBaseStats == null)
                    continue;
                partyHealthLevel.SetLevel(nextLevel, memberBaseStats);
            }
            // 全回復させる
            AllMemberRestoreHealth();
            ChangeHud();
        }

        public void AllMemberRestoreHealth()
        {
            foreach (MemberRuntimeCache memberCache in memberCaches)
            {
                BaseStats memberBaseStats = memberCache.baseStats;
                if (memberBaseStats == null)
                    continue;
                // 全回復
                memberCache.health.RestoreHealth();
            }
        }

        public void SetVisibility(bool isVisible)
        {
            currentMemberCache.spriteRenderer.enabled = isVisible;
            // 現在のplayerのHandの状態も変えて武器も表示、非表示する
            currentMemberCache.handTransform.gameObject.SetActive(isVisible);
            // Legも表示、非表示する
            currentMemberCache.legTransform.gameObject.SetActive(isVisible);
        }

        public void SetTempInvincible(bool value)
        {
            Health health = currentMemberCache.health;
            if (health != null)
                health.IsTempInvincible = value;
        }

        public void MoveToRelativePosition(Vector3 position, float timeout = 0)
        {
            if (currentPlayer == null)
                return;
            Mover mover = currentMemberCache.mover;
            if (mover != null)
            {
                StartCoroutine(mover.MoveToRelativePosition(position, timeout));
            }
        }

        private void playChangeSe()
        {
            audioSource.PlayOneShot(switchCharacterSound, switchCharacterVolume);
        }

        [System.Serializable]
        struct StatusSaveData
        {
            public float healthPoints;
            public float experiencePoints;
            public string currentPlayerName;
            public JToken currentPlayerPosition;
            public int partyHealthLevel;
            public float partyExperiencePoints;
        }

        public JToken CaptureAsJToken()
        {
            // HP, Experienceを保存
            StatusSaveData statusSaveData = new StatusSaveData();
            statusSaveData.healthPoints = currentMemberCache.health.GetHealthPoints();
            statusSaveData.experiencePoints = currentMemberCache.experience.GetExperiencePoints();
            statusSaveData.currentPlayerName = currentPlayer.name;
            statusSaveData.currentPlayerPosition = currentMemberCache.transform.position.ToToken();
            // PartyHealthLevelを保存
            if (partyHealthLevel != null)
            {
                statusSaveData.partyHealthLevel = partyHealthLevel.GetCurrentLevel();
            }
            return JToken.FromObject(statusSaveData);
        }

        public void RestoreFromJToken(JToken state)
        {
            // セーブ時のキャラクターをcurrentPlayerとして復元
            StatusSaveData statusSaveData = state.ToObject<StatusSaveData>();
            string currentPlayerName = statusSaveData.currentPlayerName;
            if (currentPlayerName == null)
                return;

            currentPlayer = Array.Find(members, member => member.name == currentPlayerName);
            RefreshCurrentMemberCache();
            // PartyHealthLevelを復元
            partyHealthLevel.SetLevel(
                statusSaveData.partyHealthLevel,
                currentMemberCache.baseStats
            );
            // キャラごとにステータスを持たなかった時代のセーブデータ対応
            SynchronizePartyMembersHealthLevel();
            // HP, Experienceを復元
            currentMemberCache.health.SetHealthPoints(statusSaveData.healthPoints);
            currentMemberCache.experience.SetExperiencePoints(statusSaveData.experiencePoints);
            currentMemberCache.transform.position = statusSaveData.currentPlayerPosition.ToVector3();
            SwitchToPlayer(currentPlayer);
        }

        private void BuildMemberCaches()
        {
            memberCaches = new MemberRuntimeCache[members.Length];
            for (int i = 0; i < members.Length; i++)
            {
                memberCaches[i] = new MemberRuntimeCache(members[i]);
            }
        }

        private MemberRuntimeCache GetMemberCache(GameObject member)
        {
            for (int i = 0; i < memberCaches.Length; i++)
            {
                if (memberCaches[i].gameObject == member)
                    return memberCaches[i];
            }

            return null;
        }

        private void RefreshCurrentMemberCache()
        {
            currentMemberCache = GetMemberCache(currentPlayer);
        }

        class MemberRuntimeCache
        {
            public readonly GameObject gameObject;
            public readonly Transform transform;
            public readonly Health health;
            public readonly BaseStats baseStats;
            public readonly Experience experience;
            public readonly Rigidbody2D rigidbody2D;
            public readonly Mover mover;
            public readonly Leg leg;
            public readonly Jump jump;
            public readonly PlayerStun playerStun;
            public readonly Fighter fighter;
            public readonly SpriteRenderer spriteRenderer;
            public readonly Transform handTransform;
            public readonly Transform legTransform;
            public readonly IStoppable[] stoppables;

            public MemberRuntimeCache(GameObject gameObject)
            {
                this.gameObject = gameObject;
                transform = gameObject.transform;
                health = gameObject.GetComponent<Health>();
                baseStats = gameObject.GetComponent<BaseStats>();
                experience = gameObject.GetComponent<Experience>();
                rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
                mover = gameObject.GetComponent<Mover>();
                leg = gameObject.GetComponent<Leg>();
                jump = gameObject.GetComponent<Jump>();
                playerStun = gameObject.GetComponent<PlayerStun>();
                fighter = gameObject.GetComponent<Fighter>();
                spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                handTransform = transform.Find("Hand");
                legTransform = transform.Find("Leg");
                stoppables = gameObject.GetComponentsInChildren<IStoppable>(true);
            }
        }
    }
}
