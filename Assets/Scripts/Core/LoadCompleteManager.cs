using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VLCNP.Core
{
    public class LoadCompleteManager : MonoBehaviour
    {
        public static LoadCompleteManager Instance { get; private set; }

        [SerializeField] private int initializeFrame = 2;
        public bool IsLoaded { get; private set; } = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            StartCoroutine(InitializeObjects());
        }

        private IEnumerator InitializeObjects()
        {
            // initializeFrame分待つ
            for (int i = 0; i < initializeFrame; i++)
            {
                yield return null;
            }

            IsLoaded = true;
            Debug.Log("All objects initialized. Load complete.");
        }
    }    
}
