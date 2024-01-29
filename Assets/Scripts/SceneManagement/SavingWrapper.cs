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

        // IEnumerator Start() {
        //     print("@@@@@@@SavingWrapper@@@@@@@");
        //     Fader fader = GameObject.FindWithTag("SceneFader").GetComponent<Fader>();
        //     fader.FadeOutImmediate();
        //     // ゲームスタート時のロードはやめる
        //     // yield return GetComponent<JsonSavingSystem>().LoadLastScene(defaultSaveFile);
        //     print("@@@@@@@SavingWrapper FadeIn@@@@@@@");
        //     yield return fader.FadeIn(fadeInTime);
        // }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Load("save");
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                Delete();
                Delete("save");
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

        public IEnumerator Load(string fileName=defaultSaveFile)
        {
            print($"Loading: {fileName}");
            yield return GetComponent<JsonSavingSystem>().LoadLastScene(fileName);
        }

        public void Delete(string fileName=defaultSaveFile)
        {
            GetComponent<JsonSavingSystem>().Delete(fileName);
        }
    }
}
