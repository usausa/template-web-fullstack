# Prometheus のポート

`Prometheus:Uri` は既定 `http://0.0.0.0:9464`(全インターフェース。Linux / コンテナ向け)、Development は `http://localhost:9464`。

- Windows の http.sys は `0.0.0.0` を受け付けず(エラー 50)、全インターフェースを表す `+` は URI 形式の設定に書けない。Windows で外部から収集させる場合は `http://<マシン名>:9464` を設定し、URL ACL(`netsh http add urlacl url=http://<マシン名>:9464/ user=<user>`)か Windows サービスで束縛する
- **9464 が Hyper-V / WinNAT の除外ポート範囲に入っているとアクセス拒否で起動しない**。`netsh interface ipv4 show excludedportrange protocol=tcp` で確認し、動的ポート範囲が 1024 始まりなら `netsh int ipv4 set dynamic tcp start=49152 num=16384`(管理者・要再起動)で IANA 既定へ戻す
