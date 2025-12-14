#!/usr/bin/env bash
set -e

APP_NAME=ImageBarser
CONFIG=Release
FRAMEWORK=net8.0
OUT=dist
RIDS=(
  osx-arm64
  osx-x64
  linux-x64
  win-x64
)
rm -rf $OUT
mkdir -p $OUT
for RID in "${RIDS[@]}"; do
  echo "Building for $RID"

  dotnet publish \
    -c $CONFIG \
    -r $RID \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -o $OUT/$RID

  if [[ "$RID" == win-* ]]; then
    mv $OUT/$RID/$APP_NAME.exe $OUT/$RID/$APP_NAME-$RID.exe
  else
    mv $OUT/$RID/$APP_NAME $OUT/$RID/$APP_NAME-$RID
    chmod +x $OUT/$RID/$APP_NAME-$RID
  fi
done

echo "Build complete. Artifacts in $OUT/"