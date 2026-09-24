using UnityEngine;

namespace VLCNP.Effects
{
    /**
     * FollowMainCamera でカメラに付いていく環境粒子を、カメラが瞬間移動した後に移動先で作り直す。
     * 粒子はワールド空間や遅れて動く座標系で動くので、シーン開始やカメラの切り替えで元の場所に取り残され、しばらく画面が空になる。
     * 移動したフレームの Simulate は移動前の位置で粒子を作るので、そのフレームは消すだけにして、次のフレームで 1 周期ぶん先までシミュレーションし直す。
     * FollowMainCamera(1001)の後に動かす。
     */
    [DefaultExecutionOrder(1002)]
    public class AmbientParticleRewarm : MonoBehaviour
    {
        [SerializeField, Header("1フレームでこの距離以上動いたら作り直す")]
        private float jumpDistance = 4f;

        private ParticleSystem[] particleSystems;
        private Vector3 lastPosition;
        private bool hasLastPosition;
        private bool rewarmPending;

        private void Awake()
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }

        private void OnEnable()
        {
            hasLastPosition = false;
            rewarmPending = false;
        }

        private void LateUpdate()
        {
            if (rewarmPending)
            {
                rewarmPending = false;
                Rewarm();
            }

            Vector3 position = transform.position;
            if (!hasLastPosition || (position - lastPosition).sqrMagnitude >= jumpDistance * jumpDistance)
            {
                Clear();
                rewarmPending = true;
            }
            lastPosition = position;
            hasLastPosition = true;
        }

        private void Clear()
        {
            foreach (ParticleSystem system in particleSystems)
            {
                system.Clear(false);
            }
        }

        private void Rewarm()
        {
            foreach (ParticleSystem system in particleSystems)
            {
                system.Simulate(system.main.duration, false, true, true);
                system.Play(false);
            }
        }
    }
}
