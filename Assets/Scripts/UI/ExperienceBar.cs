using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using VLCNP.Stats;

namespace VLCNP.UI
{
    public class ExperienceBar : MonoBehaviour
    {
        private Slider slider;
        Experience playerExperience;
        BaseStats baseStats;

        void Awake()
        {
            slider = GetComponent<Slider>();
            slider.value = 1;
            playerExperience = GameObject.FindWithTag("Player").GetComponent<Experience>();
            baseStats = GameObject.FindWithTag("Player").GetComponent<BaseStats>();
        }

        private void LateUpdate()
        {
            if (baseStats.isReachedMaxLevel()) {
                slider.value = 1;
                return;
            }
            float experienceToNextLevel = baseStats.GetExperienceToNextLevel();
            float currentExperience = playerExperience.GetExperiencePoints();
            float experienceToCurrentLevel = baseStats.GetExperienceToCurrentLevel();
            slider.value = (currentExperience - experienceToCurrentLevel) / (experienceToNextLevel - experienceToCurrentLevel);
        }

        public void SetPlayerExperience(GameObject newPlayer)
        {
            playerExperience = newPlayer.GetComponent<Experience>();
            baseStats = newPlayer.GetComponent<BaseStats>();
        }
    }    
}
