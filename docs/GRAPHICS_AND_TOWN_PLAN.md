# グラフィックと町の様子を上げる計画（GRAPHICS AND TOWN PLAN）

作成：2026-11-25／担当：グラフィック・町並み調査
対象：`godot/`（Godot 4.3、Chapter 0 病院＋ミシガン住宅街プロトタイプ）

---

## 0. 結論と、正直な前提

- GTA6 本体は数千人規模・専用エンジン製で、その複製はこのプロジェクトでは不可能。
  ここで目指すのは **「1区画だけを、密度と光で“本物の町”に見せる」** こと。
  オープンワールドの広さではなく、**プレイヤーが実際に歩く狭い範囲の密度**で勝負する。
  本作は操作が限られたストーリー主導型なので、この方針と相性が良い
  （見せる範囲を絞れる＝その範囲にだけ予算を全部使える）。
- 見た目を決める順番は **①光（ライティング）②地面と道路の質感 ③小物の密度
  ④建物のバリエーション ⑤モデルの精細さ**。モデル単体のポリゴン数ではない。
  「粘土感」の正体は、ほぼ①〜③の欠落である。
- **最大の前提条件：現在の `project.godot` は `gl_compatibility` レンダラーである。**
  このままだと Godot 4 の見た目向上機能の大部分（SDFGI、ボリューメトリックフォグ、
  SSR、SSIL、Decal 等）が**使えない**。最初の一手は Forward+ への切り替えであり、
  それには Vulkan 対応 GPU での確認が必須になる（§4 の環境切り分け参照）。

### 現在の土台の実測（2026-11-25 にファイルを確認）

| 項目 | 現状 |
|---|---|
| レンダラー | GL Compatibility（`renderer/rendering_method="gl_compatibility"`） |
| 町 | `build_scenes.gd` の手続き生成：Box の家10軒・店1軒、道路、横断歩道、街灯、信号、車5台、木 |
| 空・光 | ProceduralSkyMaterial＋DirectionalLight3D（影ON、最大距離120m）、Filmic トーンマップ、薄いフォグ |
| 材質 | すべて単色 `StandardMaterial3D`（テクスチャなし）← 粘土感の主因 |
| 人物 | カプセル等の簡易 NPC（`npc.gd`） |

---

## 1. 無料・商用利用可のアセット入手先（2026-11-25 に Web で確認済み）

確認方法：各サイトの公式ライセンス／サポートページを実際に開いて読んだ。
確認できなかったものは「未確認」と明記し、推測で「使える」とは書かない。

### 1-1. 確認済み（そのまま採用候補）

#### Kenney（モデル・小物・車・街の基本キット）
- ライセンス確認先：https://kenney.nl/support
  公式サポートに「all game assets on the asset pages are public domain licensed (CC0).
  You're free to use them, even in commercial projects」「Attribution is not required」
  と明記されていることを確認した。注意点は1つだけ：Kenney のロゴ自体は使わない。
- 本作に効くパック（いずれも上記 CC0 の対象。パックページは kenney.nl/assets 配下）：
  - City Kit (Suburban)：https://kenney.nl/assets/city-kit-suburban
    ― 郊外の家・フェンス・庭まわり。ミシガン住宅街の土台。
  - City Kit (Roads)：https://kenney.nl/assets/city-kit-roads
    ― 道路パーツ、街灯などの小物。
  - City Kit (Commercial)：https://kenney.nl/assets/city-kit-commercial
    ― 店舗・低層商業ビル。フロリダ編のロードサイド店にも転用可。
  - Car Kit：https://kenney.nl/assets/car-kit ― 車両一式。
  - Nature Kit：https://kenney.nl/assets/nature-kit ― 木・岩・茂み・花（330モデル規模）。
  - Furniture Kit：https://kenney.nl/assets/furniture-kit ― 病院・家・バーの室内小物。
- 形式：GLB/glTF を含む（Godot 4 は glTF をそのままインポート可）。
- 弱点：ローポリで様式化されている。フォトリアル路線とは混ぜにくい（§1-3 の路線判断参照）。

