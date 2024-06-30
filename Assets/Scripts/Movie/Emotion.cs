using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Movie
{
    public class Emotion : MonoBehaviour
    {
        // SEを設定
        [SerializeField] AudioClip se = null;

        public void Bikkuri(float waitFrame = 90)
        {
            AudioSource.PlayClipAtPoint(se, transform.position);
            GameObject obj = Instantiate(Resources.Load<GameObject>("Emotion/Bikkuri"), transform.position, Quaternion.identity);
            // waitFrameフレーム後に削除
            Destroy(obj, waitFrame / 60);
        }
    }
}
