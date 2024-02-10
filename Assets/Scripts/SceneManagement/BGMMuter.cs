using UnityEngine;
using VLCNP.Core;

namespace VLCNP.SceneManagement
{
    // SEがなるときに一時的にBGMの音量をさげるコンポーネント
    [RequireComponent(typeof(BGMWrapper))]
    public class BGMMuter : MonoBehaviour
    {
        // muteしたときにBGMの音量を何倍に下げるか
        [SerializeField] float volumeScale = 0.1f;
        BGMWrapper bgmWrapper;
        StartBGM bgm;
        float originalVolume = -1f;
        bool isMute = false;

        void Awake()
        {
            bgm = GameObject.FindWithTag("BGM")?.GetComponent<StartBGM>();
            // BGMの音量を変更するためのコンポーネントを取得
            bgmWrapper = GetComponent<BGMWrapper>();
        }

        public void SetMute()
        {
            if (bgm == null) return;
            // bgmから現在の音量を取得
            originalVolume = bgm.GetAudioVolume();
            // BGMの音量を下げる
            bgmWrapper.ChangeVolume(originalVolume * volumeScale);
            isMute = true;
        }

        public void RemoveMute()
        {
            if (!isMute) return;
            // BGMの音量を元に戻す
            bgmWrapper.ChangeVolume(originalVolume);
            isMute = false;
        }

    }
}
