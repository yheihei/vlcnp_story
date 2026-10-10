using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using VLCNP.Attributes;
using VLCNP.Combat.EnemyAction;
using VLCNP.Control;
using VLCNP.Core;
using VLCNP.Movie;
using VLCNP.UI;

namespace VLCNP.Combat
{
    /**
     * 決めた数の敵を倒すまで、複数の地点から一定間隔で敵を湧かせる防衛戦。
     * 湧かせる地点は画面に映っているものを優先して選ぶ。
     * 数えるのはこの防衛戦で湧かせた敵の死亡だけで、同じ敵は一度しか数えない。
     * 倒す数に足りる分しか湧かせないので、規定数を倒した時点で湧かせた敵は残らない。
     * 守る相手がいれば、湧かせた敵はその相手へ向かい、触れると被弾させる。決めた回数の被弾でゲームオーバーになる。
     * そのときはカメラを守る相手へ寄せてセリフを言わせ、守る相手を消してからゲームオーバーにする。
     * 戦闘中だけ有効にする物(見えない壁)・止める物(話しかける判定)、起き上がって撃ってくるアーチャー、
     * HUD、BGM、カメラの引きをまとめて切り替える。
     * 操作キャラか守る相手が倒れたら、その場で防衛戦を終えて戦闘中の物を片付ける(ゲームオーバーからはシーンごと読み直される)。
     */
    public class DefenseBattle : MonoBehaviour, IStoppable
    {
        enum State
        {
            Ready,
            Fighting,
            Cleared,
            Failed,
        }

        [SerializeField, Tooltip("規定数を倒したら立てる。立っていれば開始しない")]
        Flag clearedFlag = Flag.None;

        [Header("湧かせる敵")]
        [SerializeField]
        GameObject enemyPrefab;

        [SerializeField]
        Transform[] spawnPoints;

        [SerializeField, Min(1)]
        int requiredKills = 20;

        [SerializeField, Min(1), Tooltip("同時に出ている数の上限")]
        int maxAliveEnemies = 4;

        [SerializeField, Min(0f)]
        float firstSpawnDelay = 1f;

        [SerializeField, Min(0.1f)]
        float spawnInterval = 2.5f;

        [SerializeField, Min(0f), Tooltip("プレイヤーとの横の距離がこれより近い地点からは湧かせない")]
        float minSpawnDistanceFromPlayer = 3f;

        [SerializeField, Min(0f), Tooltip("画面の左右と上の端からこれより内側にある地点を、映っている地点として優先する")]
        float visibleSpawnMargin = 1f;

        [SerializeField, Tooltip("この範囲の外に出た敵は数えずに消し、代わりを湧かせる。地面に潜っている深さも含める")]
        Rect arenaBounds = new Rect(0f, 0f, 10f, 10f);

        [Header("戦闘中の切り替え")]
        [SerializeField]
        DefenseBattleArcher[] archers;

        [SerializeField, Tooltip("戦闘中だけ有効にする(見えない壁など)")]
        GameObject[] activeWhileFighting;

        [SerializeField, Tooltip("戦闘中は無効にする(話しかける判定など)")]
        Collider2D[] disabledWhileFighting;

        [SerializeField]
        DefenseBattleHud hud;

        [SerializeField, Tooltip("戦闘中だけカメラを引く")]
        EventCameraZoom cameraZoom;

        [Header("守る相手")]
        [SerializeField, Tooltip("湧いた敵が向かう相手。敵に触れられると被弾し、決めた回数でゲームオーバーになる")]
        DefenseTarget defenseTarget;

        [SerializeField, Tooltip("守る相手が倒れたときの、ゲームオーバーの最初の文言")]
        string targetLostMessage = "ミタマを守れなかった...";

        [SerializeField, Min(0f), Tooltip("守る相手が倒れたときのヒットストップ(実時間)。操作キャラが倒れたときと同じ長さ")]
        float targetLostHitStopTime = 1f;

        [SerializeField, Min(0f), Tooltip("守る相手が倒れたあと、カメラを守る相手へ寄せる時間")]
        float targetLostCameraTime = 1.2f;

        [SerializeField, Tooltip("守る相手が倒れたときのセリフの Flowchart。操作を止めたままにするため FlowchartStopAllGuard は付けない")]
        Fungus.Flowchart targetLostFlowchart;

        [SerializeField]
        string targetLostBlockName = "MitamaLost";

        [SerializeField, Min(0f), Tooltip("セリフのあと、守る相手を消していく時間")]
        float targetLostFadeTime = 1.5f;

