using System.Collections;
using VLCNP.Saving;
using UnityEngine;

namespace VLCNP.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        const string defaultSaveFile = "save";
        [SerializeField] float fadeInTime = 0.2f;

        IEnumerator Start() {
            Fader fader = FindObjectOfType<Fader>();
            fader.FadeOutImmediate();
            yield return GetComponent<JsonSavingSystem>().LoadLastScene(defaultSaveFile);
            yield return fader.FadeIn(fadeInTime);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                Delete();
            }
        }

        public void Save()
        {
            print($"Saving: {defaultSaveFile}");
            GetComponent<JsonSavingSystem>().Save(defaultSaveFile);
        }

        public void Load()
        {
            print($"Loading: {defaultSaveFile}");
            GetComponent<JsonSavingSystem>().Load(defaultSaveFile);
        }

        public void Delete()
        {
            GetComponent<JsonSavingSystem>().Delete(defaultSaveFile);
        }
    }
}