#### Quaternius（人物 NPC の本命）
- **重要：ライセンスが変わっている。** かつて各パックに書かれていた CC0 ではなく、
  現在の公式ライセンスは独自の **Quaternius Asset License (QAL) v1.0**。
  確認先：https://quaternius.com/license.html（ページ記載の最終更新 8/28/2026）
  - 要旨：個人・教育・**商用ゲームに無料で使用可、クレジット不要**。
  - ただし **アセットそのものの再販売・再配布は禁止**（完成したゲームに組み込んで
    配布するのは可）。著作者を偽ることも禁止。
  - 第7条により、入手時点の版が適用される。**ダウンロード日と、その時点の
    ライセンス文を必ず保存すること**（§5 のアセット台帳）。
  - なお itch.io のパックページには今も「(CC0 License)」と書かれているものがある
    （例：https://quaternius.itch.io/universal-base-characters ）。
    表記が食い違う以上、安全側として **QAL の条件（再配布禁止）で扱う**。
- 本作に効くパック：
  - Universal Base Characters：https://quaternius.com/packs/universalbasecharacters.html
    ― 男女×体型6種の素体＋髪型。NPC の顔と体型のバリエーション源。
  - Universal Animation Library（歩行・待機などの汎用アニメーション）。
    パックページの正確なURLは本調査では未確認 → ダウンロード時に公式サイト
    （https://quaternius.com/ ）から辿って記録すること。
  - Public Transport 等の乗り物パックも存在するが、本作の時代物には車種が合わない
    場合があるため採用は個別判断。
- 弱点：様式はローポリ寄り。髪・服の色替えで「別人化」する運用が前提。

#### Poly Haven（空＝HDRI と、フォトリアル寄せテクスチャ／小物）
- ライセンス確認先：https://polyhaven.com/license
  「Our assets are all licensed as CC0」「You can use our assets for any purpose,
  including commercial work」「You do not need to give credit」「You can redistribute
  them ... even in a product you sell」と明記されていることを確認した。
  - 注意：CC0 なのはアセット本体のみ。サイトのロゴ・紹介文・サンプル画像は対象外。
    またサイトのスクレイピングは規約で禁止。取得は各アセットページか公式 API から。
  - 公式 API（キー不要）：https://api.polyhaven.com/assets?type=hdris
    （type は hdris / textures / models）。API には別途 API Terms があるため、
    自動取得を組む前にそちらも読むこと。
- 本作での使い道：
  - **HDRI（空と環境光）**：曇りのミシガン、晴れたフロリダなど、場面ごとの空を
    1枚で差し替える。光の説得力への寄与が最も大きい投資の1つ。
  - テクスチャ：アスファルト、コンクリート、レンガ等の PBR セット（最大8Kまであるが
    本作では 1K〜2K を落とす。§2-4）。
  - モデル：椅子・テーブル等の実写寄り小物。様式が浮く場合は「主役の部屋だけ」に限定。

#### ambientCG（テクスチャの主力）
- ライセンス確認先：https://docs.ambientcg.com/license/
  「All ambientCG assets are provided under the Creative Commons CC0 1.0 Universal
  License」「You can include the raw files in your project, for example a video game」
  と明記されていることを確認した。クレジット不要（任意の書式が同ページに提示あり）。
- 使い道：PBR マテリアル（地面、舗装、レンガ、木材、金属、汚れ系デカール素材）。
  アセットページの短縮URL形式は `https://ambientcg.com/a/<id>`
  （例として同ドキュメントに https://ambientcg.com/a/PavingStones036 が提示されている）。
- **落とし穴：法線マップは NormalGL 版を使う。** ambientCG は NormalGL / NormalDX の
  両方を同梱する。Godot の標準マテリアルが期待するのは OpenGL 形式（GL）であり、
  DX 版を入れると凹凸が反転して「なんか変」になる。取り込み時のチェック項目に固定する。

#### KayKit（Kay Lousberg）― 使えるが様式に注意
- 確認先（パックページ）：https://kaylousberg.itch.io/kaykit-dungeon-pack
  「Free for personal and commercial use, no attribution required. (CC0 Licensed)」
  と明記されていることを確認した。ただし同ページの注意書きどおり、
  未改変コピーの再販売や自作の主張はしないこと。
