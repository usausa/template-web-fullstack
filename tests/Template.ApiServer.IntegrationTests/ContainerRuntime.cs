namespace Template.ApiServer;

using System.Runtime.CompilerServices;

// 結合テストはPostgreSQLコンテナ(Testcontainers)を使う。コンテナランタイムが無い環境(CI等)では起動せずスキップする。
// 環境変数 TEST_CONTAINER で明示できる(docker / podman / none)。未設定なら DOCKER_HOST と名前付きパイプから自動検出する
public static class ContainerRuntime
{
    // Podman machine既定の名前付きパイプ。別名のmachineや他の接続先はDOCKER_HOSTで指定する
    private const string PodmanEndpoint = "npipe://./pipe/podman-machine-default";

    public static bool IsAvailable { get; }

    // スキップ理由
    public static string Reason { get; }

    static ContainerRuntime()
    {
        var setting = Environment.GetEnvironmentVariable("TEST_CONTAINER");
        var mode = String.IsNullOrWhiteSpace(setting) ? Detect() : setting.Trim().ToUpperInvariant();
        switch (mode)
        {
            case "DOCKER":
                IsAvailable = true;
                Reason = string.Empty;
                break;
            case "PODMAN":
                UsePodman();
                IsAvailable = true;
                Reason = string.Empty;
                break;
            default:
                IsAvailable = false;
                Reason = "コンテナランタイム(Docker / Podman)が無いため結合テストを実行しない。TEST_CONTAINER=docker|podman で明示できる";
                break;
        }
    }

    // TestcontainersがDOCKER_HOSTを読む前に検出と設定を済ませる
    [ModuleInitializer]
    internal static void Initialize()
    {
        _ = IsAvailable;
    }

    private static string Detect()
    {
        // 接続先が明示されていればTestcontainersに任せる
        if (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOCKER_HOST")))
        {
            return "DOCKER";
        }

        if (OperatingSystem.IsWindows())
        {
            var pipes = Directory.EnumerateFiles(@"\\.\pipe\").Select(Path.GetFileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (pipes.Contains("docker_engine"))
            {
                return "DOCKER";
            }

            return pipes.Contains("podman-machine-default") ? "PODMAN" : "NONE";
        }

        // Unix系はDocker / rootless PodmanのソケットをTestcontainersが自動で探す
        var runtimeDir = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
        return File.Exists("/var/run/docker.sock") || (!String.IsNullOrEmpty(runtimeDir) && File.Exists(Path.Combine(runtimeDir, "podman", "podman.sock")))
            ? "DOCKER"
            : "NONE";
    }

    private static void UsePodman()
    {
        if (OperatingSystem.IsWindows() && String.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOCKER_HOST")))
        {
            Environment.SetEnvironmentVariable("DOCKER_HOST", PodmanEndpoint);
        }

        // rootlessのPodmanではRyuk(後片付け用コンテナ)が動かない
        if (String.IsNullOrEmpty(Environment.GetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED")))
        {
            Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
        }
    }
}
