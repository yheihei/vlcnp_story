using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Stats;

namespace VLCNP.DebugSctipt
{
    public class DebugExperience : MonoBehaviour
    {
        // キャラクターごとに経験値を設定する
        [SerializeField]
        private CharacterToExperience[] characterToExperiences;

        [System.Serializable]
        class CharacterToExperience
        {
            public Experience characterExperience;
            public int experience;
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        void Start()
        {
            // キャラクターごとに経験値を設定
            foreach (CharacterToExperience e in characterToExperiences)
            {
                e.characterExperience.SetExperiencePoints(e.experience);
            }
        }
#endif
    }
}
