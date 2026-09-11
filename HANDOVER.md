# HANDOVER (template-web-fullstack)

作成日: 2026-09-06(2026-09-11 に `template-web-api-postgres` から改名)。template-web-api をベースに、DB を PostgreSQL(Npgsql)へ切り替えた変種として作られたが、**PostgreSQL / Redis / YARP を Aspire で束ねるフルスタックのサンプル**へ位置づけを変更した。計画は [tmpl-plan-fullstack.md](../tmpl-plan-fullstack.md)。

## 1. 状態

- ビルド 0 警告(Debug / Release)/ UnitTests 13 件・IntegrationTests 8 件(Testcontainers の postgres:18-alpine で実行)全通過
- jb inspectcode(2026.2、Release、--no-build --no-swea)指摘 0 件
- 実機確認済み(Podman の postgres:18-alpine に対して login → create 201 → duplicate 409 → list / get 200 → update 204 → delete 204 → get 404)
- ソリューション名・プロジェクト名・名前空間はベースのまま(リポジトリ名のみで識別)。Git は未初期化

## 2. 決定事項

- **ベース(template-web-api)には手を入れない**。方言差は本リポジトリ側で吸収する
- **Database:Provider による切替は設けない**(PostgreSQL 専用)。両対応にするならプロバイダ別に IDbProvider / IDialect / SQL を分けるのが定石(tmpl-guide.md §6-8)
- 一意制約違反は `PostgresException.SqlState == PostgresErrorCodes.UniqueViolation`(23505)で判定。LIKE のエスケープは backslash 方式(PostgreSQL 既定の ESCAPE 文字)
- 識別子は引用符なし(PostgreSQL が小文字に畳むため実テーブルは data / id / name / value / createdat)。Smart.Data.Accessor の列名マッピングは大文字小文字を区別しないためエンティティ側は PascalCase のまま
- 統合テストは Testcontainers.PostgreSql 4.14.0(テストクラスごとにコンテナ起動、IAsyncLifetime)。SQLite 時代のファイル DB 差替えは廃止

## 3. 主な変更点

- Host csproj: Microsoft.Data.Sqlite / SQLitePCLRaw ピン → Npgsql 10.0.3
- ApplicationExtensions.ConfigureComponents: NpgsqlConnection / DelegateDialect(23505 判定 + backslash エスケープ)
- SQL: Create.sql(BIGSERIAL / TEXT / INTEGER / TIMESTAMP、PRIMARY KEY・UNIQUE)、InsertAsync.sql(`RETURNING Id`)。Count / QueryPage / Query / Update / Delete は SQLite と共通のまま
- appsettings.json: `Host=localhost;Port=5432;Database=template;Username=postgres;Password=postgres`
- IntegrationTests: TestApplicationFactory を Testcontainers 化(PostgreSqlBuilder("postgres:18-alpine"))
- README: タイトルと 1 行説明のみ
- G-1(FileStorage の原子的置換 / 一覧系の CancellationToken 伝播)はベースと同時に反映済み

## 4. 実行方法

- ローカル DB: `podman run -d --name template-pg -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=template -p 5432:5432 postgres:18-alpine`
- 統合テスト(Podman): 環境変数 `DOCKER_HOST=npipe://./pipe/podman-machine-default`(**Testcontainers .NET の npipe 形式。Docker CLI の `npipe:////` とは異なる**)と `TESTCONTAINERS_RYUK_DISABLED=true`(rootless)。Git Bash は `//./pipe` をパス変換するため PowerShell から設定する
- Aspire AppHost は Host プロジェクトの起動のみ(PostgreSQL リソースは未定義。必要なら Aspire.Hosting.PostgreSQL の AddPostgres を追加)

## 5. 注意

- Npgsql は DateTime(Kind=Unspecified / Local)を timestamp without time zone に書き込む。UTC 化する場合は列を timestamptz にし Kind=Utc で渡す
- 起動時の `CREATE TABLE IF NOT EXISTS` はベースと同じ(マイグレーション機構なし)
