using System.Collections;
using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Combat.EnemyAction;

namespace VLCNP.Combat
{
    /**
     * 防衛戦で高台から矢を撃つスケルトンアーチャー。
     * 防衛戦が始まるまでは崩れた骨の姿で何もせず、始まると起き上がり、終わると崩れ落ちて骨に戻る。
     * 高台にいて倒せないので常に無敵にする。
     * EnemyV2Controller の検知(IDetect)として使い、起き上がっている間だけ行動(向く・撃つ)させる。
     * 骨の姿と起き上がる途中はアニメーターを止めて絵を直接切り替える(撃ちかけの矢も出なくなる)。
     */
    [RequireComponent(typeof(EnemyV2Controller))]
    public class DefenseBattleArcher : MonoBehaviour, IDetect
    {
        enum State
        {
            Collapsed,
            Rising,
            Active,
            Collapsing,
        }

        [SerializeField]
        SpriteRenderer bodyRenderer;

        [SerializeField]
        Animator animator;

        [SerializeField, Tooltip("崩れた骨の姿(スケルトンナイトの未発見時の絵)")]
        Sprite collapsedSprite;

        [SerializeField, Tooltip("起き上がる途中の絵。崩れるときは逆順に使う")]
        Sprite[] riseFrames;

        [SerializeField, Min(0.01f)]
        float riseFrameTime = 0.22f;

        [SerializeField, Min(0.01f)]
        float collapseFrameTime = 0.1f;

        [SerializeField, Min(0), Tooltip("起き上がる前に骨がカタカタ動く回数")]
        int rattleCount = 3;

        [SerializeField, Min(0.01f)]
        float rattleFrameTime = 0.08f;

        [SerializeField, Tooltip("骨の姿のときは消しておく接触攻撃")]
        GameObject directAttackDetector;

        State state = State.Collapsed;
        Health health;
        Coroutine routine;

        public bool IsActive => state == State.Active;
        public bool IsCollapsed => state == State.Collapsed;

        void Awake()
        {
            health = GetComponent<Health>();
            if (bodyRenderer == null)
                bodyRenderer = GetComponent<SpriteRenderer>();
            if (animator == null)
                animator = GetComponent<Animator>();
            // 高台にいて倒せない
            if (health != null)
                health.IsTempInvincible = true;
            ShowCollapsed();
        }

        public bool IsDetect()
        {
            return state == State.Active;
        }

        public void Rise()
        {
            if (state != State.Collapsed)
                return;
            routine = StartCoroutine(RiseRoutine());
        }

        public void Collapse()
        {
            if (state == State.Collapsed || state == State.Collapsing)
                return;
            if (routine != null)
                StopCoroutine(routine);
            routine = StartCoroutine(CollapseRoutine());
        }

        IEnumerator RiseRoutine()
        {
            state = State.Rising;
            // 骨がカタカタ動いてから起き上がる(物理の体は動かさず、絵だけ切り替える)
            if (riseFrames.Length > 0)
            {
                for (int i = 0; i < rattleCount; i++)
                {
                    bodyRenderer.sprite = riseFrames[0];
                    yield return new WaitForSeconds(rattleFrameTime);
                    bodyRenderer.sprite = collapsedSprite;
                    yield return new WaitForSeconds(rattleFrameTime);
                }
            }
            foreach (Sprite frame in riseFrames)
            {
                bodyRenderer.sprite = frame;
                yield return new WaitForSeconds(riseFrameTime);
            }
            // 立ち姿(待機)からアニメーターに任せる
            animator.enabled = true;
            if (directAttackDetector != null)
                directAttackDetector.SetActive(true);
            state = State.Active;
            routine = null;
        }

        IEnumerator CollapseRoutine()
        {
            state = State.Collapsing;
            // 撃ちかけの矢がアニメーションイベントで出ないよう、先にアニメーターを止める
            animator.enabled = false;
            if (directAttackDetector != null)
                directAttackDetector.SetActive(false);
            for (int i = riseFrames.Length - 1; i >= 0; i--)
            {
                bodyRenderer.sprite = riseFrames[i];
                yield return new WaitForSeconds(collapseFrameTime);
            }
            ShowCollapsed();
            routine = null;
        }

        void ShowCollapsed()
        {
            animator.enabled = false;
            bodyRenderer.sprite = collapsedSprite;
            if (directAttackDetector != null)
                directAttackDetector.SetActive(false);
            state = State.Collapsed;
        }
    }
}
