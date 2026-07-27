using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

/**
 * Play Mode のゲームループ側でフレーム時間・GC・描画統計を収集し、
 * 収集終了時に Temp/ 配下へ集計レポートを書き出すエディタ専用の計測プローブ。
 *
 * Eval は Play Mode 中に UniCli を固めるため、操作はすべて [MenuItem] 経由にしてある。
 * 描画統計は Scene View を巻き込まないよう UnityStats(Game View 限定)を主に使い、
 * ProfilerRecorder は補助として併記する。
 */
[InitializeOnLoad]
public static class ScenePerfProbe
{
    public const string ArmedKey = "VLCNP.ScenePerfProbe.Armed";
    public const string LabelKey = "VLCNP.ScenePerfProbe.Label";
    public const string WarmupKey = "VLCNP.ScenePerfProbe.Warmup";
    public const string DurationKey = "VLCNP.ScenePerfProbe.Duration";
    public const string ReportPath = "Temp/kaze2_perf_report.txt";

    /**
     * Play Mode 終了時に取りこぼしなくレポートを書き出すための、現在生きているサンプラーへの参照。
     */
    public static ScenePerfProbeSampler ActiveSampler;

    static ScenePerfProbe()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void Arm(string label, float warmupSeconds, float durationSeconds)
    {
        EditorPrefs.SetBool(ArmedKey, true);
        EditorPrefs.SetString(LabelKey, label);
        EditorPrefs.SetFloat(WarmupKey, warmupSeconds);
        EditorPrefs.SetFloat(DurationKey, durationSeconds);

        // Scene View だけが描画される状態を避けるため Game View を前面に出す。
        EditorApplication.ExecuteMenuItem("Window/General/Game");

        Debug.Log($"[ScenePerfProbe] Armed label={label} warmup={warmupSeconds}s duration={durationSeconds}s");
    }

    [MenuItem("Tools/Perf/Arm Sampler (Pass A: warmup 10s, sample 30s)", false, 3300)]
    public static void ArmPassA() => Arm("PassA", 10f, 30f);

    [MenuItem("Tools/Perf/Arm Sampler (Pass B: warmup 3s, sample 300s)", false, 3301)]
    public static void ArmPassB() => Arm("PassB", 3f, 300f);

    [MenuItem("Tools/Perf/Disarm Sampler", false, 3302)]
    public static void Disarm()
    {
        EditorPrefs.SetBool(ArmedKey, false);
        Debug.Log("[ScenePerfProbe] Disarmed.");
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // ゲーム側の都合でサンプラーが止まっていても、ここで必ず書き出す。
            if (ActiveSampler != null)
            {
                ActiveSampler.Flush("ExitingPlayMode");
                ActiveSampler = null;
            }

            return;
        }

        if (state != PlayModeStateChange.EnteredPlayMode)
        {
            return;
        }

        if (!EditorPrefs.GetBool(ArmedKey, false))
        {
            return;
        }

        EditorPrefs.SetBool(ArmedKey, false);

        var host = new GameObject("~ScenePerfProbeSampler");
        UnityEngine.Object.DontDestroyOnLoad(host);
        ActiveSampler = host.AddComponent<ScenePerfProbeSampler>();
    }
}

/**
 * ゲームフレームごとに 1 サンプルを取る計測本体。Editor アセンブリに置いてあるため
 * Play Mode 中のみ存在し、ビルドには一切含まれない。
 */
public sealed class ScenePerfProbeSampler : MonoBehaviour
{
    /**
     * 1 つの ProfilerRecorder とそこから読んだ値の列。
     */
    private sealed class Channel
    {
        public string Name;
        public string Unit;
        public double Scale;
        public ProfilerRecorder Recorder;
        public readonly List<double> Samples = new List<double>();
    }

    private readonly List<Channel> channels = new List<Channel>();
    private readonly List<double> frameMs = new List<double>();
    private readonly List<double> drawCalls = new List<double>();
    private readonly List<double> batches = new List<double>();
    private readonly List<double> setPassCalls = new List<double>();
    private readonly List<double> triangles = new List<double>();
    private readonly List<double> dynamicBatched = new List<double>();
    private readonly List<double> staticBatched = new List<double>();

    /**
     * カメラ位置ごとの集計。どの区画が重いかを特定するために使う。
     */
    private sealed class Bucket
    {
        public int Frames;
        public double DrawCallSum;
        public double SetPassSum;
        public double FrameMsSum;
        public double MaxDrawCalls;
        public double MaxFrameMs;
    }

    private const float BucketSize = 10f;
    private readonly Dictionary<(int, int), Bucket> buckets = new Dictionary<(int, int), Bucket>();
    private Camera trackedCamera;

    private const float FlushIntervalSeconds = 3f;