        [Header("音")]
        [SerializeField]
        BGMWrapper bgmWrapper;

        [SerializeField]
        AudioClip battleBgm;

        [SerializeField, Range(0f, 1f)]
        float battleBgmVolume = 0.3f;

        [SerializeField]
        AudioClip clearSe;

        [SerializeField, Range(0f, 1f)]
        float clearSeVolume = 0.5f;

        [SerializeField, Tooltip("守りきってからエリアのBGMに戻すまでの時間")]
        float areaBgmReturnDelay = 2.5f;

        [SerializeField, Tooltip("守る相手が倒れたときの音(操作キャラが倒れたときと同じもの)")]
        AudioClip targetLostSe;

        [SerializeField, Range(0f, 1f)]
        float targetLostSeVolume = 0.3f;

        [SerializeField]
        UnityEvent onCleared;

        State state = State.Ready;
        FlagManager flagManager;
        readonly List<Health> aliveEnemies = new List<Health>();
        readonly HashSet<Health> countedEnemies = new HashSet<Health>();
        readonly List<Transform> spawnCandidates = new List<Transform>();
        readonly Collider2D[] targetHitResults = new Collider2D[8];
        ContactFilter2D targetHitFilter;
        int killCount;
        float spawnTimer;
        Transform lastSpawnPoint;
        bool lastSpawnWasLeft;
        bool isStopped;
        bool isHitStopping;

        public bool IsStopped
        {
            get => isStopped;
            set => isStopped = value;
        }

        public bool IsFighting => state == State.Fighting;
        public bool IsCleared => state == State.Cleared;
        public bool IsFailed => state == State.Failed;
        public int KillCount => killCount;
        public int RequiredKills => requiredKills;
        public int AliveEnemyCount => aliveEnemies.Count;

        void Start()
        {
            flagManager = FlagManager.FindInScene();
            if (flagManager != null)
                flagManager.OnChangeFlag += OnChangeFlag;
            if (IsClearedFlagSet())
                state = State.Cleared;
            SetFightingObjects(false);
            // 守る相手に触れたかは敵の体(Enemy レイヤーのトリガーでない当たり判定)で見る
            targetHitFilter = new ContactFilter2D();
            targetHitFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
            targetHitFilter.useTriggers = false;
        }

        void OnDisable()
        {
            // ヒットストップの途中で消えたら、時間を止めたままにしない
            if (isHitStopping)
            {
                Time.timeScale = 1f;
                isHitStopping = false;
            }
        }

        void OnDestroy()
        {
            if (flagManager != null)
                flagManager.OnChangeFlag -= OnChangeFlag;
        }

        // シーン遷移ではシーンの開始後にセーブからフラグが戻るので、そのときも守りきった扱いにする
        void OnChangeFlag(Flag flag, bool value)
        {
            if (state == State.Ready && value && flag == clearedFlag && clearedFlag != Flag.None)
                state = State.Cleared;
        }

        bool IsClearedFlagSet()
        {
            return clearedFlag != Flag.None && flagManager != null && flagManager.GetFlag(clearedFlag);
        }

        // 会話の最後から呼ぶ。戦闘中・守りきった後は何もしない
        public void StartBattle()
        {
            if (state == State.Ready && IsClearedFlagSet())
                state = State.Cleared;
            if (state != State.Ready)
                return;
            state = State.Fighting;
            killCount = 0;
            spawnTimer = firstSpawnDelay;
            SetFightingObjects(true);
            if (defenseTarget != null)
                defenseTarget.BeginGuard();
            foreach (DefenseBattleArcher archer in archers)
            {
                if (archer != null)
                    archer.Rise();
            }
            if (hud != null)
                hud.Show(requiredKills, defenseTarget != null ? defenseTarget.MaxHits : 0);
            if (cameraZoom != null)
                cameraZoom.ZoomOut();
            PlayBgm(battleBgm, battleBgmVolume, 1f);
        }

        void Update()
        {
            if (state != State.Fighting)
                return;
            // 倒れてからゲームオーバーの会話が始まるまでの間に片付ける(会話で止められる前に見る)
            if (IsPlayerDefeated())
            {
                Fail();
                return;
            }
            if (isStopped)
                return;
            RemoveEnemiesOutOfArena();
            CheckTargetHits();
            if (state != State.Fighting)
                return;
            // 上限まで出ている間は時間を進めない(倒した直後にすぐ次を湧かせない)
            if (!CanSpawn())
                return;
            spawnTimer -= Time.deltaTime;
            if (spawnTimer > 0f)
                return;
            if (Spawn())
                spawnTimer = spawnInterval;
        }

