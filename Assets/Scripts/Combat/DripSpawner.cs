using System.Collections;
using UnityEngine;
using VLCNP.Core;

namespace VLCNP.Combat
{
    /**
     * 天井に置き、一定間隔で水滴をしたたらせる。
     * 水滴は落ちる前に天井で膨らむので、プレイヤーは落下位置を見て避けられる
     */
    public class DripSpawner : MonoBehaviour, IStoppable
    {
        [SerializeField]
        FallingDrop dropPrefab = null;

        [Header("水滴が膨らみはじめる位置。未設定なら自身の位置")]
        [SerializeField]
        Transform dropPoint = null;

        [SerializeField]
        float damage = 2f;

        [Header("水滴が膨らんでから落ちるまでの時間")]
        [SerializeField]
        float formDuration = 1f;

        [Header("落ちてから次の水滴が膨らみはじめるまでの間隔。この範囲からランダムに選ぶ")]
        [SerializeField]
        float minInterval = 1.5f;

        [SerializeField]
        float maxInterval = 3f;

        [Header("最初の水滴までの待ち時間。複数設置時にずらす用")]
        [SerializeField]
        float firstDelay = 0f;

        bool isStopped = false;
        public bool IsStopped
        {
            get => isStopped;
            set => isStopped = value;
        }

        private Coroutine dripLoopCoroutine;

        private void OnEnable()
        {
            dripLoopCoroutine = StartCoroutine(DripLoop());
        }

        private void OnDisable()
        {
            if (dripLoopCoroutine == null)
                return;

            StopCoroutine(dripLoopCoroutine);
            dripLoopCoroutine = null;
        }

        private IEnumerator DripLoop()
        {
            yield return WaitWhileRunning(firstDelay);
            while (true)
            {
                Drip();
                yield return WaitWhileRunning(formDuration + Random.Range(minInterval, maxInterval));
            }
        }

        // 停止中は時間を進めない
        private IEnumerator WaitWhileRunning(float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                if (!isStopped)
                    elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void Drip()
        {
            if (dropPrefab == null)
                return;
            Transform point = dropPoint != null ? dropPoint : transform;
            FallingDrop drop = Instantiate(dropPrefab, point.position, Quaternion.identity);
            drop.Drip(damage, formDuration);
        }
    }
}
