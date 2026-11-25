# Criminal Story - 素材の自動ダウンロード (Windows PowerShell)
# GitHub版には画像素材が入っていないため、このスクリプトで公式サイト (CC0) から取り直します。
# 使い方: このファイルがある unity フォルダで右クリック -> PowerShell で実行、
#          または PowerShell を開いて  ./fetch-assets.ps1  を実行してください。
# すべて無料・商用利用可 (CC0) の素材です。出典は ASSETS.md を参照。

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$tmp = Join-Path $root "_asset_tmp"
New-Item -ItemType Directory -Force -Path $tmp | Out-Null

function Get-File($url, $dest) {
    if (Test-Path $dest) { Write-Host "already exists: $dest"; return }
    Write-Host "downloading: $url"
    Invoke-WebRequest -Uri $url -OutFile $dest -UserAgent "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36"
}

# 1) 空の HDRI (Poly Haven, CC0)
$envDir = Join-Path $root "Assets\Resources\Environment"
New-Item -ItemType Directory -Force -Path $envDir | Out-Null
Get-File "https://dl.polyhaven.org/file/ph-assets/HDRIs/hdr/1k/kloppenheim_06_puresky_1k.hdr" (Join-Path $envDir "kloppenheim_06_puresky_1k.hdr")

# 2) 質感テクスチャ 4種 (ambientCG, CC0)
$sets = @(
    @{ Name = "Asphalt006";       Zip = "Asphalt006_4K-JPG.zip" },
    @{ Name = "Grass001";         Zip = "Grass001_4K-JPG.zip" },
    @{ Name = "Bricks056";        Zip = "Bricks056_4K-JPG.zip" },
    @{ Name = "PavingStones036";  Zip = "PavingStones036_2K-JPG.zip" }
)
foreach ($s in $sets) {
    $zipPath = Join-Path $tmp $s.Zip
    Get-File ("https://ambientcg.com/get?file=" + $s.Zip) $zipPath
    $extract = Join-Path $tmp $s.Name
    if (Test-Path $extract) { Remove-Item -Recurse -Force $extract }
    Expand-Archive -Path $zipPath -DestinationPath $extract -Force
    $destDir = Join-Path $root ("Assets\Resources\Textures\" + $s.Name)
    New-Item -ItemType Directory -Force -Path $destDir | Out-Null
    # ZIP内の名前は Asphalt006_4K_Color.jpg の形式。プロジェクト側の名前に合わせて _4K_ / _2K_ を取り除く
    Get-ChildItem -Path $extract -Recurse -Include "*_Color.jpg", "*_NormalGL.jpg", "*_Roughness.jpg" | ForEach-Object {
        $newName = $_.Name -replace "_4K-JPG", "" -replace "_2K-JPG", "" -replace "_4K_", "_" -replace "_2K_", "_"
        Copy-Item $_.FullName (Join-Path $destDir $newName) -Force
        Write-Host "placed: $newName"
    }
}

Remove-Item -Recurse -Force $tmp
Write-Host ""
Write-Host "Done. Unity でプロジェクトを開き直すか、Assets > Refresh を押してください。"
Write-Host "配置先: Assets\Resources\Environment と Assets\Resources\Textures"
