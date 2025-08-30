param([string]$Msg)

Set-Location "D:\Google Drive\Code\Final project\testing"

git status -sb

if (-not $Msg) {
  $Msg = Read-Host "Commit message"
}

git add -A
git commit -m $Msg 2>$null
git pull --rebase origin main
git push origin main
