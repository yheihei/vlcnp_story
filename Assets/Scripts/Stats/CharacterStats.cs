using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Stats
{
    [CreateAssetMenu(fileName = "CharacterStats", menuName = "Stats/New CharacterStats", order = 0)]
    public class CharacterStats : ScriptableObject
    {
        [SerializeField] StatsCharacterClass[] characterClasses = null;

        Dictionary<CharacterClass, Dictionary<Stat, float[]>> lookupTable = null;

        [System.Serializable]
        class StatsCharacterClass
        {
            [SerializeField] public CharacterClass characterClass;
            public CharacterStat[] stats;
        }

        [System.Serializable]
        class CharacterStat
        {
            public Stat stat;
            public float[] levels;
        }

        public float GetStat(Stat stat, CharacterClass characterClass, int level)
        {
            BuildLookup();
            try
            {
                return lookupTable[characterClass][stat][level - 1];
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogWarning(e.Message);
                return 0;
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.LogWarning(e.Message);
                return 0;
            }
        }

        private void BuildLookup()
        {
            if (lookupTable != null) return;

            lookupTable = new Dictionary<CharacterClass, Dictionary<Stat, float[]>>();

            foreach (StatsCharacterClass progressionClass in characterClasses)
            {
                Dictionary<Stat, float[]> startLookupTable = new Dictionary<Stat, float[]>();

                foreach (CharacterStat progressionStat in progressionClass.stats)
                {
                    startLookupTable[progressionStat.stat] = progressionStat.levels;
                }
                lookupTable[progressionClass.characterClass] = startLookupTable;
            }
        }
    }
}


