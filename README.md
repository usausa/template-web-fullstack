# template-web-fullstack

template-web-api を出発点にした**フルスタック構成のサンプル**。単体で完結する API ではなく、**複数のミドルウェアと複数の種類のサービスを Aspire で束ねて動かす**ことを示す。構成は API に限定せず、フロントエンド等を加える。

## 構成

| 要素 | 状態 | 内容 |
|---|---|---|
| API | ✅ | template-web-api と同じ構成。DB のみ PostgreSQL(Npgsql) |
| PostgreSQL | ✅ | AppHost がコンテナとして起動し、接続文字列を API へ注入する |
| Valkey | 未実装 | 分散キャッシュと出力キャッシュ。Redis 互換(BSD-3)。**Redis 本体はライセンス(RSAL / SSPL / AGPL)の理由で使わない** |
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

### 結合テスト(Testcontainers)

結合テストは PostgreSQL コンテナを Testcontainers で起動する。**コンテナランタイム(Docker / Podman)が無い環境では全件スキップ**され、失敗にはならない(CI 向け)。

| `TEST_CONTAINER` | 動作 |
|---|---|
| 未設定 | 自動検出。`DOCKER_HOST` があればそれを使い、無ければ名前付きパイプ `docker_engine` → `podman-machine-default` の順に探す。どちらも無ければスキップ |
| `docker` | Testcontainers の既定接続(Docker Desktop、または Podman の互換パイプ) |
| `podman` | `DOCKER_HOST=npipe://./pipe/podman-machine-default` と `TESTCONTAINERS_RYUK_DISABLED=true` をテストプロセス内で設定する |
| `none` | 実行しない(スキップ) |

Podman machine を起動していれば未設定のままで動く。`DOCKER_HOST` はテストプロセス内で設定するため、Git Bash から実行してもパス変換の問題は起きない。別名の machine や他の接続先は `DOCKER_HOST` を自分で設定する。

## template-web-api との違い

| 項目 | web-api | fullstack |
|---|---|---|
| DB | SQLite | PostgreSQL |
| AppHost | API のみ | PostgreSQL コンテナを含む |
| 結合テスト | インメモリ | Testcontainers(postgres:18-alpine)。ランタイムが無ければスキップ |

API の構造そのものは同じ。**差分は永続化層とオーケストレーションに閉じている**。

## Prometheus のポート

`Prometheus:Uri` は既定 `http://0.0.0.0:9464`(全インターフェース。Linux / コンテナ向け)、Development は `http://localhost:9464`。

- Windows の http.sys は `0.0.0.0` を受け付けず(エラー 50)、全インターフェースを表す `+` は URI 形式の設定に書けない。Windows で外部から収集させる場合は `http://<マシン名>:9464` を設定し、URL ACL(`netsh http add urlacl url=http://<マシン名>:9464/ user=<user>`)か Windows サービスで束縛する
- **9464 が Hyper-V / WinNAT の除外ポート範囲に入っているとアクセス拒否で起動しない**。`netsh interface ipv4 show excludedportrange protocol=tcp` で確認し、動的ポート範囲が 1024 始まりなら `netsh int ipv4 set dynamic tcp start=49152 num=16384`(管理者・要再起動)で IANA 既定へ戻す
