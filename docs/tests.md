# 結合テスト(Testcontainers)

結合テストは PostgreSQL と Valkey(`valkey/valkey:9.1-alpine`。Redis モジュールをイメージ差し替えで流用)のコンテナを Testcontainers で起動する。**コンテナランタイム(Docker / Podman)が無い環境では全件スキップ**され、失敗にはならない(CI 向け)。

| `TEST_CONTAINER` | 動作 |
|---|---|
| 未設定 | 自動検出。`DOCKER_HOST` があればそれを使い、無ければ名前付きパイプ `docker_engine` → `podman-machine-default` の順に探す。見つかっても Testcontainers が接続できなければスキップ(machine 停止後にパイプだけ残ることがある) |
| `docker` | Testcontainers の既定接続(Docker Desktop、または Podman の互換パイプ) |
| `podman` | `DOCKER_HOST=npipe://./pipe/podman-machine-default` と `TESTCONTAINERS_RYUK_DISABLED=true` をテストプロセス内で設定する |
| `none` | 実行しない(スキップ) |

Podman machine を起動していれば未設定のままで動く。`DOCKER_HOST` はテストプロセス内で設定するため、Git Bash から実行してもパス変換の問題は起きない。別名の machine や他の接続先は `DOCKER_HOST` を自分で設定する。
