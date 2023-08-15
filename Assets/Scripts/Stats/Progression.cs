using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Stats/New Progression", order = 0)]
    public class Progression : ScriptableObject
    {
        [SerializeField] ObjectStatClass[] objectStatClasses = null;

        Dictionary<StatClass, Dictionary<Stat, float[]>> lookupTable = null;

        [System.Serializable]
        class ObjectStatClass
        {
            [SerializeField] public StatClass statClass;
            public ObjectStat[] stats;
        }

        [System.Serializable]
        class ObjectStat
        {
            public Stat stat;
            public float[] levels;
        }

        public float GetStat(Stat stat, StatClass statClass, int level)
        {
            BuildLookup();
            try
            {
                return lookupTable[statClass][stat][level - 1];
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

        public int GetLevels(Stat stat, StatClass statClass)
        {
            BuildLookup();

            float[] levels = lookupTable[statClass][stat];
            return levels.Length;
        }

        private void BuildLookup()
        {
            if (lookupTable != null) return;

            lookupTable = new Dictionary<StatClass, Dictionary<Stat, float[]>>();

            foreach (ObjectStatClass progressionClass in objectStatClasses)
            {
                Dictionary<Stat, float[]> startLookupTable = new Dictionary<Stat, float[]>();

                foreach (ObjectStat progressionStat in progressionClass.stats)
                {
                    startLookupTable[progressionStat.stat] = progressionStat.levels;
                }
                lookupTable[progressionClass.statClass] = startLookupTable;
            }
        }
    }
}