    private string label;
    private float warmupSeconds;
    private float durationSeconds;
    private float startTime;
    private float lastFlushTime;
    private bool sampling;
    private bool finished;

    private void Start()
    {
        label = EditorPrefs.GetString(ScenePerfProbe.LabelKey, "PassA");
        warmupSeconds = EditorPrefs.GetFloat(ScenePerfProbe.WarmupKey, 10f);
        durationSeconds = EditorPrefs.GetFloat(ScenePerfProbe.DurationKey, 30f);
        startTime = Time.realtimeSinceStartup;

        // Render 系カウンタはプロファイラ有効時のみ値を返すため、ここで明示的に有効化する。
        UnityEngine.Profiling.Profiler.enabled = true;

        AddChannel(ProfilerCategory.Render, "Draw Calls Count", "PR Draw Calls", "calls", 1.0);
        AddChannel(ProfilerCategory.Render, "Batches Count", "PR Batches", "batches", 1.0);
        AddChannel(ProfilerCategory.Render, "SetPass Calls Count", "PR SetPass Calls", "calls", 1.0);
        AddChannel(ProfilerCategory.Memory, "GC Allocated In Frame", "GC Alloc / Frame", "bytes", 1.0);

        Debug.Log(
            $"[ScenePerfProbe] Sampler started. label={label} warmup={warmupSeconds}s duration={durationSeconds}s "
                + $"vSync={QualitySettings.vSyncCount} targetFrameRate={Application.targetFrameRate}");
    }

    private void AddChannel(ProfilerCategory category, string counterName, string displayName, string unit, double scale)
    {
        ProfilerRecorder recorder = ProfilerRecorder.StartNew(category, counterName);
        if (!recorder.Valid)
        {
            recorder.Dispose();
            return;
        }

        channels.Add(
            new Channel
            {
                Name = displayName,
                Unit = unit,
                Scale = scale,
                Recorder = recorder,
            });
    }

    private void LateUpdate()
    {
        if (finished)
        {
            return;
        }

        float elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed < warmupSeconds)
        {
            return;
        }

        if (!sampling)
        {
            sampling = true;
            Debug.Log($"[ScenePerfProbe] Warmup done, sampling for {durationSeconds}s.");
        }

        double ms = Time.unscaledDeltaTime * 1000.0;
        frameMs.Add(ms);

        // UnityStats は Game View のみの統計。Scene View の描画を含まない。
        double dc = UnityStats.drawCalls;
        double sp = UnityStats.setPassCalls;
        drawCalls.Add(dc);
        batches.Add(UnityStats.batches);
        setPassCalls.Add(sp);
        triangles.Add(UnityStats.triangles);
        dynamicBatched.Add(UnityStats.dynamicBatchedDrawCalls);
        staticBatched.Add(UnityStats.staticBatchedDrawCalls);

        AccumulateBucket(dc, sp, ms);

        foreach (Channel channel in channels)
        {
            channel.Samples.Add(channel.Recorder.LastValue * channel.Scale);
        }

        // サンプラーがゲーム側の都合で止まっても直近のデータが残るよう、こまめに書き出しておく。
        if (elapsed - lastFlushTime >= FlushIntervalSeconds)
        {
            lastFlushTime = elapsed;
            WriteReport("periodic");
        }

