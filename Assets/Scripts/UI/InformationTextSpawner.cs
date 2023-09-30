using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.UI
{
    public class InformationTextSpawner : MonoBehaviour
    {
        [SerializeField] InformationText textPrefab = null;
        [SerializeField] float SpawnOffsetY = 1.2f;

        public InformationText Spawn(string text)
        {
            // 頭の上に生成する
            InformationText instance = Instantiate<InformationText>(
                textPrefab,
                transform.position + new Vector3(0, SpawnOffsetY, 0),
                transform.rotation
            );
            instance.GetComponent<Canvas>().sortingLayerID = SortingLayer.NameToID("UI");
            instance.SetValue(text);
            return instance;
        }
    }
}
