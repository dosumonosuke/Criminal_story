#!/bin/bash
# Criminal Story - 素材の自動ダウンロード (Mac / Linux 用)
# GitHub版には画像素材が入っていないため、公式サイト (CC0) から取り直します。
# 使い方: ターミナルでこのファイルがある unity フォルダへ移動して
#          bash fetch-assets.sh
# すべて無料・商用利用可 (CC0) の素材です。出典は ASSETS.md を参照。
set -e
ROOT="$(cd "$(dirname "$0")" && pwd)"
TMP="$ROOT/_asset_tmp"
mkdir -p "$TMP"

get_file() {
  if [ -f "$2" ]; then echo "already exists: $2"; return; fi
  echo "downloading: $1"
  curl -L --fail -A "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36" -o "$2" "$1"
}

# 1) 空の HDRI (Poly Haven, CC0)
mkdir -p "$ROOT/Assets/Resources/Environment"
get_file "https://dl.polyhaven.org/file/ph-assets/HDRIs/hdr/1k/kloppenheim_06_puresky_1k.hdr" "$ROOT/Assets/Resources/Environment/kloppenheim_06_puresky_1k.hdr"

# 2) 質感テクスチャ 4種 (ambientCG, CC0)
download_set() {
  name="$1"; zip="$2"
  get_file "https://ambientcg.com/get?file=${zip}" "$TMP/${zip}"
  rm -rf "$TMP/${name}"
  mkdir -p "$TMP/${name}"
  unzip -o -q "$TMP/${zip}" -d "$TMP/${name}"
  dest="$ROOT/Assets/Resources/Textures/${name}"
  mkdir -p "$dest"
  # ZIP内の名前は Asphalt006_4K_Color.jpg の形式。プロジェクト側の名前に合わせて _4K_ / _2K_ を取り除く
  find "$TMP/${name}" -type f \( -name "*_Color.jpg" -o -name "*_NormalGL.jpg" -o -name "*_Roughness.jpg" \) | while read -r f; do
    base="$(basename "$f")"
    new="$(echo "$base" | sed -e 's/_4K-JPG//' -e 's/_2K-JPG//' -e 's/_4K_/_/' -e 's/_2K_/_/')"
    cp "$f" "$dest/$new"
    echo "placed: $new"
  done
}

download_set "Asphalt006" "Asphalt006_4K-JPG.zip"
download_set "Grass001" "Grass001_4K-JPG.zip"
download_set "Bricks056" "Bricks056_4K-JPG.zip"
download_set "PavingStones036" "PavingStones036_2K-JPG.zip"

rm -rf "$TMP"
echo ""
echo "Done. Unity でプロジェクトを開き直すか、Assets > Refresh を押してください。"
