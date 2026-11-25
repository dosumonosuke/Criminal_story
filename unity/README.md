# Criminal Story — Unity 版

制作エンジンを Godot から **Unity** に切り替えた版です（ユーザー決定）。
`../godot/` は比較・参考用に残してあります。レイキャスター試作
（`../game/index.html`）も同様です。

- **Unity 6.3 LTS（6000.3.29f1）** を前提にしています
  （`ProjectSettings/ProjectVersion.txt`）。
- レンダリングは **HDRP 17.3.0** が第一候補です
  （`Packages/manifest.json` に収録）。
- 物語・事実の方針は Godot 版と同じ：ストーリー主導／操作は限定的／
  第0章は病院で生まれてくる場面から／事実は `../docs/RESEARCH.md` の
  範囲だけ／犯罪の手口・残虐描写なし。

## 大事な設計：シーンは「コードが実行時に生成」します

`Assets/Scenes/Main.unity` の中身は `GameBootstrap` を付けた空の
GameObject 1つだけです。再生すると C# コードが次を全部組み立てます。

- 病院の病室（第0章）とミシガンの住宅街（道路・家・店・車・街灯・
  信号・樹木・人物NPC）を自動生成
- 一人称カメラ、字幕UI、ポストプロセス（HDRP Volume）
- 場面進行：第0章（セリフ自動進行）→ 暗転 → 町（目的は1つ：
  祖母に近づいて E で話しかける）→ 会話 → 完了パネル

手作りシーンファイル（YAML）の再現性に頼らないための方式です。

## Windows PC で開く手順

1. Unity Hub を入れ、**Unity 6.3 LTS（6000.3 系）** のエディタを
   インストールする。モジュールは **Windows Build Support (IL2CPP)**
   を含めること。
2. この `unity/` フォルダを PC にコピーする。
3. Unity Hub の「プロジェクトを開く」で `unity/` フォルダを選ぶ。
   初回はパッケージ解決と素材インポートで時間がかかる（数分〜）。
4. `Assets/Scenes/Main.unity` を開き、再生ボタン（▶）を押すと
   第0章から始まる。

### 操作

| 場面 | 操作 |
|---|---|
| 第0章（出産） | マウス／矢印キーで少しだけ見回す。クリック／スペースでセリフ送り（放置でも自動で進む） |
| 町 | WASD 歩く／マウス 見回す（クリックで掴む、Esc で離す）／E 話しかける |
| 会話 | クリック／スペース／E で送る |

## HDRP を有効にする手順（画質の本命。最初に必ず行う）

このプロジェクトは、HDRP パッケージは入っていますが、
**HDRP アセットの作成と割り当てはエディタ上で行う必要があり**、
それまでは内蔵パイプライン（フォールバック）で描画されます。
コードは両対応で、割り当てが済めば自動で HDRP/Lit に切り替わります。

1. メニュー **Assets > Create > Rendering > HDRP Asset** で
   HDRP アセットを作成する（迷ったら
   **Window > Rendering > HDRP Wizard** を開き、案内に従うのが確実）。
2. **Project Settings > Graphics** の
   「Scriptable Render Pipeline Settings」に、そのアセットを指定する。
3. **Project Settings > Quality** で、各 Quality レベルにも
   同じアセット（または後述の軽量版アセット）を指定する。
4. **Project Settings > Player > Other Settings** で
   Color Space が **Linear** であることを確認する（HDRP は必須）。
5. 空を HDRI にする：Project ウィンドウで
   `Assets/Resources/Environment/kloppenheim_06_puresky_1k.hdr` を選び、
   Inspector の **Texture Shape を Cube** に変えて Apply。
   コード（`PostProcessSetup`）が自動で HDRI Sky として使います。
6. ボリューメトリックフォグ：HDRP アセットの
   **Lighting > Volumetrics** を有効化する（コード側の Fog Volume は
   設定済み）。

### 高画質機能の設定（HDRP アセット／Quality で切り替える）

- **高品質シャドウ**：HDRP アセットの Lighting > Shadows で
  解像度を上げる。Quality レベルごとに別アセットを用意すると
  PC性能に合わせて切り替えられる。
- **スクリーンスペース反射（SSR）**：HDRP アセットで SSR を有効化し、
  Volume の Screen Space Reflections で品質を選ぶ（コードへの
  追加は次段階。現行 Volume は Bloom／DOF／色調整／ビネット／
  フォグ／HDRI Sky まで設定済み）。
- **レイトレーシング（対応PC向け・オフ可能）**：
  1. Player > Other Settings の Graphics APIs for Windows を
     **Direct3D12** にする。
  2. HDRP アセットで **Realtime Raytracing** を有効化し、
     Supported Ray Tracing Mode を選ぶ。
  3. Volume に Ray Tracing Settings 等のオーバーライドを足し、
     SSR 等を Ray Tracing モードに切り替える。
  4. 必須環境：DXR 対応 GPU（GeForce RTX 等）。
     **オフにする方法**：レイトレ用とは別の HDRP アセット
     （RT無効）を作り、Quality レベル「Low」に割り当てる。
     コードやシーンの変更は不要。
- **アップスケーリング**（Unity 公式ドキュメント
  「Introduction to changing resolution scale」で HDRP 対応を確認済み）：
  - 対応：**DLSS**、**FSR 1**、**FSR 2**、**TAAU**、**STP**、
    CAS、Catmull-Rom。
  - 設定場所：HDRP アセットの **Rendering > Dynamic Resolution**
    （Enable を ON。DLSS は **Enable DLSS** を ON にし、
    Package Manager で NVIDIA 関連パッケージが有効なことが前提）。
    適用範囲や品質は Camera の Dynamic Resolution と
    Quality 設定で調整する。
  - **FSR 3 は上記ドキュメントの HDRP 対応表に無いため「未確認」**
    として扱い、本プロジェクトでは使わない。

