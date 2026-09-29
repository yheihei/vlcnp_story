using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.Combat.EnemyAction
{
    /**
     * 毎回同じ高さで跳ねながらプレイヤーへ近づく。
     * 跳ぶ前に横速度の候補ごとに放物線をなぞって着地点を求め、いちばんプレイヤーに近づける跳び方を選ぶ。
     * 1回で近づけないときは2回分の跳び方を調べ、下がって助走を取ったり障害物を越えたりする。
     * 跳ぶ高さは変えないので、届かない段差は登らずにその場で跳ねる。
     */
    public class HopTowardPlayer : EnemyAction
    {
        [SerializeField]
        CapsuleCollider2D bodyCollider;

        [Header("跳躍")]
        [SerializeField, Tooltip("毎回この高さまで跳ぶ(ユニット)")]
        float jumpHeight = 2.8f;

        [SerializeField, Tooltip("横速度の上限(ユニット/秒)")]
        float maxHorizontalSpeed = 3.2f;

        [SerializeField, Tooltip("着地してから次に跳ぶまでの間")]
        float landingPauseTime = 0.35f;

        [SerializeField]
        float maxAirTime = 3f;

        [Header("着地点の計算")]
        [SerializeField, Tooltip("片側の横速度の候補数")]
        int speedSamples = 8;

        [SerializeField, Tooltip("これより深く落ちる跳び方はしない")]
        float maxDropHeight = 12f;

        [SerializeField, Tooltip("登る高さを横の距離より重く見る倍率")]
        float climbWeight = 1.2f;

        [SerializeField, Tooltip("これ以上近づけない跳び方は進んでいないとみなす")]
        float minProgress = 0.3f;

        [SerializeField, Tooltip("壁や天井にぶつかる跳び方の減点")]
        float bumpPenalty = 0.5f;

        [SerializeField, Tooltip("登れる段差の手前に残す助走の幅")]
        float runUpDistance = 0.6f;

        [SerializeField, Tooltip("この中には着地しない(水など)")]
        LayerMask avoidLayers;

        const float SimulationStep = 0.05f;
        const int MaxSimulationSteps = 90;
        // 床に触れた状態から軌道をなぞり始めないよう、少し浮かせる
        const float Lift = 0.02f;
        const float LandingNormalY = 0.9f;
        const float RunUpPenaltyWeight = 1.5f;
        const float ClimbClearance = 0.3f;

        struct HopResult
        {
            public bool IsValid;
            public float Vx;
            public Vector2 LandingCenter;
            public float FeetY;
            public bool Bumped;

            public static HopResult Invalid => new HopResult { IsValid = false };
        }

        struct SecondHop
        {
            public Vector2 Landing;
            public float Cost;
        }

        Rigidbody2D rbody;
        Transform playerTransform;
        Collider2D playerCollider;
        Coroutine hopRoutine;
        ContactFilter2D obstacleFilter;
        ContactFilter2D avoidFilter;
        readonly RaycastHit2D[] castHits = new RaycastHit2D[8];
        readonly Collider2D[] overlapResults = new Collider2D[4];
        readonly List<SecondHop> secondHops = new List<SecondHop>();

        // 2回分調べても近づけなかった位置と目標。同じ状況で重い探索を繰り返さない
        bool hasStuckRecord;
        Vector2 stuckPosition;
        Vector2 stuckTarget;

        void Awake()
        {
            rbody = GetComponent<Rigidbody2D>();
            if (bodyCollider == null)
                bodyCollider = GetComponent<CapsuleCollider2D>();

            int obstacleMask =
                Physics2D.GetLayerCollisionMask(gameObject.layer)
                & ~LayerMask.GetMask("Player", "PlayerAttack");
            obstacleFilter = new ContactFilter2D();
            obstacleFilter.SetLayerMask(obstacleMask);
            obstacleFilter.useTriggers = false;

            avoidFilter = new ContactFilter2D();
            avoidFilter.SetLayerMask(avoidLayers);
            avoidFilter.useTriggers = true;
        }

        public override void Execute()
        {
            if (IsExecuting || IsDone)
                return;
            IsExecuting = true;
            hopRoutine = StartCoroutine(Hop());
        }

        public override void Stop()
        {
            if (hopRoutine != null)
            {
                StopCoroutine(hopRoutine);
                hopRoutine = null;
            }
            if (!IsExecuting)
                return;
            // 跳んでいる途中なら横へ流れないようにして真下へ落とす
            rbody.linearVelocity = new Vector2(0f, rbody.linearVelocity.y);
            IsExecuting = false;
            IsDone = true;
        }

        IEnumerator Hop()
        {
            // 空中にいるなら着地を待ってから次の跳び方を決める
            float waited = 0f;
            int stillFrames = 0;
            while (!IsResting(ref stillFrames) && waited < maxAirTime)
            {
                waited += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            if (!TryGetTargetFeet(out Vector2 targetFeet))
            {
                yield return new WaitForSeconds(landingPauseTime);
                FinishHop();
                yield break;
            }

            HopResult plan = PlanHop(targetFeet);
            // 助走のために下がるときもプレイヤーの方を向いたまま跳ぶ
            FaceTo(targetFeet.x);
            if (!plan.IsValid)
            {
                yield return new WaitForSeconds(landingPauseTime);
                FinishHop();
                yield break;
            }

            rbody.linearVelocity = new Vector2(plan.Vx, LaunchSpeedY);

            float airTime = 0f;
            stillFrames = 0;
            while (airTime < maxAirTime)
            {
                yield return new WaitForFixedUpdate();
                airTime += Time.fixedDeltaTime;
                if (airTime > 0.1f && rbody.linearVelocity.y <= 0.01f && IsResting(ref stillFrames))
                    break;
            }

            // 着地したら滑らないように横の速度を消す
            rbody.linearVelocity = new Vector2(0f, rbody.linearVelocity.y);
            // 複数いても跳ぶ拍子がそろわないよう少しばらつかせる。麻痺などで遅くなっていれば間も延ばす
            float speedRate = Mathf.Max(GetModifiedSpeed(1f), 0.1f);
            yield return new WaitForSeconds(landingPauseTime * Random.Range(0.85f, 1.15f) / speedRate);
            FinishHop();
        }

        void FinishHop()
        {
            hopRoutine = null;
            IsDone = true;
        }

        float Gravity => -Physics2D.gravity.y * rbody.gravityScale;

        float LaunchSpeedY => Mathf.Sqrt(2f * Gravity * jumpHeight);

        HopResult PlanHop(Vector2 targetFeet)
        {
            Vector2 start = GetBodyCenter();
            Vector2 size = GetBodySize();
            float currentCost = Cost(start.x, start.y - size.y * 0.5f, targetFeet);
            float maxSpeed = GetModifiedSpeed(maxHorizontalSpeed);

            HopResult best = FindBestHop(start, size, maxSpeed, targetFeet, out float bestCost);
            if (best.IsValid && bestCost < currentCost - minProgress)
                return best;

            // 1回で近づけないときは、下がって助走を取る・段や墓を越えるなど2回分の跳び方を調べる
            if (currentCost > minProgress * 2f && !IsSameStuckSituation(start, targetFeet))
            {
                HopResult twoHop = FindTwoHopPlan(start, size, maxSpeed, targetFeet, currentCost);
                if (twoHop.IsValid)
                    return twoHop;
                hasStuckRecord = true;
                stuckPosition = start;
                stuckTarget = targetFeet;
            }

            // 届かないのでその場で跳ねる
            HopResult inPlace = Simulate(start, size, 0f);
            return inPlace.IsValid ? inPlace : best;
        }

        // プレイヤーの方向への跳び方のうち、着地点がいちばん近づくもの
        HopResult FindBestHop(
            Vector2 start,
            Vector2 size,
            float maxSpeed,
            Vector2 targetFeet,
            out float bestCost
        )
        {
            float toward = targetFeet.x >= start.x ? 1f : -1f;
            HopResult best = HopResult.Invalid;
            bestCost = float.MaxValue;
            for (int i = 0; i <= speedSamples; i++)
            {
                HopResult result = Simulate(start, size, toward * maxSpeed * i / speedSamples);
                if (!result.IsValid)
                    continue;
                float cost = Score(result, size, targetFeet);
                if (cost < bestCost)
                {
                    bestCost = cost;
                    best = result;
                }
            }
            return best;
        }

        // 1回目は左右どちらへも跳び、その着地点から2回目でどこまで近づけるかで選ぶ
        HopResult FindTwoHopPlan(
            Vector2 start,
            Vector2 size,
            float maxSpeed,
            Vector2 targetFeet,
            float currentCost
        )
        {
            HopResult best = HopResult.Invalid;
            float bestCost = currentCost - minProgress;
            secondHops.Clear();
            for (int i = -speedSamples; i <= speedSamples; i++)
            {
                if (i == 0)
                    continue;
                HopResult first = Simulate(start, size, maxSpeed * i / speedSamples);
                if (!first.IsValid || Vector2.Distance(first.LandingCenter, start) < 0.2f)
                    continue;
                float cost = Mathf.Min(
                    Score(first, size, targetFeet),
                    SecondHopCost(first.LandingCenter, size, maxSpeed, targetFeet)
                );
                if (cost < bestCost)
                {
                    bestCost = cost;
                    best = first;
                }
            }
            return best;
        }

        // 同じ場所に着地する候補が多いので、2回目の結果は着地点ごとに使い回す
        float SecondHopCost(Vector2 landing, Vector2 size, float maxSpeed, Vector2 targetFeet)
        {
            foreach (SecondHop cached in secondHops)
            {
                if (Vector2.Distance(cached.Landing, landing) < 0.15f)
                    return cached.Cost;
            }
            FindBestHop(landing, size, maxSpeed, targetFeet, out float cost);
            secondHops.Add(new SecondHop { Landing = landing, Cost = cost });
            return cost;
        }

        bool IsSameStuckSituation(Vector2 start, Vector2 targetFeet)
        {
            return hasStuckRecord
                && Vector2.Distance(start, stuckPosition) < 0.3f
                && Vector2.Distance(targetFeet, stuckTarget) < 0.5f;
        }

        float Cost(float x, float feetY, Vector2 targetFeet)
        {
            float dy = targetFeet.y - feetY;
            float vertical =
                dy > 0f ? climbWeight * Mathf.Max(0f, dy - 0.25f) : Mathf.Max(0f, -dy - 0.25f);
            return Mathf.Abs(targetFeet.x - x) + vertical;
        }

        float Score(HopResult result, Vector2 size, Vector2 targetFeet)
        {
            float cost = Cost(result.LandingCenter.x, result.FeetY, targetFeet);
            if (result.Bumped)
                cost += bumpPenalty;
            return cost + RunUpPenalty(result, size, targetFeet);
        }

        // 登れる段差の直前に着地すると次の跳躍で角に当たるので、助走の幅が残る位置を選ばせる
        float RunUpPenalty(HopResult result, Vector2 size, Vector2 targetFeet)
        {
            float toward = targetFeet.x >= result.LandingCenter.x ? 1f : -1f;
            if (Mathf.Abs(targetFeet.x - result.LandingCenter.x) < size.x)
                return 0f;
            Vector2 origin = result.LandingCenter + Vector2.up * Lift;
            if (!TryCast(origin, size, new Vector2(toward, 0f), runUpDistance, out RaycastHit2D wall))
                return 0f;
            // 段差の上面を探し、跳べる高さなら登れる段差とみなす
            Vector2 probe = new Vector2(wall.point.x + toward * 0.05f, result.FeetY + jumpHeight);
            if (!TryRaycast(probe, Vector2.down, jumpHeight, out RaycastHit2D top))
                return 0f;
            // 跳べる高さより上まで壁が続いている
            if (top.distance <= 0f || top.point.y - result.FeetY > jumpHeight - ClimbClearance)
                return 0f;
            return (runUpDistance - wall.distance) * RunUpPenaltyWeight;
        }

        // 跳んだときの軌道をたどり、どこに着地するかを求める。
        // カプセルの当たり判定を外接する箱でなぞるので、下の角が丸くない分だけ段差の角に引っかかる跳び方を避けられる
        HopResult Simulate(Vector2 startCenter, Vector2 size, float vx)
        {
            float gravity = Gravity;
            // 物理は速度を先に更新してから位置を進めるので、初速を g*dt/2 下げると実際の軌道と一致する
            Vector2 velocity = new Vector2(vx, LaunchSpeedY - gravity * Time.fixedDeltaTime * 0.5f);
            Vector2 position = startCenter + Vector2.up * Lift;
            float halfHeight = size.y * 0.5f;
            float lowestFeetY = startCenter.y - halfHeight - maxDropHeight;
            bool bumped = false;

            for (int i = 0; i < MaxSimulationSteps; i++)
            {
                Vector2 delta =
                    velocity * SimulationStep
                    + 0.5f * SimulationStep * SimulationStep * new Vector2(0f, -gravity);
                float distance = delta.magnitude;
                if (distance < 0.0001f)
                {
                    velocity.y -= gravity * SimulationStep;
                    continue;
                }
                Vector2 direction = delta / distance;
                if (!TryCast(position, size, direction, distance, out RaycastHit2D hit))
                {
                    position += delta;
                    velocity.y -= gravity * SimulationStep;
                    if (position.y - halfHeight < lowestFeetY)
                        return HopResult.Invalid;
                    continue;
                }

                position = hit.centroid;
                velocity.y -= gravity * SimulationStep * Mathf.Max(hit.fraction, 0.25f);
                Vector2 normal = hit.normal;
                if (normal.y >= LandingNormalY && velocity.y <= 0f)
                {
                    float feetY = position.y - halfHeight;
                    // 体の中心の真下に足場がないと角から滑り落ちる
                    bool isValid = HasSupport(position.x, feetY) && !IsAvoided(position, size);
                    return new HopResult
                    {
                        IsValid = isValid,
                        Vx = vx,
                        LandingCenter = position,
                        FeetY = feetY,
                        Bumped = bumped,
                    };
                }

                // 壁や天井に当たったら面に沿って滑らせる
                bumped = true;
                float into = Vector2.Dot(velocity, normal);
                if (into < 0f)
                    velocity -= into * normal;
                position += normal * 0.01f;
            }
            return HopResult.Invalid;
        }

        bool HasSupport(float x, float feetY)
        {
            return TryRaycast(new Vector2(x, feetY + 0.1f), Vector2.down, 0.25f, out RaycastHit2D hit)
                && hit.normal.y > 0.5f;
        }

        bool TryCast(
            Vector2 origin,
            Vector2 size,
            Vector2 direction,
            float distance,
            out RaycastHit2D closest
        )
        {
            int count = Physics2D.BoxCast(
                origin,
                size,
                0f,
                direction,
                obstacleFilter,
                castHits,
                distance
            );
            return TryGetClosest(count, direction, out closest);
        }

        bool TryRaycast(Vector2 origin, Vector2 direction, float distance, out RaycastHit2D closest)
        {
            int count = Physics2D.Raycast(origin, direction, obstacleFilter, castHits, distance);
            return TryGetClosest(count, direction, out closest);
        }

        bool TryGetClosest(int count, Vector2 direction, out RaycastHit2D closest)
        {
            closest = default;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = castHits[i];
                // 離れていく向きの面(踏み切った床など)には当たらない
                if (Vector2.Dot(hit.normal, direction) >= 0f)
                    continue;
                if (IsPassThroughPlatform(hit))
                    continue;
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closest = hit;
                }
            }
            return closestDistance < float.MaxValue;
        }

        // 一方通行の足場は上から乗るときだけ当たる
        static bool IsPassThroughPlatform(RaycastHit2D hit)
        {
            if (!hit.collider.usedByEffector)
                return false;
            PlatformEffector2D effector = hit.collider.GetComponent<PlatformEffector2D>();
            return effector != null && effector.useOneWay && hit.normal.y < 0.5f;
        }

        bool IsAvoided(Vector2 center, Vector2 size)
        {
            if (avoidLayers.value == 0)
                return false;
            return Physics2D.OverlapBox(center, size, 0f, avoidFilter, overlapResults) > 0;
        }

        // 地面の上か、プレイヤーの頭など地面以外の物の上で止まっている
        bool IsResting(ref int stillFrames)
        {
            stillFrames = Mathf.Abs(rbody.linearVelocity.y) < 0.05f ? stillFrames + 1 : 0;
            return IsGrounded() || stillFrames >= 3;
        }

        bool IsGrounded()
        {
            Vector2 size = GetBodySize();
            // 壁に触れているだけで接地とみなさないよう、幅を少し狭めて真下を調べる
            Vector2 castSize = new Vector2(size.x * 0.8f, size.y);
            Vector2 origin = GetBodyCenter() + Vector2.up * Lift;
            return TryCast(origin, castSize, Vector2.down, Lift + 0.06f, out RaycastHit2D hit)
                && hit.normal.y > 0.5f;
        }

        Vector2 GetBodyCenter()
        {
            Vector3 scale = transform.lossyScale;
            Vector2 offset = new Vector2(
                bodyCollider.offset.x * scale.x,
                bodyCollider.offset.y * scale.y
            );
            return (Vector2)transform.position + offset;
        }

        Vector2 GetBodySize()
        {
            Vector3 scale = transform.lossyScale;
            return new Vector2(
                bodyCollider.size.x * Mathf.Abs(scale.x),
                bodyCollider.size.y * Mathf.Abs(scale.y)
            );
        }

        void FaceTo(float x)
        {
            Vector3 localScale = transform.localScale;
            localScale.x = x < transform.position.x
                ? Mathf.Abs(localScale.x)
                : -Mathf.Abs(localScale.x);
            transform.localScale = localScale;
        }

        // 目標はプレイヤーの足元の地面。跳んでいる最中でも狙いがぶれないようにする
        bool TryGetTargetFeet(out Vector2 targetFeet)
        {
            targetFeet = default;
            if (!TryGetPlayer(out Transform player))
                return false;
            Bounds bounds = playerCollider != null
                ? playerCollider.bounds
                : new Bounds(player.position, Vector3.zero);
            Vector2 feet = new Vector2(bounds.center.x, bounds.min.y);
            if (
                TryRaycast(feet + Vector2.up * 0.1f, Vector2.down, maxDropHeight, out RaycastHit2D ground)
                && ground.normal.y > 0.5f
            )
                feet.y = ground.point.y;
            targetFeet = feet;
            return true;
        }

        bool TryGetPlayer(out Transform player)
        {
            if (
                playerTransform == null
                || !playerTransform.gameObject.activeInHierarchy
                || !playerTransform.CompareTag("Player")
            )
            {
                GameObject playerObject = GameObject.FindWithTag("Player");
                playerTransform = playerObject != null ? playerObject.transform : null;
                playerCollider = playerObject != null ? FindBodyCollider(playerObject) : null;
            }

            player = playerTransform;
            return player != null;
        }

        static Collider2D FindBodyCollider(GameObject playerObject)
        {
            foreach (Collider2D collider in playerObject.GetComponents<Collider2D>())
            {
                if (!collider.isTrigger)
                    return collider;
            }
            return null;
        }

        private void OnDrawGizmosSelected()
        {
            if (bodyCollider == null)
                return;
            Gizmos.color = Color.yellow;
            Vector3 feet = new Vector3(transform.position.x, bodyCollider.bounds.min.y, 0f);
            Gizmos.DrawLine(feet, feet + Vector3.up * jumpHeight);
        }
    }
}
