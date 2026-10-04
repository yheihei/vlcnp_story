using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VLCNP.Attributes;
using VLCNP.Core;
using VLCNP.UI;

namespace VLCNP.Combat
{
    /**
     * 決めた数の敵を倒すまで、複数の地点から一定間隔で敵を湧かせる防衛戦。
     * 数えるのはこの防衛戦で湧かせた敵の死亡だけで、同じ敵は一度しか数えない。
     * 倒す数に足りる分しか湧かせないので、規定数を倒した時点で湧かせた敵は残らない。
     * 戦闘中だけ有効にする物(見えない壁)・止める物(話しかける判定)、起き上がって撃ってくるアーチャー、
     * HUD、BGM をまとめて切り替える。
     */
    public class DefenseBattle : MonoBehaviour, IStoppable
    {
        enum State
        {
            Ready,
            Fighting,
            Cleared,
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

        [SerializeField]
        UnityEvent onCleared;

        State state = State.Ready;
        FlagManager flagManager;
        readonly List<Health> aliveEnemies = new List<Health>();
        readonly HashSet<Health> countedEnemies = new HashSet<Health>();
        readonly List<Transform> spawnCandidates = new List<Transform>();
        int killCount;
        float spawnTimer;
        bool lastSpawnWasLeft;
        bool isStopped;

        public bool IsStopped
        {
            get => isStopped;
            set => isStopped = value;
        }

        public bool IsFighting => state == State.Fighting;
        public bool IsCleared => state == State.Cleared;
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
            foreach (DefenseBattleArcher archer in archers)
            {
                if (archer != null)
                    archer.Rise();
            }
            if (hud != null)
                hud.Show(requiredKills);
            PlayBgm(battleBgm, battleBgmVolume, 1f);
        }

        void Update()
        {
            if (state != State.Fighting || isStopped)
                return;
            RemoveEnemiesOutOfArena();
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
            Transform spawnPoint = ChooseSpawnPoint();
            if (spawnPoint == null || enemyPrefab == null)
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
            lastSpawnWasLeft = IsLeftSide(spawnPoint);
            return true;
        }

        // 左右の湧き地点を交互に使い、プレイヤーのすぐ近くと、前の敵がまだ残っている地点からは湧かせない
        Transform ChooseSpawnPoint()
        {
            // やられてプレイヤーがいない間(ゲームオーバー中)は湧かせない
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
                return null;
            spawnCandidates.Clear();
            foreach (Transform point in spawnPoints)
            {
                if (point == null)
                    continue;
                if (Mathf.Abs(point.position.x - player.transform.position.x) < minSpawnDistanceFromPlayer)
                    continue;
                if (IsOccupied(point))
                    continue;
                spawnCandidates.Add(point);
            }
            if (spawnCandidates.Count == 0)
                return null;

            int otherSideCount = 0;
            foreach (Transform point in spawnCandidates)
            {
                if (IsLeftSide(point) != lastSpawnWasLeft)
                    otherSideCount++;
            }
            if (otherSideCount == 0)
                return spawnCandidates[Random.Range(0, spawnCandidates.Count)];

            int pick = Random.Range(0, otherSideCount);
            foreach (Transform point in spawnCandidates)
            {
                if (IsLeftSide(point) == lastSpawnWasLeft)
                    continue;
                if (pick-- == 0)
                    return point;
            }
            return null;
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

        bool IsLeftSide(Transform point)
        {
            return point.position.x < arenaBounds.center.x;
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
            SetFightingObjects(false);
            foreach (DefenseBattleArcher archer in archers)
            {
                if (archer != null)
                    archer.Collapse();
            }
            RemoveEnemyProjectilesInArena();
            RemoveRemainingEnemies();
            if (hud != null)
                hud.ShowCleared();
            if (clearSe != null)
                AudioSource.PlayClipAtPoint(clearSe, Camera.main != null ? Camera.main.transform.position : transform.position, clearSeVolume);
            if (battleBgm != null)
                StartCoroutine(ReturnToAreaBgm());
            onCleared?.Invoke();
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

        // 規定数を倒した時点では残らない数しか湧かせないが、念のため数えずに倒す(ドロップなし)
        void RemoveRemainingEnemies()
        {
            foreach (Health enemy in aliveEnemies)
            {
                if (enemy != null && !enemy.IsDead)
                    enemy.DeadEffectAndDestroy();
            }
            aliveEnemies.Clear();
        }

        void SetFightingObjects(bool isFighting)
        {
            foreach (GameObject target in activeWhileFighting)
            {
                if (target != null)
                    target.SetActive(isFighting);
            }
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