### 低スペックPC向けの注意

HDRP＋レイトレは重いです。配布時は Quality レベルを最低2つ用意して
ください：**High**（上記の全部入り）と **Low**（別アセットで
SSR／ボリューメトリクス／レイトレを切り、影解像度と描画解像度を
落とす）。ゲーム内設定画面は今後の実装項目です。

## 取り込み済み素材と、見た目をさらに上げる手順

取り込み済み（すべて CC0、台帳は `ASSETS.md`）：

- 空：Poly Haven の HDRI（1K）
- 道路・芝生・壁・歩道／床：ambientCG の PBR テクスチャ
  （Color／NormalGL／Roughness）。地面・道路・壁はこのテクスチャで
  「粘土感」を消すのが本版の主眼です。

### 4Kテクスチャへの差し替え手順

取り込み用コピーは容量の都合で 2048px に縮小してあります
（原版 4K は取得確認済み）。PC 側で 4K に戻すには：

1. https://ambientcg.com/a/Asphalt006 等、各素材ページから
   `*_4K-JPG.zip` をダウンロードする。
2. 中の `*_Color.jpg` `*_NormalGL.jpg` `*_Roughness.jpg` を、
   `Assets/Resources/Textures/<素材名>/` の同名ファイル
   （例：`Asphalt006_Color.jpg`）に上書きする。
3. Unity に戻ると自動で再インポートされる。NormalGL のファイルは
   Inspector で Texture Type = **Normal Map** のままであることを確認。

### 人物モデルについて（正直な現状）

- 現在の人物は基本形状の組み合わせですが、**全員に名札があり、
  服装・髪型・肌の色で判別でき、影も落ちます**。誰が誰か分からない、
  という状態にはしていません。
- 実写寄りの人物モデルは、ライセンスを確認できた無料ソースが
  現時点でありません（調査は `../docs/GRAPHICS_AND_TOWN_PLAN.md`）。
  差し替える場合の手順：
  1. Unity Asset Store で人物モデルを入手する（無料／有料とも、
     各アセットのライセンスと商用利用条件を必ず確認）。
     FBX＋Humanoid リグ付きのものを選ぶ。
  2. モデルを `Assets/Models/` にインポートし、Rig を Humanoid に設定。
  3. `Npc.Build()` の基本形状生成部分を、モデルの Prefab 生成に
     置き換える（名札と `Npc` コンポーネントはそのまま使う）。
  4. 歩行NPCには Animator に歩行クリップを設定する。
- 日本語フォント：標準フォントで文字化けする環境では、
  Noto Sans JP（SIL OFL）等のフォントファイルを
  `Assets/Resources/Fonts/` に入れ、`BuildUtil.DefaultFont()` を
  その読み込みに差し替えてください（1か所だけです）。

## Windows 書き出しと Steam まで

1. **File > Build Profiles**（Unity 6）／Build Settings で
   Windows (x86_64) を選び、Scenes に `Assets/Scenes/Main.unity`
   があることを確認して Build。
2. 出来た `.exe` を起動し、第0章→町→会話→完了パネルまで
   通しで確認する（`../docs/QUALITY_CHECKLIST.md` の P0 項目で判定）。
3. Steam への公開は、開発者本人の Steamworks 登録と登録料が
   必要です（`../docs/STEAM.md`）。SteamPipe でのアップロード、
   コンテンツアンケート（年齢制限・実在人物の注意書き）も
   そちらの手順に従います。

## ファイル構成

- `Assets/Scenes/Main.unity` ― GameBootstrap だけの最小シーン
- `Assets/Scripts/GameBootstrap.cs` ― 起動時に全部を組み立てる
- `Assets/Scripts/WorldBuilder.cs` ― 病院と町の生成（PBRテクスチャ適用）
- `Assets/Scripts/StoryDirector.cs` ― 場面進行の状態機械
- `Assets/Scripts/StoryData.cs` ― セリフ（脚本 `../docs/SCRIPT_WUORNOS.md` 準拠）
- `Assets/Scripts/PlayerController.cs` ― 一人称操作（無効化できる＝ストーリー主導）
- `Assets/Scripts/Npc.cs` ― 人物（名札・歩行）と目的マーカー
- `Assets/Scripts/SubtitleUI.cs` ― 字幕・目的・フェード等のUI一式
- `Assets/Scripts/PostProcessSetup.cs` ― HDRP Volume（Bloom／DOF／色調整／ビネット／フォグ／HDRI Sky）
- `Assets/Scripts/MatUtil.cs` ― マテリアル（HDRP/内蔵の自動切替、PBRテクスチャ読み込み）
- `Assets/Resources/` ― HDRI とテクスチャ（台帳 `ASSETS.md`）
- `Packages/manifest.json` ― HDRP 17.3.0 ほか
- `ProjectSettings/ProjectVersion.txt` ― 6000.3.29f1

## この土台の限界（未確認・未実施）

- この Linux 環境には Unity エディタが無いため、**実機での
  コンパイル・再生確認はまだできていません**。C# は構文パーサーで
  全ファイル検証済み＋API の目視レビュー済みですが、最終確認は
  上記の手順で Windows PC で開いた際に行ってください。
- HDRP アセットの割り当て、レイトレ、DLSS 等の実描画確認も
  GPU 付き PC でないとできません（手順は上に記載）。
- 物語は第0章と第1章の祖母との会話まで。第2章以降は
  `../docs/SCRIPT_WUORNOS.md` の脚本に沿って場面を足していきます。
