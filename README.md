# template-web-fullstack

template-web-api を出発点にした**フルスタック構成のサンプル**。単体で完結する API ではなく、**複数のミドルウェアと複数の種類のサービスを Aspire で束ねて動かす**ことを示す。構成は API に限定せず、フロントエンド等を加える。

## 構成

| 要素 | 状態 | 内容 |
|---|---|---|
| API | ✅ | template-web-api と同じ構成。DB のみ PostgreSQL(Npgsql) |
| PostgreSQL | ✅ | AppHost がコンテナとして起動し、接続文字列を API へ注入する |
| Redis | 未実装 | 分散キャッシュと出力キャッシュ |
| YARP | 未実装 | 前段のリバースプロキシ |
| フロントエンド | 未実装 | API の外側に置く要素。候補は Blazor WASM |

未実装分の計画は [tmpl-plan-fullstack.md](../tmpl-plan-fullstack.md)。

## 動かし方

コンテナランタイムに **Podman** を使う。Docker Desktop は不要。

```bash
dotnet run --project src/Template.ApiServer.AppHost
```

AppHost が PostgreSQL コンテナを起動し、準備完了を待ってから API を起動する。接続文字列は `ConnectionStrings__Default` として注入され、`appsettings.json` の既定値を上書きする。

データは名前付きボリュームへ残るため、再起動しても消えない。

### Podman を使う場合の環境変数

結合テスト(Testcontainers)を動かすときは次を設定する。**PowerShell から設定すること**。Git Bash はパスを変換してしまう。

```
DOCKER_HOST=npipe://./pipe/podman-machine-default
```

## template-web-api との違い

| 項目 | web-api | fullstack |
|---|---|---|
| DB | SQLite | PostgreSQL |
| AppHost | API のみ | PostgreSQL コンテナを含む |
| 結合テスト | インメモリ | Testcontainers(postgres:18-alpine) |

API の構造そのものは同じ。**差分は永続化層とオーケストレーションに閉じている**。

## 既知の問題

**`dotnet run` で API が起動しない**(2026-09-11 時点)。Prometheus の設定が `http://0.0.0.0:9464` で、Windows の `HttpListener` は `0.0.0.0` への束縛に管理者権限か URL ACL の予約を要求するため、アクセス拒否で起動に失敗する。

**この問題は template-web-api でも同じように起きる**。本テンプレート固有ではない。回避するには `appsettings.Development.json` で `Prometheus:Uri` を空にするか `http://localhost:9464` にする。
