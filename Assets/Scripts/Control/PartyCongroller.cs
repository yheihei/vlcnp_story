using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using VLCNP.UI;
using VLCNP.Attributes;
using VLCNP.Stats;

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
        CinemachineVirtualCamera virtualCamera;

        private void Awake()
        {
            SetCurrentPlayerActive();
            virtualCamera = GameObject.FindWithTag("CMCamera").GetComponent<CinemachineVirtualCamera>();
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
