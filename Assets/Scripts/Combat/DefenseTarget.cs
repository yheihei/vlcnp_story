using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Core;

namespace VLCNP.Combat
{
    /**
     * 防衛戦で守る相手(捕まっているミタマなど)。
     * 防衛戦の敵が当たり判定に触れると被弾し、決めた回数の被弾で守りきれなかったことになる。
     * 被弾後は操作キャラと同じ無敵時間のあいだ同じように点滅し、その間は被弾しない。
     * 当たり判定はコライダーではなく数値の範囲で持つ(トリガーを置くと、触れた操作キャラの接地判定が外れるため)。
     */
    public class DefenseTarget : MonoBehaviour, IStoppable
    {
        // 操作キャラが見つからないときの無敵時間(Health の既定値と同じ)
        const float DefaultInvincibleTime = 3f;

        // 点滅の速さ(Health と同じ)
        const float FlashSpeed = 10f;

        [SerializeField, Min(1)]
        int maxHits = 10;

        [SerializeField, Tooltip("位置からずらした当たり判定の中心(ワールド座標の長さ)")]
        Vector2 hitAreaOffset = Vector2.zero;

        [SerializeField, Tooltip("当たり判定の大きさ(ワールド座標の長さ)")]
        Vector2 hitAreaSize = new Vector2(1f, 1.6f);

        [SerializeField]
        SpriteRenderer bodyRenderer;

        [SerializeField, Tooltip("被弾したときの音(操作キャラと同じもの)")]
        AudioClip hitSe;

        [SerializeField, Range(0f, 1f)]
        float hitSeVolume = 0.4f;

        int hitCount;
        float invincibleTimer;
        bool isGuarding;
        bool isStopped;

        public bool IsStopped
        {
            get => isStopped;
            set => isStopped = value;
        }

        public int MaxHits => maxHits;
        public int RemainingHits => Mathf.Max(0, maxHits - hitCount);
        public bool IsDefeated => hitCount >= maxHits;
        public Vector2 HitAreaCenter => (Vector2)transform.position + hitAreaOffset;
        public Vector2 HitAreaSize => hitAreaSize;

        bool CanBeHit => isGuarding && !isStopped && !IsDefeated && invincibleTimer <= 0f;

        void Awake()
        {
            if (bodyRenderer == null)
                bodyRenderer = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (invincibleTimer <= 0f)
                return;
            invincibleTimer -= Time.deltaTime;
            SetAlpha(invincibleTimer > 0f ? Mathf.Abs(Mathf.Sin(Time.time * FlashSpeed)) : 1f);
        }

        // 防衛戦が始まったら被弾の回数を戻し、被弾を受け付ける
        public void BeginGuard()
        {
            hitCount = 0;
            invincibleTimer = 0f;
            isGuarding = true;
            SetAlpha(1f);
        }

        // 防衛戦が終わったら被弾を受け付けず、点滅も止める
        public void EndGuard()
        {
            isGuarding = false;
            invincibleTimer = 0f;
            SetAlpha(1f);
        }

        // 敵が触れたときに呼ぶ。無敵時間中などで被弾しなかったら false
        public bool TakeHit()
        {
            if (!CanBeHit)
                return false;
            hitCount++;
            invincibleTimer = GetPlayerInvincibleTime();
            if (hitSe != null)
            {
                Vector3 position = Camera.main != null ? Camera.main.transform.position : transform.position;
                AudioSource.PlayClipAtPoint(hitSe, position, hitSeVolume);
            }
            return true;
        }

        float GetPlayerInvincibleTime()
        {
            GameObject player = GameObject.FindWithTag("Player");
            return player != null && player.TryGetComponent(out Health health)
                ? health.InvincibleTime
                : DefaultInvincibleTime;
        }

        void SetAlpha(float alpha)
        {
            if (bodyRenderer == null)
                return;
            Color color = bodyRenderer.color;
            color.a = alpha;
            bodyRenderer.color = color;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(HitAreaCenter, hitAreaSize);
        }
    }
}
