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

        bool isStopped = false;
        public bool IsStopped
        {
            get => isStopped;
            set => isStopped = value;
        }

        public event Action<GameObject> OnChangeCharacter;

        KeyCode swithCharacterButton = KeyCode.Z;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            SetCurrentPlayerActive();
        }

        private void SetCurrentPlayerActive()
        {
            foreach (GameObject member in members)
            {
                if (member.gameObject != currentPlayer)
                {
                    member.gameObject.SetActive(false);
                    continue;
                }
                member.gameObject.SetActive(true);
                // playerのIStoppableをすべて取得して停止状態を解除する
                IStoppable[] stoppables = member.GetComponentsInChildren<IStoppable>();
                foreach (IStoppable stoppable in stoppables)
                {
                    stoppable.IsStopped = false;
                }
            }
        }

        private void Update()
        {
            if (isStopped)
                return;
            // 現在のキャラが死んでいれば受け付けない
            if (currentPlayer.GetComponent<Health>().IsDead)
                return;
            if (Input.GetKeyDown(swithCharacterButton))
            {
                SwitchNextPlayer();
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

        private void SwitchNextPlayer()
        {
            GameObject nextPlayer = GetNextPlayer();
            // 現在のキャラクターと同じであれば何もしない
            if (nextPlayer == currentPlayer)
                return;

            SwitchToPlayer(nextPlayer);
            playChangeSe();
        }

        private void SwitchToPlayer(GameObject nextPlayer)
        {
            GameObject previousPlayer = currentPlayer;
            Vector2 previousVelocity =
                previousPlayer.GetComponent<Rigidbody2D>()?.velocity ?? Vector2.zero;

            SetNextPlayerPosition(nextPlayer);
            nextPlayer
                .GetComponent<Health>()
                .InheritInvincible(previousPlayer.GetComponent<Health>());

            currentPlayer = nextPlayer;
            SetCurrentPlayerActive();

            ChangeHud();

            EquipWeapon();

            ApplyVelocityToCurrentPlayer(previousVelocity);

            // プレイヤーのStun状態を引き継ぐ
            PlayerStun previousPlayerStun = previousPlayer.GetComponent<PlayerStun>();
            if (previousPlayerStun != null)
            {
                PlayerStun currentPlayerStun = currentPlayer.GetComponent<PlayerStun>();
                if (currentPlayerStun != null)
                {
                    currentPlayerStun.Set(previousPlayerStun);
                }
            }

            // legの接地状態を切り替え前のプレイヤーから引き継ぐ
            Leg leg = currentPlayer.GetComponent<Leg>();
            if (leg != null)
            {
                leg.NotifiedLanded(previousPlayer.GetComponent<Leg>().IsGround);
            }

            // 切り替え前のプレイヤーのジャンプ状態を解除
            Jump jump = previousPlayer.GetComponent<Jump>();
            if (jump != null)
                jump.EndJump();

            OnChangeCharacter?.Invoke(currentPlayer);
        }

        private void EquipWeapon()
        {
            if (currentPlayer.name != "Akim")
                return;
            if (flagManager.GetFlag(Flag.VeryLongGunEquipped))
            {
                currentPlayer
                    .GetComponent<Fighter>()
                    .EquipWeapon(Resources.Load<WeaponConfig>("VeryLongGunConfig"));
            }
        }

        private void TransferPlayerStats(GameObject from, GameObject to)
        {
            to.GetComponent<Health>()
                .SetHealthPoints(from.GetComponent<Health>().GetHealthPoints());
            BaseStats toBaseStats = to.GetComponent<BaseStats>();
            PartyHealthLevel partyHealthLevel = GetComponent<PartyHealthLevel>();
            if (toBaseStats != null && partyHealthLevel != null)
            {
                partyHealthLevel.SetLevel(partyHealthLevel.GetCurrentLevel(), toBaseStats);
            }
            to.GetComponent<Experience>()
                .SetExperiencePoints(from.GetComponent<Experience>().GetExperiencePoints());
        }

        private void ApplyVelocityToCurrentPlayer(Vector2 velocity)
        {
            Rigidbody2D currentRigitBody = currentPlayer.GetComponent<Rigidbody2D>();
            if (currentRigitBody != null)
            {
                currentRigitBody.velocity = velocity;
            }
        }

        private void SynchronizePartyMembersHealthLevel()
        {
            PartyHealthLevel partyHealthLevel = GetComponent<PartyHealthLevel>();
            if (partyHealthLevel == null)
            {
                Debug.LogWarning("PartyHealthLevel component not found");
                return;
            }
            foreach (GameObject member in members)
            {
                BaseStats memberBaseStats = member.GetComponent<BaseStats>();
                if (memberBaseStats == null)
                {
                    Debug.LogWarning($"BaseStats component not found on member: {member.name}");
                    continue;
                }
                partyHealthLevel.SetLevel(partyHealthLevel.GetCurrentLevel(), memberBaseStats);
            }
        }

        private void ChangeHud()
        {
            virtualCamera.Follow = currentPlayer.transform;

            // HP表示のプレイヤーの切り替え
            hpDisplay.SetPlayer(currentPlayer);
            hpBar.SetPlayer(currentPlayer);
            // Experience表示のプレイヤーの切り替え
            experienceBar.SetPlayerExperience(currentPlayer);
            levelDisplay.SetBaseStats(currentPlayer.GetComponent<BaseStats>());
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
            // それ以外は仲間でない
            return false;
        }

        private void SetNextPlayerPosition(GameObject nextPlayer)
        {
            // 位置を前のキャラに合わせる
            nextPlayer.transform.position = currentPlayer.transform.position;
            // 向きを前のキャラに合わせる
            nextPlayer.GetComponent<Mover>().IsLeft = currentPlayer.GetComponent<Mover>().IsLeft;
            // 前のキャラの足の位置の高さ取得
            float previousFootPositionY = currentPlayer.transform.Find("Leg").localPosition.y;
            // 次のキャラの足の位置の高さ取得
            float nextFootPositionY = nextPlayer.transform.Find("Leg").localPosition.y;
            // 高さの差分を足す
            nextPlayer.transform.position += new Vector3(
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
            PartyHealthLevel partyHealthLevel = GetComponent<PartyHealthLevel>();
            if (partyHealthLevel == null)
                return;
            int nextLevel = partyHealthLevel.GetCurrentLevel() + 1;
            // member全員のHealthLevelを1あげる
            foreach (GameObject member in members)
            {
                BaseStats memberBaseStats = member.GetComponent<BaseStats>();
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
            foreach (GameObject member in members)
            {
                BaseStats memberBaseStats = member.GetComponent<BaseStats>();
                if (memberBaseStats == null)
                    continue;
                // 全回復
                member.GetComponent<Health>().RestoreHealth();
            }
        }

        public void SetVisibility(bool isVisible)
        {
            currentPlayer.GetComponent<SpriteRenderer>().enabled = isVisible;
            // 現在のplayerのHandの状態も変えて武器も表示、非表示する
            currentPlayer.transform.Find("Hand").gameObject.SetActive(isVisible);
            // Legも表示、非表示する
            currentPlayer.transform.Find("Leg").gameObject.SetActive(isVisible);
        }

        public void SetTempInvincible(bool value)
        {
            Health health = currentPlayer.GetComponent<Health>();
            if (health != null)
                health.IsTempInvincible = value;
        }

        public void MoveToRelativePosition(Vector3 position, float timeout = 0)
        {
            if (currentPlayer == null)
                return;
            Mover mover = currentPlayer.GetComponent<Mover>();
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
            statusSaveData.healthPoints = currentPlayer.GetComponent<Health>().GetHealthPoints();
            statusSaveData.experiencePoints = currentPlayer
                .GetComponent<Experience>()
                .GetExperiencePoints();
            statusSaveData.currentPlayerName = currentPlayer.name;
            statusSaveData.currentPlayerPosition = currentPlayer.transform.position.ToToken();
            // PartyHealthLevelを保存
            PartyHealthLevel partyHealthLevel = GetComponent<PartyHealthLevel>();
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
            // PartyHealthLevelを復元
            PartyHealthLevel partyHealthLevel = GetComponent<PartyHealthLevel>();
            partyHealthLevel.SetLevel(
                statusSaveData.partyHealthLevel,
                currentPlayer.GetComponent<BaseStats>()
            );
            // キャラごとにステータスを持たなかった時代のセーブデータ対応
            SynchronizePartyMembersHealthLevel();
            // HP, Experienceを復元
            currentPlayer.GetComponent<Health>().SetHealthPoints(statusSaveData.healthPoints);
            currentPlayer
                .GetComponent<Experience>()
                .SetExperiencePoints(statusSaveData.experiencePoints);
            currentPlayer.transform.position = statusSaveData.currentPlayerPosition.ToVector3();
            SwitchToPlayer(currentPlayer);
        }
    }
}
