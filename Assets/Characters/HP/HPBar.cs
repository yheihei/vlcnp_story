using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    private Slider slider;
    private GameObject player;
    private PlayerStatus playerStatus;

    void Start()
    {
        slider = GetComponent<Slider>();
        slider.value = 1;
        player = GameObject.FindWithTag("Player");
        playerStatus = player.GetComponent<PlayerStatus>();
    }

    void Update()
    {
        int hitPoints = playerStatus.HitPoints >= 0 ? playerStatus.HitPoints : 0;
        slider.value = (float) hitPoints / (float) playerStatus.MaxHitPoints;
    }
}
