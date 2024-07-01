using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Movie
{
    public class Emotion : MonoBehaviour
    {
        // SEを設定
        [SerializeField] AudioClip se = null;
        // 何フレーム後に削除するか
        int waitFrame = 90;

        public void SetWaitFrame(int frame)
        {
            waitFrame = frame;
        }

        public void Bikkuri()
        {
            AudioSource.PlayClipAtPoint(se, transform.position);
            // 子にBikkuriを生成する
            GameObject obj = Instantiate(Resources.Load<GameObject>("Emotion/Bikkuri"), transform);
            // waitFrameフレーム後に削除
            Destroy(obj, waitFrame / 60);
        }
    }
}