        bool CanSpawn()
        {
            aliveEnemies.RemoveAll(enemy => enemy == null);
            if (aliveEnemies.Count >= maxAliveEnemies)
                return false;
            // 今いる敵を全部倒せば規定数に届くなら、それ以上は湧かせない
            return killCount + aliveEnemies.Count < requiredKills;
        }

        bool Spawn()
        {
            // やられてプレイヤーがいない間(ゲームオーバー中)は湧かせない
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null || enemyPrefab == null)
                return false;
            float playerX = player.transform.position.x;
            Transform spawnPoint = ChooseSpawnPoint(playerX);
            if (spawnPoint == null)
                return false;
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            if (!enemy.TryGetComponent(out Health health))
            {
                Debug.LogWarning($"[{nameof(DefenseBattle)}] {enemyPrefab.name} に Health がないため数えられません。", this);
                Destroy(enemy);
                return false;
            }
            aliveEnemies.Add(health);
            health.onDieStarted += () => OnEnemyDied(health);
            // 守る相手がいれば、プレイヤーではなくその相手へ向かわせる
            if (defenseTarget != null && enemy.TryGetComponent(out HopTowardPlayer hop))
                hop.SetTarget(defenseTarget.transform);
            lastSpawnPoint = spawnPoint;
            lastSpawnWasLeft = spawnPoint.position.x < playerX;
            return true;
        }

        // 画面に映っている地点からランダムに選ぶ。プレイヤーの左右を交互にし、同じ地点は続けない。
        // プレイヤーのすぐ近くと、前の敵がまだ残っている地点からは湧かせない
        Transform ChooseSpawnPoint(float playerX)
        {
            spawnCandidates.Clear();
            foreach (Transform point in spawnPoints)
            {
                if (point == null)
                    continue;
                if (Mathf.Abs(point.position.x - playerX) < minSpawnDistanceFromPlayer)
                    continue;
                if (IsOccupied(point))
                    continue;
                spawnCandidates.Add(point);
            }
            KeepVisibleCandidates();
            if (spawnCandidates.Count > 1)
                spawnCandidates.Remove(lastSpawnPoint);
            if (spawnCandidates.Count == 0)
                return null;

            int otherSideCount = 0;
            foreach (Transform point in spawnCandidates)
            {
                if ((point.position.x < playerX) != lastSpawnWasLeft)
                    otherSideCount++;
            }
            if (otherSideCount == 0)
                return spawnCandidates[Random.Range(0, spawnCandidates.Count)];

            int pick = Random.Range(0, otherSideCount);
            foreach (Transform point in spawnCandidates)
            {
                if ((point.position.x < playerX) == lastSpawnWasLeft)
                    continue;
                if (pick-- == 0)
                    return point;
            }
            return null;
        }

        // 画面に映っている地点が1つでもあれば、映っていない地点は候補から外す(湧く様子を見せる)
        void KeepVisibleCandidates()
        {
            Camera camera = Camera.main;
            if (camera == null || !camera.orthographic)
                return;
            float halfHeight = camera.orthographicSize;
            float halfWidth = halfHeight * camera.aspect;
            Vector3 center = camera.transform.position;
            Rect view = Rect.MinMaxRect(
                center.x - halfWidth + visibleSpawnMargin,
                center.y - halfHeight,
                center.x + halfWidth - visibleSpawnMargin,
                center.y + halfHeight - visibleSpawnMargin
            );
            bool hasVisible = false;
            foreach (Transform point in spawnCandidates)
            {
                if (view.Contains(point.position))
                {
                    hasVisible = true;
                    break;
                }
            }
            if (hasVisible)
                spawnCandidates.RemoveAll(point => !view.Contains(point.position));
        }

        // 湧いた敵が地面から出きる前・出た直後で、まだ湧き地点に重なっているか
        bool IsOccupied(Transform point)
        {
            foreach (Health enemy in aliveEnemies)
            {
                if (enemy == null)
                    continue;
                Vector2 offset = enemy.transform.position - point.position;
                if (Mathf.Abs(offset.x) < 1f && Mathf.Abs(offset.y) < 3f)
                    return true;
            }
            return false;
        }

        // 壁抜けなどで場外に出た敵は数えずに消す(湧かせる数の上限から外れて代わりが湧く)
        void RemoveEnemiesOutOfArena()
        {
            for (int i = aliveEnemies.Count - 1; i >= 0; i--)
            {
                Health enemy = aliveEnemies[i];
                if (enemy == null)
                {
                    aliveEnemies.RemoveAt(i);
                    continue;
                }
                if (arenaBounds.Contains(enemy.transform.position))
                    continue;
                aliveEnemies.RemoveAt(i);
                Destroy(enemy.gameObject);
            }
        }