- 本作での位置づけ：作風は中世・ファンタジー寄りが中心で、本作（現代アメリカ）には
  そのままでは合わない。**刑務所・地下・小物の一部に使えるかは未確認**
  （現代物の専用パックの有無を本調査では確認できていない）。主力にはしない。

### 1-2. 未確認（使う前に必ず個別確認。現時点では採用しない）

| 入手先 | 状態 | 理由 |
|---|---|---|
| Poly Pizza | **未確認** | モデルごとにライセンスが異なるとされるが、本調査で公式条件を直接読めていない |
| OpenGameArt | **未確認** | 投稿ごとに CC0／CC-BY／GPL 等が混在する構造。個別確認なしに一括採用しない |
| Sketchfab の無料モデル | **未確認** | モデルごとの CC 条件が前提。ダウンロード可＝商用可ではない |
| Mixamo（人物アニメーション） | **未確認** | 利用には Adobe アカウントが必要で、公式条件を本調査で直接確認できていない。特に「生データの再配布」可否は公開リポジトリ運用に直結するため、確認が取れるまで使わない |
| 時代物フォント（看板用） | **未確認** | 看板の文字に使うフォントは SIL OFL 等の再配布可ライセンス品だけに限定し、入手時に記録する。具体的なフォント名は未選定 |

### 1-3. 路線判断（重要）：様式を統一しないと「安いコラージュ」になる

無料アセットは大きく2系統あり、**混ぜると一気に安く見える**。

- 系統A（様式化ローポリ）：Kenney＋Quaternius＋KayKit。形は簡素だが、色と光で統一
  すれば「1つの世界」として成立する。人物・車・建物を同じ系統で揃えられるのが強み。
- 系統B（実写寄せ）：Poly Haven＋ambientCG の PBR。地面と壁は一気に実在感が出るが、
  そこにローポリ人物を置くと浮く。実写寄りの人物モデルは、今回確認できた無料
  ソースの中には**見当たらなかった（未確認事項として残す）**。

**推奨：系統Aを骨格にし、地面・道路・壁だけ系統Bのテクスチャで底上げする
「ハイブリッド」。** 人物と建物のシルエットはAで統一し、プレイヤーの目が最も
長く触れる地面と、近寄る壁面だけ写真素材で質感を付ける。テクスチャの彩度と
粗さをA側に寄せて調整すれば破綻しにくい。最終判断は §5 の比較スクリーンショットで
行い、決めたら以後の章はその様式から外さない。

---

## 2. Godot 4 で町の見た目を上げる具体策（実装手順レベル）

対象バージョンは現在のプロジェクトと同じ Godot 4.3 系。
公式ドキュメントの裏付けがある項目には根拠を併記した。

### 2-0. Step 0：レンダラーを Forward+ に切り替える（全ての前提）

手順：
1. `project.godot` の `[rendering]` を変更する：
   - `renderer/rendering_method="forward_plus"`
   - `renderer/rendering_method.mobile="mobile"`（将来の参考。本作の主対象は PC）
2. この変更で使えるようになるもの（公式ドキュメントのレンダラー比較表で確認）：
   SDFGI、SSIL、SSR、ボリューメトリックフォグ、FogVolume、Decal（Compatibility では
   非対応）、TAA、CompositorEffect。
   根拠：https://docs.godotengine.org/en/4.3/tutorials/rendering/renderers.html 相当の
   比較表（検索で公式 godot-docs の renderers.rst を確認）。
3. 代償と注意：
   - Vulkan 対応 GPU が必須。**この Linux VM には GPU デバイス（/dev/dri）が無く、
     Forward+ の実描画を確認できない**（§4）。切り替え後の見た目確認は
     Windows PC 側の作業になる。切り替えた瞬間に「VM では未検証」の状態になる
     ことを開発ログに明記する。
   - 既存の ProceduralSkyMaterial と単色マテリアルは Forward+ でもそのまま動く
     （壊れるのは Compatibility 固有の前提だけ）。まず切り替えて、現状の町が
     同じに見えることを Windows 側で確認してから次へ進む。

