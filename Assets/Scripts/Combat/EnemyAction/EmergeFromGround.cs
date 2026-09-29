using System.Collections;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * 地面に潜って待ち、プレイヤーが近づいたら盛り土を揺らしながら地面から出てくる。
     * 潜っている間は姿と当たり判定を消し、置いた位置の足元に盛り土だけを残す。
     * 出てくる途中は地面のタイルより奥に描いて、地面から生えてくるように見せる。
     */
    public class EmergeFromGround : EnemyAction
    {
        [SerializeField]
        SpriteRenderer bodyRenderer;

        [SerializeField, Tooltip("潜っている場所の盛り土。開始時にこの敵から切り離して地面に残す")]
        Transform mound;

        [SerializeField]
        ParticleSystem dirtParticles;

        [SerializeField, Tooltip("出てくる前に盛り土が揺れる時間")]
        float rumbleTime = 0.45f;

        [SerializeField, Tooltip("地面から出きるまでの時間")]
        float emergeTime = 0.8f;

        [SerializeField]
        float shakeWidth = 0.05f;

        [SerializeField, Tooltip("出てくる途中の描画レイヤー。地面のタイルより奥にする")]
        string emergingSortingLayer = "Default";

        [SerializeField]
        int emergingSortingOrder = 99;

        [SerializeField, Tooltip("潜っている間に盛り土がときどき動く間隔")]
        Vector2 twitchInterval = new Vector2(2f, 4f);

        [SerializeField]
        float moundFadeTime = 1.2f;

        const float ShakeFrequency = 30f;

        Rigidbody2D rbody;
        Vector3 standingPosition;
        Vector3 moundPosition;
        Vector3 moundScale;
        SpriteRenderer moundRenderer;
        int defaultSortingLayerId;
        int defaultSortingOrder;
        bool isBuried;
        float nextTwitchTime;
        Coroutine emergeRoutine;
        Coroutine twitchRoutine;

        void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            if (bodyRenderer == null)
                bodyRenderer = GetComponent<SpriteRenderer>();
            defaultSortingLayerId = bodyRenderer.sortingLayerID;
            defaultSortingOrder = bodyRenderer.sortingOrder;
            if (mound != null)
                moundRenderer = mound.GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            Bury();
        }

        void Update()
        {
            if (!isBuried || IsExecuting || mound == null)
                return;
            if (Time.time < nextTwitchTime)
                return;
            ScheduleTwitch();
            twitchRoutine = StartCoroutine(Twitch());
        }

        void OnDestroy()
        {
            if (mound != null)
                Destroy(mound.gameObject);
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;
            IsExecuting = true;
            if (!isBuried)
            {
                IsDone = true;
                return;
            }
            emergeRoutine = StartCoroutine(Emerge());
        }

        // イベントなどで止められたら、出てくる途中でも出きった状態にする
        public override void Stop()
        {
            if (emergeRoutine == null)
                return;
            StopCoroutine(emergeRoutine);
            emergeRoutine = null;
            FinishEmerge();
        }

        void Bury()
        {
            // 盛り土は置いた位置の足元に残すので、沈める前に切り離す
            if (mound != null)
            {
                mound.SetParent(null, true);
                moundPosition = mound.position;
                moundScale = mound.localScale;
                mound.gameObject.SetActive(true);
            }

            standingPosition = transform.position;
            // 頭の先まで足元より下に隠れる深さへ沈める
            transform.position = standingPosition + Vector3.down * (bodyRenderer.bounds.size.y + 0.05f);
            rbody.simulated = false;
            bodyRenderer.enabled = false;
            isBuried = true;
            ScheduleTwitch();
        }

        void ScheduleTwitch()
        {
            nextTwitchTime = Time.time + Random.Range(twitchInterval.x, twitchInterval.y);
        }

        // 潜っている間、ときどき盛り土を小さく揺らして何かいることを知らせる
        IEnumerator Twitch()
        {
            const float twitchTime = 0.25f;
            EmitDirt(2);
            for (float t = 0f; t < twitchTime; t += Time.deltaTime)
            {
                if (!isBuried || IsExecuting)
                    yield break;
                SetMound(Shake(t) * 0.6f, 1f + 0.08f * Mathf.Sin(t / twitchTime * Mathf.PI), 1f);
                yield return null;
            }
            SetMound(0f, 1f, 1f);
            twitchRoutine = null;
        }

        IEnumerator Emerge()
        {
            if (twitchRoutine != null)
            {
                StopCoroutine(twitchRoutine);
                twitchRoutine = null;
            }
            FacePlayer();

            // 盛り土が揺れながら膨らむ
            float emitTimer = 0f;
            for (float t = 0f; t < rumbleTime; t += Time.deltaTime)
            {
                float progress = t / rumbleTime;
                SetMound(Shake(t), 1f + 0.35f * progress, 1f);
                emitTimer = EmitDirtOverTime(emitTimer, 0.08f, 1);
                yield return null;
            }

            // 地面のタイルより奥に描き、下からせり上がる
            bodyRenderer.sortingLayerID = SortingLayer.NameToID(emergingSortingLayer);
            bodyRenderer.sortingOrder = emergingSortingOrder;
            bodyRenderer.enabled = true;
            Vector3 buriedPosition = transform.position;
            for (float t = 0f; t < emergeTime; t += Time.deltaTime)
            {
                float progress = t / emergeTime;
                float eased = 1f - (1f - progress) * (1f - progress);
                Vector3 position = Vector3.Lerp(buriedPosition, standingPosition, eased);
                position.x += Shake(t) * (1f - progress);
                transform.position = position;
                // 体が押し上げるにつれて盛り土が崩れて平らになる
                SetMound(Shake(t) * 0.5f, Mathf.Lerp(1.35f, 0.55f, progress), 1f);
                emitTimer = EmitDirtOverTime(emitTimer, 0.05f, 2);
                yield return null;
            }

            emergeRoutine = null;
            FinishEmerge();
        }

        void FinishEmerge()
        {
            transform.position = standingPosition;
            bodyRenderer.sortingLayerID = defaultSortingLayerId;
            bodyRenderer.sortingOrder = defaultSortingOrder;
            bodyRenderer.enabled = true;
            rbody.simulated = true;
            rbody.linearVelocity = Vector2.zero;
            isBuried = false;
            EmitDirt(10);
            if (mound != null)
                StartCoroutine(FadeMound());
            IsDone = true;
        }

        IEnumerator FadeMound()
        {
            SetMound(0f, 0.55f, 1f);
            for (float t = 0f; t < moundFadeTime; t += Time.deltaTime)
            {
                SetMound(0f, 0.55f, 1f - t / moundFadeTime);
                yield return null;
            }
            // 子の土の粒も飛び終わっているので盛り土ごと隠す
            mound.gameObject.SetActive(false);
        }

        // 出てくるときからプレイヤーの方を向いておく(元絵は左向き)
        void FacePlayer()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
                return;
            Vector3 localScale = transform.localScale;
            localScale.x = player.transform.position.x < transform.position.x
                ? Mathf.Abs(localScale.x)
                : -Mathf.Abs(localScale.x);
            transform.localScale = localScale;
        }

        void SetMound(float offsetX, float heightScale, float alpha)
        {
            if (mound == null)
                return;
            mound.position = moundPosition + Vector3.right * offsetX;
            mound.localScale = new Vector3(moundScale.x, moundScale.y * heightScale, moundScale.z);
            if (moundRenderer != null)
            {
                Color color = moundRenderer.color;
                color.a = alpha;
                moundRenderer.color = color;
            }
        }

        float Shake(float time)
        {
            // 小刻みに左右へ揺らす(モリモリ)
            return Mathf.Sign(Mathf.Sin(time * ShakeFrequency * Mathf.PI)) * shakeWidth;
        }

        float EmitDirtOverTime(float timer, float interval, int count)
        {
            timer += Time.deltaTime;
            if (timer < interval)
                return timer;
            EmitDirt(count);
            return timer - interval;
        }

        void EmitDirt(int count)
        {
            if (dirtParticles != null)
                dirtParticles.Emit(count);
        }
    }
}