        // 湧かせた敵の体が守る相手の当たり判定に触れていれば、守る相手を被弾させる
        void CheckTargetHits()
        {
            if (defenseTarget == null)
                return;
            int count = Physics2D.OverlapBox(
                defenseTarget.HitAreaCenter,
                defenseTarget.HitAreaSize,
                0f,
                targetHitFilter,
                targetHitResults
            );
            for (int i = 0; i < count; i++)
            {
                Rigidbody2D body = targetHitResults[i].attachedRigidbody;
                if (body == null || !body.TryGetComponent(out Health enemy))
                    continue;
                if (enemy.IsDead || !aliveEnemies.Contains(enemy))
                    continue;
                // 無敵時間中は何体触れていても被弾しない
                if (!defenseTarget.TakeHit())
                    return;
                if (hud != null)
                    hud.SetTargetRemaining(defenseTarget.RemainingHits);
                if (defenseTarget.IsDefeated)
                    LoseTarget();
                return;
            }
        }

        // 守る相手が倒れたとき。全員を止め、操作キャラが倒れたときと同じヒットストップの後に戦闘中の物を片付ける。
        // カメラを守る相手へ寄せてセリフを言わせ、守る相手を消してから、通常のゲームオーバーへ合流する(文言だけ変える)
        void LoseTarget()
        {
            state = State.Failed;
            StoppableController controller = StoppableController.FindInScene();
            if (controller != null)
                controller.StopAll();
            StartCoroutine(TargetLostRoutine());
        }

        IEnumerator TargetLostRoutine()
        {
            if (targetLostSe != null)
                AudioSource.PlayClipAtPoint(targetLostSe, Camera.main != null ? Camera.main.transform.position : transform.position, targetLostSeVolume);
            isHitStopping = true;
            Time.timeScale = 0.001f;
            yield return new WaitForSecondsRealtime(targetLostHitStopTime);
            Time.timeScale = 1f;
            isHitStopping = false;
            EndFighting(false);
            if (hud != null)
                hud.Hide();
            yield return FocusCameraOnTarget();
            yield return PlayTargetLostTalk();
            yield return defenseTarget.FadeOut(targetLostFadeTime);
            GameOver gameOver = FindFirstObjectByType<GameOver>();
            if (gameOver != null)
                gameOver.ExecuteWithMessage(targetLostMessage);
        }

