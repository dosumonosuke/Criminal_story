# Criminal Story — Godot 4 版（3D）

レイキャスター試作（`../game/index.html`）への不満（人がいない／周囲が粘土／
意味が分からない）を受け、土台を **Godot 4.3 の本物の3D** に切り替えた版です。
`game/index.html` は比較用に残してあります。

## ゲームの方針（重要）

- **ストーリー主導**。オープンワールドや自由行動が主役ではありません。
  GTA級を目指すのはグラフィックと町の様子だけです。
- プレイヤーが操作できるのは場面の合間の限られた部分だけ：
  短く歩く／見回す／指定された人に近づいてボタンを押す、程度。
- 目的は常に1つに絞って画面に表示し、終われば自動で次の場面へ進みます。
- 第0章のような場面では、プレイヤーはほぼ見ているだけ（視点は少しだけ動かせる）。

## いま遊べる内容

1. **第0章「生まれる」**（メインシーン `scenes/opening.tscn`）
   - 1956年2月29日、ミシガン州ロチェスターの病院の病室。
   - 医師・看護師・母親（ベッド）が登場。赤ん坊視点で、最初は真っ白に
     ぼやけた世界が数秒かけて開けていく演出＋自動で進むセリフ。
   - セリフが終わると暗転して町のシーンへ自動で移ります。
   - 操作：マウス／矢印キーで少しだけ見回す。クリック／スペースでセリフ送り。
2. **町の試作**（`scenes/town.tscn`）― 第1〜2章の舞台
   - ミシガンの住宅街1区画：交差点のある道路、家10軒、食料品店（看板付き）、
     街路樹、駐車した車5台、街灯、信号機、横断歩道、白い柵、郵便受け。
   - 空・太陽光＋影、軽い霧で奥行きを表現。
   - 人物NPC：歩く近所の人3名、祖父、兄キース、そして目的の「祖母」。
     全員に名札（名前ラベル）付き。
   - 目的は1つだけ：金のマーカーの下の祖母に近づいて **E** で話しかける。
     会話（第1〜2章の事実ベース）が終わると完了パネルが出ます。
   - 操作：WASD 歩く／マウス 見回す（クリックで掴む、Escで離す）／E 話しかける。

## 動かし方

### この環境（Linux）で確認済みの方法

- Godot 本体：`~/workspace/tools/godot/Godot_v4.3-stable_linux.x86_64`
  （公式 GitHub リリース 4.3-stable をダウンロードして使用）
- 起動：`Godot --path ~/workspace/Criminal_story/godot`
- ヘッドレス確認：`Godot --headless --path . --quit-after 400`

### Windows PC で開く場合（ユーザー側の手順）

1. https://godotengine.org/download から Godot 4.3（Windows版）を入れる（無料）。
2. この `godot` フォルダを PC にコピーする。
3. Godot のプロジェクトマネージャで `project.godot` を読み込む（Import）。
4. 実行ボタン（F5相当）で第0章から始まります。
5. Steam 向け書き出しは `export_presets.cfg`（Windows Desktop プリセット）を
   用意済み。ただし書き出しには Godot の Export Templates の追加インストールが
   必要です（エディタの案内から無料で入れられます）。Steam への公開自体には
   本人の Steamworks 登録と登録料が別途必要です（`../docs/STEAM.md`）。

## シーンの作り方（再生成）

町の建物や人物は、素材ファイルではなく `scripts/build_scenes.gd` が
Godot 標準の3D形状とマテリアルから組み立てて `.tscn` に保存しています。

```
Godot --headless --path . res://tools/builder.tscn
```

これで `scenes/opening.tscn` と `scenes/town.tscn` が再生成されます。
配置や色を変えたい場合は `build_scenes.gd` を直して再実行してください。

## ファイル構成

- `project.godot` ― プロジェクト設定（メインシーンは第0章）
- `scenes/opening.tscn` ― 第0章：病院の病室（生成済み）
- `scenes/town.tscn` ― 町の試作（生成済み）
- `scripts/opening.gd` ― 第0章の演出（セリフ送り、ぼやけ→開ける演出、場面遷移）
- `scripts/player.gd` ― 一人称操作、目的表示、会話、完了パネル
- `scripts/npc.gd` ― 歩くNPC
- `scripts/marker.gd` ― 目的マーカーの上下動・回転
- `scripts/build_scenes.gd` ― シーン生成ビルダー
- `tools/builder.tscn` ― ビルダー実行用シーン
- `tools/screenshot.*` ― テスト用スクリーンショット撮影（ゲーム本体ではありません）
- `export_presets.cfg` ― Windows 向け書き出しプリセット

## 事実と表現の方針

- 日時・場所・人物は `../docs/RESEARCH.md` で確認済みの範囲だけを使います。
- 出産シーンは出産直後の病室の様子として穏やかに描き、生々しい医療描写はしません。
- 犯罪の手口・残虐描写は入れません。
