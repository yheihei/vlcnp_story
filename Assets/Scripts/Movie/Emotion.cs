using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Movie
{
    public class Emotion : MonoBehaviour
    {
        // SEを設定
        [SerializeField]
        AudioClip se = null;

        [SerializeField]
        float BikkuriVolume = 0.4f;

        // 何フレーム後に削除するか
        int waitFrame = 90;

        public void SetWaitFrame(int frame)
        {
            waitFrame = frame;
        }

        public void Bikkuri()
        {
            CreateEmotion("Emotion/Bikkuri");
        }

        public void Hatena()
        {
            CreateEmotion("Emotion/Hatena");
        }

        private void CreateEmotion(string resourcePath)
        {
            AudioSource.PlayClipAtPoint(se, transform.position, BikkuriVolume);
            GameObject obj = Instantiate(Resources.Load<GameObject>(resourcePath), transform);
            Destroy(obj, waitFrame / 60);
        }
    }
}
