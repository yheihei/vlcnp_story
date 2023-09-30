using System.Collections;
using VLCNP.Saving;
using UnityEngine;
using System.IO.Enumeration;

namespace VLCNP.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        const string defaultSaveFile = "save";
        [SerializeField] float fadeInTime = 0.2f;

        IEnumerator Start() {
            Fader fader = FindObjectOfType<Fader>();
            fader.FadeOutImmediate();
            // ゲームスタート時のロードはやめる
            // yield return GetComponent<JsonSavingSystem>().LoadLastScene(defaultSaveFile);
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

        public void Save(string fileName=defaultSaveFile)
        {
            print($"Saving: {fileName}");
            GetComponent<JsonSavingSystem>().Save(fileName);
        }

        public void LoadOnlyState(string fileName=defaultSaveFile)
        {
            print($"Loading: {fileName}");
            GetComponent<JsonSavingSystem>().LoadOnlyState(fileName);
        }

        public void Load(string fileName=defaultSaveFile)
        {
            print($"Loading: {fileName}");
            StartCoroutine(GetComponent<JsonSavingSystem>().LoadLastScene(fileName));
        }

        public void Delete()
        {
            GetComponent<JsonSavingSystem>().Delete(defaultSaveFile);
        }
    }
}