        if (elapsed >= warmupSeconds + durationSeconds)
        {
            Flush("duration reached");
        }
    }

    private void AccumulateBucket(double dc, double sp, double ms)
    {
        if (trackedCamera == null)
        {
            trackedCamera = Camera.main;
            if (trackedCamera == null)
            {
                return;
            }
        }

        Vector3 p = trackedCamera.transform.position;
        var key = (Mathf.FloorToInt(p.x / BucketSize), Mathf.FloorToInt(p.y / BucketSize));
        if (!buckets.TryGetValue(key, out Bucket bucket))
        {
            bucket = new Bucket();
            buckets[key] = bucket;
        }

        bucket.Frames++;
        bucket.DrawCallSum += dc;
        bucket.SetPassSum += sp;
        bucket.FrameMsSum += ms;
        bucket.MaxDrawCalls = Math.Max(bucket.MaxDrawCalls, dc);
        bucket.MaxFrameMs = Math.Max(bucket.MaxFrameMs, ms);
    }

    private void OnDisable() => Flush("OnDisable");

    private void OnApplicationQuit() => Flush("OnApplicationQuit");

    /**
     * レポートを確定し、以降のサンプリングを止める。多重呼び出しに耐える。
     */
    public void Flush(string reason)
    {
        if (finished)
        {
            return;
        }

        finished = true;
        WriteReport(reason);

        foreach (Channel channel in channels)
        {
            if (channel.Recorder.Valid)
            {
                channel.Recorder.Dispose();
            }
        }

        channels.Clear();
        Debug.Log(
            $"[ScenePerfProbe] Report written to {ScenePerfProbe.ReportPath} "
                + $"(frames={frameMs.Count}, reason={reason})");
    }

    private void WriteReport(string reason)
    {
        try
        {
            string report = BuildReport(reason);
            string directory = Path.GetDirectoryName(ScenePerfProbe.ReportPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(ScenePerfProbe.ReportPath, report, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ScenePerfProbe] Failed to write report: {e}");
        }
    }

    private string BuildReport(string reason)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# ScenePerfProbe report");
        sb.AppendLine($"label: {label}");
        sb.AppendLine($"scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().path}");
        sb.AppendLine($"unityVersion: {Application.unityVersion}");
        sb.AppendLine($"buildTarget: {EditorUserBuildSettings.activeBuildTarget}");
        sb.AppendLine($"vSyncCount: {QualitySettings.vSyncCount}");
        sb.AppendLine($"targetFrameRate: {Application.targetFrameRate}");
        sb.AppendLine($"capturedAt: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"writeReason: {reason}");
        sb.AppendLine($"elapsedSinceStart: {Time.realtimeSinceStartup - startTime:F1}s");
        sb.AppendLine($"sampledFrames: {frameMs.Count}");
        sb.AppendLine();
        sb.AppendLine("metric\tunit\tsamples\tmin\tavg\tp95\tmax");

        AppendStats(sb, "Frame Time", "ms", frameMs);
        AppendStats(sb, "Draw Calls (GameView)", "calls", drawCalls);
        AppendStats(sb, "Batches (GameView)", "batches", batches);
        AppendStats(sb, "SetPass Calls (GameView)", "calls", setPassCalls);
        AppendStats(sb, "Triangles (GameView)", "tris", triangles);
        AppendStats(sb, "Dynamic Batched Draw Calls", "calls", dynamicBatched);
        AppendStats(sb, "Static Batched Draw Calls", "calls", staticBatched);

        foreach (Channel channel in channels)
        {
            AppendStats(sb, channel.Name, channel.Unit, channel.Samples);
        }

        AppendBuckets(sb);
        return sb.ToString();
    }

    /**
     * カメラ位置 10x10 ユニットのマス目ごとに、描画コストとフレーム時間をまとめる。
     */
    private void AppendBuckets(StringBuilder sb)
    {
        if (buckets.Count == 0)
        {
            return;
        }

        var rows = new List<KeyValuePair<(int, int), Bucket>>(buckets);
        rows.Sort((a, b) => b.Value.MaxDrawCalls.CompareTo(a.Value.MaxDrawCalls));

        var c = CultureInfo.InvariantCulture;
        sb.AppendLine();
        sb.AppendLine("## per-camera-region (bucket = 10x10 world units, sorted by maxDrawCalls)");
        sb.AppendLine("camX\tcamY\tframes\tavgDrawCalls\tmaxDrawCalls\tavgSetPass\tavgFrameMs\tmaxFrameMs");

        foreach (KeyValuePair<(int, int), Bucket> row in rows)
        {
            Bucket b = row.Value;
            sb.AppendLine(
                $"{row.Key.Item1 * BucketSize}\t{row.Key.Item2 * BucketSize}\t{b.Frames}"
                    + $"\t{(b.DrawCallSum / b.Frames).ToString("F1", c)}\t{b.MaxDrawCalls.ToString("F0", c)}"
                    + $"\t{(b.SetPassSum / b.Frames).ToString("F1", c)}"
                    + $"\t{(b.FrameMsSum / b.Frames).ToString("F2", c)}\t{b.MaxFrameMs.ToString("F2", c)}");
        }
    }

    private static void AppendStats(StringBuilder sb, string name, string unit, List<double> values)
    {
        if (values.Count == 0)
        {
            sb.AppendLine($"{name}\t{unit}\t0\t-\t-\t-\t-");
            return;
        }

        var sorted = new List<double>(values);
        sorted.Sort();

        double sum = 0;
        for (int i = 0; i < sorted.Count; i++)
        {
            sum += sorted[i];
        }

        double p95 = sorted[Mathf.Clamp(Mathf.CeilToInt(sorted.Count * 0.95f) - 1, 0, sorted.Count - 1)];
        var c = CultureInfo.InvariantCulture;
        sb.AppendLine(
            $"{name}\t{unit}\t{sorted.Count}\t{sorted[0].ToString("F2", c)}\t{(sum / sorted.Count).ToString("F2", c)}"
                + $"\t{p95.ToString("F2", c)}\t{sorted[sorted.Count - 1].ToString("F2", c)}");
    }
}
