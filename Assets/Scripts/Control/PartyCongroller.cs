using UnityEngine;
using Cinemachine;
using VLCNP.UI;
using VLCNP.Attributes;
using VLCNP.Stats;
using VLCNP.Core;
using VLCNP.Saving;
using Newtonsoft.Json.Linq;
using System;

namespace VLCNP.Control
{
    public class PartyCongroller : MonoBehaviour, IJsonSaveable
    {
        [SerializeField] GameObject currentPlayer;
        [SerializeField] GameObject[] members;
        [SerializeField] HPDisplay hpDisplay;
        [SerializeField] HPBar hpBar;
        [SerializeField] ExperienceBar experienceBar;
        [SerializeField] LevelDisplay levelDisplay;
        [SerializeField] GameOver gameOver;
        [SerializeField] FlagManager flagManager;
        [SerializeField]
        CinemachineVirtualCamera virtualCamera;
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
            }
        }

        private void Update()
        {
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
            SetNextPlayerPosition(nextPlayer);
            currentPlayer = nextPlayer;
            SetCurrentPlayerActive();

            virtualCamera.Follow = currentPlayer.transform;

            // 前のキャラクターのHPを次のキャラクターに引き継ぐ
            currentPlayer.GetComponent<Health>().SetHealthPoints(previousPlayer.GetComponent<Health>().GetHealthPoints());
            // HP表示のプレイヤーの切り替え
            hpDisplay.SetPlayer(currentPlayer);
            hpBar.SetPlayer(currentPlayer);

            // 前のキャラクターのExperienceを次のキャラクターに引き継ぐ
            currentPlayer.GetComponent<Experience>().SetExperiencePoints(previousPlayer.GetComponent<Experience>().GetExperiencePoints());
            // Experience表示のプレイヤーの切り替え
            experienceBar.SetPlayerExperience(currentPlayer);
            levelDisplay.SetBaseStats(currentPlayer.GetComponent<BaseStats>());

            // 前のキャラクターの死亡判定を次のキャラクターに引き継ぐ
            gameOver.SetPlayerHealth(currentPlayer.GetComponent<Health>());

            // キャラクターの変更イベントを発火
            OnChangeCharacter?.Invoke(currentPlayer);
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

        public void SetVisibility(bool isVisible)
        {
            // 今有効なmemberを表示、非表示する
            currentPlayer.SetActive(isVisible);
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
            statusSaveData.healthPoints = currentPlayer.GetComponent<Health>().GetHealthPoints();
            statusSaveData.experiencePoints = currentPlayer.GetComponent<Experience>().GetExperiencePoints();
            return JToken.FromObject(statusSaveData);
        }

        public void RestoreFromJToken(JToken state)
        {
            // HP, Experienceを復元
            StatusSaveData statusSaveData = state.ToObject<StatusSaveData>();
            currentPlayer.GetComponent<Health>().SetHealthPoints(statusSaveData.healthPoints);
            currentPlayer.GetComponent<Experience>().SetExperiencePoints(statusSaveData.experiencePoints);
        }
    }
}
