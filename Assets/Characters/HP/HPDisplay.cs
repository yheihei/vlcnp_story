using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPDisplay : MonoBehaviour
{
    private Text text;
    private GameObject player;
    private PlayerStatus playerStatus;

    void Start()
    {
        text = GetComponent<Text>();
        player = GameObject.FindWithTag("Player");
        playerStatus = player.GetComponent<PlayerStatus>();
    }

    void Update()
    {
        int hitPoints = playerStatus.HitPoints >= 0 ? playerStatus.HitPoints : 0;
        text.text = $"{hitPoints}";
    }
}
