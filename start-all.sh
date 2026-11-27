#!/usr/bin/env bash
# Starts everything that is installed: PHP saves, Java news, then the C# game.
cd "$(dirname "$0")"

PIDS=()
cleanup() { for p in "${PIDS[@]}"; do kill "$p" 2>/dev/null; done; }
trap cleanup EXIT

if command -v php >/dev/null; then
  php -S 127.0.0.1:8082 -t php >/dev/null 2>&1 & PIDS+=($!)
  echo "PHP save service on http://127.0.0.1:8082"
else
  echo "PHP not found: saving is switched off."
fi

if command -v java >/dev/null; then
  java java/NewsServer.java >/dev/null 2>&1 & PIDS+=($!)
  echo "Java news service on http://127.0.0.1:8081"
else
  echo "Java not found: news is switched off."
fi

if [ ! -f libcareer.so ] && [ ! -f libcareer.dylib ] && command -v g++ >/dev/null; then
  ./build-native.sh
fi

echo "Starting the game on http://localhost:5000 (press Ctrl+C to stop everything)"
dotnet run
