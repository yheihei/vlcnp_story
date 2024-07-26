using UnityEngine;
using Cinemachine;
using VLCNP.UI;
using VLCNP.Attributes;
using VLCNP.Stats;
using VLCNP.Core;
using VLCNP.Saving;
using Newtonsoft.Json.Linq;
using System;
using VLCNP.Movement;
using System.Collections;

namespace VLCNP.Control
{
    public class PartyCongroller : MonoBehaviour, IJsonSaveable, IStoppable
    {
        [SerializeField] GameObject currentPlayer;
        [SerializeField] GameObject[] members;
        [SerializeField] HPDisplay hpDisplay;
        [SerializeField] HPBar hpBar;
        [SerializeField] ExperienceBar experienceBar;
        [SerializeField] LevelDisplay levelDisplay;
        GameOver gameOver;
        [SerializeField] FlagManager flagManager;
        [SerializeField]
        CinemachineVirtualCamera virtualCamera;

        bool isStopped = false;
        public bool IsStopped { get => isStopped; set => isStopped = value; }

        public event Action<GameObject> OnChangeCharacter;

        private void Awake()
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
                IStoppable[] stoppables = member.GetComponents<IStoppable>();
                foreach (IStoppable stoppable in stoppables)
                {
                    stoppable.IsStopped = false;
                }
            }
        }

        public void SetCurrentPlayerByName(string name = "Akim")
        {

            GameObject nextPlayer = Array.Find(members, member => member.name == name);
            if (nextPlayer == null) return;
            GameObject previousPlayer = currentPlayer;
            SetNextPlayerPosition(nextPlayer);
            currentPlayer = nextPlayer;
            SetCurrentPlayerActive();

            // 前のキャラクターのHPを次のキャラクターに引き継ぐ
            currentPlayer.GetComponent<Health>().SetHealthPoints(previousPlayer.GetComponent<Health>().GetHealthPoints());

            // 前のキャラクターのExperienceを次のキャラクターに引き継ぐ
            currentPlayer.GetComponent<Experience>().SetExperiencePoints(previousPlayer.GetComponent<Experience>().GetExperiencePoints());
            ChangeDisplay();
            // キャラクターの変更イベントを発火
            OnChangeCharacter?.Invoke(currentPlayer);
        }

        private void Update()
        {
            if (isStopped) return;
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S))
            {
                SwitchNextPlayer();
            }
        }

        private void SwitchNextPlayer()
        {
            GameObject nextPlayer = GetNextPlayer();
            // 現在のキャラクターと同じであれば何もしない
            if (nextPlayer == currentPlayer) return;

            GameObject previousPlayer = currentPlayer;
            Vector2 previousVelocity = previousPlayer.GetComponent<Rigidbody2D>()?.velocity ?? Vector2.zero;
            SetNextPlayerPosition(nextPlayer);
            // 次のキャラクターに前のキャラクターの無敵状態を引き継ぐ
            nextPlayer.GetComponent<Health>().InheritInvincible(previousPlayer.GetComponent<Health>());

            currentPlayer = nextPlayer;
            SetCurrentPlayerActive();

            // 前のキャラクターのHPを次のキャラクターに引き継ぐ
            currentPlayer.GetComponent<Health>().SetHealthPoints(previousPlayer.GetComponent<Health>().GetHealthPoints());

            // 前のキャラクターのExperienceを次のキャラクターに引き継ぐ
            currentPlayer.GetComponent<Experience>().SetExperiencePoints(previousPlayer.GetComponent<Experience>().GetExperiencePoints());
            ChangeDisplay();

            // 前のキャラクターのRigitbodyの速度を引き継ぐ
            Rigidbody2D currentRigitBody = currentPlayer.GetComponent<Rigidbody2D>();
            if (currentRigitBody != null)
            {
                currentRigitBody.velocity = previousVelocity;
            }

            // キャラクターを止めていたら動かす
            IStoppable stoppable = currentPlayer.GetComponent<IStoppable>();
            if (stoppable != null)
            {
                stoppable.IsStopped = false;
            }

            // キャラクターの変更イベントを発火
            OnChangeCharacter?.Invoke(currentPlayer);
        }

        private void ChangeDisplay()
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
                // 名前がAkimであれば既に仲間なのでループを抜ける
                if (nextPlayer.name == "Akim") break;
                // 名前がLeeleeであれば、JoinedLeeleeフラグが立っていれば仲間なのでループを抜ける
                if (nextPlayer.name == "Leelee" && flagManager.GetFlag(Flag.JoinedLeelee)) break;
                // 見つからなければ次のキャラクターを選択
                index = (index + 1) % members.Length;
            }
            return nextPlayer;
        }

        private void SetNextPlayerPosition(GameObject nextPlayer)
        {
            // 位置を前のキャラに合わせる
            nextPlayer.transform.position = currentPlayer.transform.position;
            // 前のキャラの足の位置の高さ取得
            float previousFootPositionY = currentPlayer.transform.Find("Leg").localPosition.y;
            // 次のキャラの足の位置の高さ取得
            float nextFootPositionY = nextPlayer.transform.Find("Leg").localPosition.y;
            // 高さの差分を足す
            nextPlayer.transform.position += new Vector3(0, previousFootPositionY - nextFootPositionY, 0);
        }

        public GameObject GetCurrentPlayer()
        {
            return currentPlayer;
        }

        public void IncrementHealthLevel()
        {
            currentPlayer.GetComponent<BaseStats>().IncrementHealthLevel();
            hpBar.SetPlayer(currentPlayer);
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
            if (health != null) health.IsTempInvincible = value;
        }

        public void MoveToRelativePosition(Vector3 position, float timeout = 0)
        {
            if (currentPlayer == null) return;
            Mover mover = currentPlayer.GetComponent<Mover>();
            if (mover != null)
            {
                StartCoroutine(mover.MoveToRelativePosition(position, timeout));
            }
        }

        [System.Serializable]
        struct StatusSaveData
        {
            public float healthPoints;
            public float experiencePoints;
            public string currentPlayerName;
            public JToken currentPlayerPosition;
        }

        public JToken CaptureAsJToken()
        {
            // HP, Experienceを保存
            StatusSaveData statusSaveData = new StatusSaveData();
            statusSaveData.healthPoints = currentPlayer.GetComponent<Health>().GetHealthPoints();
            statusSaveData.experiencePoints = currentPlayer.GetComponent<Experience>().GetExperiencePoints();
            statusSaveData.currentPlayerName = currentPlayer.name;
            statusSaveData.currentPlayerPosition = currentPlayer.transform.position.ToToken();
            return JToken.FromObject(statusSaveData);
        }

        public void RestoreFromJToken(JToken state)
        {
            // セーブ時のキャラクターをcurrentPlayerとして復元
            StatusSaveData statusSaveData = state.ToObject<StatusSaveData>();
            string currentPlayerName = statusSaveData.currentPlayerName;
            if (currentPlayerName != null)
            {
                currentPlayer = Array.Find(members, member => member.name == currentPlayerName);
            }
            currentPlayer.GetComponent<Health>().SetHealthPoints(statusSaveData.healthPoints);
            currentPlayer.GetComponent<Experience>().SetExperiencePoints(statusSaveData.experiencePoints);
            currentPlayer.transform.position = statusSaveData.currentPlayerPosition.ToVector3();
            SetCurrentPlayerActive();
            ChangeDisplay();
            // キャラクターの変更イベントを発火
            OnChangeCharacter?.Invoke(currentPlayer);
        }
    }
}
