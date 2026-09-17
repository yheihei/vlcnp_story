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
            if (characterToExperiences == null)
            {
                return;
            }

            // キャラクターごとに経験値を設定
            foreach (CharacterToExperience e in characterToExperiences)
            {
                // シーンに配置していないキャラクターの未設定項目は飛ばす。
                if (e == null || e.characterExperience == null)
                {
                    continue;
                }

                e.characterExperience.SetExperiencePoints(e.experience);
            }
        }
#endif
    }
}
