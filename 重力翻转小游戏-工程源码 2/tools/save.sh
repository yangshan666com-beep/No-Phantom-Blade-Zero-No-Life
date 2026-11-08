#!/bin/bash
set -e
cd "$(dirname "$0")/.."

if [ -z "$1" ]; then
  echo "用法: ./tools/save.sh \"这次的改动说明\""
  exit 1
fi

git add -A
git commit -m "$1"
git push
