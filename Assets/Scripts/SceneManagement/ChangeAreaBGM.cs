using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Core;

namespace VLCNP.SceneManagement
{
    public class ChangeAreaBGM : MonoBehaviour
    {
        [SerializeField] float fadeOutTime = 1f;
        [SerializeField] float fadeWaitTime = 0.2f;
        [SerializeField] float fadeInTime = 1f;
        private AudioSource BGM;
        private AreaBGM areaBGM;

        public void Execute()
        {
            StartCoroutine(Change());
        }

        private IEnumerator Change()
        {
            // 現在のBGMを取得
            BGM = GameObject.FindWithTag("BGM").GetComponent<AudioSource>();
            // エリアのBGMを取得
            areaBGM = GameObject.FindWithTag("AreaBGM").GetComponent<AreaBGM>();
            print("BGM.clip.name: " + BGM.clip.name);
            print("areaBGM.GetAudioClip().name: " + areaBGM.GetAudioClip().name);
            if (BGM.clip.name == areaBGM.GetAudioClip().name)
            {
                print("クリップの変更なし");
                yield break;
            }
            print("クリップの変更");
            // clipの変更があれば変更
            yield return BGMFadeRoutine(0, fadeWaitTime);
            BGM.Stop();
            BGM.clip = areaBGM.GetAudioClip();
            BGM.volume = areaBGM.GetVolume();
            BGM.pitch = areaBGM.GetPitch();
            print("BGM.clip.name: " + BGM.clip.name);
            print("BGM.volume: " + BGM.volume);
            print("BGM.pitch: " + BGM.pitch);
            BGM.Play();
        }

        private IEnumerator BGMFadeRoutine(float targetVolume, float time)
        {
            while (!Mathf.Approximately(BGM.volume, targetVolume))
            {
                BGM.volume = Mathf.MoveTowards(BGM.volume, targetVolume, Time.deltaTime / time);
                yield return null;
            }
        }
    }    
}