### 2-1. ライティング（見た目の6割はここで決まる）

共通の作り方：
1. **空は HDRI に差し替える**（Poly Haven、CC0）。手順：`.hdr`/`.exr` を
   プロジェクトに入れる → Sky リソースの `sky_material` に `PanoramaSkyMaterial`
   を作り `panorama` に HDRI を設定 → Environment の `background_mode=SKY`、
   `ambient_light_source=SKY`、`reflected_light_source=SKY`。
   これで「空の見た目・環境光・反射」が1枚で揃い、単色マテリアルの平坦さが消える。
   場面ごとに HDRI を差し替える（ミシガン冬の曇天／フロリダの強い日差し／夕方）。
   ※ HDRI が無い場面（室内・刑務所）はこの手順を使わず §2-1-C へ。
2. **太陽（DirectionalLight3D）は1つだけ**にし、影を主役にする：
   - `shadow_enabled=true`、町の規模（約140m四方）に合わせ
     `directional_shadow_max_distance` は歩行範囲＋余裕（目安 150〜200）。
   - 影の分割（PSSM）は 4 分割を基本にし、近距離の影の解像感を優先する。
   - 太陽の角度は場面の時刻で固定する（朝の長い影／真昼の短い影／夕方の橙）。
     角度を変えたら HDRI も同じ時刻帯のものに合わせる（空と影の方向の不一致は
     一番バレる嘘）。
3. **トーンマッピングは ACES、調整で色を締める**：
   Environment の `tonemap_mode=ACES`、`adjustment_enabled` で彩度をわずかに
   落とし（目安 0.9 前後）、コントラストを微増。無料アセットの「原色っぽさ」を
   ここで統一するのが §1-3 ハイブリッドの接着剤になる。
4. **間接光（GI）の使い分け**：
   - 屋外の町：**SDFGI**（Forward+ 専用。ベイク不要でカメラに追従する全体照明）。
     `sdfgi_enabled=true`、カスケード数は町の規模なら 4 を起点に調整。
   - 静止物が多い室内（家の中、バー、病室）：**LightmapGI** でベイクする方が
     安定して綺麗になる。静的オブジェクトに `geometry/lightmap` の対象設定をし、
     ベイクは Windows PC 側で行う（§4）。
   - 限られた室内空間（刑務所の廊下など）：**VoxelGI** も候補。範囲が小さい場面で
     動的キャラにも間接光が乗る。3方式を1場面で全部盛りにしない（重さと破綻の元）。

場面別レシピ：
- **A. ミシガン住宅街（昼）**：曇り〜晴れの HDRI、太陽エネルギー控えめ、
  SDFGI ON、フォグは薄く（§2-2）。冬章（1956年2月）は「低い太陽＋雪の反射で
  全体が明るいのに寒い」方向：環境光を青寄りにし、地面テクスチャを雪版に差し替え。
- **B. フロリダ・ハイウェイ（昼〜夕）**：強い太陽＋青空 HDRI、彩度は上げすぎない、
  夕方の場面では太陽を低くして看板の発光（Emission＋Glow）を効かせる。
  雨上がり場面では SSR（画面空間反射）で濡れたアスファルトを表現（§2-3）。
- **C. 刑務所・病院（室内）**：窓からの光は SpotLight3D／OmniLight3D の数を
  絞って配置し、基本は LightmapGI ベイク。蛍光灯の「緑っぽい白」は
  ライト色とマテリアルの色温度で作る。天井灯ごとに実ライトを置かず、
  「発光マテリアル（Emission）＋代表点だけ実ライト」が軽くて見栄えする定石。

### 2-2. フォグと空気感

1. 通常フォグ：Environment の `fog_enabled=true`、色は空の地平線色に合わせる。
   町の端（何も無い平原）が見える問題を、フォグ＋遠景の木立と建物シルエットで隠す。
   現在の `fog_density=0.0035` は起点として妥当。Forward+ では高さフォグ
   （`fog_height`／`fog_height_density`）も使えるので、朝の場面では地面近くに
   薄く溜まる設定を足す。