        // 追従カメラを今の追従先から守る相手まで寄せる。
        // 追従カメラは横に遅れず付いていくので、間を動く目印を追わせて滑らかにする(ゲームオーバーからはシーンごと読み直される)
        IEnumerator FocusCameraOnTarget()
        {
            GameObject cmCamera = GameObject.FindWithTag("CMCamera");
            if (cmCamera == null || !cmCamera.TryGetComponent(out CinemachineVirtualCamera virtualCamera))
                yield break;
            Vector3 end = defenseTarget.transform.position;
            Vector3 start = virtualCamera.Follow != null ? virtualCamera.Follow.position : end;
            Transform focus = new GameObject("TargetLostCameraFocus").transform;
            focus.SetParent(transform, false);
            focus.position = start;
            virtualCamera.Follow = focus;
            for (float t = 0f; t < targetLostCameraTime; t += Time.deltaTime)
            {
                focus.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t / targetLostCameraTime));
                yield return null;
            }
            focus.position = end;
        }

        // 守る相手のセリフを流し、読み終わるまで待つ
        IEnumerator PlayTargetLostTalk()
        {
            if (targetLostFlowchart == null)
                yield break;
            Fungus.Block block = targetLostFlowchart.FindBlock(targetLostBlockName);
            bool finished = false;
            if (block == null || !targetLostFlowchart.ExecuteBlock(block, 0, () => finished = true))
                yield break;
            yield return new WaitUntil(() => finished);
        }

        void OnEnemyDied(Health enemy)
        {
            if (state != State.Fighting)
                return;
            if (!countedEnemies.Add(enemy))
                return;
            aliveEnemies.Remove(enemy);
            killCount = Mathf.Min(killCount + 1, requiredKills);
            if (hud != null)
                hud.SetKillCount(killCount);
            if (killCount >= requiredKills)
                Clear();
        }

        void Clear()
        {
            state = State.Cleared;
            if (clearedFlag != Flag.None && flagManager != null)
                flagManager.SetFlag(clearedFlag, true);
            EndFighting(true);
            if (hud != null)
                hud.ShowCleared();
            if (clearSe != null)
                AudioSource.PlayClipAtPoint(clearSe, Camera.main != null ? Camera.main.transform.position : transform.position, clearSeVolume);
            if (battleBgm != null)
                StartCoroutine(ReturnToAreaBgm());
            onCleared?.Invoke();
        }

        // 操作キャラが倒れたか(倒れるとゲームオーバーになり、仲間への切り替えもできない)
        bool IsPlayerDefeated()
        {
            GameObject player = GameObject.FindWithTag("Player");
            return player != null && player.TryGetComponent(out Health health) && health.IsDead;
        }

        // 操作キャラが倒れたとき。このシーンでは始め直さない(リトライではシーンごと読み直される)。
        // BGM はゲームオーバー側が切り替える
        void Fail()
        {
            state = State.Failed;
            EndFighting(false);
            if (hud != null)
                hud.Hide();
        }

        // 戦闘中だけ有効にした物を戻してアーチャーを崩し、残った敵と矢を消してカメラを戻す。
        // 守りきったときは話しかける判定などを戻し、敵に倒れる演出を出す。
        // 負けたときは、倒れたキャラに話しかける案内が出ないよう判定は止めたままにし、敵は演出なしで消す
        void EndFighting(bool cleared)
        {
            SetActiveWhileFighting(false);
            if (cleared)
                SetDisabledWhileFighting(false);
            if (defenseTarget != null)
                defenseTarget.EndGuard();
            foreach (DefenseBattleArcher archer in archers)
            {
                if (archer != null)
                    archer.Collapse();
            }
            RemoveEnemyProjectilesInArena();
            RemoveRemainingEnemies(cleared);
            if (cameraZoom != null)
                cameraZoom.Restore();
        }

        // 飛んでいる矢・地面に刺さった矢を消す(敵の弾は Enemy レイヤー)
        void RemoveEnemyProjectilesInArena()
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            foreach (Projectile projectile in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
            {
                if (projectile.gameObject.layer != enemyLayer)
                    continue;
                if (!arenaBounds.Contains(projectile.transform.position))
                    continue;
                Destroy(projectile.gameObject);
            }
        }

        // 数えずに消す(ドロップなし)。規定数を倒した時点では残らない数しか湧かせないので、守りきったときは念のため
        void RemoveRemainingEnemies(bool showDeadEffect)
        {
            foreach (Health enemy in aliveEnemies)
            {
                if (enemy == null || enemy.IsDead)
                    continue;
                if (showDeadEffect)
                    enemy.DeadEffectAndDestroy();
                else
                    Destroy(enemy.gameObject);
            }
            aliveEnemies.Clear();
        }

        void SetFightingObjects(bool isFighting)
        {
            SetActiveWhileFighting(isFighting);
            SetDisabledWhileFighting(isFighting);
        }

        void SetActiveWhileFighting(bool isFighting)
        {
            foreach (GameObject target in activeWhileFighting)
            {
                if (target != null)
                    target.SetActive(isFighting);
            }
        }

        void SetDisabledWhileFighting(bool isFighting)
        {
            foreach (Collider2D target in disabledWhileFighting)
            {
                if (target != null)
                    target.enabled = !isFighting;
            }
        }

        IEnumerator ReturnToAreaBgm()
        {
            BGMWrapper wrapper = GetBgmWrapper();
            if (wrapper == null)
                yield break;
            wrapper.FadeOut(1f);
            yield return new WaitForSeconds(areaBgmReturnDelay);
            AreaBGM areaBgm = GameObject.FindWithTag("AreaBGM")?.GetComponent<AreaBGM>();
            if (areaBgm == null || areaBgm.GetAudioClip() == null)
                yield break;
            wrapper.Play(areaBgm.GetAudioClip(), areaBgm.GetVolume(), areaBgm.GetPitch());
        }

        void PlayBgm(AudioClip clip, float volume, float pitch)
        {
            if (clip == null)
                return;
            BGMWrapper wrapper = GetBgmWrapper();
            if (wrapper != null)
                wrapper.Play(clip, volume, pitch);
        }

        BGMWrapper GetBgmWrapper()
        {
            if (bgmWrapper == null)
                bgmWrapper = FindFirstObjectByType<BGMWrapper>();
            return bgmWrapper;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(arenaBounds.center, arenaBounds.size);
        }
    }
}
