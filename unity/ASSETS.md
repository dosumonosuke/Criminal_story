# 取り込み素材台帳（Unity版）

2026-11-25 に、この環境から実際にダウンロードして取り込んだ素材の一覧。
すべて `Assets/Resources/` 配下にあり、コードから読み込まれる。
ライセンス確認の根拠は `../docs/GRAPHICS_AND_TOWN_PLAN.md` §1。

## 取り込み済み

| 素材 | 出典 | ライセンス | 置き場所 | 用途 |
|---|---|---|---|---|
| HDRI「Kloppenheim 06 Pure Sky」1K (.hdr) | Poly Haven — https://polyhaven.com/license （CC0）／DL: https://dl.polyhaven.org/file/ph-assets/HDRIs/hdr/1k/kloppenheim_06_puresky_1k.hdr | CC0（商用可・クレジット不要・再配布可） | `Assets/Resources/Environment/kloppenheim_06_puresky_1k.hdr` | 空と環境光。内蔵パイプラインでは Panoramic Skybox、HDRP では Cubemap 化して HDRI Sky に使用（手順は README） |
| Asphalt006（Color / NormalGL / Roughness） | ambientCG — https://docs.ambientcg.com/license/ （CC0）／`Asphalt006_4K-JPG.zip` | CC0 | `Assets/Resources/Textures/Asphalt006/` | 道路 |
| Grass001（同上3枚） | ambientCG（CC0）／`Grass001_4K-JPG.zip` | CC0 | `Assets/Resources/Textures/Grass001/` | 芝生の地面 |
| Bricks056（同上3枚） | ambientCG（CC0）／`Bricks056_4K-JPG.zip` | CC0 | `Assets/Resources/Textures/Bricks056/` | 家・店の壁 |
| PavingStones036（同上3枚） | ambientCG（CC0）／`PavingStones036_2K-JPG.zip` | CC0 | `Assets/Resources/Textures/PavingStones036/` | 歩道・病院の床 |

### 解像度についての正直な注記

- 原版は 4K（PavingStones036 のみ 2K）でダウンロードし、取得できたことを確認した。
- ただし 4K の JPG 12枚で合計約232MB になり、そのままではリポジトリに
  入れられないため、**取り込み用コピーは最長辺 2048px に縮小**してある
  （合計約19MB）。画質をさらに上げたい場合は、README の
  「4Kテクスチャへの差し替え手順」で原版に置き換えること。
- 法線マップは NormalGL 版を使用（ambientCG の注意どおり。DX版は凹凸が反転する）。
  `.meta` で Texture Type = Normal Map に設定済み。
- Roughness マップは同梱しているが、現行コードでは HDRP/Standard の
  パック形式（MaskMap 等）と形式が違うため、スカラー値で近似している
  （`MatUtil.GetTextured` のコメント参照）。パック化は今後の改善項目。

## 取り込めなかったもの（代替案つき）

| 素材 | 状況 | 理由・代替案 |
|---|---|---|
| Kenney（City Kit Suburban / Roads / Car Kit 等、CC0） | **未取得** | kenney.nl のアセットページは取得できたが、ダウンロードリンクが JavaScript 駆動で、この環境の自動取得では ZIP に到達できなかった。代替：Windows PC のブラウザで https://kenney.nl/assets/city-kit-suburban 等から手動ダウンロードし、`Assets/Models/Kenney/` に入れる（README 手順）。それまでの町の造形は、PBRテクスチャを貼った基本形状で代用している |
| Quaternius（人物モデル） | **未取得（意図的）** | ライセンスが CC0 から独自の QAL v1.0 に変更されており（再配布禁止等）、条件を保存したうえで入れるべきため、今回の自動取り込みには含めなかった。入れる場合はダウンロード日とライセンス文をこの台帳に追記すること |
| 実写寄りの人物モデル | **確認できる無料ソースなし** | `../docs/GRAPHICS_AND_TOWN_PLAN.md` の調査どおり。差し替え手順は README の「人物モデルについて」に記載 |
