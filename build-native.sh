#!/usr/bin/env bash
# Builds the C and C++ library for Linux or macOS. Needs gcc and g++.
set -e
cd "$(dirname "$0")"

case "$(uname -s)" in
  Darwin) OUT=libcareer.dylib ;;
  *)      OUT=libcareer.so ;;
esac

gcc -O2 -fPIC -c native/curves.c -o native/curves.o
g++ -O2 -fPIC -shared -o "$OUT" native/market_value.cpp native/curves.o
rm -f native/curves.o
echo "Built $OUT"
