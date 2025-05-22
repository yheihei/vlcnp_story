using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Stats; // PoisonStatusクラスを参照するために追加

namespace VLCNP.Combat
{
    public class PoisonAttacher : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start() { }

        // ダメージを受けた際に呼び出される想定のメソッド
        public void AttachPoison(GameObject target)
        {
            PoisonStatus poisonStatus = target.GetComponent<PoisonStatus>();
            if (poisonStatus == null)
            {
                // PoisonStatusコンポーネントがなければ追加する
                poisonStatus = target.AddComponent<PoisonStatus>();
            }
            poisonStatus.ActivatePoison();
            Debug.Log($"Attempted to attach poison to {target.name}");
        }
    }
}
