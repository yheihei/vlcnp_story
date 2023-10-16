using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Control
{
    public class PartyCongroller : MonoBehaviour
    {
        [SerializeField] GameObject currentPlayer;
        [SerializeField] GameObject[] members;

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
            SetNextPlayerPosition(index);
            currentPlayer = members[index];
            SetCurrentPlayerActive();
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