2. ボリューメトリックフォグ（Forward+ 専用）：`volumetric_fog_enabled=true`、
   密度はごく薄く（重い処理なので常時ONの主役にしない）。
   効く場面を絞る：朝の住宅街、刑務所廊下の光の筋（FogVolume を光源前に置く）、
   フロリダの夕方の逆光。`volumetric_fog_length` は歩行範囲に合わせて短く切る。
3. 空の雲：HDRI の空に雲が含まれていれば追加作業なし。ProceduralSky のまま
   行く場合は雲表現が弱いので、原則 HDRI へ移行する（§2-1-1）。

### 2-3. ポストプロセス（盛る順番と止めどころ）

Environment で有効化する順（上から順に効果が大きい）：
1. **SSAO**（`ssao_enabled`）：物と地面の接地点に影の締まりが出て「浮き」が消える。
   粘土感対策として最優先級。半径・強さは初期値から微調整。
2. **Glow**（`glow_enabled`）：看板・街灯・窓明かりの発光を滲ませる。
   しきい値を上げ気味にして「光っている所だけ」に限定する（全体がボヤける設定は禁止）。
3. **SSIL**（`ssil_enabled`、Forward+ 専用）：室内の柔らかい間接光の補助。
   重ければ切る候補1号。
4. **SSR**（`ssr_enabled`、Forward+ 専用）：濡れた路面・車のボディの映り込み専用。
   乾いた昼の住宅街ではほぼ不要なので場面ごとに ON/OFF する。
5. **被写界深度（DOF）**：通常の歩行中は切る。出産シーンや会話の寄りなど、
   カットシーン的な場面でのみ `dof_blur` を使う（本作の演出方針と相性が良い）。
6. **アンチエイリアス**：Forward+ では TAA が使える。静止画は綺麗だが動きで
   にじむ場合は MSAA 3D（2x〜4x）と比較して決める。比較はスクリーンショットと
   実プレイの両方で行う（§5）。
7. カメラ側：FOV は歩行時 70〜75 を基準に固定し、場面で変えない（酔いと
   スケール感のブレ防止）。赤ん坊視点は FOV を広げ気味＋低い位置で表現する
   （Chapter 0 の既存実装方針を踏襲）。

### 2-4. テクスチャ運用（解像度と取り込みの決まり）

- 取得元は ambientCG／Poly Haven（§1）。ダウンロード時に選ぶ解像度の決まり：
  - 地面・道路・壁など大面積で近寄るもの：**2K**
  - 家の壁・屋根など中距離の主役：**2K（壁）／1K（屋根）**
  - 小物・遠景：**1K 以下**。8K は本作では落とさない（容量と VRAM の無駄）。
- PBR セットの対応（Godot の StandardMaterial3D）：
  - Color → `albedo_texture`
  - NormalGL → `normal_texture`（**DX 版は使わない**、§1-1 ambientCG の注意）
  - Roughness → `roughness_texture`、AO → `ao_texture`（チャンネル指定に注意）、
    Displacement は原則使わない（重い・破綻しやすい）。凹凸は法線で表現する。
- 繰り返しの隠し方（タイリングの「壁紙感」を消す）：
  - テクスチャの `uv1_scale` を実寸に合わせる（例：レンガは実物大換算）。
  - 同じ壁に Decal で汚れ・ひび・落書きを重ねる（§2-6）。
  - 大面積の地面は2種のテクスチャを頂点色やマスクで混ぜるか、Decal の
    まだら汚れで単調さを割る。最初から完璧を狙わず「Decal で割る」が最速。
- インポート設定：テクスチャは VRAM 圧縮を有効化し、ミップマップ生成を ON
  （遠景のチラつき防止）。プロジェクト既存設定の
  `textures/vram_compression/import_etc2_astc=true` は PC 向けには BPTC が本命で
  ある点に注意し、Windows 向けビルド時に圧縮形式を再確認する（未確認事項：
  現在の export プリセットの中身は本調査では未読 → §5 の確認リストへ）。

### 2-5. 建物のバリエーション（「全軒同じ家」を終わらせる）

現在の `_make_house` は色違いの同型家である。次
...[truncated 20862 chars]