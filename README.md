# template-web-fullstack

template-web-api を出発点にした**フルスタック構成のサンプル**。単体で完結する API ではなく、**複数のミドルウェアと複数の種類のサービスを Aspire で束ねて動かす**ことを示す。構成は API に限定せず、フロントエンド等を加える。

## 構成

| 要素 | 状態 | 内容 |
|---|---|---|
| API | ✅ | template-web-api と同じ構成。DB のみ PostgreSQL(Npgsql) |
| PostgreSQL | ✅ | AppHost がコンテナとして起動し、接続文字列を API へ注入する |
| Valkey | ✅ | AppHost がコンテナとして起動。HybridCache の L2 と出力キャッシュの保管先(下記)。Redis 互換(BSD-3)。**Redis 本体はライセンス(RSAL / SSPL / AGPL)の理由で使わない** |
| YARP | ✅ | AppHost がコンテナ(`Aspire.Hosting.Yarp`)として起動する前段のリバースプロキシ。`/api/*` を API へ振り分ける(下記) |
| フロントエンド | 未実装 | API の外側に置く要素。候補は Blazor WASM |

未実装分の計画は [tmpl-plan-fullstack.md](../tmpl-plan-fullstack.md)。

## 動かし方

コンテナランタイムに **Podman** を使う。Docker Desktop は不要。

```bash
dotnet run --project src/Template.ApiServer.AppHost
```

AppHost が PostgreSQL と Valkey のコンテナを起動し、準備完了を待ってから API を起動する。接続文字列は `ConnectionStrings__Default` / `ConnectionStrings__cache` として注入され、`appsettings.json` の既定値(`localhost` の 5432 / 6379)を上書きする。Valkey のパスワードは AppHost が生成し、接続文字列に含まれる。

データは名前付きボリュームへ残るため、再起動しても消えない。

### 前段プロキシ(YARP)

`gateway` リソースが YARP のコンテナ(`mcr.microsoft.com/dotnet/nightly/yarp`)で、API の準備完了を待ってから起動する。プロジェクトは増やさず、経路は AppHost の `WithConfiguration` で書く。

```csharp
builder.AddYarp("gateway")
    .WithConfiguration(yarp => yarp.AddRoute("/api/{**catch-all}", apiserver))
    .WaitFor(apiserver);
```

`/api/*` だけを API へ転送する(`/health` や `/swagger` は API 直接)。gateway のホスト側ポートはダッシュボードで確認する。フロントエンド等を足す場合は `AddRoute` を追加し、経路で振り分ける。レート制限やカスタムミドルウェアが要る場合は YARP をプロジェクト化する(`src/Template.ApiServer.Gateway`)。

### キャッシュ(Valkey + HybridCache)

| 層 | 登録 | 役割 |
|---|---|---|
| L1 | (`AddHybridCache()` が `IMemoryCache` を登録する) | プロセス内(既定 1 分)。`AddMemoryCache()` を別途書く必要はない |
| L2 | `AddStackExchangeRedisCache()` | Valkey。複数インスタンスで共有する(既定 5 分) |
| 窓口 | `AddHybridCache()` | `HybridCache` 1 つで L1 / L2 の読み書き・直列化・同時要求の合流を担う |

使う側は `DataService` だけ。一覧(`QueryPageAsync`)を検索条件ごとのキー(`data:list:{name}:{sort}:{desc}:{page}:{size}`)でタグ `data` 付きで保持し、登録・更新・削除で `RemoveByTagAsync("data")` により一括で無効化する。パッケージ名や API 名に残る `Redis` はプロトコル名で、サーバーは Valkey。

### 出力キャッシュ(Valkey)

`AddOutputCache` + `AddStackExchangeRedisOutputCache` で応答そのものを Valkey に保持する(`app.UseOutputCache()` は認可の後)。付けるのは**認証なしの公開エンドポイントだけ**(`/api/v1/test/time` に `CacheOutput(CachePolicies.Public)`、30 秒)。認証付きのエンドポイントに素朴に付けると利用者間で応答が混ざるため、既定ポリシーは `Authorization` ヘッダー付きの要求と `Set-Cookie` 付きの応答を保存しない。キャッシュから返った応答には `Age` ヘッダーが付く。

### 結合テスト(Testcontainers)

結合テストは PostgreSQL と Valkey(`valkey/valkey:9.1-alpine`。Redis モジュールをイメージ差し替えで流用)のコンテナを Testcontainers で起動する。**コンテナランタイム(Docker / Podman)が無い環境では全件スキップ**され、失敗にはならない(CI 向け)。

| `TEST_CONTAINER` | 動作 |
|---|---|
| 未設定 | 自動検出。`DOCKER_HOST` があればそれを使い、無ければ名前付きパイプ `docker_engine` → `podman-machine-default` の順に探す。見つかっても Testcontainers が接続できなければスキップ(machine 停止後にパイプだけ残ることがある) |
| `docker` | Testcontainers の既定接続(Docker Desktop、または Podman の互換パイプ) |
| `podman` | `DOCKER_HOST=npipe://./pipe/podman-machine-default` と `TESTCONTAINERS_RYUK_DISABLED=true` をテストプロセス内で設定する |
| `none` | 実行しない(スキップ) |

Podman machine を起動していれば未設定のままで動く。`DOCKER_HOST` はテストプロセス内で設定するため、Git Bash から実行してもパス変換の問題は起きない。別名の machine や他の接続先は `DOCKER_HOST` を自分で設定する。

## template-web-api との違い

| 項目 | web-api | fullstack |
|---|---|---|
| DB | SQLite | PostgreSQL |
| AppHost | API のみ | PostgreSQL / Valkey / YARP(gateway)コンテナを含む |
| キャッシュ | `AddMemoryCache` のみ(未使用) | HybridCache(メモリ + Valkey)で一覧をキャッシュし、更新系で無効化。公開エンドポイントの出力キャッシュも Valkey |
| 結合テスト | インメモリ | Testcontainers(postgres:18-alpine / valkey:9.1-alpine)。ランタイムが無ければスキップ |

API の構造そのものは同じ。**差分は永続化層・キャッシュとオーケストレーションに閉じている**。

## Prometheus のポート

`Prometheus:Uri` は既定 `http://0.0.0.0:9464`(全インターフェース。Linux / コンテナ向け)、Development は `http://localhost:9464`。

- Windows の http.sys は `0.0.0.0` を受け付けず(エラー 50)、全インターフェースを表す `+` は URI 形式の設定に書けない。Windows で外部から収集させる場合は `http://<マシン名>:9464` を設定し、URL ACL(`netsh http add urlacl url=http://<マシン名>:9464/ user=<user>`)か Windows サービスで束縛する
- **9464 が Hyper-V / WinNAT の除外ポート範囲に入っているとアクセス拒否で起動しない**。`netsh interface ipv4 show excludedportrange protocol=tcp` で確認し、動的ポート範囲が 1024 始まりなら `netsh int ipv4 set dynamic tcp start=49152 num=16384`(管理者・要再起動)で IANA 既定へ戻す
