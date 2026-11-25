# 3D版で使う素材とライセンス

## 現在（2026-11-25、町1区画の試作）

外部アセットは **まだ使っていません**。ダウンロードせずに土台を作るため、
町並み・病院・人物はすべて Godot 標準の3D形状（Box／Cylinder／Sphere／Capsule）と
自作の StandardMaterial3D（色・粗さ・発光の設定）で組み立てています。

| もの | 出どころ | ライセンス |
|---|---|---|
| Godot Engine 4.3 本体 | https://godotengine.org / 公式GitHubリリース | MIT License |
| 町・病院・人物の3D形状とマテリアル | 本プロジェクトで自作（`godot/scripts/build_scenes.gd`） | 本プロジェクトのもの |
| 空（ProceduralSkyMaterial） | Godot 標準機能で生成 | MIT License（エンジン側） |

この環境で使った Godot 本体：
`~/workspace/tools/godot/Godot_v4.3-stable_linux.x86_64`（4.3.stable.official）

## 今後使う予定の無料アセット（未導入）

質を上げる次の段階で、以下から **CC0（パブリックドメイン相当）のものだけ** を
選んで導入する前提です。導入したら、この表に「使ったファイル名・入手ページ・
ライセンス」を1点ずつ追記します。

| 候補 | 内容 | ライセンス | 状態 |
|---|---|---|---|
| Kenney (https://kenney.nl/assets) | 建物・家具・小物などの3Dモデル | CC0 | 未導入 |
| Quaternius (https://quaternius.com/) | 人物・街並みなどの3Dモデル | CC0 | 未導入 |
| Poly Haven (https://polyhaven.com/) | HDRI（空・光）とテクスチャ | CC0 | 未導入 |

## 注意

- CC0 以外（利用条件付きの無料素材など）を入れる場合は、条件を確認してから
  ここに記録すること。
- 有料アセットや、再配布が禁止された素材は使わない。
