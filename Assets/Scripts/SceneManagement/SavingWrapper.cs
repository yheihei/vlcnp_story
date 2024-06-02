using System.Collections;
using VLCNP.Saving;
using UnityEngine;
using System.IO.Enumeration;

namespace VLCNP.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        const string defaultSaveFile = "save";

        public void Save(string fileName = defaultSaveFile)
        {
            print($"Saving: {fileName}");
            GetComponent<JsonSavingSystem>().Save(fileName);
        }

        public void AutoSave()
        {
            GetComponent<JsonSavingSystem>().Save(JsonSavingSystem.AUTO_SAVE_FILE_NAME);
        }

        public void LoadOnlyState(string fileName = defaultSaveFile)
        {
            print($"Loading: {fileName}");
            GetComponent<JsonSavingSystem>().LoadOnlyState(fileName);
        }

        public IEnumerator Load(string fileName = defaultSaveFile)
        {
            print($"Loading: {fileName}");
            yield return GetComponent<JsonSavingSystem>().LoadLastScene(fileName);
        }

        public void Delete(string fileName = defaultSaveFile)
        {
            GetComponent<JsonSavingSystem>().Delete(fileName);
        }
    }
}
