using Cinemachine;
using UnityEditor;
using UnityEngine;
using VLCNP.Attributes;
using VLCNP.Movie;

/**
 * #661 被弾時の小さな画面揺れとボス撃破時の一回の揺れを入れるエディタ拡張。
 *
 * Apply は次を行う(再実行しても同じ値に揃え直すだけ)。
 *   1. Player.prefab に CinemachineImpulseSource(Uniform / Bump / 0.2 秒)と HealthCameraShake(被弾で揺らす)を付ける。
 *      Leelee / Orochi / VLMitama の Variant は Player.prefab から継承する
 *   2. ボス 4 体(VLOrochiVariant / VLMitamaBoss / VLNarukamiBoss / VLKamaitachi)に
 *      CinemachineImpulseSource(Uniform / Explosion / 0.4 秒)と HealthCameraShake(死亡で揺らす)を付ける
 * 受け手の CinemachineImpulseListener は Core.prefab の CMCamera に付いている(チャンネル 1)。
 * 揺れの大きさはこのスクリプト内の値が正。手直しはここへ戻す。
 *
 * Debug 配下はプレイモードでの確認用(Eval を使わずに Menu.Execute から呼ぶ)。
 */
public static class Issue661CameraShakeSetup
{
    private const string PlayerPrefabPath = "Assets/Game/Characters/Player.prefab";

    private static readonly string[] BossPrefabPaths =
    {
        "Assets/Game/Characters/Enemy/VLOrochiVariant.prefab",
        "Assets/Game/Characters/Enemy/VLMitamaBoss.prefab",
        "Assets/Game/Characters/Enemy/VLNarukamiBoss.prefab",
        "Assets/Game/Characters/Enemy/VLKamaitachi.prefab",
    };

    /** 被弾: 振幅 0.15 ユニット前後、0.2 秒、回転なし(Uniform + Bump は位置だけ動く) */
    private static readonly Vector3 DamageVelocity = new Vector3(0.1f, -0.11f, 0f);
    private const float DamageDuration = 0.2f;

    /** ボス撃破: 振幅 0.4 前後、0.4 秒(Explosion の波形は先頭で速度の 1.4 倍まで振れる) */
    private static readonly Vector3 DieVelocity = new Vector3(0.2f, -0.2f, 0f);
    private const float DieDuration = 0.4f;

    [MenuItem("Tools/Issue661/Apply Camera Shake Setup")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[Issue661] プレイモード中は実行できません。");
            return;
        }

        ApplyToPrefab(PlayerPrefabPath, CinemachineImpulseDefinition.ImpulseShapes.Bump,
            DamageDuration, DamageVelocity, shakeOnDamage: true, shakeOnDie: false);
        foreach (string path in BossPrefabPaths)
        {
            ApplyToPrefab(path, CinemachineImpulseDefinition.ImpulseShapes.Explosion,
                DieDuration, DieVelocity, shakeOnDamage: false, shakeOnDie: true);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[Issue661] Apply 完了");
    }

    private static void ApplyToPrefab(string path, CinemachineImpulseDefinition.ImpulseShapes shape,
        float duration, Vector3 velocity, bool shakeOnDamage, bool shakeOnDie)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            if (root.GetComponent<Health>() == null)
            {
                Debug.LogError($"[Issue661] {path} のルートに Health がありません。スキップします。");
                return;
            }

            var source = root.GetComponent<CinemachineImpulseSource>();
            if (source == null)
                source = root.AddComponent<CinemachineImpulseSource>();
            var def = source.m_ImpulseDefinition;
            def.m_ImpulseChannel = 1;
            def.m_ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
            def.m_ImpulseShape = shape;
            def.m_ImpulseDuration = duration;
            source.m_DefaultVelocity = velocity;

            var shake = root.GetComponent<HealthCameraShake>();
            if (shake == null)
                shake = root.AddComponent<HealthCameraShake>();
            var so = new SerializedObject(shake);
            so.FindProperty("shakeOnDamage").boolValue = shakeOnDamage;
            so.FindProperty("shakeOnDie").boolValue = shakeOnDie;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Debug.Log($"[Issue661] {path}: ImpulseSource({shape}, {duration}s, {velocity}) + HealthCameraShake(damage={shakeOnDamage}, die={shakeOnDie})");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // ---------------------------------------------------------------- プレイモード確認用

    /** 現在操作中のプレイヤー(有効な HealthCameraShake 付き Player タグ)に 1 ダメージを与える */
    [MenuItem("Tools/Issue661/Debug/Damage Current Player")]
    public static void DebugDamageCurrentPlayer()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogError("[Issue661] プレイモードで実行してください。");
            return;
        }
        foreach (var shake in Object.FindObjectsOfType<HealthCameraShake>())
        {
            if (!shake.CompareTag("Player"))
                continue;
            var health = shake.GetComponent<Health>();
            Debug.Log($"[Issue661] {shake.name} に TakeDamage(1) (stopped={health.IsStopped}, timeSinceLastHit={health.TimeSinceLastHit:F2})");
            health.TakeDamage(1f);
            return;
        }
        Debug.LogError("[Issue661] 有効なプレイヤーの HealthCameraShake が見つかりません。");
    }

    /** 現在操作中のプレイヤーに 0 ダメージ(ガード相当)を与える */
    [MenuItem("Tools/Issue661/Debug/Zero Damage Current Player")]
    public static void DebugZeroDamageCurrentPlayer()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogError("[Issue661] プレイモードで実行してください。");
            return;
        }
        foreach (var shake in Object.FindObjectsOfType<HealthCameraShake>())
        {
            if (!shake.CompareTag("Player"))
                continue;
            var health = shake.GetComponent<Health>();
            Debug.Log($"[Issue661] {shake.name} に TakeDamage(0) (stopped={health.IsStopped}, timeSinceLastHit={health.TimeSinceLastHit:F2})");
            health.TakeDamage(0f);
            return;
        }
        Debug.LogError("[Issue661] 有効なプレイヤーの HealthCameraShake が見つかりません。");
    }

    /** シーン内のボス(Player タグ以外の HealthCameraShake 付き)を Kill する */
    [MenuItem("Tools/Issue661/Debug/Kill Boss")]
    public static void DebugKillBoss()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogError("[Issue661] プレイモードで実行してください。");
            return;
        }
        foreach (var shake in Object.FindObjectsOfType<HealthCameraShake>())
        {
            if (shake.CompareTag("Player"))
                continue;
            var health = shake.GetComponent<Health>();
            Debug.Log($"[Issue661] {shake.name} を Kill (stopped={health.IsStopped})");
            health.Kill();
            DebugLogImpulse();
            return;
        }
        Debug.LogError("[Issue661] ボスの HealthCameraShake が見つかりません。");
    }

    /** メインカメラ位置での揺れ量と、メインカメラと vcam の位置差をログに出す */
    [MenuItem("Tools/Issue661/Debug/Log Impulse")]
    public static void DebugLogImpulse()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[Issue661] Main Camera がありません。");
            return;
        }
        bool active = CinemachineImpulseManager.Instance.GetImpulseAt(
            cam.transform.position, false, 1, out Vector3 pos, out Quaternion rot);
        GameObject vcam = GameObject.FindWithTag("CMCamera");
        Vector3 diff = vcam != null ? cam.transform.position - vcam.transform.position : Vector3.zero;
        Debug.Log($"[Issue661] impulse active={active} pos={pos:F3} |pos|={pos.magnitude:F3} rot={rot.eulerAngles:F2} cam-vcam={diff:F3} time={Time.time:F3} timeScale={Time.timeScale}");
    }
}
