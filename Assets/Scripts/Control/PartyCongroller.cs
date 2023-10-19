using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using VLCNP.UI;
using VLCNP.Attributes;
using VLCNP.Stats;
using VLCNP.Core;

namespace VLCNP.Control
{
    public class PartyCongroller : MonoBehaviour
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
            if (Input.GetKeyDown(KeyCode.A))
            {
                SwitchPlayer();
            }
        }

        private void SwitchPlayer()
        {
            int index = System.Array.IndexOf(members, currentPlayer);
            index = (index + 1) % members.Length;
            // 次のキャラクターの仲間フラグをチェックして、仲間でなければ次のキャラクターを選択
            GameObject nextPlayer = null;
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
            // 現在のキャラクターと同じであれば何もしない
            if (nextPlayer == currentPlayer) return;
            GameObject previousPlayer = currentPlayer;
            SetNextPlayerPosition(index);
            currentPlayer = members[index];
            SetCurrentPlayerActive();
            virtualCamera.Follow = currentPlayer.transform;
            // 前のキャラクターのHPを次のキャラクターに引き継ぐ
            currentPlayer.GetComponent<Health>().SetHealthPointsFromOther(previousPlayer.GetComponent<Health>());
            // HP表示のプレイヤーの切り替え
            hpDisplay.SetPlayer(currentPlayer);
            hpBar.SetPlayer(currentPlayer);
            // 前のキャラクターのExperienceを次のキャラクターに引き継ぐ
            currentPlayer.GetComponent<Experience>().SetExperiencePointsFromOther(previousPlayer.GetComponent<Experience>());
            // Experience表示のプレイヤーの切り替え
            experienceBar.SetPlayerExperience(currentPlayer);
            levelDisplay.SetBaseStats(currentPlayer.GetComponent<BaseStats>());
            // 前のキャラクターの死亡判定を次のキャラクターに引き継ぐ
            gameOver.SetPlayerHealth(currentPlayer.GetComponent<Health>());
        }

        private void SetNextPlayerPosition(int index)
        {
            // 位置を前のキャラに合わせる
            members[index].transform.position = currentPlayer.transform.position;
            // 前のキャラの足の位置の高さ取得
            float previousFootPositionY = currentPlayer.transform.Find("Leg").localPosition.y;
            // 次のキャラの足の位置の高さ取得
            float nextFootPositionY = members[index].transform.Find("Leg").localPosition.y;
            // 高さの差分を足す
            members[index].transform.position += new Vector3(0, previousFootPositionY - nextFootPositionY, 0);
        }
    }
}
