# 構成と動かし方

## 構成

| 要素 | 状態 | 内容 |
|---|---|---|
| API | ✅ | template-web-api と同じ構成。DB のみ PostgreSQL(Npgsql) |
| PostgreSQL | ✅ | AppHost がコンテナとして起動し、接続文字列を API へ注入する |
| 初期化 | ✅ | AppHost が `initializer`(`src/Template.ApiServer.Initializer`)を先に動かし、表を作ってから API を起動する(下記) |
| PostgreSQL の管理画面 | ✅ | AppHost が pgweb(`WithPgWeb()`。`sosedoff/pgweb:0.17.0`・MIT)をコンテナで起動する。ダッシュボードの `pgweb` の URL から、`template` の表の閲覧と SQL の実行ができる |
| Valkey | ✅ | AppHost がコンテナとして起動。HybridCache の L2 と出力キャッシュの保管先(下記)。Redis 互換(BSD-3)。**Redis 本体はライセンス(RSAL / SSPL / AGPL)の理由で使わない** |
| YARP | ✅ | AppHost がコンテナ(`Aspire.Hosting.Yarp`)として起動する前段のリバースプロキシ。`/api/*` を API へ振り分ける(下記) |
| フロントエンド | 未実装 | API の外側に置く要素。候補は Blazor WASM |

## 動かし方

コンテナランタイムに **Podman** を使う。Docker Desktop は不要。

```bash
dotnet run --project src/Template.ApiServer.AppHost
```

AppHost が PostgreSQL と Valkey のコンテナを起動し、PostgreSQL の準備完了を待って `initializer` が表を作る。API は `initializer` の終了と Valkey の準備完了を待ってから起動する。接続文字列は `ConnectionStrings__Default` / `ConnectionStrings__cache` として注入され、`appsettings.json` の既定値(`localhost` の 5432 / 6379)を上書きする。Valkey のパスワードは AppHost が生成し、接続文字列に含まれる。

データは名前付きボリュームへ残るため、再起動しても消えない。`Schema.sql` は `CREATE TABLE IF NOT EXISTS` なので、表に列を足したときは既存のボリュームの表は変わらない(ボリュームを消して作り直すか、pgweb から `ALTER TABLE` を流す)。

### データの初期化(initializer)

`initializer` リソースは、表を作って終わるコンソール(`src/Template.ApiServer.Initializer`)。Host の `Assets/Data/Schema.sql` をリンクで取り込み、API の起動時の作成と同じ `DatabaseService.InitializeAsync` で流す。

```csharp
var initializer = builder.AddProject<Projects.Template_ApiServer_Initializer>("initializer")
    .WithReference(database, connectionName: "Default")
    .WaitFor(database);

var apiserver = builder.AddProject<Projects.Template_ApiServer_Host>("apiserver")
    // ...
    .WithEnvironment("Database__Initialize", "false")
    .WaitForCompletion(initializer)
```

- API は `WaitForCompletion` で `initializer` が正常に終わる(終了コード 0)のを待つ。表を作れなければ `initializer` は 0 以外で終わり、API は起動しない
- API の起動時の作成は `Database:Initialize`(`appsettings.json` の既定は `true`)で切り替える。AppHost は API に `Database__Initialize=false` を渡し、表を作るのを `initializer` だけにする。AppHost を使わずに API を単体で起動するときと結合テストは、既定のまま API の起動時に作る
- 表の定義は `Schema.sql`(`CREATE TABLE IF NOT EXISTS`)の 1 か所。適用の履歴(版の管理)は持たない

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

## template-web-api との違い

| 項目 | web-api | fullstack |
|---|---|---|
| DB | SQLite | PostgreSQL |
| AppHost | API のみ | PostgreSQL / Valkey / YARP(gateway)コンテナと `initializer` を含む |
| 表の作成 | API の起動時に `Schema.sql` を流す | `initializer` が先に流し、API の起動時の作成は設定(`Database:Initialize`)で止める |
| キャッシュ | `AddMemoryCache` のみ(未使用) | HybridCache(メモリ + Valkey)で一覧をキャッシュし、更新系で無効化。公開エンドポイントの出力キャッシュも Valkey |
| 結合テスト | インメモリ | Testcontainers(postgres:18-alpine / valkey:9.1-alpine)。ランタイムが無ければスキップ |

API の構造そのものは同じ。**差分は永続化層・キャッシュとオーケストレーションに閉じている**。
