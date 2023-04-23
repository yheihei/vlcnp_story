using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceDisplay : MonoBehaviour
{
    private Text text;
    private GameObject player;
    private PlayerLevel playerLevel;

    void Start()
    {
        text = GetComponent<Text>();
        playerLevel = GameObject.FindWithTag("Player").GetComponent<PlayerLevel>();
    }

    void Update()
    {
        text.text = $"{playerLevel.Level}";
    }
}
